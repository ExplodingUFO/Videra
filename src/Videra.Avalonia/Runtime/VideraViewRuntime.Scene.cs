using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    public async Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var startedAt = Stopwatch.StartNew();
        var importResult = await _sceneCoordinator.ImportSingleAsync(path, cancellationToken).ConfigureAwait(true);
        if (importResult.Failure is not null)
        {
            return ModelLoadResult.Failed(path, importResult.Failure.Exception, startedAt.Elapsed);
        }

        AppendSceneEntry(importResult.Entry!);
        return ModelLoadResult.Success(path, importResult.SceneObject!, startedAt.Elapsed);
    }

    public async Task<ModelLoadBatchResult> LoadModelsAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var startedAt = Stopwatch.StartNew();
        var importResult = await _sceneCoordinator.ImportBatchAsync(paths, cancellationToken).ConfigureAwait(true);

        if (importResult.Failures.Count == 0)
        {
            ReplaceSceneEntries(importResult.Entries);
        }

        return new ModelLoadBatchResult(importResult.SceneObjects, importResult.Failures, startedAt.Elapsed);
    }

    public void AddObject(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Add(obj);
            return;
        }

        _sceneCoordinator.AddObject(obj);
        RefreshSceneDiagnostics();
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

        _sceneCoordinator.ReplaceScene(objects);
        RefreshSceneDiagnostics();
    }

    public void ClearScene()
    {
        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Clear();
            return;
        }

        _sceneCoordinator.ClearScene();
        RefreshSceneDiagnostics();
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

        _sceneCoordinator.RebuildFromItems(newList);
        RefreshSceneDiagnostics();
    }

    internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _sceneCoordinator.ApplyCollectionChange(sender, e);
        RefreshSceneDiagnostics();
    }

    private void AppendSceneEntry(SceneDocumentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _sceneCoordinator.AppendSceneEntry(entry);
        RefreshSceneDiagnostics();
    }

    private void ReplaceSceneEntries(IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _sceneCoordinator.ReplaceSceneEntries(entries);
        RefreshSceneDiagnostics();
    }

    private void OnSceneBackendReady()
    {
        _sceneCoordinator.OnBackendReady();
        RefreshSceneDiagnostics();
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
