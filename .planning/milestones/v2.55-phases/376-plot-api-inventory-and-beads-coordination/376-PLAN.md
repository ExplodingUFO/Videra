# Phase 376 Plan: Plot API Inventory and Beads Coordination

## Goal

Prepare v2.55 for autonomous execution by grounding the phase split in real code, creating dependency-aware beads, and defining safe parallel worktree boundaries.

## Tasks

1. Verify current planning state.
   - Check `.planning/PROJECT.md`, `.planning/STATE.md`, `.planning/ROADMAP.md`, and `.planning/REQUIREMENTS.md`.
   - Confirm v2.55 is active and v2.54 is archived.

2. Inventory code boundaries.
   - Read-only inspect Plot API, axis/snapshot surfaces, streaming/demo/docs/test surfaces.
   - Capture file owners and existing semantics in `376-CONTEXT.md`.

3. Define requirements and roadmap.
   - Replace stale v2.54 requirements with v2.55 requirements.
   - Replace the "Ready for next milestone" roadmap with phases 376-382.
   - Update state to show Phase 376 ready.

4. Create beads.
   - Create epic `Videra-v255`.
   - Create phase beads `Videra-v255.1` through `Videra-v255.7`.
   - Add parent-child and blocking dependencies.

5. Define handoff boundaries.
   - Mark Phases 377-379 as parallelizable after Phase 376.
   - Record branch/worktree names, write boundaries, and validation commands.

## Verification

- `gsd-sdk query roadmap.get-phase 376`
- `bd ready --json`
- `bd dep tree Videra-v255`
- `git diff --check -- .planning/REQUIREMENTS.md .planning/ROADMAP.md .planning/STATE.md .planning/phases/376-plot-api-inventory-and-beads-coordination`

## Handoff

After Phase 376 closes, claim `Videra-v255.2`, `Videra-v255.3`, and `Videra-v255.4` in separate worktrees if the main branch is clean enough to branch from. Do not start Phase 380 until Phase 377 has merged.
