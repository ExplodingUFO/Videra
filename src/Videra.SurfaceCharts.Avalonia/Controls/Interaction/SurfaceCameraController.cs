using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceCameraController
{
    public SurfaceCameraController(SurfaceViewport initialViewport)
    {
        CurrentViewport = initialViewport;
    }

    public SurfaceViewport CurrentViewport { get; private set; }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        CurrentViewport = viewport;
    }
}
