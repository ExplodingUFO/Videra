# Videra.SurfaceCharts.Avalonia - SurfaceChartView 控件模块

[English](../../../src/Videra.SurfaceCharts.Avalonia/README.md) | [中文](videra-surfacecharts-avalonia.md)

`Videra.SurfaceCharts.Avalonia` 提供专用的 `SurfaceChartView` 控件层。它独立于 `VideraView`，不承载 viewer 模式，也不耦合到 `VideraView` 的选择、标注或相机链路。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 只依赖 `Videra.SurfaceCharts.Core`。
- `SurfaceChartView` 作为 UI shell，输入解释、调度和探测 overlay 分离。
- 独立 Demo 使用这条控制层，不回流到 `VideraView`。

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `SurfaceChartView`
- `SurfaceProbeOverlayPresenter`
- `SurfaceProbeOverlayState`
