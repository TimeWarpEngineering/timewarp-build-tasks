# Bring repo to audit baseline

## Description

`ganda repo audit` on master reported 28 failed checks (26 auto-fixable). Apply `ganda repo audit --fix` as the policy baseline, then repair anything the fixes break, and prove the NuGet package still builds and packs identically (same files, same version, `build/TimeWarp.Build.Tasks.props` still present at the exact PackageId casing).

## Checklist

- [x] Record baseline: `dotnet build -c Release` and `dotnet pack -c Release`, nupkg listing saved
- [x] `ganda repo audit --fix` (twice; second run stabilized at 4 failures) and commit
- [x] Repair NU1108 restore cycle caused by the fix adding a TimeWarp.Build.Tasks reference to every project
- [x] Repair RS0030 (banned ProcessStartInfo) build break caused by the new BannedApiAnalyzers
- [x] kebab-path-names: rename props to kebab-case on disk, pin packed name via PackagePath
- [x] nuget-package-icon: use `PackagePath="\"` for logo.png
- [x] Re-pack and diff nupkg against baseline (identical listing and nuspec)
- [x] analyzer-suppressions: keep RS0030 NoWarn, accept it in `.timewarp/suppression-baseline.json` (documented hand edit), csproj comment states the Amuru circular-dependency reason
- [x] source-file-line-cap: removed the in-repo NuGet cache (root cause) instead of baselining it; `.timewarp/line-cap-baseline.json` deleted
- [x] ci-code-coverage: `ci-code-coverage.severity = off` under `[ganda.audit]`
- [x] Remove `RestorePackagesPath`/`LocalNuGetCache`, delete `local-nuget-cache/`, drop its gitignore line, re-seed suppression baseline (249 to 9 entries)
- [x] Final audit clean, build and pack re-verified

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-10)

## Results

### Audit counts
- Before: Passed 11 / Failed 28 / Skipped 7
- Final: Passed 42 / Failed 0 / Skipped 3 (re-run after the cache removal)

### What --fix changed
`.github/workflows/workflow.yml`, `.gitignore`, `Directory.Build.props` (imports msbuild/repository.props; adds BannedApiAnalyzers, TimeWarp.SourceGenerators, TimeWarp.Build.Tasks package references), `Directory.Packages.props` (new pins; removed orphaned `Microsoft.SourceLink.GitHub` pin, which no project referenced), `source/Directory.Build.props` (`PackageVersion` renamed to `Version`, still 1.0.0), removed `.vscode/settings.json` from git. Added: `.editorconfig`, `.envrc`, `.githooks/`, `.timewarp/suppression-baseline.json`, `.vscode/tasks.json`, `BannedSymbols.txt`, `glossary.md`, `msbuild/repository.props`, `tools/dev-cli/` scaffold, and `.gitkeep` placeholders for `documentation/`, `skills/`, `tests/`, `kanban/archived`, `kanban/backlog`.

### Manual repairs beyond --fix
- `timewarp-build-tasks.csproj`: `<PackageReference Remove="TimeWarp.Build.Tasks" />` (the package cannot reference itself; NU1108 cycle).
- `timewarp-build-tasks.csproj`: `NoWarn RS0030` because `git-metadata-task.cs` uses `ProcessStartInfo` and the task DLL is standalone in MSBuild, so it cannot depend on TimeWarp.Amuru. (Using `Process.StartInfo` member access does not avoid RS0030; tried.)
- Renamed `build/TimeWarp.Build.Tasks.props` to `build/timewarp-build-tasks.props`; csproj packs it with `PackagePath="build/TimeWarp.Build.Tasks.props"` and `buildTransitive/TimeWarp.Build.Tasks.props`. `claude.md` references updated.
- `source/Directory.Build.props`: logo `PackagePath="/"` to `"\"` (check did not accept `/`).

### Owner decisions applied (previously remaining failures)
- `analyzer-suppressions`: `ProcessStartInfo` and `NoWarn RS0030` kept. The normal replacement, TimeWarp.Amuru, could create an indirect circular dependency for this build-tasks package. Accepted per `repo-audit.md` (raising the baseline is a hand edit of the JSON): `source/timewarp-build-tasks/timewarp-build-tasks.csproj` -> `RS0030: 1` in `.timewarp/suppression-baseline.json`.
- `source-file-line-cap`: the hits were restored package files under gitignored `local-nuget-cache/`. The check walks the disk and ignores .gitignore (skips only `.git`, `obj`, `bin`, `artifacts`; no exclude key). Owner decision: remove the cache entirely. `RestorePackagesPath` pointed at it, but nothing in the repo restores from a local feed, so the isolation did nothing. Removed `RestorePackagesPath` and `LocalNuGetCache` from root `Directory.Build.props` (kept `LocalNuGetFeed`, the PackageOutputPath), deleted the 607 MB directory, dropped its `.gitignore` line (no check required it), deleted `.timewarp/line-cap-baseline.json`, and re-seeded `.timewarp/suppression-baseline.json` with `ganda repo audit --fix` (249 entries down to 9, no `local-nuget-cache` entries, `RS0030: 1` for the csproj kept). Packages now restore to the global `~/.nuget/packages`.
- `ci-code-coverage`: off in `.editorconfig` (no test project).

### Verification
- `dotnet build -c Release`: 0 warnings, 0 errors before and after.
- `dotnet pack -c Release`: `TimeWarp.Build.Tasks.1.0.0.nupkg` before and after. `unzip -l` listing (names and sizes) identical; nuspec identical; `build/` and `buildTransitive/` props byte-identical to baseline. Contains `build/TimeWarp.Build.Tasks.props`.
- Not verified: building `tools/dev-cli` runfiles/endpoints (not part of the slnx build), and a real consumer project importing the package.

## Notes

Plain `dotnet pack` defaults to Release; the DLL path in the csproj uses `$(OutputPath)`, so a Debug build then pack fails with NU5019 (pre-existing).
