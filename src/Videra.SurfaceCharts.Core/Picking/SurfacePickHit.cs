using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one resolved 3D pick hit on the surface chart.
/// </summary>
public readonly record struct SurfacePickHit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfacePickHit"/> struct.
    /// </summary>
    /// <param name="tileKey">The tile that produced the hit.</param>
    /// <param name="sampleX">The resolved horizontal sample-space coordinate.</param>
    /// <param name="sampleY">The resolved vertical sample-space coordinate.</param>
    /// <param name="worldPosition">The resolved world-space hit position.</param>
    /// <param name="value">The resolved finite surface value.</param>
    /// <param name="isApproximate">Whether the hit came from a coarse tile.</param>
    /// <param name="distanceToCamera">The positive camera-space distance to the hit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate or value is not finite, or when <paramref name="distanceToCamera"/> is negative.</exception>
    public SurfacePickHit(
        SurfaceTileKey tileKey,
        double sampleX,
        double sampleY,
        Vector3 worldPosition,
        double value,
        bool isApproximate,
        double distanceToCamera)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Pick-hit coordinates must be finite.");
        }

        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Pick-hit coordinates must be finite.");
        }

        if (!float.IsFinite(worldPosition.X) || !float.IsFinite(worldPosition.Y) || !float.IsFinite(worldPosition.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(worldPosition), "Pick-hit world position must be finite.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Pick-hit values must be finite.");
        }

        if (!double.IsFinite(distanceToCamera) || distanceToCamera < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceToCamera), "Pick-hit distance must be finite and non-negative.");
        }

        TileKey = tileKey;
        SampleX = sampleX;
        SampleY = sampleY;
        WorldPosition = worldPosition;
        Value = value;
        IsApproximate = isApproximate;
        DistanceToCamera = distanceToCamera;
    }

    /// <summary>
    /// Gets the tile that produced the hit.
    /// </summary>
    public SurfaceTileKey TileKey { get; }

    /// <summary>
    /// Gets the resolved horizontal sample-space coordinate.
    /// </summary>
    public double SampleX { get; }

    /// <summary>
    /// Gets the resolved vertical sample-space coordinate.
    /// </summary>
    public double SampleY { get; }

    /// <summary>
    /// Gets the resolved world-space hit position.
    /// </summary>
    public Vector3 WorldPosition { get; }

    /// <summary>
    /// Gets the resolved finite surface value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets a value indicating whether the hit came from a coarse tile approximation.
    /// </summary>
    public bool IsApproximate { get; }

    /// <summary>
    /// Gets the positive distance from the camera origin to the hit.
    /// </summary>
    public double DistanceToCamera { get; }
}
