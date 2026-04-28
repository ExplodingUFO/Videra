# Videra.SurfaceCharts.Avalonia - VideraChartView 控件模块

[English](../../../src/Videra.SurfaceCharts.Avalonia/README.md) | [中文](videra-surfacecharts-avalonia.md)

`Videra.SurfaceCharts.Avalonia` 提供专用的 `VideraChartView` 以及 `Plot.Add.Surface`、`Plot.Add.Waterfall`、`Plot.Add.Scatter` 控件层。它独立于 `VideraView`，不承载 viewer 模式，也不耦合到 `VideraView` 的选择、标注或相机链路。
`Videra.SurfaceCharts.Processing` 只在 surface/cache-backed 路径需要，不是每条 chart 路径都必须安装。
独立 Demo 会先把 `Start here: In-memory first chart` 作为默认入口，等基线 first chart 跑通后，再切到 `Explore next: Cache-backed streaming`，需要 explicit/non-uniform 坐标、独立 `ColorField` 与 pinned-probe 分析级路径时再看 `Try next: Analytics proof`，需要第二个控件证明时再看 `Try next: Waterfall proof`，需要 scatter 路径证明时再看 repo-owned `Try next: Scatter proof`。

Phases 181-182 之后，这条已交付的 surface 路径在交互驻留更紧，probe 路径抖动更低，而且没有把边界扩大出现有 chart-local 路径。

VideraChartView 现在以 `ViewState` 作为 chart-view 契约，持久化 camera 与 data-window 状态。
图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。
对外交互诊断是 `InteractionQuality` + `InteractionQualityChanged`，状态为 `Interactive` / `Refine`。
`VideraChartView` 在 direct scatter 路径上复用同一套 chart-local 术语：left-drag navigation 报告 `Interactive`，release/capture lost 后回到 `Refine`，`ScatterChartRenderingStatus` 同时携带 `InteractionQuality` 和 columnar streaming/FIFO diagnostics。Columnar scatter 数据仍来自 `ScatterColumnarSeries`，通过 `ReplaceRange(...)` / `AppendRange(...)`、可选 `fifoCapacity` 和高容量默认 `Pickable=false` 表达。
VideraChartView 通过 chart-local `Plot.OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。
公开 overlay 配置入口是 `SurfaceChartOverlayOptions` / `Plot.OverlayOptions`，overlay state 类型继续保持 internal。
宿主拥有 `ISurfaceTileSource`、持久化的 `ViewState`、color-map 选择，以及 chart-local 产品 UI。
`VideraChartView` 拥有 chart-local built-in 手势、tile scheduling/cache、overlay presentation、native-host/render-host orchestration，以及 `RenderingStatus` 投影。
对外渲染 truth 是 `RenderingStatus` + `RenderStatusChanged`，字段包括 `ActiveBackend`、`IsReady`、`IsFallback`、`FallbackReason`、`UsesNativeSurface`、`ResidentTileCount`、`VisibleTileCount` 和 `ResidentTileBytes`。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 只依赖 `Videra.SurfaceCharts.Core`。
- `VideraChartView` 作为 UI shell，输入解释、调度和探测 overlay 分离。
- 独立 Demo 使用这条控制层，不回流到 `VideraView`。

## 当前能力

- 接受宿主提供的 `ISurfaceTileSource`
- 现在以 `ViewState` 作为 chart-view 契约，持久化 camera 与 data-window 状态
- 提供宿主可调用的 `FitToData()`、`ResetCamera()` 和 `ZoomTo(...)`
- 提供 built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + left-drag` focus zoom
- 提供显式 `Interactive` / `Refine` 质量切换
- `VideraChartView` 暴露 `InteractionQuality`、retained point count、append/replacement batch count、FIFO dropped points、configured FIFO capacity 与 pickable point count
- 提供 chart-local `GPU-first` 渲染主路径，并保留显式 `software fallback` 的 chart-local 回退语义（不表示后端 / viewer 降级）
- 暴露宿主可见的 `RenderingStatus` / `RenderStatusChanged`
- 提供 chart-local `OverlayOptions`，用于 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为
- 提供 axis/legend overlays、hover readout 与 `Shift + left-click` pinned probe
- 配合 `Videra.SurfaceCharts.Processing` 走 overview-first、lazy cache 读取与 view-aware tile residency

## 当前限制

- Linux Wayland 会话当前仍记录为 `XWayland compatibility` 路径，不宣称 compositor-native Wayland surface embedding

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Processing](videra-surfacecharts-processing.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)

## 术语

- `VideraChartView`
- `SurfaceProbeOverlayPresenter`
- `SurfaceProbeOverlayState`
