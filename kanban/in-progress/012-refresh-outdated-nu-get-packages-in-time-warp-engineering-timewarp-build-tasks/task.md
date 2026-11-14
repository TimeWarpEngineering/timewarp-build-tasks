# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-build-tasks

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-build-tasks.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
  TimeWarp.Nuru.DevCli 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 2.0.0 to 2.0.1-beta.1 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Goal (implementer restatement)

Bump TimeWarp.Nuru and TimeWarp.Nuru.DevCli from 4.0.0-beta.12 to 4.0.0-beta.13 in this repo's
Directory.Packages.props (central package management), and set the package `<Version>` in
source/Directory.Build.props from 2.0.0 to 2.0.1-beta.1 so the wave can release after merge.

Acceptance criteria:
- Directory.Packages.props pins both Nuru packages at 4.0.0-beta.13; no other package changes.
- source/Directory.Build.props `<Version>` is `2.0.1-beta.1` (hyphen prerelease suffix).
- `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` pass; the dev CLI still compiles
  against the new Nuru/DevCli packages.

## Session

- Minted by ganda task wave run 20261114T054552Z-9a0f39
- 2026-11-14 implementer: bumped TimeWarp.Nuru and TimeWarp.Nuru.DevCli to 4.0.0-beta.13 in
  Directory.Packages.props, set `<Version>2.0.1-beta.1</Version>` in source/Directory.Build.props,
  refreshed the version mention in the tools/dev-cli/Directory.Build.props comment, and
  re-ran `dev self-install` so ./bin/dev is built against the new Nuru/DevCli.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 6f91d32d-d37d-409b-a339-098c88a95d9c, max-turns 500, Done — 2026-11-14T05:49:49Z

## Results

### Changes

- `Directory.Packages.props`: TimeWarp.Nuru 4.0.0-beta.12 -> 4.0.0-beta.13; TimeWarp.Nuru.DevCli 4.0.0-beta.12 -> 4.0.0-beta.13.
- `source/Directory.Build.props`: `<Version>` 2.0.0 -> 2.0.1-beta.1.
- `tools/dev-cli/Directory.Build.props`: comment only (beta.12 -> beta.13).

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | Pass — 0 warnings, 0 errors; produced `artifacts/packages/TimeWarp.Build.Tasks.2.0.1-beta.1.nupkg` |
| Test | `./bin/dev test` | Pass — "Tests completed successfully!" |
| Audit | `ganda repo audit` | Pass (exit 0) — 2 pre-existing advisory warnings, non-blocking: `memsearch-scaffold` (outdated .githooks/*.cs) and `vscode-window-icon` (.vscode/settings.json missing); neither touched by this change |

### How to validate

Smoke:

```bash
grep -o 'TimeWarp.Nuru[.A-Za-z]*" Version="[^"]*"' Directory.Packages.props
grep '<Version>' source/Directory.Build.props
dotnet run --file tools/dev-cli/dev.cs -- self-install && ./bin/dev build && ./bin/dev test
```

Expect: both Nuru packages show `4.0.0-beta.13`; `<Version>2.0.1-beta.1</Version>`; self-install,
build, and test succeed and the build emits `TimeWarp.Build.Tasks.2.0.1-beta.1.nupkg`.
