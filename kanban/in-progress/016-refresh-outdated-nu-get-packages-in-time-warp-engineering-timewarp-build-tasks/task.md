# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-build-tasks

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-build-tasks.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the two dev-only TimeWarp pins in `Directory.Packages.props` from 4.0.0-beta.16 to
4.0.0-beta.17: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` (used only by `tools/dev-cli`).

Acceptance criteria:
- `Directory.Packages.props` pins both packages at 4.0.0-beta.17; nothing else in that file changes.
- `<Version>` in `source/Directory.Build.props` is untouched (no release).
- The dev CLI still compiles and runs on the new pins (`self-install`, then `./bin/dev build`).
- `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261126T021837Z-0cb631
- Implementer: bumped TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 in
  `Directory.Packages.props`. Boy-scout audit fixes: `dev-cli-telemetry` (error) fixed by adding
  `.UseTelemetry(options => options.ServiceName = "timewarp-build-tasks-dev")` to the builder
  chain in `tools/dev-cli/dev.cs`. `vscode-window-icon` (warning) fixed by `ganda repo audit --fix`,
  which wrote `.vscode/settings.json`. That file is gitignored, so the fix is local only.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 5ffff9e5-5d74-44f9-a5e5-0ec2ba22af92, max-turns 500, Done — 2026-11-26T02:22:56Z
- Reviewer: grok session 01a215a9-e00a-7cb0-9fdd-52274fed1838 (2026-11-26). Effort 1, roster general. Round 1. Disposition clean.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a215a9-e00a-7cb0-9fdd-52274fed1838, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-26T02:27:34Z

## Results

- `Directory.Packages.props`: TimeWarp.Nuru and TimeWarp.Nuru.DevCli pinned at 4.0.0-beta.17.
- `tools/dev-cli/dev.cs`: telemetry turned on (audit `dev-cli-telemetry`).
- `source/Directory.Build.props` `<Version>` is unchanged.
- Review: round 1, effort 1, roster general. Counts: bug 0, suggestion 0, nit 0 (open 0, fixed 0, wontfix 0). Disposition: clean. No wontfix and no escalation. Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

### Gates

- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (once after the pin bump, once after the dev.cs edit): PASS
- `./bin/dev build`: PASS (0 warnings, 0 errors)
- Test files: none. The change touches no test file, and `tests/` has no file that covers
  the dev CLI or these packages.
- `ganda repo audit`: FAIL on the first run (`dev-cli-telemetry` error, `vscode-window-icon` warning).
  After the fix, `ganda repo audit --fix`: PASS ("Repository passes all audit checks").

### How to validate

Smoke: `dotnet run --file tools/dev-cli/dev.cs -- self-install && ./bin/dev build && ./bin/dev --help`
Expect: restore picks up TimeWarp.Nuru / TimeWarp.Nuru.DevCli 4.0.0-beta.17, the build reports
"Build completed successfully!", and `--help` prints "Development CLI for timewarp-build-tasks".

## Review

- Round 1: Done
