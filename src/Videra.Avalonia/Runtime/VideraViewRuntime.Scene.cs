using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.IO;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    public async Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var startedAt = Stopwatch.StartNew();
        try
        {
            var asset = await Task.Run(() => ModelImporter.Import(path), cancellationToken).ConfigureAwait(true);
            var loadedObject = SceneUploadCoordinator.Upload(asset, ResolveSceneResourceFactory());
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

        _engine.AddObject(obj);
        _sceneDocument = new SceneDocument(_engine.SceneObjects);
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
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

        _engine.ClearObjects();
        foreach (var obj in objects)
        {
            _engine.AddObject(obj);
        }

        _sceneDocument = new SceneDocument(_engine.SceneObjects);
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    public void ClearScene()
    {
        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Clear();
            return;
        }

        _engine.ClearObjects();
        _sceneDocument = SceneDocument.Empty;
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    public IResourceFactory? GetResourceFactory() => _renderSession.ResourceFactory;

    internal void UpdateItemsSubscription(IEnumerable? oldList, IEnumerable? newList)
    {
        if (oldList is INotifyCollectionChanged oldIncc)
        {
            oldIncc.CollectionChanged -= OnCollectionChanged;
        }

        if (newList is INotifyCollectionChanged newIncc)
        {
            newIncc.CollectionChanged += OnCollectionChanged;
        }

        _engine.ClearObjects();
        if (newList is not null)
        {
            foreach (var item in newList)
            {
                if (item is Object3D obj)
                {
                    _engine.AddObject(obj);
                }
            }
        }

        _sceneDocument = new SceneDocument(_engine.SceneObjects);
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (Object3D item in e.NewItems)
            {
                _engine.AddObject(item);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (Object3D item in e.OldItems)
            {
                _engine.RemoveObject(item);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _engine.ClearObjects();
        }

        _sceneDocument = new SceneDocument(_engine.SceneObjects);
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    private IResourceFactory ResolveSceneResourceFactory()
    {
        return GetResourceFactory() ?? new SoftwareResourceFactory();
    }

    private bool TryGetMutableSceneCollection(out IList boundCollection)
    {
        if (_owner.Items is IList list && !list.IsReadOnly && !list.IsFixedSize)
        {
            boundCollection = list;
            return true;
        }

        boundCollection = null!;
        return false;
    }
}
