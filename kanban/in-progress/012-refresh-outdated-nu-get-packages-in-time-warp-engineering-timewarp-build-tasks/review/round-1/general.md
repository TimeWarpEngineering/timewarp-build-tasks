# Round 1 — general

**Date:** 2026-11-14
**Scope reviewed:** branch `task/012-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (merge-base `a80297b`), product commit `d173ffe`

## Summary

The change pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.13` in central package management and sets the packed `<Version>` to `2.0.1-beta.1`. The dev CLI references both packages without inline versions, so the pins are the only version source. The self-consume pin `TimeWarp.Build.Tasks` stays at the published `2.0.0`. No other package versions changed, and the diff does not touch tests, analyzers, or audit config.

The implementer restatement matches the original task. Recorded gates cover the change: `./bin/dev build` (0 warnings, 0 errors, `TimeWarp.Build.Tasks.2.0.1-beta.1.nupkg`), `./bin/dev test` (pass), and `ganda repo audit` (exit 0, two named pre-existing advisories on files this diff does not touch). This review did not re-run those gates.

`claude.md` still says the package version is `2.0.0` defined in `source/Directory.Build.props`, and its install example still uses `--version 2.0.0`. That is left as-is: the task did not ask to edit `claude.md`, and the published package consumers install remains `2.0.0` until the wave releases. Local `master` is two kanban-land commits ahead (`4e7ba8d`, `ae5fa22`); those commits do not change product files.

## Issues

No issues.
