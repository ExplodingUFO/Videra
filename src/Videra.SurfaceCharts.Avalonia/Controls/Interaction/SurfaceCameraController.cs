using System.Numerics;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceCameraController
{
    public SurfaceCameraController(SurfaceViewport initialViewport)
        : this(CreateInitialViewState(initialViewport))
    {
    }

    public SurfaceCameraController(SurfaceViewState initialViewState)
    {
        CurrentViewState = initialViewState;
    }

    public SurfaceViewState CurrentViewState { get; private set; }

    public SurfaceViewport CurrentViewport => CurrentViewState.DataWindow.ToViewport();

    public SurfaceChartProjectionSettings ProjectionSettings => CurrentViewState.Camera.ToProjectionSettings();

    public void UpdateViewState(SurfaceViewState viewState)
    {
        CurrentViewState = viewState;
    }

    public void UpdateDataWindow(SurfaceDataWindow dataWindow)
    {
        CurrentViewState = new SurfaceViewState(dataWindow, CurrentViewState.Camera);
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        UpdateDataWindow(viewport.ToDataWindow());
    }

    public void UpdateProjectionSettings(SurfaceChartProjectionSettings projectionSettings)
    {
        var currentCamera = CurrentViewState.Camera;
        var nextCamera = new SurfaceCameraPose(
            currentCamera.Target,
            projectionSettings.YawDegrees,
            projectionSettings.PitchDegrees,
            currentCamera.Distance,
            currentCamera.FieldOfViewDegrees);
        CurrentViewState = new SurfaceViewState(CurrentViewState.DataWindow, nextCamera);
    }

    private static SurfaceViewState CreateInitialViewState(SurfaceViewport viewport)
    {
        var target = new Vector3(
            (float)(viewport.StartX + (viewport.Width * 0.5d)),
            0f,
            (float)(viewport.StartY + (viewport.Height * 0.5d)));
        var diagonal = Math.Sqrt((viewport.Width * viewport.Width) + (viewport.Height * viewport.Height));
        var camera = new SurfaceCameraPose(
            target,
            SurfaceCameraPose.DefaultYawDegrees,
            SurfaceCameraPose.DefaultPitchDegrees,
            Math.Max(diagonal, 1d),
            SurfaceCameraPose.DefaultFieldOfViewDegrees);

        return new SurfaceViewState(viewport.ToDataWindow(), camera);
    }
}
