using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public interface ISurfaceChartRenderBackend
{
    SurfaceChartRenderBackendKind Kind { get; }

    bool UsesNativeSurface { get; }

    SurfaceRenderScene? SoftwareScene { get; }

    SurfaceChartRenderSnapshot Render(SurfaceChartRenderInputs inputs);
}
