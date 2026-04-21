---
summary_date: 2026-04-16T17:23:31.4399056+08:00
phase: 27
plan: 27-02
status: complete
requirements-completed:
  - SHDR-01
  - SHDR-02
---

# 27-02 Summary

- Extended [SurfaceChartGpuResidentTile.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuResidentTile.cs) with reusable vertex shadow data and in-place color updates.
- Updated [SurfaceChartGpuRenderBackend.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs) to reuse resident buffers on color-map changes instead of recreating GPU resources.
- Updated [SurfaceChartSoftwareRenderBackend.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs) so software/fallback rendering rebuilds current-color scenes from active `ColorMap`.
- Added GPU reuse regression coverage in [SurfaceChartGpuFallbackTests.cs](F:/CodeProjects/DotnetCore/Videra/tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartGpuFallbackTests.cs).
