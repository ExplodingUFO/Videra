# Phase 419 Context: Feature/Demo Surface Inventory

## Beads

- Parent: `Videra-dwf`
- API inventory: `Videra-mula`
- Demo/support inventory: `Videra-jyrv`
- Validation truth inventory: `Videra-i8zb`

## Goal

Establish the real SurfaceCharts feature/demo expansion surface before changing
APIs or demo behavior. This phase is read/inventory first and must not add
product behavior.

## Scope

- Map current native chart feature APIs, especially `VideraChartView.Plot`,
  `Plot.Add.*`, series handles, axes, styling, data shaping, live-data helpers,
  interaction state, and support evidence.
- Map current SurfaceCharts demo gallery, recipe catalog, snippets, scenario
  selection, support summary, and code ownership.
- Map cookbook docs, repository tests, CI workflows, release-readiness filters,
  generated roadmap checks, no-compat guardrails, and no-fake-evidence gates.

## Constraints

- Do not introduce compatibility adapters, old chart controls, direct public
  `Source`, hidden fallback/downshift behavior, broad workbench scope, or fake
  validation evidence.
- Do not add external-library parity claims to planning or user-facing docs.
- Keep later implementation split into small beads with explicit dependencies,
  ownership, write scope, validation commands, and handoff notes.

## Expected Outputs

- `419-API-INVENTORY.md`
- `419-DEMO-INVENTORY.md`
- `419-VALIDATION-INVENTORY.md`
- `419-SUMMARY.md`
- `419-VERIFICATION.md`

