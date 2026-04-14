# Videra.SurfaceCharts.Avalonia - SurfaceChartView 控件模块

[English](../../../src/Videra.SurfaceCharts.Avalonia/README.md) | [中文](videra-surfacecharts-avalonia.md)

`Videra.SurfaceCharts.Avalonia` 提供专用的 `SurfaceChartView` 控件层。它独立于 `VideraView`，不承载 viewer 模式，也不耦合到 `VideraView` 的选择、标注或相机链路。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 只依赖 `Videra.SurfaceCharts.Core`。
- `SurfaceChartView` 作为 UI shell，输入解释、调度和探测 overlay 分离。
- 独立 Demo 使用这条控制层，不回流到 `VideraView`。

## 当前能力

- 接受宿主提供的 `ISurfaceTileSource`
- 根据宿主提供的 `Viewport` 渲染曲面 tile
- 配合 `Videra.SurfaceCharts.Processing` 走 overview-first 与 lazy cache 读取
- 保留 probe overlay 的状态与渲染路径

## 当前限制

- 还没有完成内建鼠标缩放 / 拖拽 / orbit 交互
- 还没有完成坐标轴、刻度、标签与图例系统
- Demo 目前仍以宿主切换 viewport 预设为主，而不是完整交互式 chart camera

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Processing](videra-surfacecharts-processing.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `SurfaceChartView`
- `SurfaceProbeOverlayPresenter`
- `SurfaceProbeOverlayState`
