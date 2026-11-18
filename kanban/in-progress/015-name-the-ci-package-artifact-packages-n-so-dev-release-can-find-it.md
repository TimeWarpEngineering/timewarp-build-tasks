# Name the CI package artifact Packages-N so dev release can find it

## Description

`dev release` for 2.0.1-beta.2 refused on 2026-11-18:

> no successful CI run of workflow.yml with a Packages-* artifact exists for e298abd

Task 014 moved the dev CLI to TimeWarp.Nuru.DevCli 4.0.0-beta.16, whose release guard
requires a `Packages-*` artifact on the green master run. This repo's workflow uploaded the
package as `nuget-packages`. Other org repos (timewarp-amuru, timewarp-nuru) name it
`Packages-${{ github.run_number }}`.

## Checklist

- [x] Upload step names the artifact `Packages-${{ github.run_number }}`
- [x] `ganda repo audit` passes
- [ ] Master push run after merge carries a `Packages-*` artifact and `dev release --dry-run` passes

## Notes

This repo's release event still rebuilds the package rather than promoting the CI artifact.
Only the artifact name changes here.

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-18), blocker found while releasing 2.0.1-beta.2 for Steven
