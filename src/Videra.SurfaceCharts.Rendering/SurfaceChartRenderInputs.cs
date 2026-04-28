using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Rendering;

public sealed record SurfaceChartRenderInputs
{
    private SurfaceCameraFrame? _cameraFrame;

    public SurfaceMetadata? Metadata { get; init; }

    public IReadOnlyList<SurfaceTile> LoadedTiles { get; init; } = Array.Empty<SurfaceTile>();

    public SurfaceColorMap? ColorMap { get; init; }

    public SurfaceViewState ViewState { get; init; }

    public SurfaceCameraFrame? CameraFrame
    {
        get => _cameraFrame ?? CreateCompatibilityCameraFrame();
        init => _cameraFrame = value;
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
}
