using System.Numerics;

namespace Videra.Core.Scene;

public readonly record struct SceneAuthoringPlacement(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public static SceneAuthoringPlacement Identity { get; } = new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public static SceneAuthoringPlacement At(float x, float y, float z)
    {
        return At(new Vector3(x, y, z));
    }

    public static SceneAuthoringPlacement At(Vector3 position)
    {
        return new SceneAuthoringPlacement(position, Quaternion.Identity, Vector3.One);
    }

    public static SceneAuthoringPlacement From(
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        return new SceneAuthoringPlacement(position, rotation, scale);
    }

    public Matrix4x4 ToMatrix()
    {
        return Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateFromQuaternion(Rotation)
            * Matrix4x4.CreateTranslation(Position);
    }
}
