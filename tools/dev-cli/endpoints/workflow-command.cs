#region Purpose
// Executes the verify-only CI workflow, and skips it for a non-build pull request.
#endregion
#region Design
// Verify-only scaffold: self-install -> clean -> build -> test.
// Self-install uses the runfile because bin/dev is gitignored and missing on a
// fresh CI checkout. Later steps use ./bin/dev (bin/dev.exe on Windows).
// The first step is the shared pull request build classifier from
// TimeWarp.Nuru.DevCli (PullRequestBuildClassifier.DecideAsync, timewarp-nuru
// 595). On a pull_request whose diff touches only declared non-build paths it
// skips self-install, clean, build, and test and exits 0. Push, release, a
// dispatch, and a local run always build. This repo's additions to the rule
// (readme.md and assets/ are packed into the package) live in
// .timewarp/dev.jsonc under pullRequestBuild; the rule itself is not copied
// here. This scaffold does not pack, attest, or promote; workflow.yml runs the
// push and release build and publish steps.
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

      PullRequestBuildDecision decision = await PullRequestBuildClassifier.DecideAsync(RepoRoot, ct);
      Terminal.WriteLine(decision.Message);
      if (!decision.RequiresBuild)
      {
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
}
