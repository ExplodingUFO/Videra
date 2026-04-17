using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
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
            var loadedObject = SceneUploadCoordinator.CreateDeferredObject(asset);
            AppendSceneEntry(loadedObject, asset);
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

        var startedAt = Stopwatch.StartNew();
        var pathArray = paths.ToArray();
        var maxConcurrency = Math.Clamp(Environment.ProcessorCount, 1, 4);
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var imports = await Task.WhenAll(pathArray.Select(
                (path, index) => ImportSceneEntryAsync(path, index, gate, cancellationToken)))
            .ConfigureAwait(true);

        Array.Sort(imports, static (left, right) => left.Index.CompareTo(right.Index));

        var loadedObjects = imports
            .Where(static result => result.SceneObject is not null)
            .Select(static result => result.SceneObject!)
            .ToArray();
        var failures = imports
            .Where(static result => result.Failure is not null)
            .Select(static result => result.Failure!)
            .ToArray();

        if (failures.Length == 0)
        {
            ReplaceSceneEntries(imports.Select(static result => result.DocumentEntry!));
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

        PublishSceneDocument(_sceneDocument.Add(obj));
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

        PublishSceneDocument(SceneDocument.Empty);
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

        PublishSceneDocument(BuildSceneDocument(EnumerateSceneObjects(newList)));
    }

    internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _ = e;
        PublishSceneDocument(BuildSceneDocument(EnumerateSceneObjects(sender as IEnumerable)));
    }

    private void AppendSceneEntry(Object3D sceneObject, ImportedSceneAsset importedAsset)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        ArgumentNullException.ThrowIfNull(importedAsset);

        PublishSceneDocument(_sceneDocument.Add(sceneObject, importedAsset));
    }

    private void ReplaceSceneEntries(IEnumerable<SceneDocument.DocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        PublishSceneDocument(SceneDocument.Empty.Replace(entries));
    }

    private void PublishSceneDocument(SceneDocument sceneDocument)
    {
        _sceneDocument = sceneDocument ?? SceneDocument.Empty;
        SynchronizeSceneDocumentToEngine();
        SynchronizeOverlayPresentation();
        _renderSession.Invalidate(RenderInvalidationKinds.Scene);
    }

    private void SynchronizeSceneDocumentToEngine()
    {
        var desiredObjects = _sceneDocument.SceneObjects;
        var desiredSet = desiredObjects.ToHashSet(ReferenceEqualityComparer.Instance);

        foreach (var existing in _engine.SceneObjects.ToArray())
        {
            if (!desiredSet.Contains(existing))
            {
                _engine.RemoveObject(existing);
            }
        }

        var resourceFactory = GetResourceFactory();
        var currentSet = _engine.SceneObjects.ToHashSet(ReferenceEqualityComparer.Instance);
        foreach (var sceneObject in desiredObjects)
        {
            if (resourceFactory != null && sceneObject.VertexBuffer == null)
            {
                SceneUploadCoordinator.Upload(sceneObject, resourceFactory, _logger);
            }

            if (!currentSet.Contains(sceneObject))
            {
                _engine.AddObject(sceneObject);
            }
        }
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

        var importedAssetsByObject = new Dictionary<Object3D, ImportedSceneAsset?>(
            ReferenceEqualityComparer.Instance);
        foreach (var entry in _sceneDocument.Entries)
        {
            importedAssetsByObject[entry.SceneObject] = entry.ImportedAsset;
        }

        return SceneDocument.Empty.Replace(sceneObjects.Select(sceneObject =>
        {
            importedAssetsByObject.TryGetValue(sceneObject, out var importedAsset);
            return new SceneDocument.DocumentEntry(sceneObject, importedAsset);
        }));
    }

    private static IEnumerable<Object3D> EnumerateSceneObjects(IEnumerable? sequence)
    {
        if (sequence is null)
        {
            return Array.Empty<Object3D>();
        }

        return sequence.OfType<Object3D>().ToArray();
    }

    private static async Task<ImportedSceneEntryResult> ImportSceneEntryAsync(
        string path,
        int index,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var asset = await Task.Run(() => ModelImporter.Import(path), cancellationToken).ConfigureAwait(false);
            var sceneObject = SceneUploadCoordinator.CreateDeferredObject(asset);
            return new ImportedSceneEntryResult(
                index,
                new SceneDocument.DocumentEntry(sceneObject, asset),
                sceneObject,
                Failure: null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ImportedSceneEntryResult(
                index,
                DocumentEntry: null,
                SceneObject: null,
                new ModelLoadFailure(path, ex));
        }
        finally
        {
            gate.Release();
        }
    }

    private sealed record ImportedSceneEntryResult(
        int Index,
        SceneDocument.DocumentEntry? DocumentEntry,
        Object3D? SceneObject,
        ModelLoadFailure? Failure);
}
