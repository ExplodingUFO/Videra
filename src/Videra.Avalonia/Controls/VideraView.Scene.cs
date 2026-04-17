using Videra.Core.Graphics;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    /// <summary>
    /// Imports a single model file into a backend-neutral scene asset, appends it to the retained scene document,
    /// and schedules GPU upload when a ready resource factory is available.
    /// </summary>
    /// <param name="path">Absolute or relative path to a supported model file.</param>
    /// <param name="cancellationToken">Cancellation token for the import work.</param>
    /// <returns>
    /// A result describing the loaded object, failure information when import fails, and total elapsed time.
    /// </returns>
    public Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default) =>
        _runtime.LoadModelAsync(path, cancellationToken);

    /// <summary>
    /// Imports a bounded batch of model files and replaces the active scene only when every requested file succeeds.
    /// </summary>
    /// <param name="paths">The model file paths to import.</param>
    /// <param name="cancellationToken">Cancellation token for the batch import work.</param>
    /// <returns>
    /// A batch result containing every successfully imported object, any failures, and the total batch duration.
    /// </returns>
    public Task<ModelLoadBatchResult> LoadModelsAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default) =>
        _runtime.LoadModelsAsync(paths, cancellationToken);

    /// <summary>
    /// Adds a host-owned scene object to the retained scene document and schedules upload if the object is not yet resident.
    /// </summary>
    /// <param name="obj">The object to add to the active scene.</param>
    public void AddObject(Object3D obj) => _runtime.AddObject(obj);

    /// <summary>
    /// Replaces the active scene with the supplied objects.
    /// </summary>
    /// <param name="objects">The objects that should become the new active scene.</param>
    public void ReplaceScene(IEnumerable<Object3D> objects) => _runtime.ReplaceScene(objects);

    /// <summary>
    /// Clears every object from the active scene.
    /// </summary>
    public void ClearScene() => _runtime.ClearScene();
}
