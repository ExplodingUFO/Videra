using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a persisted 3D camera pose for a surface chart.
/// </summary>
public sealed record SurfaceCameraPose
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceCameraPose"/> class.
    /// </summary>
    public SurfaceCameraPose(
        Vector3 target,
        double yaw,
        double pitch,
        double distance,
        double fieldOfView,
        SurfaceProjectionMode projectionMode)
    {
        ValidateFinite(yaw, nameof(yaw));
        ValidateFinite(pitch, nameof(pitch));
        ValidateFinite(distance, nameof(distance));
        ValidateFinite(fieldOfView, nameof(fieldOfView));

        if (distance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Camera distance must be positive.");
        }

        if (fieldOfView <= 0.0 || fieldOfView >= 180.0)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldOfView), "Camera field of view must be between 0 and 180 degrees.");
        }

        Target = target;
        Yaw = yaw;
        Pitch = pitch;
        Distance = distance;
        FieldOfView = fieldOfView;
        ProjectionMode = projectionMode;
    }

    /// <summary>
    /// Gets the camera target in chart space.
    /// </summary>
    public Vector3 Target { get; }

    /// <summary>
    /// Gets the yaw angle in degrees.
    /// </summary>
    public double Yaw { get; }

    /// <summary>
    /// Gets the pitch angle in degrees.
    /// </summary>
    public double Pitch { get; }

    /// <summary>
    /// Gets the target distance.
    /// </summary>
    public double Distance { get; }

    /// <summary>
    /// Gets the camera field of view in degrees.
    /// </summary>
    public double FieldOfView { get; }

    /// <summary>
    /// Gets the projection mode.
    /// </summary>
    public SurfaceProjectionMode ProjectionMode { get; }

    private static void ValidateFinite(double value, string paramName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Camera values must be finite.");
        }
    }
}
