# Round 1 — general
**Date:** 2026-11-11
**Scope reviewed:** branch `task/010-retarget-the-task-to-net110-and-release-as-200` vs `master`, product commit `3fed9ad` plus the packed `TimeWarp.Build.Tasks.2.0.0.nupkg`

## Summary

The change drops the repo `netstandard2.0` pin, ships `TimeWarp.Build.Tasks` 2.0.0 as a `net11.0`-only task package, points `UsingTask` at `build/net11.0/`, and fails non-Core or pre-11 hosts with `TWBT0001` instead of a task-load error. `Microsoft.Build.Framework` and `Microsoft.Build.Utilities.Core` are pinned at 18.12.2 (`PrivateAssets=all`, not packed). The self-consume pin stays 1.0.2. Docs name the .NET 11 SDK / `dotnet build` requirement and point Visual Studio and `MSBuild.exe` users at 1.x.

The implementer restatement matches the original requirements, including no publish and keeping `ProcessStartInfo` / `RS0030`. No misread.

Re-checked against the tree, not only Results. SDK `11.0.100` reports MSBuild `18.12.2.53018`. `project.assets.json` resolves both Microsoft.Build packages to 18.12.2. The on-disk nupkg has no `lib/` and no dependency group; both props copies match source; both DLLs are `TargetFrameworkAttribute` `.NETCoreApp,Version=v11.0`. A scratch `net11.0` consumer against that nupkg built with 0 warnings / 0 errors and printed `CommitHash` / `CommitDate` matching its HEAD. On this SDK the guard evaluates `RuntimeType=Core`, `RuntimeVersion=11.0.0`, `VersionLessThan('11.0')=False`. A Full-runtime simulation of the same condition emitted exactly one `TWBT0001` and did not continue to the task. `git grep netstandard` outside `kanban/` is empty. CI already installs SDK `11.0.x`. `ganda repo audit` fails only `kanban-landed` because this live kitchen is not on origin-home yet; that is not a product finding. `TimeWarp.SourceGenerators` was not re-added.

## Issues

No issues.
