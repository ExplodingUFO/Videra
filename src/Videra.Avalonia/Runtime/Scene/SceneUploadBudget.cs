namespace Videra.Avalonia.Runtime.Scene;

/** Budget for one upload drain pass. <see cref="MaxObjectsPerFrame"/> counts
 *  primitive-level objects (<see cref="Videra.Core.Graphics.Object3D"/>), not scene
 *  entries, because a single entry may expand into multiple independently uploadable
 *  primitives. */
internal readonly record struct SceneUploadBudget(
    // Maximum primitive-level objects to upload this frame.
    int MaxObjectsPerFrame,
    long MaxBytesPerFrame)
{
    public static SceneUploadBudget None { get; } = new(0, 0);

    public static SceneUploadBudget Interactive { get; } = new(1, 8L * 1024 * 1024);

    public static SceneUploadBudget Idle { get; } = new(2, 32L * 1024 * 1024);

    public static SceneUploadBudget Resolve(
        bool isInteractive,
        int pendingObjectCount,
        long pendingUploadBytes)
    {
        if (pendingObjectCount <= 0 || pendingUploadBytes <= 0)
        {
            return None;
        }

        if (isInteractive)
        {
            if (pendingObjectCount >= 8 || pendingUploadBytes >= 64L * 1024 * 1024)
            {
                return new SceneUploadBudget(2, 16L * 1024 * 1024);
            }

            return Interactive;
        }

        if (pendingObjectCount >= 12 || pendingUploadBytes >= 96L * 1024 * 1024)
        {
            return new SceneUploadBudget(4, 64L * 1024 * 1024);
        }

        if (pendingObjectCount >= 6 || pendingUploadBytes >= 48L * 1024 * 1024)
        {
            return new SceneUploadBudget(3, 48L * 1024 * 1024);
        }

        return Idle;
    }
}
