# Phase 27 Research

## Observed Constraints

### Render-State Truth

- `SurfaceChartRenderState.ApplyIncrementalChanges(...)` 当前在 color-map 切换时为每个 resident tile 重建 `SurfaceRenderTile`。
- 这条路径会在 GPU 和 software host 共用的 shared state 上发生，所以即便 GPU backend 不替换资源，主线仍然先付出一次 per-tile recolor 成本。

### Backend Truth

- `SurfaceChartGpuRenderBackend` 当前把 `SurfaceRenderVertex.Color` 直接展开成 `VertexPositionNormalColor[]` 并创建 vertex buffer。
- color-map 变化会通过 `ColorChangedKeys` 重新调用 `AddOrReplaceTileResources(...)`，导致 vertex buffer 重建。

### Platform Truth

- `IResourceFactory.CreatePipeline(PipelineDescription)` 在 SurfaceCharts 当前使用的平台实现里还没有真正消费 caller-supplied shader/layout/resource-set 描述。
- `ICommandExecutor.SetVertexBuffer(buffer, index)` 已经能在 Win/Linux 路径绑定 constant buffers，但 chart-local custom shader injection 现在不是一个稳定前提。

## Chosen Direction

在不扩张平台抽象和 `VideraView` 边界的前提下，Phase 27 采取这条收敛路径：

1. **Stop render-state recolor churn**
   - resident tiles 保留 geometry/sample truth；
   - color-map 切换只标记 global dirty，不更新 per-tile colors。

2. **Backend-owned color remap**
   - GPU resident tile 保留可重用 vertex shadow；
   - backend 使用 `SurfaceColorMapLut` / `SurfaceColorMapUploadCache` 管理 active color-map truth；
   - color-map 切换时只 `UpdateArray(...)` 现有 vertex buffer。

3. **Software scene rebuild on demand**
   - software backend 保持 current-color truth，但这份重建只发生在 software render path / fallback path 内。

## Risks

- `SurfaceChartResidentTile.SoftwareRenderTile` 不再代表 current active color-map；software backend 需要显式使用 `inputs.ColorMap` 重建 scene，不能再直接信 resident colors。
- fake graphics backends/tests 需要补 `UpdateArray` 计数，否则无法证明“更新而不是重建资源”。

## Verification Plan

- core render-state tests: color-map change no longer replaces resident render tile.
- core GPU tests: color-map change reuses vertex/index buffers and updates existing vertex buffer content.
- integration tests: legend/value-range follows new color map after active color-map change.
- benchmark build: `Videra.SurfaceCharts.Benchmarks` remains buildable after new LUT/cache types land.
