# Review framework — task 013

**Date:** 2026-11-16
**Host task:** kanban/in-progress/013-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/013-adopt-the-shared-pull-request-build-classifier-and` vs `master` (merge-base `facb4ba`, ahead 3). Product commit `80a7304`. Kanban commits `14a4c94`, `3acb682`. 573 lines changed (270 insertions, 303 deletions).
**Plan / brief:** Adopt `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15, delete the local rule, declare packed paths in `.timewarp/dev.jsonc`, and keep `workflow.yml` `pull_request` unfiltered so every pull request reports CI.
**Effort:** 2 (by diff, 573 lines). Roster axis is general only — no specialist reviewers.
**Reviewer roster:** general
**Session IDs:** review oracle grok `01a1e4c6-597f-7cf3-b8e3-ade6a2f19753` (2026-11-16). Implementer session not reported (see task.md ## Session).

## Budget (by-diff)

- Lines changed: 573
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
