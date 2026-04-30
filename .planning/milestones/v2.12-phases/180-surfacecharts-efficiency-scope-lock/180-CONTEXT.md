# Phase 180 Context: SurfaceCharts Efficiency Scope Lock

## Milestone

`v2.12 SurfaceCharts Residency and Probe Efficiency`

## Requirement

`SCE-01`: Before implementation starts, lock `v2.12` to `SurfaceChartView` residency churn, probe latency, and invalidation/allocation efficiency work while freezing explicit non-goals around new chart families, chart-kernel widening, viewer/runtime breadth, backend/UI/platform expansion, compatibility shims, and migration adapters.

## Scope

- Tile request planning, residency prioritization, and prefetch behavior on the existing cache-backed / overview-first surface path.
- Probe latency and interaction-path churn on the shipped `SurfaceChartView` line.
- Render/overlay invalidation and allocation pressure only where it directly affects those churn/probe hotspots.
- Focused benchmarks, docs, and repository guardrails only.

## Starting Point

- `v2.11` already closed the current viewer/runtime mainline around static glTF/PBR renderer consumption.
- The next bounded gap from the existing roadmap is on the `SurfaceCharts` side: camera-driven residency churn, probe latency, and avoidable invalidation/allocation pressure.
- `SurfaceCharts` already has concrete benchmark coverage and chart-local runtime seams; this milestone should stay inside those seams instead of widening into generic `Chart3D`, a new family, or a broader chart kernel rewrite.

