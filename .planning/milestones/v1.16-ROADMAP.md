# Roadmap

## Active Milestone

- `v1.16 SurfaceCharts Adoption Surface`
- Status: `active`
- Focus: turn `SurfaceCharts` into a clear source-first adoption surface with one first-chart story, one support truth, and one release boundary

### Phase 87: SurfaceCharts Product Boundary and First-Chart Contract

**Goal:** freeze the public `SurfaceCharts` story around one chart-local contract and one canonical `first chart` vocabulary.
**Depends on:** `v1.15` complete
**Plans:** 3 planned

Plans:

- [x] 87-01: define the canonical `first chart` story and chart-local ownership language
- [x] 87-02: align `SurfaceChartView` contract docs around `ViewState`, interaction, overlays, and rendering status
- [x] 87-03: lock root/module/sample docs to the same chart-stack boundary

### Phase 88: Source-First Evaluation Path

**Goal:** give `SurfaceCharts` a narrow source-first sample/evaluation path distinct from the broader demo.
**Depends on:** Phase 87
**Plans:** 3 planned

Plans:

- [x] 88-01: add or refine a minimal `first chart` sample / entrypoint
- [x] 88-02: make the sample reflect the chosen public contract instead of demo-only behavior
- [x] 88-03: align onboarding docs with the new source-first evaluation path

### Phase 89: SurfaceCharts Consumer Evidence and Support Contract

**Goal:** prove the chosen chart evaluation path in CI and support artifacts so issue reports carry the same runtime truth.
**Depends on:** Phase 88
**Plans:** 3 planned

Plans:

- [x] 89-01: add CI evidence for the chart adoption path
- [x] 89-02: emit diagnostics or support-ready artifacts from the chart path
- [x] 89-03: align troubleshooting / alpha-feedback guidance with the same chart evidence story

### Phase 90: Release Truth and Adoption Checklist

**Goal:** align release, support, and package-boundary docs so `SurfaceCharts` source-first truth survives milestone closeout.
**Depends on:** Phase 89
**Plans:** 3 planned

Plans:

- [x] 90-01: align README, support matrix, and release docs around source-first chart truth
- [x] 90-02: add maintainer checklist items that distinguish viewer package truth from chart adoption truth
- [x] 90-03: lock the release/support boundary with repository guards or documentation checks

## Recently Shipped

- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
- `v1.14 Compatibility and Quality Hardening` — shipped `2026-04-20`; phases `80-83`; requirements `12/12`; audit `tech_debt`; archive: `.planning/milestones/v1.14-ROADMAP.md`, `.planning/milestones/v1.14-REQUIREMENTS.md`, `.planning/milestones/v1.14-MILESTONE-AUDIT.md`
