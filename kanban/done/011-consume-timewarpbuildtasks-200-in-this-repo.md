# Consume TimeWarp.Build.Tasks 2.0.0 in this repo

## Description

`Directory.Packages.props` pinned `TimeWarp.Build.Tasks` at `1.0.2`, the version this repo
consumes for its own dev CLI and hook runfiles. 2.0.0 (net11.0, task 010) is now published
and indexed on nuget.org, so the repo moves to it.

## Checklist

- [x] Pin `TimeWarp.Build.Tasks` `1.0.2` -> `2.0.0` in `Directory.Packages.props`
- [x] `dev build --clean` passes
- [x] Fresh `tools/dev-cli/dev.cs` build restores 2.0.0 from nuget.org and gets git metadata
- [x] `ganda nuget outdated` reports all packages up to date
- [x] `ganda repo audit` passes

## Results

- The fresh dev CLI build's `project.assets.json` lists `TimeWarp.Build.Tasks/2.0.0`, and its
  generated assembly info carries `CommitHash` and `CommitDate` for this worktree's HEAD. This
  is the first build that consumes the published 2.0.0 package rather than a local pack.
- `<Version>` stays `2.0.0`: the pin is not packed, so there is nothing new to release.

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-12)
