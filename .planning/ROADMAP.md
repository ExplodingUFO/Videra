# Roadmap

## Active Milestone

- `v1.17 修`
- Status: `active`
- Focus: restore the repo green line after benchmark compile drift, SurfaceCharts warnings-as-errors failures, and the Linux `XWayland` consumer-smoke regression surfaced by the post-`v1.16` CI run

### Phase 91: Benchmark Compile Closure

**Goal:** repair the benchmark sources so repo-wide `verify` and matching-host native validation stop failing before platform-specific checks even begin.
**Depends on:** `v1.16` complete locally
**Plans:** 3 planned

Plans:

- [x] 91-01: replace invalid static-type instantiation in viewer benchmarks with legal call patterns or narrow wrappers
- [x] 91-02: prove the shared `verify` prelude no longer fails on benchmark compile drift
- [x] 91-03: align any benchmark-targeted support code/tests with the repaired viewer runtime usage

### Phase 92: SurfaceCharts Warnings-as-Errors Closure

**Goal:** clear the current SurfaceCharts analyzer failures on the repo-facing quality gate without relaxing the active rule set.
**Depends on:** Phase 91
**Plans:** 3 planned

Plans:

- [x] 92-01: remove the current `CA2007` / `S3267` / `S2325` / `S4200` failures in `Videra.SurfaceCharts.Processing` and `Videra.SurfaceCharts.Avalonia`
- [x] 92-02: keep the existing analyzer policy intact instead of suppressing or downgrading the rules
- [x] 92-03: prove both direct project builds and the `Videra.Core.Tests` warnings-as-errors evidence path pass

### Phase 93: Linux XWayland Consumer Smoke Stabilization

**Goal:** make the Linux `XWayland` consumer smoke path finish with a completed result artifact and actionable diagnostics instead of a silent early exit.
**Depends on:** Phase 92
**Plans:** 3 planned

Plans:

- [x] 93-01: isolate why the smoke app exits after `LoadModelAsync starting` on the `XWayland` path
- [x] 93-02: make the smoke artifact set always include enough result/trace context to explain failures
- [ ] 93-03: validate reference-model load, ready-backend state, and snapshot/bundle export under `XWayland`

### Phase 94: Green-Line Revalidation and Transition

**Goal:** rerun the previously red checks and record one repaired baseline for the next milestone decision.
**Depends on:** Phase 93
**Plans:** 3 planned

Plans:

- [ ] 94-01: rerun `verify`, targeted native validation, and quality-gate evidence against the repaired sources
- [ ] 94-02: rerun Linux `XWayland` consumer smoke and confirm the repaired artifact story
- [ ] 94-03: update planning/support truth from a red baseline to a repaired baseline and prepare the next closeout handoff

## Recently Shipped

- `v1.16 SurfaceCharts Adoption Surface` — completed locally `2026-04-20`; phases `87-90`; requirements `9/9`; archive snapshot: `.planning/milestones/v1.16-ROADMAP.md`, `.planning/milestones/v1.16-REQUIREMENTS.md`; audit artifact not yet captured at transition
- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
