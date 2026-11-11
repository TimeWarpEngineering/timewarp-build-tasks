# Review framework — task 009

**Date:** 2026-11-11
**Host task:** kanban/in-progress/009-drop-timewarpsourcegenerators-analyzer-reference-to-break-the-build-tasks-source-generators-cycle/
**Diff scope:** branch `task/009-drop-timewarpsourcegenerators-analyzer-reference-t` vs `master` (merge-base). Product commit `042649a`. Kitchen commits `8878f19`, `a3a3bd0`.
**Plan / brief:** Drop the lint-only `TimeWarp.SourceGenerators` analyzer pin and reference, remove the TW0007/TWA002 config that existed only for it, turn off `global-usings-analyzer` so `--fix` cannot restore the core-repo cycle, and bump `<Version>` to 1.0.2. Package stays `netstandard2.0`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** OpenCode `ses_e3640a20cffeHrHGCcvPZAmpIT` (Grok 4.7, 2026-11-11)

## Budget (by-diff)

- Lines changed: 73
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
