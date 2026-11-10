#region Purpose
// Executes the verify-only CI workflow, and skips it for a non-build pull request.
#endregion
#region Design
// Verify-only scaffold: self-install -> clean -> build -> test.
// Self-install uses the runfile because bin/dev is gitignored and missing on a
// fresh CI checkout. Later steps use ./bin/dev (bin/dev.exe on Windows).
// A pull_request whose every path is a declared non-build path
// (WorkflowCommand.NonBuildPaths.RequiresBuild) skips self-install, clean,
// build, and test. Paths under source/, tests/, tools/, and msbuild/ always
// build, including markdown. .editorconfig builds because warnings are errors.
// The allowlist matches WorkflowChangeFilter.RequiresBuild: kanban/**,
// documentation/**, assets/**, notes/**, .github/** except workflows, a file
// named .gitignore, and markdown outside those four roots. An empty diff does
// not build. Base and head SHAs come from GITHUB_EVENT_PATH; the diff is
// base...head. An unreadable event or diff runs the full pipeline. Push and a
// local run do not skip. Repos that publish NuGet replace this file with the
// Nuru promotion pipeline. This scaffold does not pack, attest, or promote.
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI workflow")]
public sealed class WorkflowCommand : ICommand<Unit>
{
  internal sealed class Handler : IRequestHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private CancellationToken Ct;
    private string RepoRoot = null!;
    private string DevBin = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      Ct = ct;

      if (!FindRepoRoot()) return Unit.Value;

      if (await TrySkipNonBuildPullRequestAsync())
      {
        Terminal.WriteLine(NonBuildPaths.SkipMessage);
        Terminal.WriteLine("\nWorkflow completed successfully!".Green());
        return Unit.Value;
      }

      Terminal.WriteLine("Pipeline: self-install -> clean -> build -> test");
      if (!await SelfInstallAsync()) return Unit.Value;
      if (!await RunStepAsync("clean", "Clean failed!")) return Unit.Value;
      if (!await RunStepAsync("build", "Build failed!")) return Unit.Value;
      if (!await RunStepAsync("test", "Tests failed!")) return Unit.Value;

      Terminal.WriteLine("\nWorkflow completed successfully!".Green());
      return Unit.Value;
    }

    private bool FindRepoRoot()
    {
      string? root = Git.FindRoot();
      if (root is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      RepoRoot = root;
      string binaryName = OperatingSystem.IsWindows() ? "dev.exe" : "dev";
      DevBin = Path.Combine(RepoRoot, "bin", binaryName);
      Terminal.WriteLine("Starting CI workflow...");
      return true;
    }

    private async Task<bool> TrySkipNonBuildPullRequestAsync()
    {
      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      if (!string.Equals(eventName, "pull_request", StringComparison.Ordinal))
      {
        return false;
      }

      string? eventPath = Environment.GetEnvironmentVariable("GITHUB_EVENT_PATH");
      if (string.IsNullOrWhiteSpace(eventPath) || !File.Exists(eventPath))
      {
        Terminal.WriteLine("Could not read the pull request event; running the full pipeline.");
        return false;
      }

      string json = await File.ReadAllTextAsync(eventPath, Ct);
      if (!NonBuildPaths.TryReadPullRequestShas(json, out string baseSha, out string headSha))
      {
        Terminal.WriteLine("Could not read the pull request base and head; running the full pipeline.");
        return false;
      }

      IReadOnlyList<string>? changed = await ListChangedPathsAsync(RepoRoot, baseSha, headSha, Ct);
      if (changed is null)
      {
        Terminal.WriteLine("Could not diff the pull request; running the full pipeline.");
        return false;
      }

      return !NonBuildPaths.RequiresBuild(changed);
    }

    private static async Task<IReadOnlyList<string>?> ListChangedPathsAsync(
      string repoRoot,
      string baseSha,
      string headSha,
      CancellationToken cancellationToken)
    {
      if (!NonBuildPaths.IsSha(baseSha) || !NonBuildPaths.IsSha(headSha))
      {
        return null;
      }

      CommandOutput result = await Shell.Builder("git")
        .WithArguments("diff", "--name-only", baseSha + "..." + headSha)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(cancellationToken);

      if (result.ExitCode != 0)
      {
        return null;
      }

      string text = (result.Stdout ?? "")
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace('\r', '\n');
      List<string> paths = [];
      foreach (string line in text.Split('\n'))
      {
        string trimmed = line.Trim();
        if (trimmed.Length > 0)
        {
          paths.Add(trimmed);
        }
      }

      return paths;
    }

    private async Task<bool> SelfInstallAsync()
    {
      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--file", "tools/dev-cli/dev.cs", "--", "self-install")
        .WithWorkingDirectory(RepoRoot)
        .WithNoValidation()
        .RunAsync(Ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Self-install failed!".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> RunStepAsync(string subcommand, string failureMessage)
    {
      int exitCode = await Shell.Builder(DevBin)
        .WithArguments(subcommand)
        .WithNoValidation()
        .RunAsync(Ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine(failureMessage.Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }
  }

  /// <summary>
  /// Path allowlist for skipping build and test on a pull request.
  /// Same rule as <c>WorkflowChangeFilter.RequiresBuild</c>.
  /// </summary>
  public static class NonBuildPaths
  {
    /// <summary>Printed when a pull request diff does not require a build.</summary>
    public const string SkipMessage =
      "Pull request changes are declared non-build paths. Skipping self-install, clean, build, and test.";

    /// <summary>
    /// True when any path is outside the declared non-build allowlist.
    /// An empty list does not require a build.
    /// </summary>
    public static bool RequiresBuild(IEnumerable<string> relativePaths)
    {
      ArgumentNullException.ThrowIfNull(relativePaths);
      foreach (string raw in relativePaths)
      {
        if (!IsNonBuildPath(raw))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Reads <c>pull_request.base.sha</c> and <c>pull_request.head.sha</c>.
    /// Both must be hex SHAs.
    /// </summary>
    internal static bool TryReadPullRequestShas(string? json, out string baseSha, out string headSha)
    {
      baseSha = "";
      headSha = "";
      if (string.IsNullOrWhiteSpace(json))
      {
        return false;
      }

      try
      {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("pull_request", out JsonElement pullRequest))
        {
          return false;
        }

        return TrySha(pullRequest, "base", out baseSha) && TrySha(pullRequest, "head", out headSha);
      }
      catch (JsonException)
      {
        return false;
      }
    }

    /// <summary>True when <paramref name="value"/> is 7 to 64 hex characters.</summary>
    internal static bool IsSha(string value)
    {
      ArgumentNullException.ThrowIfNull(value);
      if (value.Length is < 7 or > 64)
      {
        return false;
      }

      foreach (char character in value)
      {
        bool hex = character is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');
        if (!hex)
        {
          return false;
        }
      }

      return true;
    }

    private static bool IsNonBuildPath(string raw)
    {
      if (string.IsNullOrWhiteSpace(raw))
      {
        return false;
      }

      string path = raw.Replace('\\', '/').Trim();
      if (path.StartsWith("./", StringComparison.Ordinal))
      {
        path = path[2..];
      }

      if (path.Length == 0)
      {
        return false;
      }

      if (path.StartsWith(".github/workflows/", StringComparison.Ordinal)
        || string.Equals(path, ".github/workflows", StringComparison.Ordinal))
      {
        return false;
      }

      if (IsBuildRoot(path))
      {
        return false;
      }

      if (path.StartsWith("kanban/", StringComparison.Ordinal)
        || path.StartsWith("documentation/", StringComparison.Ordinal)
        || path.StartsWith("assets/", StringComparison.Ordinal)
        || path.StartsWith("notes/", StringComparison.Ordinal)
        || path.StartsWith(".github/", StringComparison.Ordinal))
      {
        return true;
      }

      string name = path;
      int slash = path.LastIndexOf('/');
      if (slash >= 0)
      {
        name = path[(slash + 1)..];
      }

      if (string.Equals(name, ".gitignore", StringComparison.Ordinal))
      {
        return true;
      }

      return name.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBuildRoot(string path)
    {
      string[] roots = ["source", "tests", "tools", "msbuild"];
      foreach (string root in roots)
      {
        if (string.Equals(path, root, StringComparison.Ordinal)
          || path.StartsWith(root + "/", StringComparison.Ordinal))
        {
          return true;
        }
      }

      return false;
    }

    private static bool TrySha(JsonElement pullRequest, string side, out string sha)
    {
      sha = "";
      if (!pullRequest.TryGetProperty(side, out JsonElement element))
      {
        return false;
      }

      if (!element.TryGetProperty("sha", out JsonElement shaElement)
        || shaElement.ValueKind != JsonValueKind.String)
      {
        return false;
      }

      string? value = shaElement.GetString();
      if (value is null || !IsSha(value))
      {
        return false;
      }

      sha = value;
      return true;
    }
  }
}
