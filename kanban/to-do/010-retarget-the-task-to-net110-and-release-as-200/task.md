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

## Checklist

- [ ] Task project and repo default TFM are `net11.0`; no `netstandard2.0` left outside kanban history
- [ ] nupkg has `build/` and `buildTransitive/` props plus the task DLL under a `net11.0` folder only
- [ ] `Microsoft.Build.*` pins bumped; SDK MSBuild version and chosen pin recorded
- [ ] Dead `netstandard2.0` workarounds removed
- [ ] `<Version>` is `2.0.0`; `dev check-version` reports it as new
- [ ] Self-consume pin is `1.0.2`
- [ ] Clear error under non-Core MSBuild
- [ ] `readme.md` / `claude.md` updated
- [ ] `dev build` 0 warnings / 0 errors on SDK 11; `ganda repo audit` clean
- [ ] Consumer smoke passed (see Acceptance)

## Acceptance

- Pack the 2.0.0 nupkg, reference it from a throwaway `net11.0` project inside a git repo
  through a local source (use a scratch `NUGET_PACKAGES` folder so the local 2.0.0 build
  does not land in `~/.nuget/packages`), run `dotnet build`, and confirm the built
  assembly carries `CommitHash` and `CommitDate` `AssemblyMetadata` matching that repo.
  Record the commands and values under Results.
- `unzip -l` of the nupkg is recorded under Results and shows no `netstandard2.0` path.

## Notes

- Do not publish or tag. Stop at the open pull request; release is a separate human step.
- Out of scope: the NU1108 self-reference removal in the csproj, BannedApiAnalyzers, and
  the dev-cli pins (Nuru, Amuru, Terminal).

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11), from Steven's instruction in that session
