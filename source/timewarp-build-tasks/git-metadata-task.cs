namespace TimeWarp.Build.Tasks;
using System;
using System.Diagnostics;
using Microsoft.Build.Framework;

/// <summary>
/// MSBuild task that retrieves git commit metadata (hash and date) for embedding in assembly metadata.
/// </summary>
public sealed class GitMetadataTask : Microsoft.Build.Utilities.Task
{
  /// <summary>
  /// The full git commit hash (SHA-1).
  /// </summary>
  [Output]
  public string CommitHash { get; set; } = string.Empty;

  /// <summary>
  /// The commit date in ISO 8601 format (yyyy-MM-ddTHH:mm:ss).
  /// </summary>
  [Output]
  public string CommitDate { get; set; } = string.Empty;

  /// <summary>
  /// Executes the task to retrieve git metadata.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  public override bool Execute()
  {
    try
    {
      // Get commit hash
      CommitHash = RunGitCommand("rev-parse HEAD");

      // Get commit timestamp
      string unixTimeString = RunGitCommand("log -1 --format=%ct");

      if (long.TryParse(unixTimeString, out long unixSeconds))
      {
        CommitDate = DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            .ToString("yyyy-MM-dd'T'HH:mm:ss");
      }
      else
      {
        Log.LogWarning($"Failed to parse git timestamp: {unixTimeString}");
        CommitDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss");
      }

      Log.LogMessage(MessageImportance.Low, $"Git metadata: Hash={CommitHash}, Date={CommitDate}");
      return true;
    }
    catch (Exception ex)
    {
      Log.LogWarning($"Failed to retrieve git metadata: {ex.Message}. Using fallback values.");
      CommitHash = "unknown";
      CommitDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss");
      return true; // Don't fail the build if git is unavailable
    }
  }

  /// <summary>
  /// Runs a git command and returns its output.
  /// </summary>  
  private static string RunGitCommand(string arguments)
  {
    ProcessStartInfo processStartInfo = new("git", arguments)
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using Process process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start git process");
    string output = process.StandardOutput.ReadToEnd().Trim();
    string error = process.StandardError.ReadToEnd().Trim();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
      throw new InvalidOperationException($"Git command failed: {error}");
    }

    return output;
  }
}
