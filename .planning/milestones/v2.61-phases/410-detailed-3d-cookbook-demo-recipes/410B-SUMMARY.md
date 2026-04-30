---
phase: 410B
title: "Waterfall, Axes, and Linked-View Cookbook Summary"
bead: Videra-zcd
status: complete
created_at: 2026-04-30
---

# Phase 410B Summary

Phase 410B added detailed native SurfaceCharts cookbook coverage for waterfall authoring, axis and overlay configuration, linked views, explicit disposable link lifetime, and `ViewState` / `FitToData()` host choices.

## Changed Files

- `samples/Videra.SurfaceCharts.Demo/Recipes/waterfall.md`
- `samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookWaterfallLinkedRecipeTests.cs`
- `.planning/phases/410-detailed-3d-cookbook-demo-recipes/410B-SUMMARY.md`

## Outcomes

- `Plot.Add.Waterfall` is documented through a minimal matrix recipe and a production-style `SurfaceMatrix` setup.
- `Plot.Axes` usage is documented for labels, units, current limits, bounds, `AutoScale()`, and `ClearBounds()`.
- `Plot.OverlayOptions` usage is documented for minor ticks, grid plane, axis-side mode, precision, per-axis formatters, legend title, crosshair, and legend position.
- `LinkViewWith` is documented as a two-chart `ViewState` synchronization lifetime that must be disposed when synchronization ends.
- `ViewState` restore and `FitToData()` refit choices are described without claiming evidence that the docs did not verify.
- Focused tests pin required native tokens and reject scope creep terms in the new recipe files.

## Verification

- Passed: `git diff --check -- samples/Videra.SurfaceCharts.Demo/Recipes/waterfall.md samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookWaterfallLinkedRecipeTests.cs .planning/phases/410-detailed-3d-cookbook-demo-recipes/410B-SUMMARY.md`.
- Passed: `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter FullyQualifiedName~SurfaceChartsCookbookWaterfallLinkedRecipeTests --no-restore` after `dotnet restore tests/Videra.Core.Tests/Videra.Core.Tests.csproj` created the missing worktree-local assets file.

## Boundaries Preserved

- Recipes use the current `VideraChartView` and `Plot.Add.*` model.
- Recipes do not add runtime code, renderer behavior, export formats, broader demo controls, or support evidence beyond the documented native chart path.
- No skipped check is represented as passing evidence.
