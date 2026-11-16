# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-build-tasks to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: no `pull_request` paths filter; `dev workflow` has a local copy of the classification rule. Work: switch to the package classifier and delete the local copy.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Goal (implementer restatement)

Make this repo meet the org one-workflow CI standard. Every pull request must report `CI`, so the
`pull_request` trigger in `workflow.yml` has no `paths` / `paths-ignore` filter (it has none today).
The decision to skip build and test on a pull request that touches only non-build paths (kanban,
docs, ...) moves into C#: `dev workflow` calls `PullRequestBuildClassifier.DecideAsync` from
`TimeWarp.Nuru.DevCli` 4.0.0-beta.15, prints its message, and exits 0 on skip. The local copy of the
rule (`WorkflowCommand.NonBuildPaths`) is deleted. Paths that are packed into the NuGet package from
outside the protected roots (`readme.md`, `assets/`) are declared as build paths in
`.timewarp/dev.jsonc` `pullRequestBuild`, not by editing the rule. The CI pull request job must call
`dev workflow` so the classifier actually runs. Push, release and dispatch paths keep their current
build/publish steps.

Acceptance:
- `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` pinned at 4.0.0-beta.15.
- `tools/dev-cli` contains `PullRequestBuildClassifier.DecideAsync(` in the workflow command and no
  local classification rule.
- `workflow.yml` `pull_request` has no paths filter; pull requests run `dev workflow`.
- Extra build roots declared through `pullRequestBuild` config.
- `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` (incl. `workflow-file` and
  `dev-workflow-classifier`) pass.

## Checklist

- [x] `ganda repo audit --fix` run and committed
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` filter kept with a why comment
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Results

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.13 -> 4.0.0-beta.15.
  Nuru beta.15 raises its own minimums, so restore failed with NU1605 downgrades until these also moved:
  `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` 3.0.0-beta.2 -> 3.0.0-beta.5, `TimeWarp.Terminal`
  2.0.0-beta.1 -> 2.0.0-beta.2, `TimeWarp.Build.Tasks` 2.0.0 -> 2.0.1-beta.1 (all on nuget.org).
- `tools/dev-cli/endpoints/workflow-command.cs`: the first step is
  `PullRequestBuildClassifier.DecideAsync(RepoRoot, ct)`; it prints `decision.Message` and returns
  (exit 0) when `!decision.RequiresBuild`. The local `NonBuildPaths` rule, the event reader, and the
  git diff helper are deleted (about 250 lines). Push, release, and a local run always build.
- `.timewarp/dev.jsonc` (new): `pullRequestBuild.buildPaths = ["readme.md", "assets/**"]`.
  `source/Directory.Build.props` packs `readme.md` and `assets/logo.png` into the package, and the
  default rule treats both as non-build.
- `.github/workflows/workflow.yml`: `pull_request` had no paths filter and still has none; added a
  header comment on why. There is no `push` filter to keep. Pull requests now run
  `dotnet run --file tools/dev-cli/dev.cs -- workflow` (self-install, clean, build, test, or the
  classifier skip); push, release, and dispatch keep `dotnet build --configuration Release` and the
  publish steps unchanged. Upload artifacts sets `if-no-files-found: warn` because a skipped pull
  request builds no packages.
- `ganda repo audit --fix` added `.githooks/prepare-commit-msg` and `.githooks/prepare-commit-msg.cs`
  (audit baseline); committed as is. Per the oracle, `--fix` ran once at the end as the single audit.
- CI run URL for this task's pull request: recorded by the host after open-pr (the PR does not exist yet
  in this node).

### Gates

| Gate | Command | Result |
|------|---------|--------|
| build | `./bin/dev build` | pass, 0 warnings, 0 errors |
| test | `./bin/dev test` | pass ("Tests completed successfully!") |
| audit | `ganda repo audit --fix` | pass, "Repository passes all audit checks." (incl. `workflow-file`, `dev-workflow-classifier`) |

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
# kanban-only diff (the land-kanban commit of this task)
echo "{\"pull_request\":{\"base\":{\"sha\":\"$(git rev-parse 05e98d8)\"},\"head\":{\"sha\":\"$(git rev-parse 14a4c94)\"}}}" > /tmp/pr.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=/tmp/pr.json ./bin/dev workflow; echo "exit=$?"
grep -rn "PullRequestBuildClassifier.DecideAsync(" tools/dev-cli
grep -n "paths" .github/workflows/workflow.yml
```

Expect:

- `Pull request changes only declared non-build paths. Skipping build and test. The rule is
  PullRequestBuildClassifier (TimeWarp.Nuru.DevCli). (1 changed paths)`, then `Workflow completed
  successfully!` and `exit=0`.
- A diff that touches `.githooks/...` or `readme.md` prints `Build path changed: <path>. Full build.`
  and starts `self-install -> clean -> build -> test` (verified locally with a synthetic
  `readme.md`-only commit).
- One grep hit in `tools/dev-cli/endpoints/workflow-command.cs`; no `paths:` / `paths-ignore:` key in
  `workflow.yml`.

### Review

- Round 1, effort 2, roster: general (grok session `01a1e4c6-597f-7cf3-b8e3-ade6a2f19753`, 2026-11-16).
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0; nit 0. Final open count: 0.
- Disposition: `clean`. No wontfix and no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- The review read the diff and the recorded gates. It did not re-run `./bin/dev build`, `./bin/dev test`, or `ganda repo audit`.

## Learnings

- Goal restatement checked against the original task (2026-11-16 review). It matches. The pull-request job calls `dev workflow` so requirement 3 runs on GitHub. This repo had no push `paths` filter to keep.

## Review

- Round 1: Done

## Notes


After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle 2026-11-16: classifier adopted, gates green (see Results).
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-16T14:32:38Z
- Review oracle: general (grok), session `01a1e4c6-597f-7cf3-b8e3-ade6a2f19753`, effort 2, turn cap 120, round 1 disposition clean — 2026-11-16
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1e4c6-597f-7cf3-b8e3-ade6a2f19753, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-16T14:43:20Z
