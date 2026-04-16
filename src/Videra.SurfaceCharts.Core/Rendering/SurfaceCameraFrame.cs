using System.Numerics;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Represents the shared camera and projection state for surface-chart rendering.
/// </summary>
public readonly record struct SurfaceCameraFrame
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceCameraFrame"/> struct.
    /// </summary>
    /// <param name="position">The world-space camera position.</param>
    /// <param name="target">The world-space camera target.</param>
    /// <param name="up">The resolved camera up vector.</param>
    /// <param name="viewMatrix">The world-to-view transform.</param>
    /// <param name="projectionMatrix">The view-to-clip transform.</param>
    /// <param name="viewProjectionMatrix">The combined view-projection transform.</param>
    /// <param name="inverseViewProjectionMatrix">The inverse combined transform.</param>
    /// <param name="plotBounds">The active world-space plot bounds.</param>
    /// <param name="viewportPixels">The output viewport dimensions in pixels.</param>
    /// <param name="nearPlane">The near clipping plane distance.</param>
    /// <param name="farPlane">The far clipping plane distance.</param>
    /// <param name="projectionSettings">The compatibility yaw/pitch projection settings.</param>
    public SurfaceCameraFrame(
        Vector3 position,
        Vector3 target,
        Vector3 up,
        Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix,
        Matrix4x4 viewProjectionMatrix,
        Matrix4x4 inverseViewProjectionMatrix,
        SurfacePlotBounds plotBounds,
        Vector2 viewportPixels,
        float nearPlane,
        float farPlane,
        SurfaceChartProjectionSettings projectionSettings)
    {
        ValidateVector(position, nameof(position));
        ValidateVector(target, nameof(target));
        ValidateVector(up, nameof(up));

        if (!float.IsFinite(viewportPixels.X) || !float.IsFinite(viewportPixels.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(viewportPixels), "Viewport pixels must be finite.");
        }

        if (viewportPixels.X <= 0f || viewportPixels.Y <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportPixels), "Viewport pixels must be positive.");
        }

        if (!float.IsFinite(nearPlane) || nearPlane <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(nearPlane), "Near plane must be finite and positive.");
        }

        if (!float.IsFinite(farPlane) || farPlane <= nearPlane)
        {
            throw new ArgumentOutOfRangeException(nameof(farPlane), "Far plane must be finite and greater than the near plane.");
        }

        Position = position;
        Target = target;
        Up = up;
        ViewMatrix = viewMatrix;
        ProjectionMatrix = projectionMatrix;
        ViewProjectionMatrix = viewProjectionMatrix;
        InverseViewProjectionMatrix = inverseViewProjectionMatrix;
        PlotBounds = plotBounds;
        ViewportPixels = viewportPixels;
        NearPlane = nearPlane;
        FarPlane = farPlane;
        ProjectionSettings = projectionSettings;
    }

    /// <summary>
    /// Gets the world-space camera position.
    /// </summary>
    public Vector3 Position { get; }

    /// <summary>
    /// Gets the world-space camera target.
    /// </summary>
    public Vector3 Target { get; }

    /// <summary>
    /// Gets the resolved camera up vector.
    /// </summary>
    public Vector3 Up { get; }

    /// <summary>
    /// Gets the world-to-view transform.
    /// </summary>
    public Matrix4x4 ViewMatrix { get; }

    /// <summary>
    /// Gets the view-to-clip transform.
    /// </summary>
    public Matrix4x4 ProjectionMatrix { get; }

    /// <summary>
    /// Gets the combined view-projection transform.
    /// </summary>
    public Matrix4x4 ViewProjectionMatrix { get; }

    /// <summary>
    /// Gets the inverse combined view-projection transform.
    /// </summary>
    public Matrix4x4 InverseViewProjectionMatrix { get; }

    /// <summary>
    /// Gets the active world-space plot bounds.
    /// </summary>
    public SurfacePlotBounds PlotBounds { get; }

    /// <summary>
    /// Gets the output viewport dimensions in pixels.
    /// </summary>
    public Vector2 ViewportPixels { get; }

    /// <summary>
    /// Gets the near clipping plane distance.
    /// </summary>
    public float NearPlane { get; }

    /// <summary>
    /// Gets the far clipping plane distance.
    /// </summary>
    public float FarPlane { get; }

    /// <summary>
    /// Gets the compatibility yaw/pitch projection settings.
    /// </summary>
    public SurfaceChartProjectionSettings ProjectionSettings { get; }

    private static void ValidateVector(Vector3 value, string parameterName)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Vector components must be finite.");
        }
    }
}

/// <summary>
/// Represents the world-space plot bounds for the active surface-chart data window.
/// </summary>
public readonly record struct SurfacePlotBounds
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfacePlotBounds"/> struct.
    /// </summary>
    /// <param name="minimum">The minimum world-space corner.</param>
    /// <param name="maximum">The maximum world-space corner.</param>
    public SurfacePlotBounds(Vector3 minimum, Vector3 maximum)
    {
        if (!float.IsFinite(minimum.X) || !float.IsFinite(minimum.Y) || !float.IsFinite(minimum.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Plot-bounds minimum must be finite.");
        }

        if (!float.IsFinite(maximum.X) || !float.IsFinite(maximum.Y) || !float.IsFinite(maximum.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Plot-bounds maximum must be finite.");
        }

        if (minimum.X > maximum.X || minimum.Y > maximum.Y || minimum.Z > maximum.Z)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Plot-bounds minimum must not exceed maximum.");
        }

        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the minimum world-space corner.
    /// </summary>
    public Vector3 Minimum { get; }

    /// <summary>
    /// Gets the maximum world-space corner.
    /// </summary>
    public Vector3 Maximum { get; }

    /// <summary>
    /// Gets the center of the plot bounds.
    /// </summary>
    public Vector3 Center => (Minimum + Maximum) * 0.5f;

    /// <summary>
    /// Gets the world-space size of the plot bounds.
    /// </summary>
    public Vector3 Size => Maximum - Minimum;

    /// <summary>
    /// Returns the eight world-space plot corners.
    /// </summary>
    public Vector3[] GetCorners()
    {
        return
        [
            new Vector3(Minimum.X, Minimum.Y, Minimum.Z),
            new Vector3(Minimum.X, Minimum.Y, Maximum.Z),
            new Vector3(Minimum.X, Maximum.Y, Minimum.Z),
            new Vector3(Minimum.X, Maximum.Y, Maximum.Z),
            new Vector3(Maximum.X, Minimum.Y, Minimum.Z),
            new Vector3(Maximum.X, Minimum.Y, Maximum.Z),
            new Vector3(Maximum.X, Maximum.Y, Minimum.Z),
            new Vector3(Maximum.X, Maximum.Y, Maximum.Z),
        ];
    }
}
