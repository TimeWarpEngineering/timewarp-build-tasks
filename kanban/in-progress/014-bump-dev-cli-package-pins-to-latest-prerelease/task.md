# Bump dev CLI package pins to latest prerelease

## Description

`ganda nuget outdated` on 2026-11-18 reports four pins in `Directory.Packages.props`
behind. All four are dev-only: they are consumed by `tools/dev-cli` (and hook runfiles),
not by the shipped `TimeWarp.Build.Tasks` package, and none of them is packed.

| Package | Current | Target |
|---------|---------|--------|
| `TimeWarp.Nuru` | `4.0.0-beta.15` | `4.0.0-beta.16` |
| `TimeWarp.Nuru.DevCli` | `4.0.0-beta.15` | `4.0.0-beta.16` |
| `TimeWarp.Amuru` | `3.0.0-beta.5` | `3.0.0-beta.10` |
| `TimeWarp.Amuru.Tools` | `3.0.0-beta.5` | `3.0.0-beta.10` |

## Goal (implementer restatement)

Move the four dev-only pins in `Directory.Packages.props` to the newest prerelease on the
feed (Nuru and Nuru.DevCli to `4.0.0-beta.16`, Amuru and Amuru.Tools to `3.0.0-beta.10`;
`ganda nuget outdated` on 2026-11-18 confirms these are still the newest). Touch no other pin.

Done means:
- the four pins are bumped and nothing else in the props file changes;
- `./bin/dev build` is clean and the dev CLI is rebuilt (self-install) against the new pins,
  fixing any call-site breaks rather than pinning back;
- the `.githooks` runfiles still compile and run (the task commit goes through them);
- the packed `TimeWarp.Build.Tasks` nupkg has the same file list as master and no new
  nuspec dependency;
- `ganda nuget outdated` reports everything up to date and `ganda repo audit` passes;
- `<Version>` stays put unless `dev check-version` refuses.

## Requirements

1. Bump the four pins above in `Directory.Packages.props`. If a newer prerelease than the
   target has been published by the time this runs, take the newest one and record it.
2. Leave every other pin alone (`Microsoft.Build.*`, `TimeWarp.Build.Tasks`,
   `TimeWarp.Terminal`, `Microsoft.CodeAnalysis.BannedApiAnalyzers`).
3. If the new Nuru, DevCli or Amuru versions break `tools/dev-cli` or the hook runfiles
   (API or behavior change), fix the call sites in this task. Do not pin back to dodge it.
4. Refresh the dev CLI binary so `./bin/dev` is built against the new pins
   (`./bin/dev self-install` or `./tools/dev-cli/dev.cs self-install`).
5. The shipped package must not change: same nupkg file listing as master, and no new
   dependency in the nuspec.
6. `<Version>` in `source/Directory.Build.props`: these pins are not packed, so no version
   bump is needed for the change itself. Bump only if a repo gate (`dev check-version` in
   the pre-PR sequence) refuses without one, and then take the next beta. Do not release.

## Checklist

- [x] Four pins bumped in `Directory.Packages.props`
- [x] `./bin/dev build` 0 warnings / 0 errors; `./bin/dev test` passes (no test files in `tests/`; full `dev test` is CI's job)
- [x] Fresh dev CLI build restores the new Nuru, DevCli, Amuru and Amuru.Tools versions
- [x] Hook runfiles under `.githooks/` still build and run (a commit goes through)
- [x] nupkg file listing unchanged from master; nuspec has no new dependencies
- [x] `ganda nuget outdated` reports all packages up to date
- [x] `ganda repo audit` passes

## Notes

- TimeWarp packages resolve from nuget.org and the org feed
  `https://nuget.pkg.github.com/TimeWarpEngineering/index.json`.
- `ganda runfile cache --clear` forces a runfile recompile if a stale cached build hides
  the new pins.
- Do not add `TimeWarp.SourceGenerators` back (task 009).

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-18), from Steven's "put all four on one task and launch it"
- Implement (2026-11-18): pins bumped to the targets in the table; `ganda nuget outdated`
  showed no newer prerelease than beta.16 / beta.10 at run time.
- Amuru 3.0.0-beta.10 ships analyzer AMURU0001, which refused
  `Shell.Builder("dotnet")` with `run` in `tools/dev-cli/endpoints/workflow-command.cs`.
  Fixed the call site: `DotNet.Run().WithFile("tools/dev-cli/dev.cs").WithArguments("self-install")`.
  Decompiled `DotNetRunBuilder.Build()` emits `dotnet run --file tools/dev-cli/dev.cs -- self-install`,
  the same command line as before.
- `dev check-version` refused (2.0.1-beta.1 already released), so per requirement 6
  `<Version>` went to `2.0.1-beta.2`. No release.
- Boy-scout: `ganda repo audit` raised advisory `vscode-window-icon` (missing
  `.vscode/settings.json`, gitignored); `ganda repo audit --fix` created it locally. Nothing tracked changed.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 98596f1c-2bc0-4f51-bec0-6037b352e213, max-turns 500, Done — 2026-11-18T15:53:50Z

## Results

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.15 -> 4.0.0-beta.16;
  `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` 3.0.0-beta.5 -> 3.0.0-beta.10. Other pins unchanged.
- `tools/dev-cli/endpoints/workflow-command.cs`: self-install step uses `DotNet.Run()` (AMURU0001).
- `source/Directory.Build.props`: `<Version>` 2.0.1-beta.1 -> 2.0.1-beta.2 (check-version gate).

### Gates

- `ganda runfile cache --clear` then `dotnet run --file tools/dev-cli/dev.cs -- self-install`: first
  run failed with AMURU0001 (above); after the fix it succeeded. Runfile `project.assets.json`
  shows Nuru 4.0.0-beta.16, Nuru.DevCli 4.0.0-beta.16, Amuru 3.0.0-beta.10, Amuru.Tools 3.0.0-beta.10.
- `./bin/dev build`: succeeded, 0 warnings, 0 errors (rerun after the version bump: same).
- Test files: none added or edited; `tests/` holds no test files and nothing covers
  `WorkflowCommand`. Full `./bin/dev test` left to CI.
- Hook runfiles: `dotnet build .githooks/<hook>.cs` for all six hooks exit 0, restored on Amuru 3.0.0-beta.10.
- nupkg vs master: built `origin/master` (git archive into a temp dir) and diffed with this branch's
  package (both before the version bump): file listing identical (9 entries); nuspec identical
  except the `<repository>` branch/commit; no `<dependencies>` element in either.
- `./bin/dev check-version`: refused at 2.0.1-beta.1, passes at 2.0.1-beta.2.
- `ganda nuget outdated`: "All packages are up to date!"
- `ganda repo audit`: passed with 1 advisory (`vscode-window-icon`); after `--fix`, rerun passes all checks.

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev build
./bin/dev check-version
ganda nuget outdated
```

Expect: self-install succeeds with no AMURU0001 error; build reports 0 warnings / 0 errors and packs
`TimeWarp.Build.Tasks.2.0.1-beta.2.nupkg`; check-version says the version is new; nuget outdated
reports all packages up to date. `unzip -Z1 artifacts/packages/TimeWarp.Build.Tasks.2.0.1-beta.2.nupkg`
lists the same 9 entries as master and the nuspec has no `<dependencies>`.
