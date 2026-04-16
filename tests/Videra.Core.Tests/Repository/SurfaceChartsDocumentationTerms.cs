namespace Videra.Core.Tests.Repository;

internal static class SurfaceChartsDocumentationTerms
{
    public const string SurfaceChartsOnboardingHeading =
        "## Surface Charts Onboarding";

    public const string SurfaceChartsFamilyBoundarySentence =
        "The surface-chart module family is a sibling product area, independent from `VideraView`.";

    public const string SurfaceChartsDemoSentence =
        "`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.";

    public const string SurfaceChartViewSentence =
        "The dedicated `SurfaceChartView` control lives in `Videra.SurfaceCharts.Avalonia`.";

    public const string SurfaceChartsRendererStatusSentence =
        "The shipped chart surface is `GPU-first` with explicit `software fallback`, and hosts can inspect `RenderingStatus` / `RenderStatusChanged`.";

    public const string SurfaceChartsViewStateSentence =
        "SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.";

    public const string SurfaceChartsInteractionSentence =
        "SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom on top of the `ViewState` runtime contract.";

    public const string SurfaceChartsInteractionQualitySentence =
        "The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.";

    public const string SurfaceChartsOverlayOptionsSentence =
        "Hosts can keep professional axis, grid, and legend behavior chart-local through `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.";

    public const string SurfaceChartsDemoProbeSentence =
        "hover readout and `Shift + LeftClick` pinned probes on the chart surface";

    public const string ChineseSurfaceChartsFamilyBoundarySentence =
        "surface-chart 模块家族与 `VideraView` 相互独立。";

    public const string ChineseSurfaceChartsTruthSentence =
        "当前对外 truth 是：独立 Demo、built-in `left-drag orbit` / `right-drag pan` / `wheel dolly` / `Ctrl + Left drag` focus zoom、hover 与 `Shift + LeftClick` pinned probe、可见 `RenderingStatus`，以及显式 `Interactive` / `Refine` 质量切换。";

    public const string ChineseSurfaceChartsViewStateSentence =
        "SurfaceChartView 现在以 `ViewState` 作为主 chart-view 契约，而 `Viewport` 只保留为兼容桥接。";

    public const string ChineseSurfaceChartsInteractionQualitySentence =
        "图表在交互过程中进入 `Interactive` 质量模式，并在输入停稳后回到 `Refine`。";

    public const string ChineseSurfaceChartsOverlayOptionsSentence =
        "SurfaceChartView 通过 chart-local `OverlayOptions` 提供 formatter、标题/单位覆盖、minor ticks、grid plane 与 axis-side 行为。";

    public const string ChineseAvaloniaRenderStatusSentence =
        "暴露宿主可见的 `RenderingStatus` / `RenderStatusChanged`";

    public const string ChineseAvaloniaProbeSentence =
        "提供 axis/legend overlays、hover readout 与 `Shift + LeftClick` pinned probe";

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
