namespace Videra.Core.Tests.Repository;

internal static class SurfaceChartsDocumentationTerms
{
    public const string SurfaceChartsOnboardingHeading =
        "## Surface Charts Onboarding";

    public const string SurfaceChartsFamilyBoundarySentence =
        "The surface-chart module family is a sibling product area, independent from `VideraView`.";

    public const string SurfaceChartsFirstChartSentence =
        "For the canonical SurfaceCharts story, start from `Videra.SurfaceCharts.Avalonia`, add `Videra.SurfaceCharts.Processing` only for the surface/cache-backed path, and use [Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md) as the repository reference app for the same path.";

    public const string SurfaceChartsDemoFirstChartSentence =
        "For the canonical surface/cache-backed chart story, start from `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing`, and use this demo as the repository reference app for that same path. `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` in `Videra.SurfaceCharts.Avalonia` are the current chart control entrypoints, and this demo includes a repo-owned `Try next: Scatter proof` path.";

    public const string SurfaceChartsDemoSentence =
        "`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.";

    public const string SurfaceChartViewSentence =
        "The dedicated `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` controls remain the public chart entrypoints in `Videra.SurfaceCharts.Avalonia`.";

    public const string SurfaceChartsRendererStatusSentence =
        "The shipped chart surface is `GPU-first` with explicit `software fallback`, and hosts can inspect `RenderingStatus` / `RenderStatusChanged`.";

    public const string SurfaceChartsViewStateSentence =
        "SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.";

    public const string SurfaceChartsInteractionSentence =
        "SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, `Ctrl + left-drag` focus zoom, and `Shift + left-click` pinned probe on top of the `ViewState` runtime contract.";

    public const string SurfaceChartsInteractionQualitySentence =
        "The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.";

    public const string SurfaceChartsInteractionDiagnosticsSentence =
        "The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.";

    public const string SurfaceChartsOverlayOptionsSentence =
        "Hosts can keep professional axis, grid, and legend behavior chart-local through `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.";

    public const string SurfaceChartsOverlayBoundarySentence =
        "The public overlay configuration seam is `SurfaceChartOverlayOptions` through `OverlayOptions`; overlay state types remain internal.";

    public const string SurfaceChartsOwnershipSentence =
        "Hosts own `ISurfaceTileSource`, persisted `ViewState`, color-map selection, and chart-local product UI.";

    public const string SurfaceChartControlOwnershipSentence =
        "`SurfaceChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection.";

    public const string SurfaceChartsRenderingStatusFieldsSentence =
        "The public rendering truth is `RenderingStatus` + `RenderStatusChanged` with `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, and `ResidentTileCount`.";

    public const string SurfaceChartsSourceFirstSentence =
        "The `Videra.SurfaceCharts.*` family is part of the current public package promise, while `Videra.SurfaceCharts.Demo` remains repository-only.";

    public const string ChineseSurfaceChartsFamilyBoundarySentence =
        "surface-chart 模块家族与 `VideraView` 相互独立。";

    public const string ChineseSurfaceChartsFirstChartSentence =
        "当前公开 chart 入口是 `Videra.SurfaceCharts.Avalonia`，而 `Videra.SurfaceCharts.Processing` 只在 surface/cache-backed 路径需要；[Videra.SurfaceCharts.Demo](../../samples/Videra.SurfaceCharts.Demo/README.md) 则作为同一路径的 repository reference app。";

    public const string ChineseSurfaceChartsTruthSentence =
        "当前对外 truth 是：独立 Demo、built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + left-drag` focus zoom、hover 与 `Shift + left-click` pinned probe、可见 `RenderingStatus`，以及显式 `Interactive` / `Refine` 质量切换；scatter 路径通过 repo-owned `Try next: Scatter proof` 暴露。";

    public const string ChineseSurfaceChartsViewStateSentence =
        "SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。";

    public const string ChineseSurfaceChartsInteractionQualitySentence =
        "图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。";

    public const string ChineseSurfaceChartsInteractionDiagnosticsSentence =
        "对外交互诊断是 `InteractionQuality` + `InteractionQualityChanged`，状态为 `Interactive` / `Refine`。";

    public const string ChineseSurfaceChartsOverlayOptionsSentence =
        "SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。";

    public const string ChineseSurfaceChartsOverlayBoundarySentence =
        "公开 overlay 配置入口是 `SurfaceChartOverlayOptions` / `OverlayOptions`，overlay state 类型继续保持 internal。";

    public const string ChineseSurfaceChartsOwnershipSentence =
        "宿主拥有 `ISurfaceTileSource`、持久化的 `ViewState`、color-map 选择，以及 chart-local 产品 UI。";

    public const string ChineseSurfaceChartControlOwnershipSentence =
        "`SurfaceChartView` 拥有 chart-local built-in 手势、tile scheduling/cache、overlay presentation、native-host/render-host orchestration，以及 `RenderingStatus` 投影。";

    public const string ChineseSurfaceChartsRenderingStatusFieldsSentence =
        "对外渲染 truth 是 `RenderingStatus` + `RenderStatusChanged`，字段包括 `ActiveBackend`、`IsReady`、`IsFallback`、`FallbackReason`、`UsesNativeSurface` 和 `ResidentTileCount`。";

    public const string ChineseAvaloniaRenderStatusSentence =
        "暴露宿主可见的 `RenderingStatus` / `RenderStatusChanged`";

    public const string ChineseAvaloniaProbeSentence =
        "提供 axis/legend overlays、hover readout 与 `Shift + left-click` pinned probe";

    public const string ChineseSurfaceChartsSourceFirstSentence =
        "`Videra.SurfaceCharts.*` 已经进入当前公开包承诺，`Videra.SurfaceCharts.Demo` 继续保持 repository-only。";

    public const string ChineseProcessingStatisticsSentence =
        "通过 `SurfaceTileStatistics` 保留 reduced tile 的 source-region truth";

    public const string Phase13HistoricalRecoverySentence =
        "The current v1.2 milestone no longer treats that gap as active because Phase 19 later recovered `VIEW-01`, `VIEW-02`, and `VIEW-03`.";

    public const string Phase14HistoricalRecoverySentence =
        "The current v1.2 milestone no longer treats that gap as active because Phase 20 later recovered `INT-01`, `INT-02`, `INT-03`, and `INT-04`.";

    public const string Phase18HistoricalRecoverySentence =
        "Historical note: when Phase 18 was verified on 2026-04-14, the remaining chart limitation was still the host-driven overview/detail workflow that was later recovered by Phase 20.";

    public const string Phase19HistoricalRecoverySentence =
        "Historical note: at verification time the remaining milestone gap was isolated to Phase 20";

    public static readonly string[] ExpectedModuleReadmeTerms =
    [
        "Videra.SurfaceCharts.Core",
        "Videra.SurfaceCharts.Rendering",
        "Videra.SurfaceCharts.Avalonia",
        "Videra.SurfaceCharts.Processing",
        "Videra.SurfaceCharts.Demo",
        "SurfaceChartView"
    ];

    public static readonly string[] GuardedSurfaceChartsEntryPointPaths =
    [
        "README.md",
        "samples/Videra.SurfaceCharts.Demo/README.md",
        "docs/zh-CN/README.md",
        "docs/zh-CN/modules/videra-surfacecharts-avalonia.md"
    ];

    public static readonly string[] SurfaceChartsFirstChartTokens =
    [
        "canonical SurfaceCharts story",
        "`Videra.SurfaceCharts.Avalonia`",
        "`Videra.SurfaceCharts.Processing`",
        "`Videra.SurfaceCharts.Demo`"
    ];

    public static readonly string[] SurfaceChartsFamilyBoundaryTokens =
    [
        "surface-chart module family",
        "sibling",
        "independent from `VideraView`"
    ];

    public static readonly string[] SurfaceChartsDemoEntryTokens =
    [
        "`Videra.SurfaceCharts.Demo`",
        "independent demo application",
        "surface-chart module family"
    ];

    public static readonly string[] SurfaceChartViewEntryTokens =
    [
        "`SurfaceChartView`",
        "`WaterfallChartView`",
        "`ScatterChartView`",
        "public chart entrypoints",
        "`Videra.SurfaceCharts.Avalonia`"
    ];

    public static readonly string[] SurfaceChartsStartHereTokens =
    [
        "`Start here: In-memory first chart`",
        "`Explore next: Cache-backed streaming`",
        "baseline",
        "first chart"
    ];

    public static readonly string[] SurfaceChartsRendererStatusTokens =
    [
        "`GPU-first`",
        "`software fallback`",
        "`RenderingStatus`",
        "`RenderStatusChanged`"
    ];

    public static readonly string[] SurfaceChartsRendererBoundaryTokens =
    [
        "`SurfaceChartView`",
        "chart-local renderer seam",
        "`VideraView`"
    ];

    public static readonly string[] SurfaceChartsGpuFallbackTokens =
    [
        "`GPU-first`",
        "`software fallback`",
        "unsupported or fallback-triggering environments"
    ];

    public static readonly string[] SurfaceChartsDemoGpuFallbackTokens =
    [
        "`GPU-first`",
        "`software fallback`",
        "`SurfaceChartView`",
        "native-host or GPU initialization"
    ];

    public static readonly string[] SurfaceChartsViewStateTokens =
    [
        "SurfaceChartView",
        "`ViewState`",
        "`Viewport`",
        "compatibility bridge"
    ];

    public static readonly string[] SurfaceChartsInteractionTokens =
    [
        "`left-drag orbit`",
        "`right-drag pan`",
        "`wheel dolly`",
        "`Ctrl + left-drag`",
        "`Shift + left-click`",
        "`ViewState`"
    ];

    public static readonly string[] SurfaceChartsInteractionQualityTokens =
    [
        "`Interactive`",
        "`Refine`",
        "during motion",
        "input settles"
    ];

    public static readonly string[] SurfaceChartsDemoFirstChartTokens =
    [
        "canonical surface/cache-backed chart story",
        "`Videra.SurfaceCharts.Avalonia`",
        "`Videra.SurfaceCharts.Processing`",
        "repository reference app",
        "`SurfaceChartView`",
        "`WaterfallChartView`",
        "`ScatterChartView`",
        "`Try next: Scatter proof`"
    ];

    public static readonly string[] SurfaceChartsTriFamilyControlTokens =
    [
        "`SurfaceChartView`",
        "`ScatterChartView`",
        "`WaterfallChartView`"
    ];

    public static readonly string[] SurfaceChartsInteractionDiagnosticsTokens =
    [
        "`InteractionQuality`",
        "`InteractionQualityChanged`",
        "`Interactive`",
        "`Refine`"
    ];

    public static readonly string[] SurfaceChartsOverlayBoundaryTokens =
    [
        "`SurfaceChartOverlayOptions`",
        "`OverlayOptions`",
        "internal"
    ];

    public static readonly string[] SurfaceChartsOverlayOptionsTokens =
    [
        "`OverlayOptions`",
        "formatter",
        "title/unit override",
        "minor ticks",
        "grid plane",
        "axis-side selection"
    ];

    public static readonly string[] SurfaceChartsOwnershipTokens =
    [
        "`ISurfaceTileSource`",
        "persisted `ViewState`",
        "color-map selection",
        "chart-local product UI"
    ];

    public static readonly string[] SurfaceChartControlOwnershipTokens =
    [
        "`SurfaceChartView` owns",
        "tile scheduling/cache",
        "overlay presentation",
        "native-host/render-host orchestration",
        "`RenderingStatus` projection"
    ];

    public static readonly string[] SurfaceChartsRenderingStatusFieldTokens =
    [
        "`RenderingStatus`",
        "`RenderStatusChanged`",
        "`ActiveBackend`",
        "`IsReady`",
        "`IsFallback`",
        "`FallbackReason`",
        "`UsesNativeSurface`",
        "`ResidentTileCount`"
    ];

    public static readonly string[] SurfaceChartsSourceFirstTokens =
    [
        "`Videra.SurfaceCharts.*`",
        "public package promise",
        "`Videra.SurfaceCharts.Demo`",
        "repository-only"
    ];

    public static readonly string[] SurfaceChartsScatterProofTokens =
    [
        "`Try next: Scatter proof`",
        "repo-owned scatter proof path",
        "`ScatterChartView`"
    ];

    public static readonly string[] SurfaceChartsScalarCompatibilityTokens =
    [
        "`SurfaceMatrix`",
        "source-first",
        "regular-grid",
        "`SurfaceScalarField`",
        "`ColorField`",
        "`SurfaceMask`"
    ];

    public static readonly string[] SurfaceChartsAvaloniaScalarCompatibilityTokens =
    [
        "`SurfaceMatrix`",
        "source-first",
        "regular-grid entrypoint",
        "same `SurfaceChartView` shell",
        "`SurfaceScalarField`",
        "`ColorField`",
        "`SurfaceMask`"
    ];

    public static readonly string[] SurfaceChartsAvaloniaReadmeContractTokens =
    [
        "`InteractionQuality`",
        "`InteractionQualityChanged`",
        "`Interactive`",
        "`Refine`",
        "`SurfaceChartOverlayOptions`",
        "`OverlayOptions`",
        "internal",
        "`ISurfaceTileSource`",
        "persisted `ViewState`",
        "color-map selection",
        "chart-local product UI",
        "`SurfaceChartView` owns",
        "tile scheduling/cache",
        "overlay presentation",
        "native-host/render-host orchestration",
        "`RenderingStatus` projection",
        "`ActiveBackend`",
        "`IsReady`",
        "`IsFallback`",
        "`FallbackReason`",
        "`UsesNativeSurface`",
        "`ResidentTileCount`"
    ];

    public static readonly string[] SurfaceChartViewTypeXmlDocTokens =
    [
        "persisted <see cref=\"ViewState\"/>",
        "built-in orbit, pan, dolly, and focus interaction"
    ];

    public static readonly string[] SurfaceChartRenderingStatusXmlDocTokens =
    [
        "chart-local rendering backend",
        "fallback state",
        "native-surface usage",
        "<c>ActiveBackend</c>",
        "<c>IsReady</c>",
        "<c>IsFallback</c>",
        "<c>FallbackReason</c>",
        "<c>UsesNativeSurface</c>",
        "<c>ResidentTileCount</c>"
    ];

    public static readonly string[] SurfaceChartViewportXmlDocTokens =
    [
        "compatibility bridge",
        "authoritative <see cref=\"ViewState\"/>"
    ];

    public static readonly string[] SurfaceChartOverlayOptionsXmlDocTokens =
    [
        "chart-local overlay layout and formatting options",
        "formatter",
        "title/unit override",
        "minor ticks",
        "grid plane",
        "axis-side selection"
    ];

    public static readonly string[] SurfaceChartInteractionQualityXmlDocTokens =
    [
        "diagnostic interaction-quality mode",
        "<c>Interactive</c>",
        "<c>Refine</c>"
    ];

    public static readonly string[] ChineseSurfaceChartsFirstChartTokens =
    [
        "surface/cache-backed",
        "Videra.SurfaceCharts.Avalonia",
        "Videra.SurfaceCharts.Processing",
        "Videra.SurfaceCharts.Demo"
    ];

    public static readonly string[] ChineseSurfaceChartsFamilyBoundaryTokens =
    [
        "surface-chart",
        "`VideraView`",
        "相互独立"
    ];

    public static readonly string[] ChineseSurfaceChartsStartHereTokens =
    [
        "`Start here: In-memory first chart`",
        "`Explore next: Cache-backed streaming`",
        "first chart"
    ];

    public static readonly string[] ChineseSurfaceChartsTruthTokens =
    [
        "独立 Demo",
        "`left-drag orbit`",
        "`right-drag pan`",
        "`wheel dolly`",
        "`Ctrl + left-drag`",
        "`Shift + left-click`",
        "`RenderingStatus`",
        "`Interactive`",
        "`Refine`",
        "`Try next: Scatter proof`"
    ];

    public static readonly string[] ChineseSurfaceChartsScatterProofTokens =
    [
        "`Try next: Scatter proof`",
        "repo-owned",
        "scatter 路径"
    ];

    public static readonly string[] ChineseSurfaceChartsViewStateTokens =
    [
        "SurfaceChartView",
        "`ViewState`",
        "`Viewport`",
        "兼容桥接"
    ];

    public static readonly string[] ChineseSurfaceChartsInteractionQualityTokens =
    [
        "`Interactive`",
        "`Refine`",
        "交互过程中",
        "输入停稳后"
    ];

    public static readonly string[] ChineseSurfaceChartsInteractionDiagnosticsTokens =
    [
        "`InteractionQuality`",
        "`InteractionQualityChanged`",
        "`Interactive`",
        "`Refine`"
    ];

    public static readonly string[] ChineseSurfaceChartsOverlayBoundaryTokens =
    [
        "`SurfaceChartOverlayOptions`",
        "`OverlayOptions`",
        "internal"
    ];

    public static readonly string[] ChineseSurfaceChartsOverlayOptionsTokens =
    [
        "SurfaceChartView",
        "`OverlayOptions`",
        "formatter",
        "标题/单位覆盖",
        "minor ticks",
        "grid plane",
        "axis-side"
    ];

    public static readonly string[] ChineseAvaloniaRenderStatusTokens =
    [
        "`RenderingStatus`",
        "`RenderStatusChanged`"
    ];

    public static readonly string[] ChineseAvaloniaProbeTokens =
    [
        "axis/legend overlays",
        "hover readout",
        "`Shift + left-click`",
        "pinned probe"
    ];

    public static readonly string[] ChineseSurfaceChartsOwnershipTokens =
    [
        "`ISurfaceTileSource`",
        "`ViewState`",
        "color-map",
        "chart-local 产品 UI"
    ];

    public static readonly string[] ChineseSurfaceChartControlOwnershipTokens =
    [
        "`SurfaceChartView`",
        "chart-local built-in 手势",
        "tile scheduling/cache",
        "overlay presentation",
        "`RenderingStatus` 投影"
    ];

    public static readonly string[] ChineseSurfaceChartsRenderingStatusFieldTokens =
    [
        "`RenderingStatus`",
        "`RenderStatusChanged`",
        "`ActiveBackend`",
        "`IsReady`",
        "`IsFallback`",
        "`FallbackReason`",
        "`UsesNativeSurface`",
        "`ResidentTileCount`"
    ];

    public static readonly string[] ChineseSurfaceChartsSourceFirstTokens =
    [
        "`Videra.SurfaceCharts.*`",
        "公开",
        "`Videra.SurfaceCharts.Demo`",
        "repository-only"
    ];

    public static readonly string[] ChineseSurfaceChartsScalarCompatibilityTokens =
    [
        "`SurfaceMatrix`",
        "source-first",
        "regular-grid",
        "`SurfaceScalarField`",
        "`ColorField`",
        "`SurfaceMask`"
    ];

    public static readonly string[] ExpectedChineseModulePages =
    [
        "docs/zh-CN/modules/videra-surfacecharts-core.md",
        "docs/zh-CN/modules/videra-surfacecharts-avalonia.md",
        "docs/zh-CN/modules/videra-surfacecharts-processing.md"
    ];

    public static readonly string[] ExpectedVerifyTargets =
    [
        "samples/Videra.Demo/Videra.Demo.csproj",
        "samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj"
    ];

    public static readonly (string Path, string[] Requirements)[] RecoveredSummaryRequirementMetadata =
    [
        (".planning/phases/19-surfacechart-runtime-and-view-state-recovery/19-01-SUMMARY.md", ["VIEW-01"]),
        (".planning/phases/19-surfacechart-runtime-and-view-state-recovery/19-02-SUMMARY.md", ["VIEW-03"]),
        (".planning/phases/19-surfacechart-runtime-and-view-state-recovery/19-03-SUMMARY.md", ["VIEW-01", "VIEW-02"]),
        (".planning/phases/20-built-in-interaction-and-camera-workflow-recovery/20-01-SUMMARY.md", ["INT-01", "INT-02"]),
        (".planning/phases/20-built-in-interaction-and-camera-workflow-recovery/20-02-SUMMARY.md", ["INT-02", "INT-03"]),
        (".planning/phases/20-built-in-interaction-and-camera-workflow-recovery/20-03-SUMMARY.md", ["INT-04"])
    ];

    public static readonly string[] StaleSurfaceChartsTerms =
    [
        "axis, tick, label, and legend presentation is not complete yet",
        "full built-in mouse orbit / pan / zoom interaction is not complete yet",
        "built-in mouse orbit / pan / zoom is not complete",
        "the demo still uses host-driven `overview/detail` presets",
        "host-driven `overview/detail` presets",
        "this demo does not expose a scatter path",
        "does not expose a scatter path",
        "demo evidence paths do not include a scatter UI",
        "demo path set does not include a scatter UI",
        "do not claim the demo exposes a scatter path",
        "还没有完成坐标轴、刻度、标签与图例系统",
        "还没有交付完成态的 built-in orbit / pan / dolly 工作流",
        "由宿主驱动的 overview/detail 视口切换",
        "Scatter 路径属于已发布控件层，但不在当前 Demo UI 中"
    ];
}
