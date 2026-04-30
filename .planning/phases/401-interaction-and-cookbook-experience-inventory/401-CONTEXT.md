---
phase: 401
title: "Interaction and Cookbook Experience Inventory"
status: completed
bead: Videra-v259.1
completed_at: "2026-04-30T15:09:05+08:00"
---

# Phase 401 Context

## Scope

Phase 401 is an inventory and coordination phase for v2.59. It maps current
SurfaceCharts interaction APIs, Plot/code-experience surfaces, demo cookbook
entry points, docs, tests, and guardrails before Phase 402/403 implementation.

It does not change product code.

## Evidence Inputs

Phase 401 split the inventory into three Beads-backed workstreams:

- `Videra-imn`: interaction and Plot API inventory.
- `Videra-nye`: cookbook demo and docs inventory.
- `Videra-2ly`: validation and guardrail inventory.

The detailed outputs are:

- `401A-INTERACTION-INVENTORY.md`
- `401B-COOKBOOK-INVENTORY.md`
- `401C-VALIDATION-INVENTORY.md`

## Current Surface Summary

- `VideraChartView.Plot` exposes `Plot3D` as the chart-local API facade.
- `Plot.Add.*` covers surface, waterfall, scatter, bar, and contour families.
- `Plot.Axes`, `Plot.OverlayOptions`, `Plot.SavePngAsync(...)`, interaction
  profile/commands, probe, selection, and draggable overlay recipes already
  exist.
- `samples/Videra.SurfaceCharts.Demo` already has a cookbook gallery, six
  recipe groups, copy-snippet support, support summary, and snapshot capture.
- Docs already route users through the package/cookbook/support path, but some
  current demo proof paths have drifted ahead of docs and cookbook recipes.

## Gaps Selected For v2.59

- Bar and contour add paths are visible in demo flows but are not yet first-class
  cookbook recipes and docs entries.
- Bar and contour add APIs return the base `Plot3DSeries` handle, unlike the
  typed surface/waterfall/scatter handle paths.
- Current interaction helper examples are spread across APIs and recipes; users
  need short copyable examples for commands, probe, selection, and draggable
  overlays without a generic editor.
- Existing cookbook snippets can be made more self-contained where they rely on
  implied caller-owned variables.
- Docs/demo text-contract tests should prevent visible demo entries from
  drifting away from README/cookbook coverage.

## Boundary

ScottPlot 5 remains an ergonomics inspiration only. Phase 402 and Phase 403 must
not introduce ScottPlot compatibility adapters, parity claims, old chart
controls, direct public `Source`, hidden fallback/downshift behavior, renderer
or backend expansion, PDF/vector export, or a generic plotting workbench.
