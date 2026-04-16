using System.Numerics;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Rendering;

public sealed record SurfaceChartRenderInputs
{
    private static readonly SurfaceViewport DefaultViewport = new(0d, 0d, 1d, 1d);
    private SurfaceViewport? _viewport;
    private SurfaceChartProjectionSettings? _projectionSettings;
    private SurfaceViewState? _viewState;
    private SurfaceCameraFrame? _cameraFrame;

    public SurfaceMetadata? Metadata { get; init; }

    public IReadOnlyList<SurfaceTile> LoadedTiles { get; init; } = Array.Empty<SurfaceTile>();

    public SurfaceColorMap? ColorMap { get; init; }

    public SurfaceViewState ViewState
    {
        get => _viewState ?? CreateCompatibilityViewState(_viewport ?? DefaultViewport, _projectionSettings ?? SurfaceChartProjectionSettings.Default);
        init => _viewState = value;
    }

    public SurfaceCameraFrame? CameraFrame
    {
        get => _cameraFrame ?? CreateCompatibilityCameraFrame();
        init => _cameraFrame = value;
    }

    public SurfaceViewport Viewport
    {
        get => _viewState?.ToViewport() ?? _viewport ?? DefaultViewport;
        init => _viewport = value;
    }

    public SurfaceChartProjectionSettings ProjectionSettings
    {
        get => CameraFrame?.ProjectionSettings ?? _projectionSettings ?? _viewState?.Camera.ToProjectionSettings() ?? SurfaceChartProjectionSettings.Default;
        init => _projectionSettings = value;
    }

    public double ViewWidth { get; init; }

    public double ViewHeight { get; init; }

    public IntPtr NativeHandle { get; init; }

    public bool HandleBound { get; init; }

    public float RenderScale { get; init; } = 1f;

    private SurfaceCameraFrame? CreateCompatibilityCameraFrame()
    {
        if (Metadata is null || ViewWidth <= 0d || ViewHeight <= 0d || !float.IsFinite(RenderScale) || RenderScale <= 0f)
        {
            return null;
        }

        return SurfaceProjectionMath.CreateCameraFrame(Metadata, ViewState, ViewWidth, ViewHeight, RenderScale);
    }

    private static SurfaceViewState CreateCompatibilityViewState(
        SurfaceViewport viewport,
        SurfaceChartProjectionSettings projectionSettings)
    {
        var target = new Vector3(
            (float)(viewport.StartX + (viewport.Width * 0.5d)),
            0f,
            (float)(viewport.StartY + (viewport.Height * 0.5d)));
        var diagonal = Math.Sqrt((viewport.Width * viewport.Width) + (viewport.Height * viewport.Height));
        var camera = new SurfaceCameraPose(
            target,
            projectionSettings.YawDegrees,
            projectionSettings.PitchDegrees,
            Math.Max(diagonal, 1d),
            SurfaceCameraPose.DefaultFieldOfViewDegrees);
        return new SurfaceViewState(viewport.ToDataWindow(), camera);
    }
}
