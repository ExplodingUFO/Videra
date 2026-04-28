using System.Numerics;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceCameraController
{
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
        CurrentViewState = new SurfaceViewState(
            dataWindow,
            CurrentViewState.Camera,
            CurrentViewState.DisplaySpace);
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
        CurrentViewState = new SurfaceViewState(
            CurrentViewState.DataWindow,
            nextCamera,
            CurrentViewState.DisplaySpace);
    }

}
