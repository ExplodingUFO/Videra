# Phase 27 Context

## Goal

把 surface-chart 的 color-map 切换从 render-state 里的 per-tile recolor/rebuild 迁到 backend-owned color mapping 路径，避免 GPU 主路径在切换色图时按 resident tile 数量重建颜色数组和 GPU 资源。

## Boundary

- 只在 `Videra.SurfaceCharts.Rendering` 和 chart-local tests / benchmarks 内落地。
- 不把 chart-specific 需求反推到 `VideraView` 公共抽象。
- software path 继续保持可用，但不强行把它和 GPU path 绑成同一份 resident color contract。

## Current Truth

- `SurfaceChartRenderState` 在 `ColorMap` 变化时会为每个 resident tile 重新 `BuildTile(...)`，并把 `ColorChangedKeys` 标成每个 tile。
- `SurfaceChartGpuRenderBackend` 在收到 `ColorChangedKeys` 后会 `AddOrReplaceTileResources(...)`，导致 color-map 切换时重建 vertex buffers。
- 平台 resource-factory 抽象虽然有 `CreateShader` / `CreateResourceSet`，但 Windows / Linux / macOS 当前实现还没有 chart-local custom pipeline injection 的可靠基线。

## Decision

Phase 27 采用 backend-owned color mapping：

1. render-state 在 color-map 切换时只发出 global `ColorDirty`，不再 per-tile 重建 resident software colors。
2. software backend 在需要时按当前 `ColorMap` 重新生成 `SoftwareScene`。
3. GPU backend 保存 backend-owned vertex shadow / color-map LUT cache，并在 color-map 切换时原地更新现有 vertex buffers，而不是替换资源。

## Verification Focus

- color-map 切换时 GPU path 不新增 vertex/index buffer 资源。
- software fallback 仍然显示正确颜色。
- legend/value-range 继续以 active color map 为准。
