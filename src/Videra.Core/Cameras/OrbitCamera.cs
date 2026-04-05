using System;
using System.Numerics;

namespace Videra.Core.Cameras;

/// <summary>
/// An orbit camera that revolves around a target point in 3D space.
/// Supports rotation (yaw/pitch), zoom (radius), and panning (target offset)
/// with configurable speeds and axis inversion.
/// Generates a perspective projection matrix suitable for Metal/Veldrid
/// depth ranges [0, 1].
/// </summary>
public class OrbitCamera
{
    // ==========================================
    // 1. Fields & Properties
    // ==========================================

    /// <summary>
    /// The distance from the camera to <see cref="Target"/>.
    /// Clamped to the range [0.1, 200] during zoom operations.
    /// </summary>
    private float _radius = 15.0f;

    // State properties

    /// <summary>
    /// Gets the current yaw angle in radians (horizontal rotation around the target).
    /// </summary>
    public float Yaw { get; private set; } = 0.5f;

    /// <summary>
    /// Gets the current pitch angle in radians (vertical tilt).
    /// Clamped to the range [-1.5, 1.5] to prevent gimbal lock.
    /// </summary>
    public float Pitch { get; private set; } = 0.5f;

    // Spatial properties

    /// <summary>
    /// Gets the current world-space position of the camera,
    /// computed from <see cref="Target"/>, <see cref="Yaw"/>, <see cref="Pitch"/>, and radius.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    /// Gets or sets the world-space point the camera orbits around.
    /// Defaults to <see cref="Vector3.Zero"/>.
    /// </summary>
    public Vector3 Target { get; private set; } = Vector3.Zero;

    /// <summary>
    /// Gets the up direction vector used to construct the view matrix.
    /// Always <see cref="Vector3.UnitY"/>.
    /// </summary>
    public Vector3 Up { get; } = Vector3.UnitY;

    // Configuration parameters

    /// <summary>
    /// Gets or sets the sensitivity multiplier applied to rotation input.
    /// Defaults to <c>0.01f</c>.
    /// </summary>
    public float RotationSpeed { get; set; } = 0.01f;

    /// <summary>
    /// Gets or sets the sensitivity multiplier applied to zoom input.
    /// Defaults to <c>1.0f</c>.
    /// </summary>
    public float ZoomSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the sensitivity multiplier applied to pan input.
    /// Defaults to <c>0.02f</c>.
    /// </summary>
    public float PanSpeed { get; set; } = 0.02f;

    /// <summary>
    /// Gets or sets whether horizontal (yaw) rotation is inverted.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool InvertX { get; set; }

    /// <summary>
    /// Gets or sets whether vertical (pitch) rotation is inverted.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool InvertY { get; set; }

    /// <summary>
    /// Gets or sets the vertical field of view in radians.
    /// Defaults to 45 degrees (<c>PI / 4</c>).
    /// </summary>
    public float FieldOfView { get; set; } = MathF.PI / 4.0f;

    // Matrix storage
    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;

    // Public read-only matrices

    /// <summary>
    /// Gets the current view matrix (look-at), recomputed whenever the camera
    /// position, target, or orientation changes.
    /// </summary>
    public Matrix4x4 ViewMatrix => _viewMatrix;

    /// <summary>
    /// Gets the current perspective projection matrix, suitable for
    /// Metal/Veldrid with a [0, 1] depth range.
    /// Updated via <see cref="UpdateProjection"/> or <see cref="UpdateAspectRatio"/>.
    /// </summary>
    public Matrix4x4 ProjectionMatrix => _projectionMatrix;

    /// <summary>
    /// Creates a new <see cref="OrbitCamera"/> and computes the initial
    /// camera position and view matrix from the default parameters.
    /// </summary>
    public OrbitCamera()
    {
        // 初始化时计算一次位置和视图矩阵
        UpdatePosition();
    }

    // ==========================================
    // 2. Projection matrix
    // ==========================================

    /// <summary>
    /// Recomputes the perspective projection matrix from the given viewport dimensions.
    /// The matrix uses a [0, 1] depth range compatible with Metal/Veldrid backends,
    /// overriding the default System.Numerics [-1, 1] range.
    /// </summary>
    /// <param name="width">The viewport width in pixels. Treated as 1 if zero.</param>
    /// <param name="height">The viewport height in pixels. Treated as 1 if zero.</param>
    public void UpdateProjection(uint width, uint height)
    {
        // 防止除以 0
        float w = width > 0 ? width : 1;
        float h = height > 0 ? height : 1;
        float aspectRatio = w / h;

        float near = 0.1f;
        float far = 1000f;

        // 【关键修复】手动构建适用于 Metal/Veldrid [0, 1] 深度范围的透视矩阵
        // System.Numerics.Matrix4x4.CreatePerspectiveFieldOfView 默认是 [-1, 1]
        // 这里的公式生成的是 [0, 1]，修复了 Metal 下的遮挡问题。
        
        float f = 1.0f / MathF.Tan(FieldOfView * 0.5f);

        _projectionMatrix = new Matrix4x4(
            f / aspectRatio, 0,  0,  0,
            0,               f,  0,  0,
            0,               0,  far / (near - far), -1,
            0,               0,  (near * far) / (near - far), 0
        );
        
        // 注意：如果你发现画面依然上下颠倒，可以尝试将 M22 (f) 改为 -f
        // 但如果只是前后遮挡问题，上面的公式是正确的。
    }
    
    /// <summary>
    /// Recomputes the perspective projection matrix from the given aspect ratio.
    /// Useful when the raw aspect ratio is already known (e.g. during window resize).
    /// Uses a [0, 1] depth range compatible with Metal/Veldrid backends.
    /// </summary>
    /// <param name="aspectRatio">The viewport aspect ratio (width / height).</param>
    public void UpdateAspectRatio(float aspectRatio)
    {
        float near = 0.1f;
        float far = 1000f;
        float f = 1.0f / MathF.Tan(FieldOfView * 0.5f);

        _projectionMatrix = new Matrix4x4(
            f / aspectRatio, 0,  0,  0,
            0,               f,  0,  0,
            0,               0,  far / (near - far), -1,
            0,               0,  (near * far) / (near - far), 0
        );
    }

    // ==========================================
    // 3. Interaction methods (Rotate, Zoom, Pan)
    // ==========================================

    /// <summary>
    /// Applies a rotation delta to the camera's yaw and pitch angles,
    /// respecting <see cref="RotationSpeed"/>, <see cref="InvertX"/>,
    /// and <see cref="InvertY"/> settings.
    /// Pitch is clamped to [-1.5, 1.5] radians to prevent gimbal lock.
    /// Recomputes the camera position and view matrix.
    /// </summary>
    /// <param name="deltaX">Horizontal input delta (e.g. mouse movement along X).</param>
    /// <param name="deltaY">Vertical input delta (e.g. mouse movement along Y).</param>
    public void Rotate(float deltaX, float deltaY)
    {
        var xChange = deltaX * RotationSpeed;
        var yChange = deltaY * RotationSpeed;
        
        Yaw += InvertX ? -xChange : xChange;

        if (InvertY) Pitch += yChange;
        else Pitch -= yChange;
        
        // 限制俯仰角，防止万向节死锁 (Gimbal Lock)
        Pitch = Math.Clamp(Pitch, -1.5f, 1.5f);

        UpdatePosition();
    }

    /// <summary>
    /// Adjusts the orbit radius (distance from camera to target) by the given delta,
    /// scaled by <see cref="ZoomSpeed"/>. The radius is clamped to [0.1, 200].
    /// Recomputes the camera position and view matrix.
    /// </summary>
    /// <param name="delta">Zoom input delta. Positive values zoom out; negative values zoom in.</param>
    public void Zoom(float delta)
    {
        _radius -= delta * 0.5f * ZoomSpeed;
        _radius = Math.Clamp(_radius, 0.1f, 200f);
        UpdatePosition();
    }

    /// <summary>
    /// Pans the camera's <see cref="Target"/> point in screen-aligned directions,
    /// scaled by <see cref="PanSpeed"/>. The camera position moves along with the target
    /// so the viewing angle is preserved.
    /// </summary>
    /// <param name="x">Horizontal pan delta (right is positive).</param>
    /// <param name="y">Vertical pan delta (up is positive).</param>
    public void Pan(float x, float y)
    {
        var forward = Vector3.Normalize(Target - Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, Up));
        
        // 重新计算局部 Up 向量，确保平移方向正确
        var upLocal = Vector3.Cross(right, forward);
        
        Target += right * x * PanSpeed + upLocal * y * PanSpeed;
        UpdatePosition();
    }

    // ==========================================
    // 4. View matrix update
    // ==========================================

    /// <summary>
    /// Recomputes <see cref="Position"/> from spherical coordinates (yaw, pitch, radius)
    /// relative to <see cref="Target"/>, then updates <see cref="ViewMatrix"/>
    /// using <see cref="Matrix4x4.CreateLookAt"/>.
    /// </summary>
    private void UpdatePosition()
    {
        // 1. 计算笛卡尔坐标 (位置)
        // 使用 MathF 替代 Math 以保持 float 精度一致
        var x = _radius * MathF.Cos(Pitch) * MathF.Sin(Yaw);
        var y = _radius * MathF.Sin(Pitch);
        var z = _radius * MathF.Cos(Pitch) * MathF.Cos(Yaw);
        
        Position = Target + new Vector3(x, y, z);

        // 2. 【关键修复】更新 View 矩阵
        // 您的原始代码漏掉了这一行，导致 ViewMatrix 永远是 Identity 或 Zero
        _viewMatrix = Matrix4x4.CreateLookAt(Position, Target, Up);
    }
}
