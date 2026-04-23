using System.Diagnostics;
using System.Runtime.CompilerServices;
using Videra.Avalonia.Controls;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneImportService
{
    private readonly object _importedAssetGate = new();
    private readonly Dictionary<ImportedSceneAssetReuseKey, WeakReference<ImportedSceneAsset>> _importedAssetCache = [];
    private Func<string, ImportedSceneAsset>? _modelImporter;

    public SceneImportService(Func<string, ImportedSceneAsset>? modelImporter = null)
    {
        _modelImporter = modelImporter;
    }

    public void SetModelImporter(Func<string, ImportedSceneAsset>? modelImporter)
    {
        if (ReferenceEquals(_modelImporter, modelImporter))
        {
            return;
        }

        _modelImporter = modelImporter;
        lock (_importedAssetGate)
        {
            _importedAssetCache.Clear();
        }
    }

    public async Task<ImportedAssetLoadResult> ImportSingleAsync(string path, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.StartNew();
        try
        {
            var asset = await Task.Run(() => ResolveImportedAsset(path), cancellationToken).ConfigureAwait(false);
            return new ImportedAssetLoadResult(asset, null, startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ImportedAssetLoadResult(
                null,
                new ModelLoadFailure(path, ex),
                startedAt.Elapsed);
        }
    }

    public async Task<ImportedAssetBatchLoadResult> ImportBatchAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var pathArray = paths.ToArray();
        var startedAt = Stopwatch.StartNew();
        var maxConcurrency = Math.Clamp(Environment.ProcessorCount, 1, 4);
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var indexedPaths = pathArray
            .Select((path, index) => new IndexedImportPath(index, path))
            .ToArray();
        var importTasks = new Dictionary<string, Task<ImportedAssetBatchResult>>(StringComparer.Ordinal);

        foreach (var path in indexedPaths.Select(static indexedPath => indexedPath.Path).Distinct(StringComparer.Ordinal))
        {
            importTasks.Add(path, ImportBatchAssetAsync(path, gate, cancellationToken));
        }

        var importedAssets = await Task.WhenAll(importTasks.Values).ConfigureAwait(false);
        var importedAssetByPath = importedAssets.ToDictionary(static result => result.Path, StringComparer.Ordinal);
        var imports = indexedPaths
            .Select(indexedPath => CreateBatchAssetResult(indexedPath.Index, importedAssetByPath[indexedPath.Path]))
            .ToArray();

        Array.Sort(imports, static (left, right) => left.Index.CompareTo(right.Index));

        return new ImportedAssetBatchLoadResult(
            imports.Where(static result => result.Asset is not null).Select(static result => result.Asset!).ToArray(),
            imports.Where(static result => result.Failure is not null).Select(static result => result.Failure!).ToArray(),
            startedAt.Elapsed);
    }

    private async Task<ImportedAssetBatchResult> ImportBatchAssetAsync(
        string path,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                var asset = await Task.Run(() => ResolveImportedAsset(path), cancellationToken).ConfigureAwait(false);
                return new ImportedAssetBatchResult(path, asset, Failure: null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ImportedAssetBatchResult(path, Asset: null, new ModelLoadFailure(path, ex));
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private static ImportedAssetBatchIndexResult CreateBatchAssetResult(int index, ImportedAssetBatchResult importedAsset)
    {
        if (importedAsset.Asset is null)
        {
            return new ImportedAssetBatchIndexResult(index, null, importedAsset.Failure);
        }

        return new ImportedAssetBatchIndexResult(index, importedAsset.Asset, Failure: null);
    }

    private ImportedSceneAsset ResolveImportedAsset(string path)
    {
        var importer = _modelImporter
            ?? throw new InvalidOperationException(
                "No model importer is configured for this VideraView. Add the required Videra.Import.* package(s) and assign VideraViewOptions.ModelImporter before calling LoadModelAsync(...) or LoadModelsAsync(...).");
        var key = CreateReuseKey(path, importer);
        if (TryGetImportedAsset(key, out var asset))
        {
            return asset;
        }

        asset = importer(path);
        StoreImportedAsset(key, asset);
        return asset;
    }

    private bool TryGetImportedAsset(ImportedSceneAssetReuseKey key, out ImportedSceneAsset asset)
    {
        lock (_importedAssetGate)
        {
            CleanupImportedAssetCache(key);
            if (_importedAssetCache.TryGetValue(key, out var reference) &&
                reference.TryGetTarget(out asset!))
            {
                return true;
            }

            _importedAssetCache.Remove(key);
        }

        asset = null!;
        return false;
    }

    private void StoreImportedAsset(ImportedSceneAssetReuseKey key, ImportedSceneAsset asset)
    {
        lock (_importedAssetGate)
        {
            CleanupImportedAssetCache(key);
            _importedAssetCache[key] = new WeakReference<ImportedSceneAsset>(asset);
        }
    }

    private void CleanupImportedAssetCache(ImportedSceneAssetReuseKey currentKey)
    {
        foreach (var entry in _importedAssetCache.ToArray())
        {
            if (!entry.Value.TryGetTarget(out _) ||
                (entry.Key.FullPath == currentKey.FullPath && entry.Key != currentKey))
            {
                _importedAssetCache.Remove(entry.Key);
            }
        }
    }

    private static ImportedSceneAssetReuseKey CreateReuseKey(string path, Func<string, ImportedSceneAsset> modelImporter)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return new ImportedSceneAssetReuseKey(
                RuntimeHelpers.GetHashCode(modelImporter),
                path,
                fullPath,
                File.Exists(fullPath)
                    ? File.GetLastWriteTimeUtc(fullPath)
                    : DateTime.MinValue);
        }
        catch
        {
            return new ImportedSceneAssetReuseKey(RuntimeHelpers.GetHashCode(modelImporter), path, path, DateTime.MinValue);
        }
    }
}

internal sealed record ImportedAssetLoadResult(
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure,
    TimeSpan Duration);

internal sealed record ImportedAssetBatchLoadResult(
    IReadOnlyList<ImportedSceneAsset> Assets,
    IReadOnlyList<ModelLoadFailure> Failures,
    TimeSpan Duration);

internal sealed record ImportedAssetBatchIndexResult(
    int Index,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure);

internal readonly record struct IndexedImportPath(
    int Index,
    string Path);

internal sealed record ImportedAssetBatchResult(
    string Path,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure);

internal readonly record struct ImportedSceneAssetReuseKey(
    int ImporterIdentity,
    string RequestPath,
    string FullPath,
    DateTime LastWriteTimeUtc);
