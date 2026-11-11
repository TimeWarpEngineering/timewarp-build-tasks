# Drop TimeWarp.SourceGenerators analyzer reference to break the Build Tasks <-> Source Generators cycle

## Description

Remove the lint-only `TimeWarp.SourceGenerators` `PackageReference` at `Directory.Build.props:33` (`PrivateAssets=all`). It was added by `ganda repo audit --fix` as the audit baseline (task 006). It is used only for analyzers, not for the shipped MSBuild task.

Source Generators already consumes `TimeWarp.Build.Tasks`. This back-reference creates a repo-level cycle on the Core repo dependency tree, so each release of one leaves the other one release behind. Build Tasks must sit at the top of the core build order, with no core-repo dependencies.

Accept losing the TWA analyzer checks (TW0007, TWA0004, TWA002, TWA0023, TWA0038, and any other rules that package currently reports). If something must replace them, it must not be a core TimeWarp package.

Steven considered and rejected splitting the analyzers into a separate package in the Source Generators repo. That package would still live in the same repo, so it would not break the cycle.

Leave this task in to-do, unassigned, until an implementer picks it up.

## Requirements

1. Remove the `TimeWarp.SourceGenerators` `PackageReference` from root `Directory.Build.props` (currently the minified ItemGroup on line 33, `PrivateAssets=all`).
2. Remove the corresponding `PackageVersion` pin from `Directory.Packages.props` (currently `TimeWarp.SourceGenerators` 1.0.0-beta.11 on the same minified line as other pins).
3. Confirm there is no `TimeWarp.SourceGenerators` reference anywhere in the repo (`PackageReference`, `PackageVersion`, `Import`, Sdk-style, or other). Mentions in this kitchen and in done-task history may stay.
4. Clean leftover analyzer configuration that exists only because that package was present:
   - `.editorconfig`: the `TW0007` (file-level usings) and `TWA002` (move XML documentation to markdown, currently `silent`) entries.
   - `.timewarp/suppression-baseline.json`: the `TWA002: 1` entry under `.editorconfig`.
   - Any other TWA/TW0* editorconfig or baseline entries that become orphaned once the package is gone.
5. Update the ganda audit so a later `ganda repo audit --fix` does not put the Source Generators reference back (task 006's audit baseline added it). The audit for this repo must be clean, or must explicitly allow this repo not to take that package. Do not replace the reference with any other core TimeWarp package.
6. Build and tests must pass on the net11 SDK. The shipped package stays on `netstandard2.0` (MSBuild task; do not retarget).
7. Bump `<Version>` in `source/Directory.Build.props` from `1.0.1` to `1.0.2` in this same task, so the change can be released as soon as it merges (1.0.1 is already published). Do not change the `TimeWarp.Build.Tasks` `PackageVersion` pin in `Directory.Packages.props`; that is the published version the repo consumes.

## Checklist

- [ ] Remove `TimeWarp.SourceGenerators` `PackageReference` from `Directory.Build.props`
- [ ] Remove `TimeWarp.SourceGenerators` `PackageVersion` from `Directory.Packages.props`
- [ ] Confirm no `TimeWarp.SourceGenerators` reference remains anywhere in the repo
- [ ] Clean leftover `.editorconfig` TW0007 / TWA002 (and any other orphaned TW/TWA) settings
- [ ] Clean leftover `TWA002` (and any other orphaned TW/TWA) entries in `.timewarp/suppression-baseline.json`
- [ ] Update the ganda audit so `--fix` will not re-add the package; audit is clean or documents the exemption
- [ ] Build and tests pass on the net11 SDK (package remains `netstandard2.0`)
- [ ] Bump `<Version>` to `1.0.2` in `source/Directory.Build.props`; `dev check-version` passes

## Acceptance

- No `TimeWarp.SourceGenerators` reference anywhere in the repo.
- Build and tests pass on the net11 SDK.
- `<Version>` is `1.0.2` and `dev check-version` reports it as new.
- The ganda audit is updated so the baseline no longer requires this package, and `--fix` will not restore the cycle.

## Notes

- The self-`TimeWarp.Build.Tasks` `PackageReference` in the same `Directory.Build.props` ItemGroup, and the `PackageReference Remove="TimeWarp.Build.Tasks"` in `source/timewarp-build-tasks/timewarp-build-tasks.csproj` (NU1108, task 006), are out of scope. Do not drop those unless they become broken by this change.
- RS0030 / BannedApiAnalyzers / the Amuru circular-dependency suppression in the csproj and suppression baseline stay. Those are not Source Generators.
- Nuru, Amuru, and Terminal pins in `Directory.Packages.props` are for `tools/dev-cli` and are also out of scope.

## Session

- Created: 3235355 (2026-11-11)
- Scope added: version bump to 1.0.2 in this task, merge automatically, release when green (Steven, 2026-11-11, session ccd4893c)
