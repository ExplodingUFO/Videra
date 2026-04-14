using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed record SurfaceChartRenderInputs
{
    public SurfaceMetadata? Metadata { get; init; }

    public IReadOnlyList<SurfaceTile> LoadedTiles { get; init; } = Array.Empty<SurfaceTile>();

    public SurfaceColorMap? ColorMap { get; init; }

    public SurfaceViewport Viewport { get; init; }

    public SurfaceChartProjectionSettings ProjectionSettings { get; init; } = SurfaceChartProjectionSettings.Default;

    public double ViewWidth { get; init; }

    public double ViewHeight { get; init; }

    public IntPtr NativeHandle { get; init; }

    public bool HandleBound { get; init; }

    public float RenderScale { get; init; } = 1f;
}
