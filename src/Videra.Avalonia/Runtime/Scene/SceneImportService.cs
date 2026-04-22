using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Core.Exceptions;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Videra.Import.Gltf;
using Videra.Import.Obj;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneImportService
{
    private readonly object _importedAssetGate = new();
    private readonly Dictionary<ImportedSceneAssetReuseKey, WeakReference<ImportedSceneAsset>> _importedAssetCache = [];
    private readonly SceneDocumentMutator _mutator;

    public SceneImportService(SceneDocumentMutator mutator)
    {
        _mutator = mutator ?? throw new ArgumentNullException(nameof(mutator));
    }

    public async Task<ImportedSceneResult> ImportSingleAsync(string path, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.StartNew();
        try
        {
            var asset = await Task.Run(() => ResolveImportedAsset(path), cancellationToken).ConfigureAwait(false);
            var sceneObject = SceneObjectFactory.CreateDeferred(asset);
            var entry = _mutator.CreateImportedEntry(sceneObject, asset);
            return new ImportedSceneResult(entry, null, startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ImportedSceneResult(
                null,
                new ModelLoadFailure(path, ex),
                startedAt.Elapsed);
        }
    }

    public async Task<ImportedSceneBatchResult> ImportBatchAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
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

        foreach (var indexedPath in indexedPaths)
        {
            if (!importTasks.ContainsKey(indexedPath.Path))
            {
                importTasks.Add(indexedPath.Path, ImportBatchAssetAsync(indexedPath.Path, gate, cancellationToken));
            }
        }

        var importedAssets = await Task.WhenAll(importTasks.Values).ConfigureAwait(false);
        var importedAssetByPath = importedAssets.ToDictionary(static result => result.Path, StringComparer.Ordinal);
        var imports = indexedPaths
            .Select(indexedPath => CreateBatchEntryResult(indexedPath.Index, importedAssetByPath[indexedPath.Path]))
            .ToArray();

        Array.Sort(imports, static (left, right) => left.Index.CompareTo(right.Index));

        return new ImportedSceneBatchResult(
            imports.Where(static result => result.Entry is not null).Select(static result => result.Entry!).ToArray(),
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
                var asset = await Task.Run(() => ImportAsset(path), cancellationToken).ConfigureAwait(false);
                StoreImportedAsset(CreateReuseKey(path), asset);
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

    private ImportedSceneBatchEntryResult CreateBatchEntryResult(int index, ImportedAssetBatchResult importedAsset)
    {
        if (importedAsset.Asset is null)
        {
            return new ImportedSceneBatchEntryResult(index, null, importedAsset.Failure);
        }

        var sceneObject = SceneObjectFactory.CreateDeferred(importedAsset.Asset);
        var entry = _mutator.CreateImportedEntry(sceneObject, importedAsset.Asset);
        return new ImportedSceneBatchEntryResult(index, entry, Failure: null);
    }

    private ImportedSceneAsset ResolveImportedAsset(string path)
    {
        var key = CreateReuseKey(path);
        if (TryGetImportedAsset(key, out var asset))
        {
            return asset;
        }

        asset = ImportAsset(path);
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

    private static ImportedSceneAssetReuseKey CreateReuseKey(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return new ImportedSceneAssetReuseKey(
                fullPath,
                File.Exists(fullPath)
                    ? File.GetLastWriteTimeUtc(fullPath)
                    : DateTime.MinValue);
        }
        catch
        {
            return new ImportedSceneAssetReuseKey(path, DateTime.MinValue);
        }
    }

    private static ImportedSceneAsset ImportAsset(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".gltf" or ".glb" => GltfModelImporter.Import(path),
            ".obj" => ObjModelImporter.Import(path),
            _ => throw new InvalidModelInputException(
                $"File extension '{Path.GetExtension(path)}' is not supported. Supported formats: {string.Join(", ", GltfModelImporter.SupportedFormats.Concat(ObjModelImporter.SupportedFormats))}",
                "LoadModel",
                new Dictionary<string, string?> { ["Extension"] = Path.GetExtension(path) })
        };
    }
}

internal sealed record ImportedSceneResult(
    SceneDocumentEntry? Entry,
    ModelLoadFailure? Failure,
    TimeSpan Duration);

internal sealed record ImportedSceneBatchResult(
    IReadOnlyList<SceneDocumentEntry> Entries,
    IReadOnlyList<ModelLoadFailure> Failures,
    TimeSpan Duration);

internal sealed record ImportedSceneBatchEntryResult(
    int Index,
    SceneDocumentEntry? Entry,
    ModelLoadFailure? Failure);

internal readonly record struct IndexedImportPath(
    int Index,
    string Path);

internal sealed record ImportedAssetBatchResult(
    string Path,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure);

internal readonly record struct ImportedSceneAssetReuseKey(
    string FullPath,
    DateTime LastWriteTimeUtc);
