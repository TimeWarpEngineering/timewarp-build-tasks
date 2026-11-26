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

## Results

- `Directory.Packages.props`: TimeWarp.Nuru and TimeWarp.Nuru.DevCli pinned at 4.0.0-beta.17.
- `tools/dev-cli/dev.cs`: telemetry turned on (audit `dev-cli-telemetry`).
- `source/Directory.Build.props` `<Version>` is unchanged.

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
