using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceCameraController
{
    public SurfaceCameraController(SurfaceViewport initialViewport)
    {
        CurrentViewport = initialViewport;
    }

    public SurfaceViewport CurrentViewport { get; private set; }

    public SurfaceChartProjectionSettings ProjectionSettings { get; private set; } = SurfaceChartProjectionSettings.Default;

    public void UpdateViewport(SurfaceViewport viewport)
    {
        CurrentViewport = viewport;
    }

    public void UpdateProjectionSettings(SurfaceChartProjectionSettings projectionSettings)
    {
        ProjectionSettings = projectionSettings;
    }
}
