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

- [ ] Four pins bumped in `Directory.Packages.props`
- [ ] `./bin/dev build` 0 warnings / 0 errors; `./bin/dev test` passes
- [ ] Fresh dev CLI build restores the new Nuru, DevCli, Amuru and Amuru.Tools versions
- [ ] Hook runfiles under `.githooks/` still build and run (a commit goes through)
- [ ] nupkg file listing unchanged from master; nuspec has no new dependencies
- [ ] `ganda nuget outdated` reports all packages up to date
- [ ] `ganda repo audit` passes

## Notes

- TimeWarp packages resolve from nuget.org and the org feed
  `https://nuget.pkg.github.com/TimeWarpEngineering/index.json`.
- `ganda runfile cache --clear` forces a runfile recompile if a stale cached build hides
  the new pins.
- Do not add `TimeWarp.SourceGenerators` back (task 009).

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-18), from Steven's "put all four on one task and launch it"
