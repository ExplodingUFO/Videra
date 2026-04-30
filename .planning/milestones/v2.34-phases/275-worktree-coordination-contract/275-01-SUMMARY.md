# Phase 275 Summary: Worktree Coordination Contract

## Completed

- Added worktree redirect guidance to `docs/beads-coordination.md`.
- Documented the expected `bd worktree list` shape and worktree `bd context --json` fields.
- Clarified that Beads issue truth is shared while Git branches, file ownership, conflict resolution, and release decisions remain separate.

## Verification

- `bd worktree list` showed main as `shared` and existing phase worktrees as `redirect -> Videra`.
- `bd context --json` from `.worktrees/phase231` reported `is_redirected: true`, `is_worktree: true`, database `Videra`, and the expected project id.

