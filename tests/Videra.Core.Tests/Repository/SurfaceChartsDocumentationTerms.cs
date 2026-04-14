namespace Videra.Core.Tests.Repository;

internal static class SurfaceChartsDocumentationTerms
{
    public const string SurfaceChartsFamilyBoundarySentence =
        "The surface-chart module family is a sibling product area, independent from `VideraView`.";

    public const string SurfaceChartsDemoSentence =
        "`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.";

    public const string SurfaceChartViewSentence =
        "The dedicated `SurfaceChartView` control lives in `Videra.SurfaceCharts.Avalonia`.";

    public const string SurfaceChartsAvaloniaBoundarySentence =
        "The surface-chart family remains independent from `VideraView`.";

    public const string SurfaceChartViewStateSentence =
        "`ViewState` is the primary public view contract for `SurfaceChartView`, while `Viewport` remains a compatibility bridge.";

    public const string ChineseSurfaceChartsFamilyBoundarySentence =
        "surface-chart 模块家族与 `VideraView` 相互独立。";

    public const string ChineseSurfaceChartViewStateSentence =
        "`ViewState` 是 `SurfaceChartView` 的主视图状态契约，`Viewport` 仅作为兼容桥接保留。";

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
        "docs/zh-CN/modules/videra-surfacecharts-avalonia.md"
    ];

    public static readonly string[] ExpectedVerifyTargets =
    [
        "samples/Videra.Demo/Videra.Demo.csproj",
        "samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj"
    ];
}
