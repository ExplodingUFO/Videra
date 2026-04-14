using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering.Software;

public sealed class SurfaceChartSoftwareRenderBackend : ISurfaceChartRenderBackend
{
    private readonly SurfaceRenderer _renderer = new();

    public SurfaceChartRenderBackendKind Kind => SurfaceChartRenderBackendKind.Software;

    public bool UsesNativeSurface => false;

    public SurfaceRenderScene? SoftwareScene { get; private set; }

    public SurfaceChartRenderSnapshot Render(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var residentTileCount = inputs.LoadedTiles.Count;
        if (inputs.Metadata is null
            || inputs.ColorMap is null
            || residentTileCount == 0
            || inputs.ViewWidth <= 0d
            || inputs.ViewHeight <= 0d)
        {
            SoftwareScene = null;
            return new SurfaceChartRenderSnapshot
            {
                ActiveBackend = Kind,
                IsReady = false,
                IsFallback = false,
                FallbackReason = null,
                UsesNativeSurface = UsesNativeSurface,
                ResidentTileCount = 0,
            };
        }

        SoftwareScene = _renderer.BuildScene(inputs.Metadata, inputs.LoadedTiles, inputs.ColorMap);
        return new SurfaceChartRenderSnapshot
        {
            ActiveBackend = Kind,
            IsReady = true,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = UsesNativeSurface,
            ResidentTileCount = residentTileCount,
        };
    }
}
