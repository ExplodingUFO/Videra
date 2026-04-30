# Phase 419 Plan: Feature/Demo Surface Inventory

## Success Criteria

1. Current feature APIs, demo recipes, support summaries, docs, and tests are
   mapped by owner and risk.
2. Candidate improvements are classified as native feature, demo-only workflow,
   validation truth, or out-of-scope broadening.
3. Phase 420-423 write sets, dependencies, and validation commands are
   identified so later work can split into small beads.

## Workstream A: API Inventory

Bead: `Videra-mula`

Worktree/branch: `.worktrees/v263-phase419-api` /
`agents/v263-phase419-api`.

Write scope:

- `.planning/phases/419-feature-demo-surface-inventory/419-API-INVENTORY.md`

Read scope:

- `src/Videra.SurfaceCharts.*`
- `tests/*SurfaceCharts*`
- existing cookbook docs where API shape is documented

Validation:

- `git diff --check`

Handoff:

- list candidate Phase 420/421 beads with dependencies and validation commands.

## Workstream B: Demo/Support Inventory

Bead: `Videra-jyrv`

Worktree/branch: `.worktrees/v263-phase419-demo` /
`agents/v263-phase419-demo`.

Write scope:

- `.planning/phases/419-feature-demo-surface-inventory/419-DEMO-INVENTORY.md`

Read scope:

- `samples/Videra.SurfaceCharts.Demo/**`
- demo-related tests and docs
- support summary and consumer-smoke evidence surfaces

Validation:

- `git diff --check`

Handoff:

- list candidate Phase 422 beads with dependencies and validation commands.

## Workstream C: Validation Truth Inventory

Bead: `Videra-i8zb`

Worktree/branch: `.worktrees/v263-phase419-validation` /
`agents/v263-phase419-validation`.

Write scope:

- `.planning/phases/419-feature-demo-surface-inventory/419-VALIDATION-INVENTORY.md`

Read scope:

- `.github/workflows/**`
- `scripts/**`
- `tests/Videra.Core.Tests/Repository/**`
- `docs/ROADMAP.generated.md`

Validation:

- `git diff --check`

Handoff:

- list candidate Phase 423/424 beads with dependencies and validation commands.

## Integration

After all three inventories return, integrate their docs into `master`, close
child beads, synthesize `419-SUMMARY.md` and `419-VERIFICATION.md`, update
ROADMAP/STATE, close `Videra-dwf`, and leave the main workspace clean before
advancing to Phase 420.

