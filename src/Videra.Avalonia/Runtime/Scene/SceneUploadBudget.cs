namespace Videra.Avalonia.Runtime.Scene;

internal readonly record struct SceneUploadBudget(
    int MaxObjectsPerFrame,
    long MaxBytesPerFrame)
{
    public static SceneUploadBudget Interactive { get; } = new(1, 8L * 1024 * 1024);

    public static SceneUploadBudget Idle { get; } = new(2, 32L * 1024 * 1024);
}
