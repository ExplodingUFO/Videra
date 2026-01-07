using System;
using System.Numerics;
using Veldrid;

namespace Videra.Core.Cameras;

public class OrbitCamera
{
    // ==========================================
    // 1. 字段与属性 (保留您的配置)
    // ==========================================
    private float _radius = 15.0f;

    // 状态属性
    public float Yaw { get; private set; } = 0.5f;
    public float Pitch { get; private set; } = 0.5f;
    
    // 空间属性
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; } = Vector3.Zero;
    public Vector3 Up { get; } = Vector3.UnitY;

    // 配置参数
    public float RotationSpeed { get; set; } = 0.01f;
    public float ZoomSpeed { get; set; } = 1.0f;
    public float PanSpeed { get; set; } = 0.02f;
    public bool InvertX { get; set; } = false;
    public bool InvertY { get; set; } = false;

    // 默认视场角 45度 (弧度)
    public float FieldOfView { get; set; } = MathF.PI / 4.0f; 

    // 矩阵存储
    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;

    // 对外只读矩阵
    public Matrix4x4 ViewMatrix => _viewMatrix;
    public Matrix4x4 ProjectionMatrix => _projectionMatrix;

    public OrbitCamera()
    {
        // 初始化时计算一次位置和视图矩阵
        UpdatePosition();
    }

    // ==========================================
    // 2. 核心修复：投影矩阵 (Projection)
    // ==========================================
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

    // ==========================================
    // 3. 交互方法 (Rotate, Zoom, Pan)
    // ==========================================
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

    public void Zoom(float delta)
    {
        _radius -= delta * 0.5f * ZoomSpeed;
        _radius = Math.Clamp(_radius, 1f, 200f);
        UpdatePosition();
    }

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
    // 4. 视图矩阵更新 (View)
    // ==========================================
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