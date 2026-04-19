namespace Videra.Core.Tests.Repository;

internal static class SurfaceChartsDocumentationTerms
{
    public const string SurfaceChartsOnboardingHeading =
        "## Surface Charts Onboarding";

    public const string SurfaceChartsFamilyBoundarySentence =
        "The surface-chart module family is a sibling product area, independent from `VideraView`.";

    public const string SurfaceChartsFirstChartSentence =
        "For the canonical first-chart story, use [Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md) as the current public chart reference and `SurfaceChartView` in `Videra.SurfaceCharts.Avalonia` as the primary chart control entrypoint.";

    public const string SurfaceChartsDemoSentence =
        "`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.";

    public const string SurfaceChartViewSentence =
        "The dedicated `SurfaceChartView` control remains the primary chart control entrypoint in `Videra.SurfaceCharts.Avalonia`.";

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
        "The `Videra.SurfaceCharts.*` family stays source-first and is not part of the current public package promise.";

    public const string ChineseSurfaceChartsFamilyBoundarySentence =
        "surface-chart 模块家族与 `VideraView` 相互独立。";

    public const string ChineseSurfaceChartsFirstChartSentence =
        "默认 first-chart 入口请先把 [Videra.SurfaceCharts.Demo](../../samples/Videra.SurfaceCharts.Demo/README.md) 当作当前公开 chart reference，并把 `Videra.SurfaceCharts.Avalonia` 中的 `SurfaceChartView` 视为主 chart control entrypoint。";

    public const string ChineseSurfaceChartsTruthSentence =
        "当前对外 truth 是：独立 Demo、built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + left-drag` focus zoom、hover 与 `Shift + left-click` pinned probe、可见 `RenderingStatus`，以及显式 `Interactive` / `Refine` 质量切换。";

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
        "`Videra.SurfaceCharts.*` 仍保持 source-first，不在当前公开包承诺内。";

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
        "还没有完成坐标轴、刻度、标签与图例系统",
        "还没有交付完成态的 built-in orbit / pan / dolly 工作流",
        "由宿主驱动的 overview/detail 视口切换"
    ];
}
