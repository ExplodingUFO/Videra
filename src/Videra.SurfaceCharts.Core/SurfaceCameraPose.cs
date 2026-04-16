using System.Numerics;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the persisted camera pose for a surface-chart view.
/// </summary>
public readonly record struct SurfaceCameraPose
{
    /// <summary>
    /// Gets the default yaw used by the chart camera pose.
    /// </summary>
    public const double DefaultYawDegrees = 0d;

    /// <summary>
    /// Gets the default pitch used by the chart camera pose.
    /// </summary>
    public const double DefaultPitchDegrees = 0d;

    /// <summary>
    /// Gets the default field of view used by the chart camera pose.
    /// </summary>
    public const double DefaultFieldOfViewDegrees = 45d;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceCameraPose"/> struct.
    /// </summary>
    /// <param name="target">The chart-space camera target.</param>
    /// <param name="yawDegrees">The yaw angle in degrees.</param>
    /// <param name="pitchDegrees">The pitch angle in degrees.</param>
    /// <param name="distance">The distance from the camera to the target.</param>
    /// <param name="fieldOfViewDegrees">The vertical field of view in degrees.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any persisted value is not finite or when <paramref name="distance"/> or <paramref name="fieldOfViewDegrees"/> is not positive.</exception>
    public SurfaceCameraPose(Vector3 target, double yawDegrees, double pitchDegrees, double distance, double fieldOfViewDegrees)
    {
        if (!float.IsFinite(target.X) || !float.IsFinite(target.Y) || !float.IsFinite(target.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(target), "Camera target components must be finite.");
        }

        if (!double.IsFinite(yawDegrees))
        {
            throw new ArgumentOutOfRangeException(nameof(yawDegrees), "Camera yaw must be finite.");
        }

        if (!double.IsFinite(pitchDegrees))
        {
            throw new ArgumentOutOfRangeException(nameof(pitchDegrees), "Camera pitch must be finite.");
        }

        if (!double.IsFinite(distance))
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Camera distance must be finite.");
        }

        if (distance <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Camera distance must be positive.");
        }

        if (!double.IsFinite(fieldOfViewDegrees))
        {
            throw new ArgumentOutOfRangeException(nameof(fieldOfViewDegrees), "Camera field of view must be finite.");
        }

        if (fieldOfViewDegrees <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldOfViewDegrees), "Camera field of view must be positive.");
        }

        Target = target;
        YawDegrees = yawDegrees;
        PitchDegrees = pitchDegrees;
        Distance = distance;
        FieldOfViewDegrees = fieldOfViewDegrees;
    }

    /// <summary>
    /// Gets the chart-space camera target.
    /// </summary>
    public Vector3 Target { get; }

    /// <summary>
    /// Gets the yaw angle in degrees.
    /// </summary>
    public double YawDegrees { get; }

    /// <summary>
    /// Gets the pitch angle in degrees.
    /// </summary>
    public double PitchDegrees { get; }

    /// <summary>
    /// Gets the distance from the camera to the target.
    /// </summary>
    public double Distance { get; }

    /// <summary>
    /// Gets the vertical field of view in degrees.
    /// </summary>
    public double FieldOfViewDegrees { get; }

    /// <summary>
    /// Adapts the persisted camera pose to the current chart projection settings.
    /// </summary>
    /// <returns>The current projection settings derived from the camera pose.</returns>
    public SurfaceChartProjectionSettings ToProjectionSettings()
    {
        return new SurfaceChartProjectionSettings(YawDegrees, PitchDegrees);
    }

    /// <summary>
    /// Creates a complete camera frame for the supplied dataset, data window, and output size.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The active data window.</param>
    /// <param name="viewWidth">The output width in device-independent pixels.</param>
    /// <param name="viewHeight">The output height in device-independent pixels.</param>
    /// <param name="renderScale">The active render scale.</param>
    /// <returns>The resolved camera frame.</returns>
    public SurfaceCameraFrame CreateCameraFrame(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow,
        double viewWidth,
        double viewHeight,
        float renderScale)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(dataWindow.ClampTo(metadata), this),
            viewWidth,
            viewHeight,
            renderScale);
    }

    /// <summary>
    /// Creates a default camera pose centered on the active data window and metadata value range.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The active data window.</param>
    /// <returns>The default camera pose for the supplied dataset slice.</returns>
    public static SurfaceCameraPose CreateDefault(SurfaceMetadata metadata, SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        var target = new Vector3(
            (float)MapWindowCenter(metadata.HorizontalAxis, clampedWindow.StartX, clampedWindow.Width, metadata.Width),
            (float)((metadata.ValueRange.Minimum + metadata.ValueRange.Maximum) * 0.5d),
            (float)MapWindowCenter(metadata.VerticalAxis, clampedWindow.StartY, clampedWindow.Height, metadata.Height));

        var horizontalSpan = MapWindowSpan(metadata.HorizontalAxis, clampedWindow.Width, metadata.Width);
        var verticalSpan = MapWindowSpan(metadata.VerticalAxis, clampedWindow.Height, metadata.Height);
        var valueSpan = metadata.ValueRange.Span;
        var diagonal = Math.Sqrt((horizontalSpan * horizontalSpan) + (valueSpan * valueSpan) + (verticalSpan * verticalSpan));
        var halfFieldOfViewRadians = (DefaultFieldOfViewDegrees * (Math.PI / 180d)) * 0.5d;
        var distance = Math.Max((diagonal * 0.5d) / Math.Tan(halfFieldOfViewRadians), 1d);

        return new SurfaceCameraPose(
            target,
            DefaultYawDegrees,
            DefaultPitchDegrees,
            distance,
            DefaultFieldOfViewDegrees);
    }

    private static double MapWindowCenter(SurfaceAxisDescriptor axis, double start, double span, int sampleCount)
    {
        if (sampleCount <= 0 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalizedCenter = Math.Clamp((start + (span * 0.5d)) / sampleCount, 0d, 1d);
        return axis.Minimum + (axis.Span * normalizedCenter);
    }

    private static double MapWindowSpan(SurfaceAxisDescriptor axis, double span, int sampleCount)
    {
        if (sampleCount <= 0 || axis.Maximum <= axis.Minimum)
        {
            return 0d;
        }

        return axis.Span * Math.Clamp(span / sampleCount, 0d, 1d);
    }
}
