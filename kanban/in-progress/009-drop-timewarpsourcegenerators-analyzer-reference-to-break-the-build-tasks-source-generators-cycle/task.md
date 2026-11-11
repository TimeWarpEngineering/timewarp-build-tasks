# Drop TimeWarp.SourceGenerators analyzer reference to break the Build Tasks <-> Source Generators cycle

## Description

Remove the lint-only `TimeWarp.SourceGenerators` `PackageReference` at `Directory.Build.props:33` (`PrivateAssets=all`). It was added by `ganda repo audit --fix` as the audit baseline (task 006). It is used only for analyzers, not for the shipped MSBuild task.

Source Generators already consumes `TimeWarp.Build.Tasks`. This back-reference creates a repo-level cycle on the Core repo dependency tree, so each release of one leaves the other one release behind. Build Tasks must sit at the top of the core build order, with no core-repo dependencies.

Accept losing the TWA analyzer checks (TW0007, TWA0004, TWA002, TWA0023, TWA0038, and any other rules that package currently reports). If something must replace them, it must not be a core TimeWarp package.

Steven considered and rejected splitting the analyzers into a separate package in the Source Generators repo. That package would still live in the same repo, so it would not break the cycle.

Leave this task in to-do, unassigned, until an implementer picks it up.

## Goal (implementer restatement)

Break the Build Tasks <-> Source Generators repo cycle by removing the lint-only
`TimeWarp.SourceGenerators` analyzer package from this repo, so Build Tasks has no
core-repo dependency. Done means:

- No `TimeWarp.SourceGenerators` `PackageReference`/`PackageVersion` (or other reference) remains.
- The `.editorconfig` TW0007/TWA002 entries and the `TWA002` baseline entry that only existed for that package are gone.
- `ganda repo audit` passes, and its `global-usings-analyzer` check (which pins the package) is turned off with a reason, so `--fix` cannot re-add it.
- Build, tests, and samples pass on the net11 SDK; the package stays `netstandard2.0`.
- `<Version>` is `1.0.2` and `dev check-version` reports it as new.

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

- [x] Remove `TimeWarp.SourceGenerators` `PackageReference` from `Directory.Build.props`
- [x] Remove `TimeWarp.SourceGenerators` `PackageVersion` from `Directory.Packages.props`
- [x] Confirm no `TimeWarp.SourceGenerators` reference remains anywhere in the repo
- [x] Clean leftover `.editorconfig` TW0007 / TWA002 (and any other orphaned TW/TWA) settings
- [x] Clean leftover `TWA002` (and any other orphaned TW/TWA) entries in `.timewarp/suppression-baseline.json`
- [x] Update the ganda audit so `--fix` will not re-add the package; audit is clean or documents the exemption
- [x] Build and tests pass on the net11 SDK (package remains `netstandard2.0`)
- [x] Bump `<Version>` to `1.0.2` in `source/Directory.Build.props`; `dev check-version` passes

## Acceptance

- No `TimeWarp.SourceGenerators` reference anywhere in the repo.
- Build and tests pass on the net11 SDK.
- `<Version>` is `1.0.2` and `dev check-version` reports it as new.
- The ganda audit is updated so the baseline no longer requires this package, and `--fix` will not restore the cycle.

## Notes

- The self-`TimeWarp.Build.Tasks` `PackageReference` in the same `Directory.Build.props` ItemGroup, and the `PackageReference Remove="TimeWarp.Build.Tasks"` in `source/timewarp-build-tasks/timewarp-build-tasks.csproj` (NU1108, task 006), are out of scope. Do not drop those unless they become broken by this change.
- RS0030 / BannedApiAnalyzers / the Amuru circular-dependency suppression in the csproj and suppression baseline stay. Those are not Source Generators.
- Nuru, Amuru, and Terminal pins in `Directory.Packages.props` are for `tools/dev-cli` and are also out of scope.

## Results

- Removed the `TimeWarp.SourceGenerators` `PackageReference` (root `Directory.Build.props`) and `PackageVersion` pin (`Directory.Packages.props`).
- Removed `.editorconfig` `TW0007.filename` and `TWA002.severity` entries, and `TWA002: 1` from `.timewarp/suppression-baseline.json`. No other TW/TWA entries existed. The `globalusingsanalyzer*` entries were already there and are not from Source Generators, so they stay.
- The ganda audit check that adds the package is `global-usings-analyzer`; it now fails without the pin/reference/TW0007 entry. Set `global-usings-analyzer.severity = off` under `[ganda.audit]` in `.editorconfig` with the cycle reason (task 009). `ganda repo audit --fix` will no longer restore it.
- Boy-scout: `vscode-window-icon` (warning) was failing because `.vscode/settings.json` was missing; `ganda repo audit --fix --checks vscode-window-icon` created it (the file is gitignored, so there is no tracked change).
- Bumped `<Version>` in `source/Directory.Build.props` 1.0.1 -> 1.0.2. The `TimeWarp.Build.Tasks` 1.0.0 consume pin is unchanged.
- Gates on .NET SDK 11.0.100: `./bin/dev build` (0 warnings, 0 errors, packs `TimeWarp.Build.Tasks.1.0.2.nupkg`, netstandard2.0), `./bin/dev test` passes, `./bin/dev verify-samples` passes, `./bin/dev check-version` reports 1.0.2 new (NuGet latest 1.0.1), `ganda repo audit` reports "Repository passes all audit checks". `project.assets.json` has no SourceGenerators entry.
- `git grep -i sourcegenerators -- ':!kanban'` finds only the `.editorconfig` comment that explains the exemption.

### How to validate

Smoke:

```bash
git grep -n -i sourcegenerators -- ':!kanban'
./bin/dev build && ./bin/dev test && ./bin/dev check-version
ganda repo audit
```

Expect:

- The grep finds only the `.editorconfig` `[ganda.audit]` comment line. No `PackageReference` or `PackageVersion` is found.
- Build succeeds with 0 warnings and produces `artifacts/packages/TimeWarp.Build.Tasks.1.0.2.nupkg`. Tests pass.
- check-version prints `Version in source is new — safe to release.` for 1.0.2.
- The audit prints `Repository passes all audit checks.` and does not run `global-usings-analyzer`.

## Session

- Created: 3235355 (2026-11-11)
- Scope added: version bump to 1.0.2 in this task, merge automatically, release when green (Steven, 2026-11-11, session ccd4893c)
- Implemented 2026-11-11 (implement oracle): removed package, editorconfig/baseline cleanup, audit exemption, version 1.0.2
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session c831b817-a3c3-4240-9fd7-3ae112d4b7b4, max-turns 500, Done — 2026-11-11T08:35:03Z
