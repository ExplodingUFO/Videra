# Videra.SurfaceCharts.Avalonia - SurfaceChartView 控件模块

[English](../../../src/Videra.SurfaceCharts.Avalonia/README.md) | [中文](videra-surfacecharts-avalonia.md)

`Videra.SurfaceCharts.Avalonia` 提供专用的 `SurfaceChartView` 控件层。它独立于 `VideraView`，不承载 viewer 模式，也不耦合到 `VideraView` 的选择、标注或相机链路。

SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。
图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。
SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 只依赖 `Videra.SurfaceCharts.Core`。
- `SurfaceChartView` 作为 UI shell，输入解释、调度和探测 overlay 分离。
- 独立 Demo 使用这条控制层，不回流到 `VideraView`。

## 当前能力

- 接受宿主提供的 `ISurfaceTileSource`
- 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接
- 提供宿主可调用的 `FitToData()`、`ResetCamera()` 和 `ZoomTo(...)`
- 提供 built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + Left drag` focus zoom
- 提供显式 `Interactive` / `Refine` 质量切换
- 提供 chart-local `GPU-first` 渲染主路径，并保留显式 `software fallback`
- 暴露宿主可见的 `RenderingStatus` / `RenderStatusChanged`
- 提供 chart-local `OverlayOptions`，用于 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为
- 提供 axis/legend overlays、hover readout 与 `Shift + LeftClick` pinned probe
- 配合 `Videra.SurfaceCharts.Processing` 走 overview-first、lazy cache 读取与 view-aware tile residency

## 当前限制

- Linux Wayland 会话当前仍记录为 `XWayland compatibility` 路径，不宣称 compositor-native Wayland surface embedding

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Processing](videra-surfacecharts-processing.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `SurfaceChartView`
- `SurfaceProbeOverlayPresenter`
- `SurfaceProbeOverlayState`
