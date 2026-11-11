# Smoke TimeWarp.Build.Tasks under .NET 11 SDK

## Description

Verify `TimeWarp.Build.Tasks` still packs and loads under the .NET 11 SDK. This package stays on `netstandard2.0` (MSBuild task / DevelopmentDependency) — do **not** retarget to `net11.0`. Scope is a smoke gate for the ecosystem .NET 11 migration order (Build.Tasks → Builder → Terminal → Amuru).

As of 2026-10-03: library TFM is `netstandard2.0`; only package deps are `Microsoft.Build.Framework` and `Microsoft.Build.Utilities.Core` (both `17.15.0-preview-25277-114`, PrivateAssets). No TimeWarp package dependencies. No `global.json` in-repo today.

## Requirements

- Keep product TFM `netstandard2.0`.
- Smoke build + pack under .NET 11 SDK (RC/GA as available on the agent).
- Confirm the packed task DLL still loads and injects `CommitHash` / `CommitDate` AssemblyMetadata on a consumer project built with SDK 11.
- Bump `Microsoft.Build.Framework` / `Microsoft.Build.Utilities.Core` **only if** SDK 11 requires newer pins for the task to load; otherwise leave as-is.
- Update README/badge wording only if it hard-asserts .NET 10 as a hard requirement for consumers (marketing badge may stay informational).
- Product implementation via this task worktree; publish kanban kitchen separately from product commits as usual.

## Checklist

- [ ] Install / use .NET 11 SDK on the build host; record `dotnet --version`.
- [ ] `dotnet build` + `dotnet pack` on `timewarp-build-tasks.slnx` under SDK 11 (warnings-as-errors clean).
- [ ] Smoke: reference the local nupkg from a throwaway `net10.0` or `net11.0` project; confirm `CommitHash` / `CommitDate` metadata after build.
- [ ] Bump `Microsoft.Build.*` CPM pins only if load/pack fails under SDK 11; document why.
- [ ] Spot-check README / docs for misleading "must use net10" language; leave `netstandard2.0` story clear.
- [ ] CI (`.github/workflows/workflow.yml`): confirm green (or note if runner lacks SDK 11 yet).

## Notes

### Why this is nearly a no-op

- MSBuild tasks run in the SDK/MSBuild host, not as a `net11.0` library consumer.
- `IncludeBuildOutput=false` + `DevelopmentDependency=true` — no runtime TFM migration.
- Builder and siblings already reference `TimeWarp.Build.Tasks` as `PrivateAssets=all`.

### Ordered work

1. Toolchain smoke under SDK 11.
2. Package pin bump only on failure.
3. Docs/badge only if misleading.

### Session

- Created: 1154329 (2026-10-03 Asia/Bangkok)
- Kitchen authored on TWE-001 for Grok Bot / cloud agent pickup
