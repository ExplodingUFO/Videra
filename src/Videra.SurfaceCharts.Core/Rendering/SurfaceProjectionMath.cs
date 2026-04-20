using System.Numerics;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Shared projection helpers for camera-true surface-chart rendering.
/// </summary>
public static class SurfaceProjectionMath
{
    private static readonly Vector3 BaseCameraOffsetDirection = Vector3.Normalize(new Vector3(-1f, 0.85f, -1f));
    private static readonly Vector3 WorldUp = Vector3.UnitY;

    /// <summary>
    /// Creates a camera frame for the supplied dataset and view state.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="viewState">The active view state.</param>
    /// <param name="viewWidth">The output width in device-independent pixels.</param>
    /// <param name="viewHeight">The output height in device-independent pixels.</param>
    /// <param name="renderScale">The output render scale.</param>
    /// <returns>The resolved camera frame.</returns>
    public static SurfaceCameraFrame CreateCameraFrame(
        SurfaceMetadata metadata,
        SurfaceViewState viewState,
        double viewWidth,
        double viewHeight,
        float renderScale)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var plotBounds = CreatePlotBounds(metadata, viewState.DataWindow);
        return CreateCameraFrame(plotBounds, viewState.Camera, viewWidth, viewHeight, renderScale);
    }

    /// <summary>
    /// Creates a camera frame for the supplied plot bounds and camera pose.
    /// </summary>
    /// <param name="plotBounds">The world-space plot bounds to frame.</param>
    /// <param name="camera">The camera pose to resolve.</param>
    /// <param name="viewWidth">The output width in device-independent pixels.</param>
    /// <param name="viewHeight">The output height in device-independent pixels.</param>
    /// <param name="renderScale">The output render scale.</param>
    /// <returns>The resolved camera frame.</returns>
    public static SurfaceCameraFrame CreateCameraFrame(
        SurfacePlotBounds plotBounds,
        SurfaceCameraPose camera,
        double viewWidth,
        double viewHeight,
        float renderScale)
    {
        if (!double.IsFinite(viewWidth) || viewWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(viewWidth), "View width must be finite and positive.");
        }

        if (!double.IsFinite(viewHeight) || viewHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(viewHeight), "View height must be finite and positive.");
        }

        if (!float.IsFinite(renderScale) || renderScale <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(renderScale), "Render scale must be finite and positive.");
        }

        var cameraOffset = GetCameraOffsetDirection(camera.YawDegrees, camera.PitchDegrees) * (float)camera.Distance;
        var position = camera.Target + cameraOffset;
        var forward = Vector3.Normalize(camera.Target - position);
        var right = Vector3.Cross(WorldUp, forward);
        if (right.LengthSquared() <= float.Epsilon)
        {
            right = Vector3.UnitX;
        }
        else
        {
            right = Vector3.Normalize(right);
        }

        var up = Vector3.Normalize(Vector3.Cross(forward, right));
        var radius = GetBoundingRadius(plotBounds, camera.Target);
        var nearPlane = Math.Max(0.01f, (float)(camera.Distance - (radius * 1.5d)));
        var farPlane = Math.Max(nearPlane + 1f, (float)(camera.Distance + (radius * 1.5d) + 1d));
        var aspectRatio = (float)(viewWidth / viewHeight);
        var fieldOfViewRadians = DegreesToRadians((float)camera.FieldOfViewDegrees);
        var viewMatrix = Matrix4x4.CreateLookAt(position, camera.Target, up);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfViewRadians, aspectRatio, nearPlane, farPlane);
        var viewProjectionMatrix = viewMatrix * projectionMatrix;

        if (!Matrix4x4.Invert(viewProjectionMatrix, out var inverseViewProjectionMatrix))
        {
            throw new InvalidOperationException("Surface camera frame could not invert the view-projection matrix.");
        }

        return new SurfaceCameraFrame(
            position,
            camera.Target,
            up,
            viewMatrix,
            projectionMatrix,
            viewProjectionMatrix,
            inverseViewProjectionMatrix,
            plotBounds,
            new Vector2((float)(viewWidth * renderScale), (float)(viewHeight * renderScale)),
            nearPlane,
            farPlane,
            camera.ToProjectionSettings());
    }

    /// <summary>
    /// Creates world-space plot bounds for the supplied dataset slice.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The active sample-space data window.</param>
    /// <returns>The world-space plot bounds for the requested data window.</returns>
    public static SurfacePlotBounds CreatePlotBounds(SurfaceMetadata metadata, SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        var minSampleX = clampedWindow.StartX;
        var maxSampleX = clampedWindow.Width <= 1d ? minSampleX : clampedWindow.StartX + clampedWindow.Width - 1d;
        var minSampleY = clampedWindow.StartY;
        var maxSampleY = clampedWindow.Height <= 1d ? minSampleY : clampedWindow.StartY + clampedWindow.Height - 1d;

        var minimum = new Vector3(
            (float)metadata.MapHorizontalCoordinate(minSampleX),
            (float)metadata.ValueRange.Minimum,
            (float)metadata.MapVerticalCoordinate(minSampleY));
        var maximum = new Vector3(
            (float)metadata.MapHorizontalCoordinate(maxSampleX),
            (float)metadata.ValueRange.Maximum,
            (float)metadata.MapVerticalCoordinate(maxSampleY));

        return new SurfacePlotBounds(minimum, maximum);
    }

    /// <summary>
    /// Projects one world-space point into screen-space coordinates.
    /// </summary>
    /// <param name="worldPosition">The world-space point to project.</param>
    /// <param name="frame">The active camera frame.</param>
    /// <returns>The screen-space X/Y position plus normalized depth in Z.</returns>
    public static Vector3 ProjectToScreen(Vector3 worldPosition, SurfaceCameraFrame frame)
    {
        var clip = Vector4.Transform(new Vector4(worldPosition, 1f), frame.ViewProjectionMatrix);
        if (Math.Abs(clip.W) <= float.Epsilon)
        {
            throw new InvalidOperationException("Projected point produced an invalid homogeneous W component.");
        }

        var inverseW = 1f / clip.W;
        var ndc = new Vector3(clip.X * inverseW, clip.Y * inverseW, clip.Z * inverseW);
        return new Vector3(
            ((ndc.X + 1f) * 0.5f) * frame.ViewportPixels.X,
            ((1f - ndc.Y) * 0.5f) * frame.ViewportPixels.Y,
            (ndc.Z + 1f) * 0.5f);
    }

    /// <summary>
    /// Unprojects one screen-space point back into world space.
    /// </summary>
    /// <param name="screenPoint">The screen-space X/Y position plus normalized depth in Z.</param>
    /// <param name="frame">The active camera frame.</param>
    /// <returns>The corresponding world-space position.</returns>
    public static Vector3 UnprojectFromScreen(Vector3 screenPoint, SurfaceCameraFrame frame)
    {
        var ndc = new Vector3(
            ((screenPoint.X / frame.ViewportPixels.X) * 2f) - 1f,
            1f - ((screenPoint.Y / frame.ViewportPixels.Y) * 2f),
            (screenPoint.Z * 2f) - 1f);
        var world = Vector4.Transform(new Vector4(ndc, 1f), frame.InverseViewProjectionMatrix);
        if (Math.Abs(world.W) <= float.Epsilon)
        {
            throw new InvalidOperationException("Unprojected point produced an invalid homogeneous W component.");
        }

        var inverseW = 1f / world.W;
        return new Vector3(world.X * inverseW, world.Y * inverseW, world.Z * inverseW);
    }

    private static Vector3 GetCameraOffsetDirection(double yawDegrees, double pitchDegrees)
    {
        var yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, DegreesToRadians((float)yawDegrees));
        var yawed = Vector3.Transform(BaseCameraOffsetDirection, yawRotation);

        var forward = Vector3.Normalize(-yawed);
        var right = Vector3.Cross(WorldUp, forward);
        if (right.LengthSquared() <= float.Epsilon)
        {
            right = Vector3.UnitX;
        }
        else
        {
            right = Vector3.Normalize(right);
        }

        var pitchRotation = Quaternion.CreateFromAxisAngle(right, DegreesToRadians((float)pitchDegrees));
        return Vector3.Normalize(Vector3.Transform(yawed, pitchRotation));
    }

    private static double GetBoundingRadius(SurfacePlotBounds plotBounds, Vector3 center)
    {
        var radius = 0d;
        foreach (var corner in plotBounds.GetCorners())
        {
            radius = Math.Max(radius, Vector3.Distance(center, corner));
        }

        return radius;
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }
}
