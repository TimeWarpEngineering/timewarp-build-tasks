# Review framework — task 014

**Date:** 2026-11-18
**Host task:** kanban/in-progress/014-bump-dev-cli-package-pins-to-latest-prerelease/
**Diff scope:** branch `task/014-bump-dev-cli-package-pins-to-latest-prerelease` (`7602ae2`) vs merge-base `1db819c` (master). Product commit `4f82e63`. Files: `Directory.Packages.props`, `source/Directory.Build.props`, `tools/dev-cli/endpoints/workflow-command.cs`.
**Plan / brief:** Bump the four dev-only pins (Nuru and Nuru.DevCli to `4.0.0-beta.16`, Amuru and Amuru.Tools to `3.0.0-beta.10`), fix call sites the new Amuru analyzer breaks, and bump `<Version>` only if `dev check-version` refuses.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer claude `98596f1c-2bc0-4f51-bec0-6037b352e213`; reviewer grok `01a1ef5d-67be-78a1-b082-4c373fa8e11f`

## Budget (by-diff)

- Lines changed: 180
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
