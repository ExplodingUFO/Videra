using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a world-space picking ray cast through the surface chart.
/// </summary>
public readonly record struct SurfacePickRay
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfacePickRay"/> struct.
    /// </summary>
    /// <param name="origin">The world-space ray origin.</param>
    /// <param name="direction">The world-space ray direction.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any vector component is not finite or when <paramref name="direction"/> is zero.</exception>
    public SurfacePickRay(Vector3 origin, Vector3 direction)
    {
        ValidateVector(origin, nameof(origin));
        ValidateVector(direction, nameof(direction));

        if (direction.LengthSquared() <= float.Epsilon)
        {
            throw new ArgumentOutOfRangeException(nameof(direction), "Pick-ray direction must be non-zero.");
        }

        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }

    /// <summary>
    /// Gets the world-space ray origin.
    /// </summary>
    public Vector3 Origin { get; }

    /// <summary>
    /// Gets the normalized world-space ray direction.
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    /// Resolves a point along the ray.
    /// </summary>
    /// <param name="distance">The positive distance along the ray.</param>
    /// <returns>The world-space point at the supplied distance.</returns>
    public Vector3 GetPoint(float distance)
    {
        return Origin + (Direction * distance);
    }

    private static void ValidateVector(Vector3 value, string parameterName)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Vector components must be finite.");
        }
    }
}
