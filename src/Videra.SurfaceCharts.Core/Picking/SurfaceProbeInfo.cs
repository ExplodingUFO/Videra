using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a resolved surface probe in both sample space and axis space.
/// </summary>
public readonly record struct SurfaceProbeInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceProbeInfo"/> struct.
    /// </summary>
    /// <param name="sampleX">The resolved horizontal sample-space coordinate.</param>
    /// <param name="sampleY">The resolved vertical sample-space coordinate.</param>
    /// <param name="axisX">The resolved horizontal axis-space coordinate.</param>
    /// <param name="axisY">The resolved vertical axis-space coordinate.</param>
    /// <param name="value">The resolved finite surface value.</param>
    /// <param name="isApproximate">Whether the resolved value comes from a coarse tile.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a coordinate or value is not finite.</exception>
    public SurfaceProbeInfo(
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double value,
        bool isApproximate)
        : this(
            sampleX,
            sampleY,
            axisX,
            axisY,
            value,
            isApproximate,
            worldPosition: new Vector3((float)axisX, (float)value, (float)axisY),
            tileKey: default,
            distanceToCamera: 0d)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceProbeInfo"/> struct.
    /// </summary>
    /// <param name="sampleX">The resolved horizontal sample-space coordinate.</param>
    /// <param name="sampleY">The resolved vertical sample-space coordinate.</param>
    /// <param name="axisX">The resolved horizontal axis-space coordinate.</param>
    /// <param name="axisY">The resolved vertical axis-space coordinate.</param>
    /// <param name="value">The resolved finite surface value.</param>
    /// <param name="isApproximate">Whether the resolved value comes from a coarse tile.</param>
    /// <param name="worldPosition">The resolved world-space position.</param>
    /// <param name="tileKey">The tile that produced the resolved probe.</param>
    /// <param name="distanceToCamera">The distance from the camera to the resolved probe.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a coordinate or value is not finite.</exception>
    public SurfaceProbeInfo(
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double value,
        bool isApproximate,
        Vector3 worldPosition,
        SurfaceTileKey tileKey,
        double distanceToCamera)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(axisX))
        {
            throw new ArgumentOutOfRangeException(nameof(axisX), "Probe axis coordinates must be finite.");
        }

        if (!double.IsFinite(axisY))
        {
            throw new ArgumentOutOfRangeException(nameof(axisY), "Probe axis coordinates must be finite.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Probe values must be finite.");
        }

        if (!float.IsFinite(worldPosition.X) || !float.IsFinite(worldPosition.Y) || !float.IsFinite(worldPosition.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(worldPosition), "Probe world positions must be finite.");
        }

        if (!double.IsFinite(distanceToCamera) || distanceToCamera < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceToCamera), "Probe distance must be finite and non-negative.");
        }

        SampleX = sampleX;
        SampleY = sampleY;
        AxisX = axisX;
        AxisY = axisY;
        Value = value;
        IsApproximate = isApproximate;
        WorldPosition = worldPosition;
        TileKey = tileKey;
        DistanceToCamera = distanceToCamera;
    }

    /// <summary>
    /// Gets the resolved horizontal sample-space coordinate.
    /// </summary>
    public double SampleX { get; }

    /// <summary>
    /// Gets the resolved vertical sample-space coordinate.
    /// </summary>
    public double SampleY { get; }

    /// <summary>
    /// Gets the resolved horizontal axis-space coordinate.
    /// </summary>
    public double AxisX { get; }

    /// <summary>
    /// Gets the resolved vertical axis-space coordinate.
    /// </summary>
    public double AxisY { get; }

    /// <summary>
    /// Gets the resolved finite surface value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets a value indicating whether the probe result comes from a coarse tile approximation.
    /// </summary>
    public bool IsApproximate { get; }

    /// <summary>
    /// Gets the resolved world-space probe position.
    /// </summary>
    public Vector3 WorldPosition { get; }

    /// <summary>
    /// Gets the tile that produced the resolved probe.
    /// </summary>
    public SurfaceTileKey TileKey { get; }

    /// <summary>
    /// Gets the distance from the camera to the resolved probe.
    /// </summary>
    public double DistanceToCamera { get; }

    internal static SurfaceProbeInfo FromPickHit(SurfaceMetadata metadata, SurfacePickHit pickHit)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new SurfaceProbeInfo(
            pickHit.SampleX,
            pickHit.SampleY,
            MapAxis(metadata.HorizontalAxis, pickHit.SampleX, metadata.Width),
            MapAxis(metadata.VerticalAxis, pickHit.SampleY, metadata.Height),
            pickHit.Value,
            pickHit.IsApproximate,
            pickHit.WorldPosition,
            pickHit.TileKey,
            pickHit.DistanceToCamera);
    }

    internal static SurfaceProbeInfo FromResolvedSample(
        SurfaceMetadata metadata,
        SurfaceTile tile,
        SurfaceProbeRequest probeRequest,
        double value)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(tile);

        return new SurfaceProbeInfo(
            probeRequest.SampleX,
            probeRequest.SampleY,
            MapAxis(metadata.HorizontalAxis, probeRequest.SampleX, metadata.Width),
            MapAxis(metadata.VerticalAxis, probeRequest.SampleY, metadata.Height),
            value,
            isApproximate: tile.Bounds.Width != tile.Width || tile.Bounds.Height != tile.Height,
            worldPosition: new Vector3(
                (float)MapAxis(metadata.HorizontalAxis, probeRequest.SampleX, metadata.Width),
                (float)value,
                (float)MapAxis(metadata.VerticalAxis, probeRequest.SampleY, metadata.Height)),
            tileKey: tile.Key,
            distanceToCamera: 0d);
    }

    private static double MapAxis(SurfaceAxisDescriptor axis, double sampleIndex, int sampleCount)
    {
        if (sampleCount <= 1 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = sampleIndex / (sampleCount - 1d);
        return axis.Minimum + (axis.Span * normalized);
    }
}
