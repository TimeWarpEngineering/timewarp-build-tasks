# Review framework — task 010

**Date:** 2026-11-11
**Host task:** kanban/in-progress/010-retarget-the-task-to-net110-and-release-as-200/
**Diff scope:** branch `task/010-retarget-the-task-to-net110-and-release-as-200` vs `master` (product commit `3fed9ad`)
**Plan / brief:** Retarget `TimeWarp.Build.Tasks` to `net11.0` only and set `<Version>` to 2.0.0 for `dotnet build` on the .NET 11 SDK. No publish or tag.
**Effort:** 2 (general only; host budget roster axes)
**Reviewer roster:** general
**Session IDs:** reviewer OpenCode `ses_e356b42d2ffewQ7saGnnBAuAE5`; implementer `4981f9a9-4508-4dff-9c11-7104a1103c11`

## Budget (by-diff)

- Lines changed: 267
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
