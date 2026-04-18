using System.Collections;
using System.Collections.Specialized;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneRuntimeCoordinator
{
    private readonly VideraEngine _engine;
    private readonly Action _refreshOverlay;
    private readonly Action _refreshSceneDiagnostics;
    private readonly Action _refreshBackendDiagnostics;
    private readonly Action<RenderInvalidationKinds> _invalidateRender;

    private readonly SceneDocumentMutator _sceneDocumentMutator = new();
    private readonly SceneDocumentStore _sceneDocumentStore;
    private readonly SceneDeltaPlanner _sceneDeltaPlanner = new();
    private readonly SceneResidencyRegistry _sceneResidencyRegistry = new();
    private readonly SceneUploadQueue _sceneUploadQueue = new();
    private readonly SceneEngineApplicator _sceneEngineApplicator = new();
    private readonly SceneItemsAdapter _sceneItemsAdapter;
    private readonly SceneImportService _sceneImportService;
    private IReadOnlyList<VideraClipPlane> _clippingPlanes = Array.Empty<VideraClipPlane>();

    public SceneRuntimeCoordinator(
        VideraEngine engine,
        Action refreshOverlay,
        Action refreshSceneDiagnostics,
        Action refreshBackendDiagnostics,
        Action<RenderInvalidationKinds> invalidateRender)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _refreshOverlay = refreshOverlay ?? throw new ArgumentNullException(nameof(refreshOverlay));
        _refreshSceneDiagnostics = refreshSceneDiagnostics ?? throw new ArgumentNullException(nameof(refreshSceneDiagnostics));
        _refreshBackendDiagnostics = refreshBackendDiagnostics ?? throw new ArgumentNullException(nameof(refreshBackendDiagnostics));
        _invalidateRender = invalidateRender ?? throw new ArgumentNullException(nameof(invalidateRender));

        CurrentDocument = SceneDocument.Empty;
        _sceneDocumentStore = new SceneDocumentStore(CurrentDocument);
        _sceneItemsAdapter = new SceneItemsAdapter(_sceneDocumentMutator);
        _sceneImportService = new SceneImportService(_sceneDocumentMutator);
    }

    public SceneDocument CurrentDocument { get; private set; }

    public SceneResidencyRegistry ResidencyRegistry => _sceneResidencyRegistry;

    public SceneUploadQueue UploadQueue => _sceneUploadQueue;

    public ulong ResourceEpoch { get; private set; } = 1;

    public IReadOnlyList<Object3D> SceneObjects => CurrentDocument.SceneObjects;

    public IReadOnlyList<VideraClipPlane> ClippingPlanes => _clippingPlanes;

    public Task<ImportedSceneResult> ImportSingleAsync(string path, CancellationToken cancellationToken)
    {
        return _sceneImportService.ImportSingleAsync(path, cancellationToken);
    }

    public Task<ImportedSceneBatchResult> ImportBatchAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
    {
        return _sceneImportService.ImportBatchAsync(paths, cancellationToken);
    }

    public void AddObject(Object3D sceneObject)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        PublishSceneDocument(_sceneDocumentMutator.Add(CurrentDocument, sceneObject));
    }

    public void ReplaceScene(IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(sceneObjects);
        PublishSceneDocument(_sceneItemsAdapter.Rebuild(CurrentDocument, sceneObjects));
    }

    public void ClearScene()
    {
        PublishSceneDocument(_sceneDocumentMutator.Clear(CurrentDocument));
    }

    public void RebuildFromItems(IEnumerable? items)
    {
        PublishSceneDocument(_sceneItemsAdapter.Rebuild(CurrentDocument, items));
    }

    public void ApplyCollectionChange(object? sender, NotifyCollectionChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        PublishSceneDocument(_sceneItemsAdapter.ApplyChange(CurrentDocument, change, sender as IEnumerable));
    }

    public void AppendSceneEntry(SceneDocumentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        PublishSceneDocument(_sceneDocumentMutator.ReplaceEntries(CurrentDocument, CurrentDocument.Entries.Append(entry)));
    }

    public void ReplaceSceneEntries(IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        PublishSceneDocument(_sceneDocumentMutator.ReplaceEntries(SceneDocument.Empty, entries));
    }

    public void OnBackendReady()
    {
        ResourceEpoch++;
        _sceneUploadQueue.Enqueue(_sceneResidencyRegistry.MarkDirtyForResourceEpoch(ResourceEpoch));
        _sceneEngineApplicator.ApplyReadyAdds(_engine, _sceneResidencyRegistry.GetReadyAdds(CurrentDocument.Entries), _sceneResidencyRegistry);
        _refreshSceneDiagnostics();
        _refreshBackendDiagnostics();
        _invalidateRender(RenderInvalidationKinds.Scene);
    }

    public void UpdateClippingPlanes(IReadOnlyList<VideraClipPlane>? clippingPlanes)
    {
        var normalizedPlanes = clippingPlanes?
            .Where(static plane => plane.IsEnabled)
            .ToArray() ?? Array.Empty<VideraClipPlane>();

        if (_clippingPlanes.SequenceEqual(normalizedPlanes))
        {
            return;
        }

        _clippingPlanes = normalizedPlanes;
        ApplyClippingToEntries(CurrentDocument.Entries);
        _sceneUploadQueue.Enqueue(_sceneResidencyRegistry.MarkDirty(CurrentDocument.Entries, ResourceEpoch));
        _refreshOverlay();
        _refreshSceneDiagnostics();
        _refreshBackendDiagnostics();
        _invalidateRender(RenderInvalidationKinds.Scene);
    }

    public SceneResidencyDiagnostics CreateDiagnostics(SceneUploadFlushResult lastFlush, SceneUploadBudget lastResolvedBudget)
    {
        return _sceneResidencyRegistry.CreateDiagnostics(CurrentDocument.Version, lastFlush, lastResolvedBudget);
    }

    private void PublishSceneDocument(SceneDocument sceneDocument)
    {
        var (previous, current) = _sceneDocumentStore.Publish(sceneDocument ?? SceneDocument.Empty);
        ApplyClippingToEntries(current.Entries);
        CurrentDocument = current;

        var delta = _sceneDeltaPlanner.Diff(previous, current);
        _sceneResidencyRegistry.Apply(delta, ResourceEpoch);
        _sceneEngineApplicator.ApplyRemovals(_engine, delta.Removed, _sceneResidencyRegistry);
        _sceneEngineApplicator.ApplyReadyAdds(_engine, _sceneResidencyRegistry.GetReadyAdds(delta.Added), _sceneResidencyRegistry);
        _sceneUploadQueue.Enqueue(_sceneResidencyRegistry.GetPendingCandidates());

        if (delta.RequiresOverlayRefresh)
        {
            _refreshOverlay();
        }

        _refreshSceneDiagnostics();
        _refreshBackendDiagnostics();
        _invalidateRender(RenderInvalidationKinds.Scene);
    }

    private void ApplyClippingToEntries(IEnumerable<SceneDocumentEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.SceneObject.ApplyClippingPlanes(_clippingPlanes);
        }
    }
}
