using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia proof control for the thin direct scatter path over immutable scatter chart data.
/// </summary>
public sealed partial class ScatterChartView : Control
{
    private static readonly SurfacePlotBounds DefaultBounds = new(
        new Vector3(-1f, -1f, -1f),
        new Vector3(1f, 1f, 1f));

    private static readonly SurfaceCameraPose DefaultCamera = CreateDefaultCamera(DefaultBounds);

    private SurfacePlotBounds _dataBounds = DefaultBounds;
    private SurfaceCameraPose _camera = DefaultCamera;
    private bool _isInteracting;

    /// <summary>
    /// Raised when the chart-local readiness surface changes.
    /// </summary>
    public event EventHandler? RenderStatusChanged;

    /// <summary>
    /// Identifies the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<ScatterChartData?> SourceProperty =
        AvaloniaProperty.Register<ScatterChartView, ScatterChartData?>(nameof(Source));

    static ScatterChartView()
    {
        SourceProperty.Changed.AddClassHandler<ScatterChartView>(
            static (view, args) => view.OnSourceChanged((ScatterChartData?)args.NewValue));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterChartView"/> class.
    /// </summary>
    public ScatterChartView()
    {
        ClipToBounds = true;
        Focusable = true;
        RenderingStatus = CreateRenderingStatus();
    }

    /// <summary>
    /// Gets or sets the immutable scatter dataset shown by the view.
    /// </summary>
    public ScatterChartData? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets the last scene built from <see cref="Source"/> through the scatter render seam.
    /// </summary>
    internal ScatterRenderScene? RenderScene { get; private set; }

    /// <summary>
    /// Gets the current chart-local render status.
    /// </summary>
    public ScatterChartRenderingStatus RenderingStatus { get; private set; }

    /// <summary>
    /// Reframes the camera to the current data bounds while preserving the current orbit.
    /// </summary>
    public void FitToData()
    {
        if (Source is null || Source.PointCount == 0)
        {
            return;
        }

        _camera = CreateFitCamera(_dataBounds, _camera);
        InvalidateChart();
    }

    /// <summary>
    /// Restores the default orbit over the current data bounds.
    /// </summary>
    public void ResetCamera()
    {
        if (Source is null || Source.PointCount == 0)
        {
            return;
        }

        _camera = CreateDefaultCamera(_dataBounds);
        InvalidateChart();
    }

    private void OnSourceChanged(ScatterChartData? source)
    {
        RenderScene = source is null ? null : ScatterRenderer.BuildScene(source);
        _dataBounds = source is null || source.PointCount == 0
            ? DefaultBounds
            : CreateDataBounds(source);
        _camera = source is null || source.PointCount == 0
            ? DefaultCamera
            : CreateDefaultCamera(_dataBounds);
        InvalidateChart();
    }

    internal void SetInteracting(bool isInteracting)
    {
        if (_isInteracting == isInteracting)
        {
            return;
        }

        _isInteracting = isInteracting;
        UpdateRenderingStatus();
    }

    private void InvalidateChart()
    {
        UpdateRenderingStatus();
        InvalidateVisual();
    }

    private void UpdateRenderingStatus()
    {
        var nextStatus = CreateRenderingStatus();
        if (RenderingStatus == nextStatus)
        {
            return;
        }

        RenderingStatus = nextStatus;
        RenderStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private ScatterChartRenderingStatus CreateRenderingStatus()
    {
        var source = Source;
        var viewSize = Bounds.Size;
        return new ScatterChartRenderingStatus
        {
            HasSource = source is not null,
            BackendKind = SurfaceChartRenderBackendKind.Software,
            IsReady = source is not null
                && source.PointCount > 0
                && RenderScene is not null
                && viewSize.Width > 0d
                && viewSize.Height > 0d,
            IsInteracting = _isInteracting,
            SeriesCount = source?.SeriesCount ?? 0,
            PointCount = source?.PointCount ?? 0,
            ColumnarSeriesCount = source?.ColumnarSeriesCount ?? 0,
            ColumnarPointCount = source?.ColumnarPointCount ?? 0,
            PickablePointCount = source?.PickablePointCount ?? 0,
            StreamingAppendBatchCount = source?.StreamingAppendBatchCount ?? 0,
            StreamingReplaceBatchCount = source?.StreamingReplaceBatchCount ?? 0,
            StreamingDroppedPointCount = source?.StreamingDroppedPointCount ?? 0,
            LastStreamingDroppedPointCount = source?.LastStreamingDroppedPointCount ?? 0,
            ConfiguredFifoCapacity = source?.ConfiguredFifoCapacity ?? 0,
            ViewSize = viewSize,
            CameraTarget = _camera.Target,
            CameraDistance = _camera.Distance,
        };
    }

    private static SurfacePlotBounds CreateDataBounds(ScatterChartData source)
    {
        var hasPoint = false;
        double minX = 0d;
        double maxX = 0d;
        double minY = 0d;
        double maxY = 0d;
        double minZ = 0d;
        double maxZ = 0d;

        foreach (var series in source.Series)
        {
            foreach (var point in series.Points)
            {
                if (!hasPoint)
                {
                    minX = maxX = point.Horizontal;
                    minY = maxY = point.Value;
                    minZ = maxZ = point.Depth;
                    hasPoint = true;
                    continue;
                }

                minX = Math.Min(minX, point.Horizontal);
                maxX = Math.Max(maxX, point.Horizontal);
                minY = Math.Min(minY, point.Value);
                maxY = Math.Max(maxY, point.Value);
                minZ = Math.Min(minZ, point.Depth);
                maxZ = Math.Max(maxZ, point.Depth);
            }
        }

        foreach (var series in source.ColumnarSeries)
        {
            var x = series.X.Span;
            var y = series.Y.Span;
            var z = series.Z.Span;

            for (var index = 0; index < series.Count; index++)
            {
                if (float.IsNaN(x[index]) || float.IsNaN(y[index]) || float.IsNaN(z[index]))
                {
                    continue;
                }

                if (!hasPoint)
                {
                    minX = maxX = x[index];
                    minY = maxY = y[index];
                    minZ = maxZ = z[index];
                    hasPoint = true;
                    continue;
                }

                minX = Math.Min(minX, x[index]);
                maxX = Math.Max(maxX, x[index]);
                minY = Math.Min(minY, y[index]);
                maxY = Math.Max(maxY, y[index]);
                minZ = Math.Min(minZ, z[index]);
                maxZ = Math.Max(maxZ, z[index]);
            }
        }

        if (!hasPoint)
        {
            return DefaultBounds;
        }

        return new SurfacePlotBounds(
            new Vector3((float)minX, (float)minY, (float)minZ),
            new Vector3((float)maxX, (float)maxY, (float)maxZ));
    }

    private static SurfaceCameraPose CreateDefaultCamera(SurfacePlotBounds bounds)
    {
        var target = bounds.Center;
        var diagonal = GetDiagonalLength(bounds.Size);
        var distance = GetFitDistance(diagonal, SurfaceCameraPose.DefaultFieldOfViewDegrees);

        return new SurfaceCameraPose(
            target,
            SurfaceCameraPose.DefaultYawDegrees,
            SurfaceCameraPose.DefaultPitchDegrees,
            distance,
            SurfaceCameraPose.DefaultFieldOfViewDegrees);
    }

    private static SurfaceCameraPose CreateFitCamera(SurfacePlotBounds bounds, SurfaceCameraPose currentCamera)
    {
        var target = bounds.Center;
        var diagonal = GetDiagonalLength(bounds.Size);
        var distance = GetFitDistance(diagonal, currentCamera.FieldOfViewDegrees);

        return new SurfaceCameraPose(
            target,
            currentCamera.YawDegrees,
            currentCamera.PitchDegrees,
            distance,
            currentCamera.FieldOfViewDegrees);
    }

    private static double GetFitDistance(double diagonal, double fieldOfViewDegrees)
    {
        var halfFieldOfViewRadians = (fieldOfViewDegrees * (Math.PI / 180d)) * 0.5d;
        return Math.Max((Math.Max(diagonal, 1d) * 0.5d) / Math.Tan(halfFieldOfViewRadians), 1d);
    }

    private static double GetDiagonalLength(Vector3 size)
    {
        return Math.Sqrt((size.X * size.X) + (size.Y * size.Y) + (size.Z * size.Z));
    }
}
