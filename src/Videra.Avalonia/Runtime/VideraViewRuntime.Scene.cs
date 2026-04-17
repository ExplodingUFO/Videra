using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;
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
        var importResult = await _sceneImportService.ImportSingleAsync(path, cancellationToken).ConfigureAwait(true);
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
        var importResult = await _sceneImportService.ImportBatchAsync(paths, cancellationToken).ConfigureAwait(true);

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

        PublishSceneDocument(_sceneDocumentMutator.Add(_sceneDocument, obj));
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

        PublishSceneDocument(BuildSceneDocument(objects));
    }

    public void ClearScene()
    {
        if (TryGetMutableSceneCollection(out var boundCollection))
        {
            boundCollection.Clear();
            return;
        }

        PublishSceneDocument(_sceneDocumentMutator.Clear(_sceneDocument));
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

        PublishSceneDocument(_sceneItemsAdapter.Rebuild(_sceneDocument, newList));
    }

    internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PublishSceneDocument(_sceneItemsAdapter.ApplyChange(_sceneDocument, e, sender as IEnumerable));
    }

    private void AppendSceneEntry(SceneDocumentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        PublishSceneDocument(_sceneDocumentMutator.ReplaceEntries(_sceneDocument, _sceneDocument.Entries.Append(entry)));
    }

    private void ReplaceSceneEntries(IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        PublishSceneDocument(_sceneDocumentMutator.ReplaceEntries(SceneDocument.Empty, entries));
    }

    private void PublishSceneDocument(SceneDocument sceneDocument)
    {
        var (previous, current) = _sceneDocumentStore.Publish(sceneDocument ?? SceneDocument.Empty);
        _sceneDocument = current;

        var delta = _sceneDeltaPlanner.Diff(previous, current);
        _sceneResidencyRegistry.Apply(delta, _sceneResourceEpoch);
        _sceneEngineApplicator.ApplyRemovals(_engine, delta.Removed, _sceneResidencyRegistry);
        _sceneEngineApplicator.ApplyReadyAdds(_engine, _sceneResidencyRegistry.GetReadyAdds(delta.Added), _sceneResidencyRegistry);
        _sceneUploadQueue.Enqueue(_sceneResidencyRegistry.GetPendingCandidates());

        if (delta.RequiresOverlayRefresh)
        {
            SynchronizeOverlayPresentation();
        }

        RefreshSceneDiagnostics();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    private void OnSceneBackendReady()
    {
        _sceneResourceEpoch++;
        _sceneUploadQueue.Enqueue(_sceneResidencyRegistry.MarkDirtyForResourceEpoch(_sceneResourceEpoch));
        _sceneEngineApplicator.ApplyReadyAdds(_engine, _sceneResidencyRegistry.GetReadyAdds(_sceneDocument.Entries), _sceneResidencyRegistry);
        RefreshSceneDiagnostics();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
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

    private SceneDocument BuildSceneDocument(IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(sceneObjects);
        return _sceneItemsAdapter.Rebuild(_sceneDocument, sceneObjects);
    }

    private static IEnumerable<Object3D> EnumerateSceneObjects(IEnumerable? sequence)
    {
        if (sequence is null)
        {
            return Array.Empty<Object3D>();
        }

        return sequence.OfType<Object3D>().ToArray();
    }
}
