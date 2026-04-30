# Phase 286: Merge and Branch Cleanup - Plan

## Goal

Merge PR `#94`, clean the milestone branch, and close Beads/Dolt handoff state.

## Tasks

1. Reconfirm PR `#94` mergeability and check status.
2. Merge the PR after checks pass.
3. Close `Videra-jnd`, export `.beads/issues.jsonl`, and preserve the Beads close state on `master`.
4. Delete the remote and local milestone feature branch.
5. Verify Git, Beads ready queue, and Dolt status are clean.

## Validation

- PR `#94` is merged into `master`.
- `master` is pushed with the final Beads export.
- `bd ready --json` returns `[]`.
- `bd sql "select * from dolt_status"` returns `(0 rows)`.
- `git status --short --branch` is clean on `master`.
