# Review framework — task 001

**Date:** 2026-08-08
**Host task:** kanban/in-progress/001-migrate-nuget-publish-workflow-to-trusted-publishing-nugetlogin/
**Diff scope:** commit `7e50dc7` — `.github/workflows/release.yml` (+ kanban checklist/session)
**Plan / brief:** Targeted OIDC migration: job `permissions` (`contents: read`, `id-token: write`), `nuget/login@v1` (`user: TimeWarp.Enterprises`), push with `steps.nuget-login.outputs.NUGET_API_KEY`. No reusable-workflow conversion. Operator E2E release verify + secret revocation deferred (items 4–5 / nuru 458-009).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** orchestrator (grok); implementer build agent 019fe071-7d0d-7803-b879-6396da7ac9cb

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
