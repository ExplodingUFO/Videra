using Videra.Core.Graphics;

namespace Videra.Avalonia.Controls;

public sealed class ModelLoadResult
{
    private ModelLoadResult(string path, Object3D? loadedObject, ModelLoadFailure? failure, TimeSpan duration)
    {
        Path = path;
        LoadedObject = loadedObject;
        Failure = failure;
        Duration = duration;
    }

    public string Path { get; }

    public Object3D? LoadedObject { get; }

    public ModelLoadFailure? Failure { get; }

    public TimeSpan Duration { get; }

    public bool Succeeded => Failure is null && LoadedObject is not null;

    public static ModelLoadResult Success(string path, Object3D loadedObject, TimeSpan duration)
    {
        return new ModelLoadResult(path, loadedObject, failure: null, duration);
    }

    public static ModelLoadResult Failed(string path, Exception exception, TimeSpan duration)
    {
        return new ModelLoadResult(path, loadedObject: null, new ModelLoadFailure(path, exception), duration);
    }
}

public sealed class ModelLoadBatchResult
{
    public ModelLoadBatchResult(
        IReadOnlyList<Object3D> loadedObjects,
        IReadOnlyList<ModelLoadFailure> failures,
        TimeSpan duration)
    {
        LoadedObjects = loadedObjects ?? throw new ArgumentNullException(nameof(loadedObjects));
        Failures = failures ?? throw new ArgumentNullException(nameof(failures));
        Duration = duration;
    }

    public IReadOnlyList<Object3D> LoadedObjects { get; }

    public IReadOnlyList<ModelLoadFailure> Failures { get; }

    public TimeSpan Duration { get; }

    public bool Succeeded => Failures.Count == 0;
}

public sealed class ModelLoadFailure
{
    public ModelLoadFailure(string path, Exception exception)
    {
        Path = path;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        ErrorMessage = exception.Message;
    }

    public string Path { get; }

    public string ErrorMessage { get; }

    public Exception Exception { get; }
}
