# Videra.SurfaceCharts.Core - 曲面图核心模块

[English](../../../src/Videra.SurfaceCharts.Core/README.md) | [中文](videra-surfacecharts-core.md)

`Videra.SurfaceCharts.Core` 定义 surface-chart 模块家族的领域契约，包括 `SurfaceChartView` 之外的共享数据结构、viewport / LOD 选择、tile 标识和 probe contract。

这条拆分也把现有 chart-local 的效率故事保持得更窄：交互驻留更紧、probe 路径抖动更低，仍然留在 chart-local 这边。
Columnar scatter streaming 也留在这个 chart-domain 层：`ScatterColumnarSeries` 通过 `ReplaceRange(...)` 做整体替换，通过 `AppendRange(...)` 接收 streaming batch，并可用正数 `fifoCapacity` 保持 bounded retained point window。高容量 columnar 路径默认 `Pickable=false`，只有宿主确实需要 per-point hit participation 时才打开。

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
- `ScatterColumnarData`
- `ScatterColumnarSeries`

这里最重要的约定是：`SurfaceTile.Width` / `Height` 表示 value-grid 维度，而 `SurfaceTile.Bounds` 表示原始数据集里的 source-space 覆盖范围。粗粒度 LOD tile 因此不再假设 `1:1 sample -> value`。

Scatter streaming 的 retained truth 包括 retained point count、append/replacement batch count、FIFO dropped points、last dropped point count 与 configured FIFO capacity。这些是 diagnostics 和 benchmark evidence 输入，不是通用 streaming framework。

## Source-First 与高级负载

`SurfaceMatrix` 仍是默认的 source-first regular-grid 入口。现有宿主可以继续走 `new SurfaceMatrix(metadata, values)` 加 `SurfacePyramidBuilder` 的默认路径，不需要为了新契约先改 `SurfaceChartView` 侧的接入方式。

如果要承载更专业的分析语义，也可以在同一套 chart shell 下逐步切到更丰富的底层负载：`SurfaceMatrix(metadata, heightField, colorField, mask)` 和 `SurfaceTile(..., heightField, colorField, mask)` 现在都支持 `SurfaceScalarField` 高度场、独立的 `ColorField`，以及一等 `SurfaceMask`。

这层拆分是刻意保持的：默认 source-first regular-grid 路径继续保持狭窄，而高级调用者可以在不放大 `SurfaceChartView` public surface 的前提下，选择独立的 `ColorField` 和一等 `SurfaceMask` 语义。

## 相关入口

- [Videra.SurfaceCharts.Avalonia](videra-surfacecharts-avalonia.md)
- [Videra.SurfaceCharts.Processing](videra-surfacecharts-processing.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `SurfaceChartView`
- `SurfacePyramidBuilder`
- `SurfaceTileSource`
- `SurfaceProbeResult`
