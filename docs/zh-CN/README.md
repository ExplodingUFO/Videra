# Videra

[English](../../README.md) | [中文](README.md)

Videra 是一套面向 .NET 桌面应用的跨平台 3D 查看组件库。公开 viewer 路径以 Avalonia 为先，核心运行时真相由 `SceneDocument` 和 imported asset catalog 承载。

当前 shipped viewer 路径保持 static-scene-only：`SceneDocument` 保留 backend-neutral 的 imported scene truth，进入运行时的资产仍以 `SceneNode`、`MeshPrimitive`、`MaterialInstance`、`Texture2D` 和 `Sampler` 为中心；其中还包括 per-primitive non-Blend material participation、occlusion texture binding/strength，以及 `KHR_texture_transform` 的 offset/scale/rotation 和 texture-coordinate override。canonical runtime path 现在可以把一个 imported entry 扩成多个 internal runtime objects，因此 mixed opaque/transparent primitive participation 可以穿过 runtime bridge，而不需要把公开边界扩大成更宽的 transparency system。当前 renderer path 已经端到端消费 baseColor 纹理采样和 occlusion texture binding/strength，其中也会应用 `KHR_texture_transform` 的 offset/scale/rotation 与 texture-coordinate override；emissive 和 normal-map-ready 仍只是保留的 runtime truth，不代表更宽的 renderer/shader/backend 着色承诺。动画、骨骼、morph targets、灯光、阴影、post-processing、额外 UI adapter，以及 Wayland/OpenGL/WebGL/backend API 扩展仍在当前边界之外。

surface-chart 模块家族与 `VideraView` 相互独立。它面向离线大矩阵、曲面图和时频图一类可视化场景；`Videra.SurfaceCharts.Demo` 保持 repository-only，只用于参考和 support repro。
当前公开控制层包括 `SurfaceChartView`、`WaterfallChartView` 和 `ScatterChartView` 三个 Avalonia 控件；`Videra.SurfaceCharts.Processing` 只在 surface/cache-backed 路径需要，不是每条 chart 路径都必须安装。独立 Demo 现在通过 repo-owned `Try next: Analytics proof` 和 `Try next: Scatter proof` 也覆盖分析与 scatter 路径。

Phases 181-182 之后，现有 chart-local 路径的交互驻留更紧、probe 路径抖动更低；当前 SurfaceCharts 硬门槛仍只落在 `ApplyResidencyChurnUnderCameraMovement` 和 `ProbeLatency`。

默认的打包支持证明请先把 `smoke/Videra.SurfaceCharts.ConsumerSmoke` 当作 surface/cache-backed proof，`surfacecharts-support-summary.txt` 也从这里产出；再把 `Videra.SurfaceCharts.Avalonia` 中的 `SurfaceChartView`、`WaterfallChartView` 和 `ScatterChartView` 视为当前 chart control entrypoint。
先从 Demo 默认的 `Start here: In-memory first chart` 路径开始，确认基线 first chart 可以渲染后，再切到 `Explore next: Cache-backed streaming`，需要 explicit/non-uniform 坐标和 `ColorField`/`pinned probe` 的分析级路径时再看 `Try next: Analytics proof`，需要第二个控件证明时再看 `Try next: Waterfall proof`。

当前 `alpha` 阶段需要明确说明：

- 已完成独立模块边界、LOD、缓存读取、`GPU-first` 渲染主路径与 Demo 路径
- `SurfaceChartView` 当前已经交付 axis/legend overlays、hover readout、`Shift + left-click` pinned probe，以及可见的 `RenderingStatus` / `RenderStatusChanged`
- SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为旧桥接。
- 当前对外交付 built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + left-drag` focus zoom
- 图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。
- 对外交互诊断是 `InteractionQuality` + `InteractionQualityChanged`，状态为 `Interactive` / `Refine`。
- SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。
- 公开 overlay 配置入口是 `SurfaceChartOverlayOptions` / `OverlayOptions`，overlay state 类型继续保持 internal。
- 宿主拥有 `ISurfaceTileSource`、持久化的 `ViewState`、color-map 选择，以及 chart-local 产品 UI。
- `SurfaceChartView` 拥有 chart-local built-in 手势、tile scheduling/cache、overlay presentation、native-host/render-host orchestration，以及 `RenderingStatus` 投影。
- 对外渲染 truth 是 `RenderingStatus` + `RenderStatusChanged`，字段包括 `ActiveBackend`、`IsReady`、`IsFallback`、`FallbackReason`、`UsesNativeSurface` 和 `ResidentTileCount`。
- Linux Wayland 会话当前仍是 `XWayland` bridge 路径，不是 compositor-native Wayland surface embedding

## 项目状态

- 当前处于早期 `alpha`
- 当前默认版本线为 `0.1.0-alpha.7`
- 公开消费者入口以 `nuget.org` 为准
- `GitHub Packages` 只保留给 `preview` / internal 验证和贡献者实验
- `smoke/Videra.WpfSmoke` 是 repository-only 的 Windows WPF smoke 证明，只用于验证和 support evidence，不是第二条公开 UI 包线或发布路径。
- `Videra.SurfaceCharts.*` 现在已经进入公开 `alpha` 包线，`Videra.SurfaceCharts.Demo` 继续保持 repository-only。
- GitHub Actions 会在 pull requests 中自动执行跨平台原生验证；Linux 会同时覆盖 `X11` 原生路径和 Wayland 会话下的 `XWayland` bridge 路径；本地 matching-host 运行仍主要用于复现和排障
- release-candidate review 使用只读的 `Release Dry Run` workflow：它通过 `scripts/Invoke-ReleaseDryRun.ps1` 读取 `eng/public-api-contract.json`，复用 `scripts/Validate-Packages.ps1`，上传 `release-dry-run-evidence`，但不会发布到 `nuget.org` 或 GitHub Packages。

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

如果只需要运行时内核而不需要 Avalonia UI 层，则直接安装 `Videra.Core`：

```bash
dotnet add package Videra.Core
```

如果还需要文件导入能力，则额外安装独立导入包：

```bash
dotnet add package Videra.Import.Gltf
dotnet add package Videra.Import.Obj
```

如果要在 Avalonia 路径上使用 importer-backed 的 `LoadModelAsync(...)` / `LoadModelsAsync(...)`，还需要显式安装 `Videra.Import.Gltf` 和/或 `Videra.Import.Obj`，并通过 `VideraViewOptions.ModelImporter` 接入对应导入器；`Videra.Avalonia` 默认安装路径不再隐式携带这些包。

当前 surface-chart 的公开安装入口是：

```bash
dotnet add package Videra.SurfaceCharts.Avalonia
dotnet add package Videra.SurfaceCharts.Processing
```

`Videra.SurfaceCharts.Avalonia` 会传递依赖 `Videra.SurfaceCharts.Core` 与 `Videra.SurfaceCharts.Rendering`。只有在你不走 `SurfaceChartView` 壳层、需要直接构建 chart-domain contracts 或自定义 tile source 时，才需要单独安装 `Videra.SurfaceCharts.Core`。

当前 `alpha` 预览线在需要时仍可通过 `GitHub Packages` 进行 `preview` 验证，但那不是默认公开安装路径。

`VIDERA_BACKEND` 只影响后端选择偏好，不会安装缺失的平台包，也不会替代 matching-host 原生验证。

默认 first-scene 入口现在优先看 [Videra.MinimalSample](../../samples/Videra.MinimalSample/README.md)，而不是直接跳进 `VideraView.Engine` 扩展流。
当前公开的 canonical viewer flow 是 `VideraViewOptions -> LoadModelAsync(...) -> FrameAll() / ResetCamera() -> BackendDiagnostics`。

## 快速入口

- [英文首页](../../README.md)
- [包矩阵](../package-matrix.md)
- [支持矩阵](../support-matrix.md)
- [发布策略](../release-policy.md)
- [发布运行手册](../releasing.md)：包含 `Release Dry Run`、`release-dry-run-evidence` 与非发布式 release-candidate 验证路径
- [Benchmark Gates](../benchmark-gates.md)
- [Alpha Feedback](../alpha-feedback.md)
- [扩展合同](extensibility.md)：`VideraView.Engine`、`RegisterPassContributor(...)`、`RegisterFrameHook(...)`、`RenderCapabilities`、`BackendDiagnostics` 与 `samples/Videra.ExtensibilitySample`
- [交互示例](../../samples/Videra.InteractionSample/README.md)：`host owns` `SelectionState`、`Annotations` 和 annotation state，`Navigate` / `Select` / `Annotate` / `Measure`，`SelectionRequested` / `AnnotationRequested`，`Measurements`、`ClippingPlanes`、`CaptureInspectionState()`、`ApplyInspectionState(...)`、`ExportSnapshotAsync(...)`、`VideraInspectionBundleService`，以及 `VideraNodeAnnotation` / `VideraWorldPointAnnotation`
- [SurfaceCharts.Core](modules/videra-surfacecharts-core.md)：`SurfaceChartView` 之外的领域契约、viewport / LOD、tile source 与 probe contract
- [SurfaceCharts.Rendering](../../src/Videra.SurfaceCharts.Rendering/README.md)：chart-local render-state orchestration 与 backend runtime seam
- [SurfaceCharts.Avalonia](modules/videra-surfacecharts-avalonia.md)：专用 `SurfaceChartView` 控件层，独立于 `VideraView`，包含 `RenderingStatus` / `RenderStatusChanged`、axis/legend overlays 与 probe overlay
- [SurfaceCharts.Processing](modules/videra-surfacecharts-processing.md)：离线 pyramid / cache 构建、persistent payload session、ordered batch reads 与 `SurfaceTileStatistics`
- [独立 Demo](../../samples/Videra.SurfaceCharts.Demo/README.md)：`Videra.SurfaceCharts.Demo`，作为 repository-only 的 support-ready chart 参考应用，保留 `Copy support summary` 复现路径，展示 source 切换、built-in interaction、probe workflow、`Try next: Analytics proof` 与 `Try next: Scatter proof` 以及 rendering path truth
- [架构说明](ARCHITECTURE.md)
- [贡献指南](CONTRIBUTING.md)
- [故障排查](troubleshooting.md)
- [原生宿主验证](native-validation.md)
- [文档导航](index.md)

## 模块文档

- [Videra.Core](modules/videra-core.md)
- [Videra.Import.Gltf](../../src/Videra.Import.Gltf/README.md)
- [Videra.Import.Obj](../../src/Videra.Import.Obj/README.md)
- [Videra.Avalonia](modules/videra-avalonia.md)
- [Videra.Platform.Windows](modules/platform-windows.md)
- [Videra.Platform.Linux](modules/platform-linux.md)
- [Videra.Platform.macOS](modules/platform-macos.md)
- [Videra.Demo](modules/demo.md)
- [Videra.SurfaceCharts.Core](modules/videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Rendering](../../src/Videra.SurfaceCharts.Rendering/README.md)
- [Videra.SurfaceCharts.Avalonia](modules/videra-surfacecharts-avalonia.md)
- [Videra.SurfaceCharts.Processing](modules/videra-surfacecharts-processing.md)
- [Videra.SurfaceCharts.Demo](../../samples/Videra.SurfaceCharts.Demo/README.md)

## 说明

默认公开入口现在以英文为主，英文版为准。中文文档保留为手动切换入口，便于中文读者快速查看项目定位、安装方式、验证方式和模块说明。

扩展入口的中文镜像集中在 [扩展合同](extensibility.md)。该页会把 `samples/Videra.ExtensibilitySample`、`disposed` 后注册调用的 `no-op` 语义，以及软件回退与 `BackendDiagnostics` / `FallbackReason` 的公开约定放在一起说明。

`host owns` `SelectionState`、`Annotations` 与 annotation state。

受控交互入口则以 [samples/Videra.InteractionSample](../../samples/Videra.InteractionSample/README.md) 为主：`host owns` `SelectionState`、`Annotations` 与 annotation state，内建模式是 `Navigate`、`Select`、`Annotate`、`Measure`，并通过 `SelectionRequested` / `AnnotationRequested` 报告 host 需要接管的选择与标注意图；选择保持 `object-level`，标注同时覆盖 object anchors 与 world-point anchors，并通过 `VideraNodeAnnotation` / `VideraWorldPointAnnotation` 表达；同时 `Measurements`、`ClippingPlanes`、`CaptureInspectionState()`、`ApplyInspectionState(...)`、`ExportSnapshotAsync(...)` 与 `VideraInspectionBundleService` 组成 viewer-first inspection workflow，overlay responsibilities split between `3D highlight/render state` and `2D label/feedback rendering`。

surface-chart 模块家族则以 `SurfaceChartView` 为中心，独立于 `VideraView`，并保持与 viewer 侧选择、标注和 camera 流程解耦。当前对外 truth 是：独立 Demo、built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + left-drag` focus zoom、hover 与 `Shift + left-click` pinned probe、可见 `RenderingStatus`，以及显式 `Interactive` / `Refine` 质量切换；`Try next: Analytics proof` 通过显式/non-uniform 坐标、独立 `ColorField` 与分析级 `pinned-probe` 工作流暴露分析语义，`Try next: Scatter proof` 则覆盖 scatter 路径。SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine``。对外交互诊断是 `InteractionQuality` + `InteractionQualityChanged`，状态为 `Interactive` / `Refine`。SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。公开 overlay 配置入口是 `SurfaceChartOverlayOptions` / `OverlayOptions`，overlay state 类型继续保持 internal。宿主拥有 `ISurfaceTileSource`、持久化的 `ViewState`、color-map 选择，以及 chart-local 产品 UI。`SurfaceChartView` 拥有 chart-local built-in 手势、tile scheduling/cache、overlay presentation、native-host/render-host orchestration，以及 `RenderingStatus` 投影。对外渲染 truth 是 `RenderingStatus` + `RenderStatusChanged`，字段包括 `ActiveBackend`、`IsReady`、`IsFallback`、`FallbackReason`、`UsesNativeSurface` 和 `ResidentTileCount`。
