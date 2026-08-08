# Consolidate all CI/CD into a single canonical workflow.yml

## Description

Org convention (timewarp-nuru 458 program; operator ruling 2026-08-08): every
repo has exactly ONE `.github/workflows/workflow.yml` carrying ALL CI/CD
functionality — modes/params are passed in (dispatch inputs, event detection),
never expressed as separate workflow files. **timewarp-nuru is the reference
implementation** (single workflow.yml: PR/merge/release/dispatch modes with
break-glass inputs). Trusted publishing policies target `workflow.yml` only.
The later 458 conversion (reusable-workflow caller) replaces workflow.yml's
CONTENT; this task fixes the SHAPE now.

Current workflow files in this repo: build.yml, release.yml

Disposition: Fold build.yml + release.yml (release already OIDC-migrated) into ONE workflow.yml. Package is referenced repo-wide — verify carefully.

SCOPE BROADENED 2026-08-08 (operator): this task was originally rename-only; it is now the FULL single-workflow consolidation for this repo. 

## Checklist

- [x] Exactly one `.github/workflows/workflow.yml` remains, carrying all CI/CD (publish path included where the repo publishes)
- [x] `sync-configurable-files.*` deleted (abandoned org mechanism) — N/A (none present)
- [x] `*.disabled` / `*.bak` workflow cruft deleted — N/A (none present)
- [x] Assistant workflows (claude*.yml), if present: explicitly kept (not CI/CD) or folded — N/A (none present)
- [x] CI still green after consolidation (and next publish verifies nuget/login where applicable) — shape only; YAML validated; existing push/PR/release steps preserved

## Notes

Created from timewarp-nuru 458-009/458 rollout session, 2026-08-08.

## Session

- Implementation: grok (2026-08-08)

## Results

Consolidated `build.yml` + `release.yml` into a single `.github/workflows/workflow.yml`. **Shape change only** — TimeWarp.Build.Tasks is referenced by other repos' builds; push/PR/release:published behavior preserved.

**Shape (event-driven modes):**
- `push` / `pull_request` (master) / `workflow_dispatch` mode=`merge` → `dotnet build --configuration Release` + upload `artifacts/packages/*.nupkg` (former build.yml)
- `release:published` / `workflow_dispatch` mode=`release` + `confirm=release` → OIDC `nuget/login@v1`, build, `dotnet nuget push` (former release.yml; break-glass dispatch is additive for nuru-shaped control only)

**Preserved exactly:**
- `fetch-depth: 0` checkout (git metadata)
- `dotnet-version: '10.0.x'` + `dotnet-quality: 'preview'`
- OIDC: `nuget/login@v1`, `user: TimeWarp.Enterprises`, `id-token: write`
- Publish source/path: `artifacts/packages/*.nupkg` → nuget.org with `--skip-duplicate`
- Publish still gated on `release:published` (not tag push)

**Deleted:** `.github/workflows/build.yml`, `.github/workflows/release.yml`

### How to validate

**Smoke**
1. `ls .github/workflows/` → only `workflow.yml`
2. `python3 -c "import yaml; yaml.safe_load(open('.github/workflows/workflow.yml'))"`
3. Confirm steps still: `dotnet build --configuration Release`, upload path `./artifacts/packages/*.nupkg`, `nuget/login@v1`, push same path with `--skip-duplicate`
4. Confirm `on.release.types` is `[published]` only (no tag-push trigger)

**Expect**
- Single workflow file
- PR/push still produce `nuget-packages` artifact
- Release published still OIDC-publishes without API key secret
- No change to package id/versioning logic (still driven by repo props/build)

**Automated**
- YAML parse exit 0
- After push: CI builds and uploads artifact; next GitHub Release verifies nuget/login push
