using Videra.Core.Cameras;

namespace Videra.Core.Interaction;

public sealed class OrbitCameraManipulator
{
    public void Rotate(OrbitCamera camera, float deltaX, float deltaY, float sensitivity = 0.5f)
    {
        ArgumentNullException.ThrowIfNull(camera);
        camera.Rotate(deltaX * sensitivity, deltaY * sensitivity);
    }

    public void Pan(OrbitCamera camera, float deltaX, float deltaY, float sensitivity = 0.01f)
    {
        ArgumentNullException.ThrowIfNull(camera);
        camera.Pan(-deltaX * sensitivity, deltaY * sensitivity);
    }

    public void Zoom(OrbitCamera camera, float wheelDeltaY, float sensitivity = 0.5f)
    {
        ArgumentNullException.ThrowIfNull(camera);
        camera.Zoom(wheelDeltaY * sensitivity);
    }
}
