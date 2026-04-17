using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Cameras;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    public void ResetCamera()
    {
        Engine.Camera.Reset();
    }

    public bool FrameAll()
    {
        if (!TryGetSceneBounds(out var sceneBounds))
        {
            return false;
        }

        return Engine.Camera.FrameBounds(sceneBounds);
    }

    public bool Frame(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.WorldBounds is not BoundingBox3 bounds)
        {
            return false;
        }

        return Engine.Camera.FrameBounds(bounds);
    }

    public void SetViewPreset(ViewPreset preset)
    {
        var target = Engine.Camera.Target;
        var radius = Engine.Camera.Radius;

        (float yaw, float pitch) = preset switch
        {
            ViewPreset.Front => (0f, 0f),
            ViewPreset.Back => (MathF.PI, 0f),
            ViewPreset.Left => (-MathF.PI * 0.5f, 0f),
            ViewPreset.Right => (MathF.PI * 0.5f, 0f),
            ViewPreset.Top => (0f, OrbitCamera.MaximumPitch),
            ViewPreset.Bottom => (0f, -OrbitCamera.MaximumPitch),
            ViewPreset.Isometric => (MathF.PI * 0.25f, MathF.Atan(1f / MathF.Sqrt(2f))),
            _ => (Engine.Camera.Yaw, Engine.Camera.Pitch)
        };

        Engine.Camera.SetOrbit(target, radius, yaw, pitch);
    }

    private bool TryGetSceneBounds(out BoundingBox3 sceneBounds)
    {
        var sceneObjects = _runtime.SceneObjects;
        BoundingBox3? aggregateBounds = null;

        foreach (var sceneObject in sceneObjects)
        {
            if (sceneObject.WorldBounds is not BoundingBox3 objectBounds)
            {
                continue;
            }

            aggregateBounds = aggregateBounds is BoundingBox3 current
                ? current.Encapsulate(objectBounds)
                : objectBounds;
        }

        if (aggregateBounds is not BoundingBox3 bounds)
        {
            sceneBounds = default;
            return false;
        }

        sceneBounds = bounds;
        return true;
    }
}
