using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering.Software;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartRenderHost
{
    private readonly ISurfaceChartRenderBackend _softwareBackend;

    public SurfaceChartRenderHost(ISurfaceChartRenderBackend? softwareBackend = null)
    {
        _softwareBackend = softwareBackend ?? new SurfaceChartSoftwareRenderBackend();
        Inputs = new SurfaceChartRenderInputs();
        Snapshot = new SurfaceChartRenderSnapshot
        {
            ActiveBackend = SurfaceChartRenderBackendKind.Software,
            IsReady = false,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = false,
            ResidentTileCount = 0,
        };
    }

    public SurfaceChartRenderInputs Inputs { get; private set; }

    public SurfaceChartRenderSnapshot Snapshot { get; private set; }

    public SurfaceRenderScene? SoftwareScene => _softwareBackend.SoftwareScene;

    public SurfaceChartRenderSnapshot UpdateInputs(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        Inputs = inputs;
        Snapshot = _softwareBackend.Render(inputs);
        return Snapshot;
    }
}
