# Migrate NuGet publish workflow to trusted publishing (nuget/login)

## Description

The trusted publishing policy for this repo already exists on NuGet.org
(owner TimeWarp.Enterprises, created 2026-08-08) but is INERT until the
publish workflow exchanges an OIDC token for a temp key instead of using a
stored secret. Org program context: timewarp-nuru kanban 458-009.

Current state (2026-08-07 org audit): secret `NUGET_API_KEY` in release.yml; trigger is already release:published (correct); publishes TimeWarp.Build.Tasks — referenced repo-wide by other repos' builds, so verify carefully.

Reference implementation: timewarp-nuru `.github/workflows/workflow.yml` —
`nuget/login@v1` step (user: TimeWarp.Enterprises) gated on the release
condition, `id-token: write` job permission, push via
`--api-key ${{ steps.nuget-login.outputs.NUGET_API_KEY }}`.

NOTE: if this repo's full convention conversion (reusable-workflow caller,
timewarp-nuru 458 rollout) is imminent, do the conversion instead — it
includes this migration for free.

## Checklist

- [x] Add `id-token: write` (with `contents: read`) permissions to the publish job
- [x] Add `nuget/login@v1` gated on the publish condition
- [x] Replace the stored-secret `--api-key` with the login step output
- [ ] Verify the publish path end-to-end on the next release
- [ ] AFTER verified: operator revokes the long-lived NuGet key and deletes the GitHub secret (org-wide revocation tracked in nuru 458-009)

## Notes

Created from the timewarp-nuru 458-009 rollout session (2026-08-08).

### Implementation plan (2026-08-08)

# Implementation Plan: Migrate NuGet publish to trusted publishing

## Summary

Targeted, single-file change to `.github/workflows/release.yml`: grant OIDC permissions, exchange via `nuget/login@v1`, and push with the short-lived key. Do **not** convert to the reusable-workflow convention (not imminent). Leave secret revocation to operators after a successful release.

## Decision: targeted migration only

| Option | Verdict |
|--------|---------|
| Targeted `release.yml` OIDC migration | **Do this** |
| Full reusable-workflow / timewarp-nuru 458 convention conversion | **Out of scope** (no other kanban tasks; not imminent) |

## Current state

| Item | Status |
|------|--------|
| Trigger `release: types: [published]` | Already correct |
| Package path `artifacts/packages/*.nupkg` | Correct (`GeneratePackageOnBuild` + `PackageOutputPath` → local feed) |
| Auth | Long-lived `secrets.NUGET_API_KEY` |
| Job permissions | None declared (GitHub defaults) |
| NuGet.org trusted publishing policy | Exists (owner `TimeWarp.Enterprises`, 2026-08-08) but **inert** until OIDC exchange runs |
| Package | `TimeWarp.Build.Tasks` — org-wide build dependency |

Reference: timewarp-nuru workflow.yml — permissions + nuget/login@v1 + steps.nuget-login.outputs.NUGET_API_KEY.

## Files to change

### Only code change

**File:** `.github/workflows/release.yml`

Three surgical edits:

1. Add job-level `permissions`
2. Add `nuget/login@v1` step (after Setup .NET, before Build — fail-fast on OIDC)
3. Replace `secrets.NUGET_API_KEY` with login step output

### Exact target YAML

```yaml
name: Release

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      id-token: write  # Required for NuGet Trusted Publishing (OIDC)

    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0  # Full history for git metadata

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        dotnet-quality: 'preview'

    - name: NuGet login (OIDC Trusted Publishing)
      if: github.event_name == 'release'
      id: nuget-login
      uses: nuget/login@v1
      with:
        user: TimeWarp.Enterprises

    - name: Build
      run: dotnet build --configuration Release

    - name: Publish to NuGet
      run: |
        dotnet nuget push artifacts/packages/*.nupkg \
        --api-key "${{ steps.nuget-login.outputs.NUGET_API_KEY }}" \
        --source https://api.nuget.org/v3/index.json \
        --skip-duplicate
```

### Diff rationale

| Change | Why |
|--------|-----|
| `permissions.contents: read` | Least privilege for checkout; required companion with explicit perms |
| `permissions.id-token: write` | Allows GitHub Actions OIDC token minting for NuGet trusted publishing |
| `nuget/login@v1` + `user: TimeWarp.Enterprises` | Matches org policy owner and nuru reference |
| `id: nuget-login` | Exposes `outputs.NUGET_API_KEY` |
| `if: github.event_name == 'release'` | Matches reference; keeps gate if triggers expand later |
| Login **before** Build | OIDC/policy misconfig fails before a full build |
| Quoted api-key expression | Matches nuru; safer shell expansion |
| Keep package path, skip-duplicate, source URL | No packaging changes |
| Keep `dotnet-quality: 'preview'` | Unrelated to auth; leave as-is |

## What not to change

- build.yml (no publish)
- Directory.Build.props / project metadata
- Delete GitHub secret / revoke NuGet key (only AFTER verified release; org-wide via 458-009)
- Reusable-workflow conversion (deferred)

## Implementation steps

1. Confirm baseline still uses secrets.NUGET_API_KEY
2. Edit release.yml to target YAML
3. Optional local YAML/actionlint sanity
4. Commit workflow-only change
5. Update kanban checklist items 1–3 after code lands; leave 4–5 for operators

## Validation

### Pre-release (code review)

- Job has both contents: read and id-token: write
- No remaining secrets.NUGET_API_KEY in .github/workflows/
- nuget/login@v1 with user TimeWarp.Enterprises and id nuget-login
- Push uses steps.nuget-login.outputs.NUGET_API_KEY only
- build.yml unchanged

### Operator NuGet.org policy preflight

| Policy field | Expected |
|--------------|----------|
| NuGet owner | TimeWarp.Enterprises |
| Package | TimeWarp.Build.Tasks (or owner-level covering it) |
| Repository | TimeWarpEngineering/timewarp-build-tasks |
| Workflow | .github/workflows/release.yml (exact path) |
| Environment | None |

### End-to-end on next release (checklist item 4)

1. Publish GitHub Release
2. Confirm NuGet login + Build + Publish succeed
3. Package visible on NuGet.org
4. Spot-check consumer restore if meaningful version bump

### After verified (checklist item 5 / 458-009)

Do not delete secret until one green OIDC publish. Coordinate org-wide key revocation with nuru 458-009.

## Risks

| Risk | Mitigation |
|------|------------|
| Policy ↔ workflow path mismatch | Preflight policy table; fail-fast login |
| No secret fallback after merge | Keep secret until one green run |
| Org-wide consumers of TimeWarp.Build.Tasks | Verify on NuGet.org |
| Premature key revocation | Only under 458-009 after matrix complete |

## Acceptance criteria

- release.yml has job permissions contents: read + id-token: write
- nuget/login@v1 with TimeWarp.Enterprises, gated, id nuget-login
- Push uses only login step output (no secrets.NUGET_API_KEY in workflows)
- Next release: login + push succeed
- Post-verify operator: secret/key revocation via 458-009

## Session

- Planning: plan agent (2026-08-08)
- Implementation: build agent (2026-08-08)
- Review: general (round 1), disposition clean (2026-08-08)
- Orchestration: grok tw-orchestrate-task (2026-08-08)

## Results

### What was implemented

Migrated the Release workflow from long-lived `secrets.NUGET_API_KEY` to NuGet Trusted Publishing via OIDC (`nuget/login@v1`). The existing NuGet.org trusted publishing policy for owner `TimeWarp.Enterprises` can now exchange a short-lived API key when a GitHub Release is published.

### Files changed

| File | Change |
|------|--------|
| `.github/workflows/release.yml` | Job `permissions` (`contents: read`, `id-token: write`); `nuget/login@v1` (`user: TimeWarp.Enterprises`, `id: nuget-login`, gated on `github.event_name == 'release'`); push uses `steps.nuget-login.outputs.NUGET_API_KEY` |
| `kanban/.../task.md` + `review/` | Plan, checklist 1–3, review framework/round-1/disposition |

### Key decisions / deviations

- **Targeted migration only** — no reusable-workflow / nuru 458 convention conversion (not imminent in this repo).
- Login step placed **before** Build so OIDC/policy failures fail fast.
- Secret deletion / long-lived key revocation **not** performed here (checklist 4–5; org-wide via nuru **458-009** after a green OIDC publish).

### Test outcomes

- Local static verification: no `secrets.NUGET_API_KEY` under `.github/`; `build.yml` unchanged (no publish).
- OIDC exchange and NuGet push cannot be exercised offline — require a real `release: published` run.

### Phase 4b review

| Field | Value |
|-------|--------|
| Effort / roster | 1 — general only |
| Rounds | 1 |
| Final counts | bug/suggestion/nit: 0 open, 0 fixed, 0 wontfix |
| Disposition | **clean** |
| Paths | `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md` |

### Residual (operator / org)

Checklist items remain open by design:

1. **Verify** publish path end-to-end on the next GitHub Release (login + push + package on NuGet.org).
2. **After verified**, revoke long-lived NuGet API key and delete GitHub secret `NUGET_API_KEY` — coordinate with nuru **458-009** (shared key may still be used by unmigrated repos).

### How to validate

**Smoke (static — code migration)**

```bash
# From repo root
test -f .github/workflows/release.yml
rg -n "id-token: write|nuget/login@v1|TimeWarp.Enterprises|steps.nuget-login.outputs.NUGET_API_KEY" .github/workflows/release.yml
rg -n "secrets.NUGET_API_KEY" .github/ || true
```

**Expect**

- `release.yml` contains job `permissions` with both `contents: read` and `id-token: write`.
- `nuget/login@v1` with `user: TimeWarp.Enterprises` and `id: nuget-login`.
- Publish step uses only `"${{ steps.nuget-login.outputs.NUGET_API_KEY }}"`.
- `rg secrets.NUGET_API_KEY .github/` finds **no** matches.

**Automated gate**

```bash
# No workflow unit tests in this repo; static check is the automated bar for the code change:
rg -n "secrets.NUGET_API_KEY" .github/; test $? -eq 1
# optional if actionlint is installed:
# actionlint .github/workflows/release.yml
```

**Operator E2E (next release — residual)**

1. Confirm NuGet.org trusted publishing policy: owner `TimeWarp.Enterprises`, repo `TimeWarpEngineering/timewarp-build-tasks`, workflow path `.github/workflows/release.yml`, no Environment.
2. Merge this change to `master`, then publish a GitHub Release.
3. Open the Release workflow run: **NuGet login** green, **Build** produces `artifacts/packages/*.nupkg`, **Publish to NuGet** green (or skip-duplicate if version already exists).
4. Confirm package version on https://www.nuget.org/packages/TimeWarp.Build.Tasks/

**Not in scope of this task’s code landing**

- Live OIDC publish success (needs GitHub Release + NuGet.org policy match).
- Deleting `NUGET_API_KEY` / revoking the long-lived NuGet key (after green run; nuru 458-009).
- Reusable-workflow convention conversion.
