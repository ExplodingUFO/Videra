---
phase: 406
title: "Cookbook QA Hardening"
bead: Videra-1h1
child_beads:
  - Videra-14s
  - Videra-ffa
status: planned
created_at: 2026-04-30
scope: "Bounded cookbook QA docs/test hardening only"
---

# Phase 406 Plan

## Scope

This phase hardens the current SurfaceCharts cookbook handoff without changing
product runtime code or demo behavior. The work is limited to docs, Core
text-contract tests, and phase evidence.

## Assumptions

- `docs/surfacecharts-release-cutover.md` remains the stable URL, but the page
  should read as the current SurfaceCharts consumer handoff instead of a stale
  v2.58-only action.
- The handoff page documents package consumption and support routing only; it
  does not approve publishing, public tags, or GitHub Release publication.
- The cookbook coverage guard should remain simple and read-only. It should not
  instantiate Avalonia controls, parse arbitrary code, or introduce a broad
  documentation framework.

## Implementation

1. Add a focused `SurfaceChartsCookbookCoverageMatrixTests` Core test that maps
   each cookbook group to root README, demo README, cutover entry, visible demo
   proof label when one exists, and code-behind snippet evidence.
2. Rename public link text from `SurfaceCharts v2.58 Release Cutover` to
   `SurfaceCharts Current Consumer Handoff` while keeping the existing file
   path stable.
3. Tighten the cutover page introduction so it clearly states the page is
   documentation-only and does not imply publish/tag/GitHub Release action.

## Success Criteria

- `Videra-14s` is satisfied when cookbook rows for first chart, styling,
  interactions, live data, linked axes, Bar, Contour, and export are covered by
  a focused matrix test.
- `Videra-ffa` is satisfied when README, docs index, demo README, and the
  cutover page consistently describe the stable route as the current consumer
  handoff without release-action implication.
- Snapshot/export scope guardrails still pass.
