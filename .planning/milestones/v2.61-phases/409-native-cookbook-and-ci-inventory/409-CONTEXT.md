---
phase: 409
title: "Native Cookbook and CI Inventory Context"
bead: Videra-63e
status: complete
created_at: 2026-04-30
---

# Phase 409 Context

Phase 409 inventories the real v2.61 surfaces before implementation. The goal
is to move toward ScottPlot5-style cookbook usability while staying native to
Videra's 3D chart model and preserving truthful CI/performance evidence.

## Inputs

- `409A-COOKBOOK-DEMO-INVENTORY.md` maps root/demo/cutover cookbook surfaces,
  visible demo recipe selector, snippet sources, support summary, and detailed
  runnable cookbook gaps.
- `409B-NATIVE-PERFORMANCE-INVENTORY.md` maps native chart ownership APIs,
  performance-sensitive data paths, Videra-native translations of ScottPlot5
  ergonomics, and performance truth risks.
- `409C-CI-VALIDATION-INVENTORY.md` maps CI/test gates, generated roadmap and
  Beads checks, snapshot scope guardrails, anti-fake rules, and final sync
  expectations.

## Key Findings

- Videra already has the native cookbook spine: `VideraChartView`,
  `Plot.Add.*`, typed 3D series handles, `Plot.Axes`, chart-local interactions,
  `DataLogger3D`, linked views, and PNG-only chart snapshots.
- Current cookbook coverage is broad but not yet detailed enough as runnable
  recipes. Gaps remain for first chart, surface/cache-backed paths, waterfall,
  scatter/live data, Bar, Contour, axes, styling, interaction, linked views,
  support evidence, and PNG snapshots.
- Performance-sensitive examples should use existing native data paths such as
  `SurfaceMatrix` -> `SurfacePyramidBuilder` -> `Plot.Add.Surface`, columnar
  scatter, and `DataLogger3D` FIFO evidence.
- CI is strongest when focused tests run first, then broader gates are composed
  near final closeout. Fresh worktrees may require restore before `--no-restore`
  gates, and demo build/tests should run sequentially to avoid artifact
  contention.
- Anti-fake evidence is a hard boundary: no fake benchmark claims, no synthetic
  support data as runtime truth, no hidden fallback/downshift, no unsupported
  backend promises, and no hand-edited generated roadmap.

## Selected Next Slices

Phase 410 should implement detailed cookbook/demo recipes:

- runnable first-chart and surface/cache-backed recipes
- waterfall, linked views, and axes recipe
- scatter/live-data recipe hardening
- bounded Bar and Contour recipe detail
- support evidence and PNG snapshot recipe

Phase 411 should implement native high-performance demo paths:

- high-performance surface recipe using existing pyramid/tile APIs
- high-performance scatter recipe using `DataLogger3D` and columnar/FIFO
  evidence
- no-hidden-fallback diagnostics wording
- performance evidence validation rejecting benchmark-style claims without real
  artifacts
- snapshot/performance separation

Phase 412 should harden CI truth:

- focused cookbook/demo CI gate
- support evidence anti-fake gate
- scope and generated-roadmap truth gate
- CI contention cleanup
