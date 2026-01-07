using System.Numerics;

namespace Videra.Core.Cameras;

public class OrbitCamera
{
    private float _radius = 15.0f;

    public float Yaw { get; private set; } = 0.5f;
    public float Pitch { get; private set; } = 0.5f;

    public OrbitCamera()
    {
        UpdatePosition();
    }

    // 配置参数
    public float RotationSpeed { get; set; } = 0.01f;
    public float ZoomSpeed { get; set; } = 1.0f;
    public float PanSpeed { get; set; } = 0.02f;
    public bool InvertX { get; set; } = false;
    public bool InvertY { get; set; } = false;

    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; } = Vector3.Zero;
    public Vector3 Up { get; } = Vector3.UnitY;

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, Up);
    public Matrix4x4 ProjectionMatrix { get; private set; }

    public void UpdateProjection(float width, float height)
    {
        if (height < 1) height = 1;
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(0.8f, width / height, 0.1f, 1000f);
    }

    public void Rotate(float deltaX, float deltaY)
    {
        var xChange = deltaX * RotationSpeed;
        var yChange = deltaY * RotationSpeed;
        Yaw += InvertX ? -xChange : xChange;

        if (InvertY) Pitch += yChange;
        else Pitch -= yChange;
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
        var upLocal = Vector3.Cross(right, forward);
        Target += right * x * PanSpeed + upLocal * y * PanSpeed;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        var x = _radius * (float)(Math.Cos(Pitch) * Math.Sin(Yaw));
        var y = _radius * (float)Math.Sin(Pitch);
        var z = _radius * (float)(Math.Cos(Pitch) * Math.Cos(Yaw));
        Position = Target + new Vector3(x, y, z);
    }
}