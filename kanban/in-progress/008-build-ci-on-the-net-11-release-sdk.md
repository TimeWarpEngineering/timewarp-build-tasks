# Build CI on the .NET 11 release SDK

## Description

`.github/workflows/workflow.yml` installed `dotnet-version: '10.0.x'` with
`dotnet-quality: 'preview'`. That was written on 2025-10-13, when .NET 10 was still in
preview. With `preview` quality the action picks the newest preview build of the channel,
not the GA release, so CI (and the release publish) kept building on a 10.0 release
candidate (PR 7 recorded `10.0.100-rc.2.25502.107`).

Install `11.0.x` at release quality instead. The package stays on `netstandard2.0`.

## Checklist

- [x] Set `dotnet-version: '11.0.x'` and remove `dotnet-quality`
- [x] `ganda repo audit` passes
- [ ] CI green on the pull request, with the installed SDK version recorded under Results

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11)

## Notes

Local SDK is `11.0.100`. No `global.json` in this repo; pinning through `global.json`
(as timewarp-state and timewarp-amuru do) was not adopted here.
