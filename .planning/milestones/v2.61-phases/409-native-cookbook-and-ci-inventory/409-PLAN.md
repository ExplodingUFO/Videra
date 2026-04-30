---
phase: 409
title: "Native Cookbook and CI Inventory Plan"
bead: Videra-63e
status: complete
created_at: 2026-04-30
---

# Phase 409 Plan

## Goal

Inventory current cookbook/demo, native 3D chart API, performance-sensitive
paths, CI gates, and anti-fake validation gaps before implementation.

## Work Split

Phase 409 was split into three Beads-backed inventory tasks:

- `Videra-5sh`: cookbook/demo surface inventory.
- `Videra-1f8`: native API/performance surface inventory.
- `Videra-3b6`: CI and anti-fake validation inventory.

Each child task wrote exactly one inventory file in a dedicated Dolt-aware
worktree branch. Product code, tests, public docs, Beads export files, roadmap,
and state files were excluded from child write sets.

## Success Criteria

1. Current SurfaceCharts demo cookbook recipes are mapped against shipped 3D
   chart APIs.
2. ScottPlot5-inspired ergonomics are translated into Videra-native concepts
   without compatibility claims.
3. Performance-sensitive demo/data paths and current evidence gaps are
   identified.
4. CI/test gaps are scoped with anti-fake validation rules.

## Verification Plan

- Verify each inventory file with `git diff --check`.
- Export Beads state after closing Phase 409 child beads.
- Regenerate `docs/ROADMAP.generated.md`.
- Run `BeadsPublicRoadmapTests`.
- Run `git diff --check` for final planning/export diff.
