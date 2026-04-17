# Videra

[English](../../README.md) | [中文](README.md)

Videra 是一套面向 .NET 桌面应用的跨平台 3D 查看组件库，核心目标是在 Avalonia 应用中提供可复用、可嵌入、可扩展的 3D 查看能力。

surface-chart 模块家族与 `VideraView` 相互独立。它面向离线大矩阵、曲面图和时频图一类可视化场景，独立 Demo 也单独发布为 `Videra.SurfaceCharts.Demo`。

当前 `alpha` 阶段需要明确说明：

- 已完成独立模块边界、LOD、缓存读取、`GPU-first` 渲染主路径与 Demo 路径
- `SurfaceChartView` 当前已经交付 axis/legend overlays、hover readout、`Shift + LeftClick` pinned probe，以及可见的 `RenderingStatus` / `RenderStatusChanged`
- SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。
- 当前对外交付 built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + Left drag` focus zoom
- 图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。
- SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。
- Linux Wayland 会话当前仍是 `XWayland compatibility` 路径，不是 compositor-native Wayland surface embedding

## 项目状态

- 当前处于早期 `alpha`
- 当前默认版本线为 `0.1.0-alpha.2`
- 公开消费者入口以 `nuget.org` 为准
- `GitHub Packages` 只保留给 `preview` / internal 验证和贡献者实验
- `Videra.SurfaceCharts.*` 仍是 source-only modules，不在当前公开包承诺内
- GitHub Actions 会在 pull requests 中自动执行跨平台原生验证；Linux 会同时覆盖 `X11` 原生路径和 Wayland 会话下的 `XWayland` 兼容路径；本地 matching-host 运行仍主要用于复现和排障

## 安装与包选择

以下中文说明用于快速索引，英文版为准。

公开消费者默认从 `nuget.org` 安装。Avalonia 应用推荐安装 `Videra.Avalonia` 加一个匹配平台包：

```bash
dotnet add package Videra.Avalonia
dotnet add package Videra.Platform.Windows
# 或
dotnet add package Videra.Platform.Linux
# 或
dotnet add package Videra.Platform.macOS
```

如果只需要渲染抽象和导入管线，则直接安装 `Videra.Core`：

```bash
dotnet add package Videra.Core
```

当前 `alpha` 预览线在需要时仍可通过 `GitHub Packages` 进行 `preview` 验证，但那不是默认公开安装路径。

`VIDERA_BACKEND` 只影响后端选择偏好，不会安装缺失的平台包，也不会替代 matching-host 原生验证。

默认 first-scene 入口现在优先看 [Videra.MinimalSample](../../samples/Videra.MinimalSample/README.md)，而不是直接跳进 `VideraView.Engine` 扩展流。

## 快速入口

- [英文首页](../../README.md)
- [包矩阵](../package-matrix.md)
- [支持矩阵](../support-matrix.md)
- [发布策略](../release-policy.md)
- [Benchmark Gates](../benchmark-gates.md)
- [Alpha Feedback](../alpha-feedback.md)
- [扩展合同](extensibility.md)：`VideraView.Engine`、`RegisterPassContributor(...)`、`RegisterFrameHook(...)`、`RenderCapabilities`、`BackendDiagnostics` 与 `samples/Videra.ExtensibilitySample`
- [交互示例](../../samples/Videra.InteractionSample/README.md)：`host owns` `SelectionState`、`Annotations` 和 annotation state，`Navigate` / `Select` / `Annotate`，`SelectionRequested` / `AnnotationRequested`，以及 `VideraNodeAnnotation` / `VideraWorldPointAnnotation`
- [SurfaceCharts.Core](modules/videra-surfacecharts-core.md)：`SurfaceChartView` 之外的领域契约、viewport / LOD、tile source 与 probe contract
- [SurfaceCharts.Avalonia](modules/videra-surfacecharts-avalonia.md)：专用 `SurfaceChartView` 控件层，独立于 `VideraView`，包含 `RenderingStatus` / `RenderStatusChanged`、axis/legend overlays 与 probe overlay
- [SurfaceCharts.Processing](modules/videra-surfacecharts-processing.md)：离线 pyramid / cache 构建、persistent payload session、ordered batch reads 与 `SurfaceTileStatistics`
- [独立 Demo](../../samples/Videra.SurfaceCharts.Demo/README.md)：`Videra.SurfaceCharts.Demo`，展示 source 切换、built-in interaction、probe workflow 与 rendering path truth
- [架构说明](ARCHITECTURE.md)
- [贡献指南](CONTRIBUTING.md)
- [故障排查](troubleshooting.md)
- [原生宿主验证](native-validation.md)
- [文档导航](index.md)

## 模块文档

- [Videra.Core](modules/videra-core.md)
- [Videra.Avalonia](modules/videra-avalonia.md)
- [Videra.Platform.Windows](modules/platform-windows.md)
- [Videra.Platform.Linux](modules/platform-linux.md)
- [Videra.Platform.macOS](modules/platform-macos.md)
- [Videra.Demo](modules/demo.md)
- [Videra.SurfaceCharts.Core](modules/videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Avalonia](modules/videra-surfacecharts-avalonia.md)
- [Videra.SurfaceCharts.Processing](modules/videra-surfacecharts-processing.md)
- [Videra.SurfaceCharts.Demo](../../samples/Videra.SurfaceCharts.Demo/README.md)

## 说明

默认公开入口现在以英文为主，英文版为准。中文文档保留为手动切换入口，便于中文读者快速查看项目定位、安装方式、验证方式和模块说明。

扩展入口的中文镜像集中在 [扩展合同](extensibility.md)。该页会把 `samples/Videra.ExtensibilitySample`、`disposed` 后注册调用的 `no-op` 语义，以及软件回退与 `BackendDiagnostics` / `FallbackReason` 的公开约定放在一起说明。

`host owns` `SelectionState`、`Annotations` 与 annotation state。

受控交互入口则以 [samples/Videra.InteractionSample](../../samples/Videra.InteractionSample/README.md) 为主：`host owns` `SelectionState`、`Annotations` 与 annotation state，内建模式是 `Navigate`、`Select`、`Annotate`，选择保持 `object-level`，标注同时覆盖 object anchors 与 world-point anchors，并通过 `VideraNodeAnnotation` / `VideraWorldPointAnnotation` 表达，overlay responsibilities split between `3D highlight/render state` and `2D label/feedback rendering`。

surface-chart 模块家族则以 `SurfaceChartView` 为中心，独立于 `VideraView`，并保持与 viewer 侧选择、标注和 camera 流程解耦。当前对外 truth 是：独立 Demo、built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + Left drag` focus zoom、hover 与 `Shift + LeftClick` pinned probe、可见 `RenderingStatus`，以及显式 `Interactive` / `Refine` 质量切换。SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。
