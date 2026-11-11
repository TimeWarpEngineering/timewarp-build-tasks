# Retarget the task to net11.0 and release as 2.0.0

## Description

Drop `netstandard2.0`. `TimeWarp.Build.Tasks` targets `net11.0` only and ships as `2.0.0`.

This is a deliberate breaking change, decided by Steven on 2026-11-11: the package no
longer needs to be backward compatible and no longer needs to load in Visual Studio or
`MSBuild.exe` (.NET Framework MSBuild). It only has to work under `dotnet build` with the
.NET 11 SDK. Do not keep a `netstandard2.0` asset and do not multi-target.

State on master at filing: library TFM `netstandard2.0` (root `Directory.Build.props`),
`Microsoft.Build.Framework` and `Microsoft.Build.Utilities.Core` pinned at `17.14.28`
(latest stable `18.12.2`), `<Version>` `1.0.2` (published), self-consume pin
`TimeWarp.Build.Tasks` `1.0.0` in `Directory.Packages.props` (latest published `1.0.2`).

## Requirements

1. Target `net11.0` for the task project. Remove the `netstandard2.0` pin from root
   `Directory.Build.props` (make `net11.0` the repo default) and drop overrides that only
   existed to escape that pin (for example `.githooks/Directory.Build.props`) if they
   become redundant.
2. Package layout: the task assembly packs under `build/net11.0/` and
   `buildTransitive/net11.0/` (or whatever single folder the props file points at). Update
   `source/timewarp-build-tasks/build/timewarp-build-tasks.props` so its `UsingTask`
   assembly path matches. It must still pack as `build/TimeWarp.Build.Tasks.props` and
   `buildTransitive/TimeWarp.Build.Tasks.props`. No `netstandard2.0` folder in the nupkg.
3. Bump `Microsoft.Build.Framework` and `Microsoft.Build.Utilities.Core` to the newest
   stable version that is not newer than the MSBuild shipped in .NET SDK `11.0.100`
   (`dotnet msbuild -version`; `18.12.2` if the SDK's MSBuild is at least that). A task
   compiled against a newer MSBuild than the host fails to load, so record the SDK's
   MSBuild version and the chosen pin under Results. They stay `PrivateAssets=all` and are
   not packed.
4. Now that the TFM is modern, remove workarounds that only existed for `netstandard2.0`
   (polyfills, `LangVersion`/nullable shims, binding-redirect settings) where they are
   dead. Keep `ProcessStartInfo` and the `RS0030` suppression: TimeWarp.Amuru is still
   off-limits here because of the core-repo cycle (tasks 006 and 009).
5. Bump `<Version>` in `source/Directory.Build.props` to `2.0.0`.
6. Bump the self-consume pin `TimeWarp.Build.Tasks` in `Directory.Packages.props` from
   `1.0.0` to `1.0.2` (the newest published version; `2.0.0` does not exist yet).
7. Make the unsupported case fail clearly: when the package is imported by .NET Framework
   MSBuild (`'$(MSBuildRuntimeType)' != 'Core'`), emit one clear error naming the
   requirement (.NET SDK 11+, `dotnet build`) instead of an obscure task-load failure.
8. Docs: `readme.md` and `claude.md` state the new requirement (.NET 11 SDK, `dotnet` CLI
   builds; Visual Studio / `MSBuild.exe` not supported from 2.0.0; use 1.x for those).
   Fix badges and any `netstandard2.0` wording.
9. CI already installs SDK `11.0.x` (task 008); change the workflow only if it needs it.
10. `ganda repo audit` clean. Do not re-add `TimeWarp.SourceGenerators` (task 009).

## Goal (implementer restatement)

Ship `TimeWarp.Build.Tasks` 2.0.0 as a `net11.0`-only MSBuild task package for `dotnet build`
on the .NET 11 SDK. Concretely:

- The repo default TFM becomes `net11.0`; the `netstandard2.0` pin and the `.githooks`
  override that only existed to escape it go away.
- The nupkg carries the task DLL under `build/net11.0/` and `buildTransitive/net11.0/`, the
  props still pack as `build/` and `buildTransitive/TimeWarp.Build.Tasks.props`, and the
  props' `UsingTask` points at `net11.0/`. No `netstandard2.0` path anywhere in the package.
- `Microsoft.Build.Framework` / `Microsoft.Build.Utilities.Core` pin to the SDK's MSBuild
  (or older), still `PrivateAssets=all`, not packed. Record SDK MSBuild version + pin.
- Remove netstandard2.0-only workarounds that are now dead; keep `ProcessStartInfo` and the
  `RS0030` suppression (no Amuru here).
- `<Version>` 2.0.0; self-consume pin `TimeWarp.Build.Tasks` 1.0.2.
- Non-Core MSBuild (VS / `MSBuild.exe`) gets one clear error naming the .NET 11 SDK /
  `dotnet build` requirement instead of a task-load failure.
- `readme.md` and `claude.md` describe the new requirement and point 1.x users at 1.x.

Acceptance: `dev build` clean on SDK 11, `ganda repo audit` clean, `unzip -l` of the nupkg
shows only `net11.0` asset folders, and a throwaway `net11.0` consumer in a git repo (scratch
`NUGET_PACKAGES`, local source) builds with `CommitHash`/`CommitDate` metadata matching that
repo's HEAD. No publish/tag.

## Checklist

- [x] Task project and repo default TFM are `net11.0`; no `netstandard2.0` left outside kanban history
- [x] nupkg has `build/` and `buildTransitive/` props plus the task DLL under a `net11.0` folder only
- [x] `Microsoft.Build.*` pins bumped; SDK MSBuild version and chosen pin recorded
- [x] Dead `netstandard2.0` workarounds removed
- [x] `<Version>` is `2.0.0`; `dev check-version` reports it as new
- [x] Self-consume pin is `1.0.2`
- [x] Clear error under non-Core MSBuild
- [x] `readme.md` / `claude.md` updated
- [x] `dev build` 0 warnings / 0 errors on SDK 11; `ganda repo audit` clean
- [x] Consumer smoke passed (see Acceptance)

## Acceptance

- Pack the 2.0.0 nupkg, reference it from a throwaway `net11.0` project inside a git repo
  through a local source (use a scratch `NUGET_PACKAGES` folder so the local 2.0.0 build
  does not land in `~/.nuget/packages`), run `dotnet build`, and confirm the built
  assembly carries `CommitHash` and `CommitDate` `AssemblyMetadata` matching that repo.
  Record the commands and values under Results.
- `unzip -l` of the nupkg is recorded under Results and shows no `netstandard2.0` path.

## Review

- Round 1: Done

## Notes

- Do not publish or tag. Stop at the open pull request; release is a separate human step.
- Out of scope: the NU1108 self-reference removal in the csproj, BannedApiAnalyzers, and
  the dev-cli pins (Nuru, Amuru, Terminal).

## Results

### Changes

- Root `Directory.Build.props`: `TargetFramework` `netstandard2.0` -> `net11.0` (repo default).
  Removed `AutoGenerateBindingRedirects` (.NET Framework binding redirects) and
  `LangVersion=latest` (only needed to lift netstandard2.0's C# 7.3 default; net11.0 defaults
  to C# 15).
- Deleted `.githooks/Directory.Build.props`: its only purpose was to override the netstandard2.0
  pin back to net11.0. All five hook runfiles still build.
- `tools/dev-cli/Directory.Build.props` keeps its `net11.0` pin: it exists for the Nuru/Amuru/
  Terminal net11.0-only packages, not to escape the old pin.
- csproj: task DLL packs to `build/net11.0/` and `buildTransitive/net11.0/`. Removed
  `CopyLocalLockFileAssemblies` (only one DLL is packed, so copying MSBuild deps to `bin` was dead).
  `ProcessStartInfo` and the `RS0030` suppression stay (no Amuru, tasks 006/009).
- Props: `UsingTask` points at `net11.0/TimeWarp.Build.Tasks.dll` and is conditioned on
  `MSBuildRuntimeType == Core`. The `TimeWarpSetGitMetadata` target first emits error
  `TWBT0001` when MSBuild is not Core or runs on a runtime older than 11.0 (older SDK).
  `TimeWarpEnableGitMetadata=false` skips the target and so the error too.
- `Microsoft.Build.Framework` / `Microsoft.Build.Utilities.Core`: `17.14.28` -> `18.12.2`
  (still `PrivateAssets=all`, not packed).
- `<Version>` `1.0.2` -> `2.0.0`; self-consume pin `TimeWarp.Build.Tasks` `1.0.0` -> `1.0.2`.
- `readme.md`: new Requirements section (.NET 11 SDK, `dotnet` CLI; VS / `MSBuild.exe` not
  supported from 2.0.0; use 1.x), dotnet badge 10.0 -> 11.0. `claude.md`: TFM, package layout,
  pins, error, plus stale file/csproj names and version fixed.
- CI workflow unchanged (already installs SDK `11.0.x`, task 008).

### MSBuild version vs pin

- `dotnet --version`: `11.0.100`
- `dotnet msbuild -version`: `MSBuild version 18.12.2-1.26529.118+297acf930 for .NET` / `18.12.2.53018`
- Newest stable on nuget.org for both packages: `18.12.2`. Chosen pin: **18.12.2** (equal to the
  SDK's MSBuild, not newer).

### Gates

- `./bin/dev build --clean`: 0 warnings, 0 errors; produced `artifacts/packages/TimeWarp.Build.Tasks.2.0.0.nupkg`.
- `./bin/dev test`: completed successfully (no test projects in the repo).
- `./bin/dev check-version`: source 2.0.0, latest NuGet 1.0.2 -> "Version in source is new".
- `ganda repo audit`: passes all checks. First run had one advisory `vscode-window-icon`
  (`.vscode/settings.json` missing); `ganda repo audit --fix` created it (gitignored, nothing to commit).
- Hook runfiles (`.githooks/*.cs`) and `tools/dev-cli/dev.cs` build with no errors.
- `grep -rn netstandard` outside `kanban/`, `.git`, build output: no matches.

### nupkg listing

```
$ unzip -l artifacts/packages/TimeWarp.Build.Tasks.2.0.0.nupkg
     483  _rels/.rels
    1135  TimeWarp.Build.Tasks.nuspec
    4778  logo.png
    5015  readme.md
    2460  build/TimeWarp.Build.Tasks.props
    7680  build/net11.0/TimeWarp.Build.Tasks.dll
    2460  buildTransitive/TimeWarp.Build.Tasks.props
    7680  buildTransitive/net11.0/TimeWarp.Build.Tasks.dll
     646  [Content_Types].xml
     730  package/services/metadata/core-properties/nuget.psmdcp
```

No `netstandard2.0` path.
The nuspec has no dependency groups.

### Consumer smoke

Scratch git repo under `/tmp/twbt-smoke-*`, `net11.0` console app with
`<PackageReference Include="TimeWarp.Build.Tasks" Version="2.0.0" PrivateAssets="all" />`,
`nuget.config` = `<clear/>` + local `artifacts/packages` + nuget.org, one commit with
`--date=2026-11-11T12:00:00Z`, `NUGET_PACKAGES=<scratch>/nuget-packages dotnet build`, then the app
prints its `AssemblyMetadata`:

```
repo HEAD=d8b4d267dc41b5ce39bf4ca494be71fae6aa8372 commit-unix=1794398400 utc=2026-11-11T12:00:00
    0 Warning(s)
    0 Error(s)
CommitHash=d8b4d267dc41b5ce39bf4ca494be71fae6aa8372
CommitDate=2026-11-11T12:00:00
```

Both match the scratch repo. `~/.nuget/packages/timewarp.build.tasks` still holds only 1.0.0-1.0.2.
The first smoke run caught a bug (`VersionLessThan` given a `System.Version`, MSB4186); fixed by
computing the runtime version into a string property first, then re-smoked.

Non-Core error path: no .NET Framework MSBuild on Linux and `MSBuildRuntimeType` is reserved, so
a copy of the props with `$(MSBuildRuntimeType)` replaced by an overridable `$(_SimRuntimeType)`
was imported into a scratch project:

- `_SimRuntimeType=Full`: exactly one error, `error TWBT0001: TimeWarp.Build.Tasks 2.x requires the
  .NET 11 SDK or later and must be built with the dotnet CLI (dotnet build). Visual Studio and
  MSBuild.exe (.NET Framework MSBuild) are not supported from 2.0.0; use TimeWarp.Build.Tasks 1.x
  for those, or set TimeWarpEnableGitMetadata=false. Current MSBuild: Full runtime 11.0.0, MSBuild 18.12.2.`
- `_SimRuntimeType=Full` + `TimeWarpEnableGitMetadata=false`: 0 errors.
- Core (default): 0 errors.

Not verified on real Visual Studio / `MSBuild.exe` (no Windows host here).

### How to validate

Smoke:

```bash
./bin/dev build --clean
unzip -l artifacts/packages/TimeWarp.Build.Tasks.2.0.0.nupkg
S=$(mktemp -d); mkdir -p $S/c && cd $S/c && git init -q
# net11.0 console csproj referencing TimeWarp.Build.Tasks 2.0.0 (PrivateAssets=all),
# nuget.config with <clear/> + <repo>/artifacts/packages + nuget.org,
# Program.cs printing typeof(Program).Assembly's AssemblyMetadataAttribute Key=Value pairs
git add -A && git commit -qm smoke
NUGET_PACKAGES=$S/pkgs dotnet build && dotnet bin/Debug/net11.0/c.dll
git rev-parse HEAD; date -u -d @$(git log -1 --format=%ct) +%Y-%m-%dT%H:%M:%S
```

Expect:

- Build 0 warnings / 0 errors.
- `unzip -l` lists `build/net11.0/TimeWarp.Build.Tasks.dll`, `buildTransitive/net11.0/TimeWarp.Build.Tasks.dll`,
  `build/TimeWarp.Build.Tasks.props`, `buildTransitive/TimeWarp.Build.Tasks.props`, and no `netstandard2.0`.
- The app prints `CommitHash=<git rev-parse HEAD>` and `CommitDate=<UTC commit time>`.
- `./bin/dev check-version` reports 2.0.0 as new; `ganda repo audit` passes.
- Under Visual Studio / `MSBuild.exe`, building a consumer fails with a single `TWBT0001` error.

### Review

Round 1, effort 2, roster general (OpenCode session `ses_e356b42d2ffewQ7saGnnBAuAE5`, 2026-11-11). The implementer restatement matches the original task; no misread. No findings. Disposition: **clean**. Final counts: bug 0 open / 0 fixed / 0 wontfix, suggestion 0, nit 0. No wontfix and no escalation.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11), from Steven's instruction in that session
- Implement (task-work, 2026-11-11): retarget to net11.0, 2.0.0, TWBT0001 guard, docs; gates and consumer smoke green.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 4981f9a9-4508-4dff-9c11-7104a1103c11, max-turns 500, Done — 2026-11-11T12:28:06Z
- Reviewer: OpenCode session ses_e356b42d2ffewQ7saGnnBAuAE5 (2026-11-11), effort 2, roster general, turn cap 120. Round 1 clean, 0 findings.
- Review oracle: review by reviewer-opencode (opencode, vendor xai, model opencode-go/grok-4.7, response model grok-4.7), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md, disposition.md — 2026-11-11T12:37:23Z
