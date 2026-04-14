# Videra.SurfaceCharts.Core - 曲面图核心模块

[English](../../../src/Videra.SurfaceCharts.Core/README.md) | [中文](videra-surfacecharts-core.md)

`Videra.SurfaceCharts.Core` 定义 surface-chart 模块家族的领域契约，包括 `SurfaceChartView` 之外的共享数据结构、viewport / LOD 选择、tile 标识和 probe contract。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 该模块家族独立于 `VideraView`，不复用 viewer 控件生命周期。
- `SurfaceChartView` 是专用的 Avalonia 控件层，而不是 `VideraView` 的一种模式。
- 首版面向离线大矩阵、曲面图和时频图类可视化，强调 LOD 与缓存友好性。

## 核心契约

- `SurfaceMetadata`
- `SurfaceViewport`
- `SurfaceLodPolicy`
- `SurfaceTileKey`
- `SurfaceTile`
- `ISurfaceTileSource`

这里最重要的约定是：`SurfaceTile.Width` / `Height` 表示 value-grid 维度，而 `SurfaceTile.Bounds` 表示原始数据集里的 source-space 覆盖范围。粗粒度 LOD tile 因此不再假设 `1:1 sample -> value`。

## 相关入口

- [Videra.SurfaceCharts.Avalonia](videra-surfacecharts-avalonia.md)
- [Videra.SurfaceCharts.Processing](videra-surfacecharts-processing.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `SurfaceChartView`
- `SurfacePyramidBuilder`
- `SurfaceTileSource`
- `SurfaceProbeResult`
