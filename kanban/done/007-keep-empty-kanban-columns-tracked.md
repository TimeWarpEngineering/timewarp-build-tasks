# Keep empty kanban columns tracked

## Description

After task 006 merged, `kanban/in-progress/` was empty, so git dropped the folder and
`ganda repo audit` failed `directory-structure` on master ("Missing 1 expected directory").
Task 006 added `.gitkeep` to `archived` and `backlog` only. The same would happen to
`to-do` or `done` if they emptied.

## Checklist

- [x] Add `.gitkeep` to `kanban/to-do/`, `kanban/in-progress/`, `kanban/done/`
- [x] `ganda repo audit` passes `directory-structure` with the column empty

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11)

## Results

Board files only, landed with `ganda kanban done --publish`. No pull request.
