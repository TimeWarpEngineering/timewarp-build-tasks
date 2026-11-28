# Review framework — task 016

**Date:** 2026-11-26
**Host task:** kanban/in-progress/016-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-build-tasks/
**Diff scope:** branch `task/016-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `41967eda293aabd1cc5209b5689edeccb22f69fd`. Product commit `8981060`. Files: `Directory.Packages.props`, `tools/dev-cli/dev.cs`. Kitchen `task.md` is not product scope. Local `master` is two land commits ahead; those commits add only the minted task card.
**Plan / brief:** Bump the two dev-only pins in `Directory.Packages.props` (`TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli`, 4.0.0-beta.16 to 4.0.0-beta.17). Do not change `<Version>` in `source/Directory.Build.props`. Edit package pins in this repo only.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer claude `5ffff9e5-5d74-44f9-a5e5-0ec2ba22af92`; reviewer grok `01a215a9-e00a-7cb0-9fdd-52274fed1838`

## Budget (by-diff)

- Lines changed: 56
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
