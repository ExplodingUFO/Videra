# Roadmap

## Active Milestone

- `v1.19 SurfaceCharts Presentation Space and Interaction Defaults`
- Status: `initialized locally on 2026-04-20`
- Focus: separate display-space from raw analytics truth, make the value axis legible by default, and formalize reversed X/Y interaction semantics as the new presentation preset

### Phase 99: Display Space Contract and Camera Persistence

**Goal:** add a first-class display-space transform so wide-strip datasets can present with more professional proportions without corrupting raw metadata, scalar semantics, or probe truth.
**Depends on:** `v1.18` analytics-core contracts merged on `master`
**Plans:** 3 planned

Plans:

- [ ] 99-01: define a public display-space contract and persist it inside `SurfaceViewState` instead of baking aspect/exaggeration into raw camera fit math
- [ ] 99-02: thread display-space resolution through camera defaults, projection, render inputs, and render-state invalidation so display transforms actually change the plotted surface shape
- [ ] 99-03: keep `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` display-aware so consumers do not lose presentation intent during normal chart commands

Success criteria:

1. Consumers can select a presentation-oriented display transform without changing source metadata, scalar-field values, or probe readouts.
2. The chart no longer defaults to a visibly stretched “long strip” shape for the demo dataset when the presentation preset is active.
3. Camera/view commands preserve the selected display transform instead of silently snapping back to raw-axis proportions.

### Phase 100: Overlay Axis Semantics and Default Presentation

**Goal:** make the chart explain itself better by default through value-axis-emphasizing overlay behavior and clearer semantic axis formatting.
**Depends on:** Phase 99
**Plans:** 3 planned

Plans:

- [ ] 100-01: extend overlay formatting to understand semantic axis roles (`Horizontal`, `Value`, `Depth`, `Legend`) instead of only raw `"X"` / `"Y"` / `"Z"` tokens
- [ ] 100-02: introduce a chart-local auto grid-plane policy that can prefer value-emphasizing or camera-aware placement instead of always falling back to the `XZ` floor plane
- [ ] 100-03: promote overlay defaults that make the value axis and axis-side choice legible on the first rendered chart

Success criteria:

1. Value-axis labeling is visually clearer in the demo and no longer feels “missing” because the overlay is anchored to an unhelpful default floor plane.
2. Consumers can format labels by semantic chart role without reverse-engineering math-space axis letters.
3. Overlay defaults remain chart-local and do not leak chart semantics back into `VideraView`.

### Phase 101: Interaction Presets and Reversed Default Semantics

**Goal:** turn the current hard-coded orbit/pan sign conventions into a public interaction contract and make the user-preferred reversed X/Y behavior the default preset.
**Depends on:** Phase 100
**Plans:** 3 planned

Plans:

- [ ] 101-01: add public interaction-options or interaction-preset contracts for orbit and pan inversion instead of leaving sign choices hard-coded in the controller
- [ ] 101-02: apply those options across built-in orbit and pan paths while preserving a legacy preset for callers who want the previous behavior
- [ ] 101-03: align interaction status/demo text so the first-chart story explains the new defaults truthfully

Success criteria:

1. Orbit and pan direction are configurable through public chart-local API instead of hidden controller math.
2. The new default preset matches the currently preferred reversed X/Y behavior for both orbit and pan.
3. Existing callers still have an explicit legacy path instead of being forced to absorb a silent behavioral break with no opt-out.

### Phase 102: Demo Proof and Source-First Truth Alignment

**Goal:** prove the new display and interaction defaults in the canonical source-first story so demo, README, and support truth all describe the same chart behavior.
**Depends on:** Phase 101
**Plans:** 3 planned

Plans:

- [ ] 102-01: refresh `SurfaceCharts.Demo` defaults, status text, and support-summary content so the first chart opens in the intended presentation and interaction mode
- [ ] 102-02: update README and consumer-facing guidance to document the new display/overlay/interaction contracts without over-claiming deferred analytics features
- [ ] 102-03: add focused tests or repository-truth coverage that lock the new demo/default semantics in place

Success criteria:

1. The canonical demo visibly demonstrates the new presentation-oriented chart defaults without manual setup steps.
2. Docs and support summaries match the shipped behavior of the demo and public contracts.
3. Regression coverage exists for the new defaults so future analytics work does not quietly revert the presentation story.

## Operational Baseline

- `v1.18` analytics-core deepening is complete and archived; the next milestone builds on generalized geometry/scalar contracts rather than replacing them
- the highest-value current gap is presentation clarity and interaction semantics, not additional backend coverage or a wider generic chart product surface

## Recently Shipped

- `v1.18 SurfaceCharts Analytics Core` — shipped `2026-04-20`; phases `95-98`; requirements `11/11`; audit `tech_debt`; archive: `.planning/milestones/v1.18-ROADMAP.md`, `.planning/milestones/v1.18-REQUIREMENTS.md`, `.planning/milestones/v1.18-MILESTONE-AUDIT.md`
- `v1.16 SurfaceCharts Adoption Surface` — completed locally `2026-04-20`; phases `87-90`; requirements `9/9`; archive snapshot: `.planning/milestones/v1.16-ROADMAP.md`, `.planning/milestones/v1.16-REQUIREMENTS.md`; audit artifact not yet captured at transition
- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
