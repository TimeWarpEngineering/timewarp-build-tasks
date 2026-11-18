# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/014-bump-dev-cli-package-pins-to-latest-prerelease` (`7602ae2`) vs merge-base `1db819c`. Product commit `4f82e63`: `Directory.Packages.props`, `source/Directory.Build.props`, `tools/dev-cli/endpoints/workflow-command.cs`.

## Summary

The four dev-only pins move to the versions in the task table. `Microsoft.Build.*` stays at `18.12.2`, and `TimeWarp.Build.Tasks`, `TimeWarp.Terminal`, and `Microsoft.CodeAnalysis.BannedApiAnalyzers` are unchanged. `<Version>` moves from `2.0.1-beta.1` to `2.0.1-beta.2`, which requirement 6 allows when `dev check-version` refuses a version already on the feed. Task 013 records `2.0.1-beta.1` on nuget.org, so that refusal matches the repo history.

`SelfInstallAsync` now calls `DotNet.Run().WithFile("tools/dev-cli/dev.cs").WithArguments("self-install")` and keeps `WithWorkingDirectory(RepoRoot)`, `WithNoValidation()`, and the existing non-zero exit handling. `DotNet` comes from `TimeWarp.Amuru.Tools`, which `tools/dev-cli/Directory.Build.props` already references, and `WithArguments` is documented as arguments to the program being run. The other dev-cli steps already use `DotNet.Clean`, `DotNet.Build`, and `DotNet.Test`. Hook runfiles call `Shell.Builder` only for `git` and `ganda`, so AMURU0001 does not apply to them. `TimeWarp.Build.Tasks.csproj` sets `SuppressDependenciesWhenPacking` and removes the self package reference, so the pin bump cannot add a nuspec dependency.

The implementer restatement matches the original task. Recorded gates cover the change: self-install after the AMURU0001 fix, `./bin/dev build` (0 warnings, 0 errors, including a rerun after the version bump), hook `dotnet build` for all six hooks, `./bin/dev check-version` passing at `2.0.1-beta.2`, `ganda nuget outdated`, and `ganda repo audit`. The checklist leaves a full `./bin/dev test` to CI because `tests/` has no test files. This review did not re-run those gates.

The recorded nupkg comparison was taken before the version bump. Targeted check of the already-built artifacts, not a rebuild: `unzip -Z1` and `unzip -p` on `artifacts/packages/TimeWarp.Build.Tasks.2.0.1-beta.1.nupkg` and `TimeWarp.Build.Tasks.2.0.1-beta.2.nupkg`. Both zips have the same ten entries. The nuspecs differ only in `<version>` (`2.0.1-beta.1` vs `2.0.1-beta.2`). Neither has a `<dependencies>` element.

## Issues

No issues.
