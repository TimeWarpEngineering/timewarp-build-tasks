# Review framework — task 012

**Date:** 2026-11-14
**Host task:** kanban/in-progress/012-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-build-tasks/
**Diff scope:** branch `task/012-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (merge-base `a80297b`). Product commit `d173ffe`. Files: `Directory.Packages.props`, `source/Directory.Build.props`, `tools/dev-cli/Directory.Build.props`.
**Plan / brief:** Bump TimeWarp.Nuru and TimeWarp.Nuru.DevCli from 4.0.0-beta.12 to 4.0.0-beta.13 in `Directory.Packages.props`, and set `<Version>` in `source/Directory.Build.props` to `2.0.1-beta.1` so the wave can release after merge.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review grok `01a1d89a-f1fe-7430-a5c0-934bc0180797`; implementer claude `6f91d32d-d37d-409b-a339-098c88a95d9c`

## Budget (by-diff)

- Lines changed: 68
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
