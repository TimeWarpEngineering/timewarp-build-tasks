# Add trusted-publishing probe mode to workflow.yml

## Description

org 458-009 probe (NuGet has no policy-enumeration API; probe = dispatch mode that runs only the nuget/login OIDC exchange and stops — success proves the workflow.yml policy matches; reference timewarp-nuru's workflow.yml).

## Checklist

- [x] probe input added
- [x] login step condition extended
- [x] probe-result step added
- [x] pipeline step skipped in probe mode
- [x] YAML valid

## Results

- Added `probe` to the `workflow_dispatch.inputs.mode` choice options and updated its description.
- Extended the `NuGet login (OIDC Trusted Publishing)` step's `if:` to also run when `mode == 'probe'`.
- Added a new `Trusted publishing probe result` step gated to `mode == 'probe'`, printing a success message after the OIDC exchange.
- Gated the `Build` step to skip when `mode == 'probe'` so probe mode does no build.
- `Upload artifacts` and `Publish to NuGet` steps were already positively allow-listed to specific event/mode combos that exclude `probe`, so they naturally do not run in probe mode — left unchanged.
- Job-level `id-token: write` permission was already present — no change needed.

### How to validate

**Smoke:** `gh workflow run workflow.yml -f mode=probe` after push → expect the "Trusted publishing probe result" step to run and go green.
**Expect:** a failure of the NuGet login step means the trusted-publishing policy is missing or misconfigured on NuGet.org for this repo + workflow.yml — not a bug in this change.
