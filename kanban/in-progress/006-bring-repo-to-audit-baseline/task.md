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
- [ ] Owner decisions on remaining failures (see Results)

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-10)

## Results

### Audit counts
- Before: Passed 11 / Failed 28 / Skipped 7
- After: Passed 37 / Failed 3 / Skipped 6

### What --fix changed
`.github/workflows/workflow.yml`, `.gitignore`, `Directory.Build.props` (imports msbuild/repository.props; adds BannedApiAnalyzers, TimeWarp.SourceGenerators, TimeWarp.Build.Tasks package references), `Directory.Packages.props` (new pins; removed orphaned `Microsoft.SourceLink.GitHub` pin, which no project referenced), `source/Directory.Build.props` (`PackageVersion` renamed to `Version`, still 1.0.0), removed `.vscode/settings.json` from git. Added: `.editorconfig`, `.envrc`, `.githooks/`, `.timewarp/suppression-baseline.json`, `.vscode/tasks.json`, `BannedSymbols.txt`, `glossary.md`, `msbuild/repository.props`, `tools/dev-cli/` scaffold, and `.gitkeep` placeholders for `documentation/`, `skills/`, `tests/`, `kanban/archived`, `kanban/backlog`.

### Manual repairs beyond --fix
- `timewarp-build-tasks.csproj`: `<PackageReference Remove="TimeWarp.Build.Tasks" />` (the package cannot reference itself; NU1108 cycle).
- `timewarp-build-tasks.csproj`: `NoWarn RS0030` because `git-metadata-task.cs` uses `ProcessStartInfo` and the task DLL is standalone in MSBuild, so it cannot depend on TimeWarp.Amuru. (Using `Process.StartInfo` member access does not avoid RS0030; tried.)
- Renamed `build/TimeWarp.Build.Tasks.props` to `build/timewarp-build-tasks.props`; csproj packs it with `PackagePath="build/TimeWarp.Build.Tasks.props"` and `buildTransitive/TimeWarp.Build.Tasks.props`. `claude.md` references updated.
- `source/Directory.Build.props`: logo `PackagePath="/"` to `"\"` (check did not accept `/`).

### Remaining failures
- `analyzer-suppressions` (Error): `timewarp-build-tasks.csproj` RS0030 suppression is not in `.timewarp/suppression-baseline.json`. Needs an owner decision: rewrite git lookup to avoid ProcessStartInfo (e.g. read `.git` directly or use MSBuild Exec in the props), or accept the suppression into the baseline.
- `ci-code-co` / `ci-code-coverage` (Warning): no test project, so no coverage step; skipped by design until tests exist.
- `source-file-line-cap` (Error): false positive. It scans gitignored `local-nuget-cache/` (restored `timewarp.nuru.devcli` content, 609 and 704 lines), not repo source. Cache exists because `RestorePackagesPath` points inside the repo.

### Verification
- `dotnet build -c Release`: 0 warnings, 0 errors before and after.
- `dotnet pack -c Release`: `TimeWarp.Build.Tasks.1.0.0.nupkg` before and after. `unzip -l` listing (names and sizes) identical; nuspec identical; `build/` and `buildTransitive/` props byte-identical to baseline. Contains `build/TimeWarp.Build.Tasks.props`.
- Not verified: building `tools/dev-cli` runfiles/endpoints (not part of the slnx build), and a real consumer project importing the package.

## Notes

Plain `dotnet pack` defaults to Release; the DLL path in the csproj uses `$(OutputPath)`, so a Debug build then pack fails with NU5019 (pre-existing).
