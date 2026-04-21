---
phase: 18-demo-docs-and-repository-truth-for-professional-charts
plan: 02
subsystem: surface-charts-docs
tags: [surface-charts, docs, localization, readme, repository-truth]
requires:
  - phase: 18-demo-docs-and-repository-truth-for-professional-charts
    provides: visible demo truth from plan 18-01
provides:
  - aligned English chart entrypoints and module/demo READMEs
  - concise Chinese routing and module mirrors with the same shipped boundary truth
  - removal of stale "axes/legend incomplete" wording from guarded entrypoints
affects: [18-03]
key-files:
  modified:
    - README.md
    - samples/Videra.SurfaceCharts.Demo/README.md
    - src/Videra.SurfaceCharts.Avalonia/README.md
    - docs/zh-CN/README.md
    - docs/zh-CN/modules/videra-surfacecharts-avalonia.md
    - docs/zh-CN/modules/videra-surfacecharts-processing.md
completed: 2026-04-14
---

# Phase 18 Plan 02 Summary

## Accomplishments
- Updated the root `README.md` so the chart onboarding section now routes readers through the current demo/control story: independent sibling boundary, axis/legend overlays, probe workflow, and renderer-status truth.
- Refreshed the demo and Avalonia READMEs to describe the shipped `GPU-first` + explicit fallback path, hover plus `Shift + LeftClick` pinned probes, and the current host-driven interaction limits.
- Updated the Chinese root and module mirrors so they match the same chart boundary, overlay/probe truth, `RenderingStatus` surface, and Phase 17 data-path reality.

## Verification
- `rg -n "Surface Charts Onboarding|axis/legend overlays|Shift \\+ LeftClick|RenderStatusChanged|RenderingStatus|SurfaceTileStatistics|XWayland compatibility" README.md samples/Videra.SurfaceCharts.Demo/README.md src/Videra.SurfaceCharts.Avalonia/README.md docs/zh-CN/README.md docs/zh-CN/modules/videra-surfacecharts-avalonia.md docs/zh-CN/modules/videra-surfacecharts-processing.md`
- `rg -n "axis labels, ticks, legends, and value probes are not complete|full built-in mouse orbit/pan/zoom is not complete|还没有完成坐标轴、刻度、标签与图例系统|built-in mouse orbit / pan / zoom is not complete" README.md docs/zh-CN/README.md src/Videra.SurfaceCharts.Avalonia/README.md docs/zh-CN/modules/videra-surfacecharts-avalonia.md`

## Notes
- Detailed chart truth still lives on the module/demo pages; the root entrypoints remain routing layers.
- `Videra.SurfaceCharts.Rendering` stays an implementation detail and is not promoted as a new public onboarding module.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
