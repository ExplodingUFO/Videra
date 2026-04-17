using Videra.Core.Graphics;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    public Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default) =>
        _runtime.LoadModelAsync(path, cancellationToken);

    public Task<ModelLoadBatchResult> LoadModelsAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default) =>
        _runtime.LoadModelsAsync(paths, cancellationToken);

    public void AddObject(Object3D obj) => _runtime.AddObject(obj);

    public void ReplaceScene(IEnumerable<Object3D> objects) => _runtime.ReplaceScene(objects);

    public void ClearScene() => _runtime.ClearScene();
}
