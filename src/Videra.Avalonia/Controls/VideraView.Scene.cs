using System.Collections;
using System.Diagnostics;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.IO;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    public async Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var startedAt = Stopwatch.StartNew();
        try
        {
            var factory = ResolveSceneResourceFactory();
            var loadedObject = await Task.Run(() => ModelImporter.Load(path, factory), cancellationToken).ConfigureAwait(true);
            AddObject(loadedObject);
            return ModelLoadResult.Success(path, loadedObject, startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ModelLoadResult.Failed(path, ex, startedAt.Elapsed);
        }
    }

    public async Task<ModelLoadBatchResult> LoadModelsAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var loadedObjects = new List<Object3D>();
        var failures = new List<ModelLoadFailure>();
        var startedAt = Stopwatch.StartNew();

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await LoadModelAsync(path, cancellationToken).ConfigureAwait(true);
            if (result.Succeeded)
            {
                loadedObjects.Add(result.LoadedObject!);
            }
            else if (result.Failure is not null)
            {
                failures.Add(result.Failure);
            }
        }

        return new ModelLoadBatchResult(loadedObjects, failures, startedAt.Elapsed);
    }

    public void AddObject(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Add(obj);
            return;
        }

        Engine.AddObject(obj);
    }

    public void ReplaceScene(IEnumerable<Object3D> objects)
    {
        ArgumentNullException.ThrowIfNull(objects);

        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Clear();
            foreach (var obj in objects)
            {
                boundCollection.Add(obj);
            }

            return;
        }

        Engine.ClearObjects();
        foreach (var obj in objects)
        {
            Engine.AddObject(obj);
        }
    }

    public void ClearScene()
    {
        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Clear();
            return;
        }

        Engine.ClearObjects();
    }

    private IResourceFactory ResolveSceneResourceFactory()
    {
        return GetResourceFactory() ?? new SoftwareResourceFactory();
    }

    private bool TryGetMutableSceneCollection(out IList boundCollection)
    {
        if (Items is IList list && !list.IsReadOnly && !list.IsFixedSize)
        {
            boundCollection = list;
            return true;
        }

        boundCollection = null!;
        return false;
    }
}
