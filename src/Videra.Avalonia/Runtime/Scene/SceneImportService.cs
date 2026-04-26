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
    private IReadOnlyList<IVideraModelImporter> _modelImporters = Array.Empty<IVideraModelImporter>();

    public SceneImportService(
        Func<string, ImportedSceneAsset>? modelImporter = null,
        IEnumerable<IVideraModelImporter>? modelImporters = null)
    {
        _modelImporter = modelImporter;
        _modelImporters = modelImporters?.ToArray() ?? Array.Empty<IVideraModelImporter>();
    }

    public void SetModelImporter(Func<string, ImportedSceneAsset>? modelImporter)
    {
        if (ReferenceEquals(_modelImporter, modelImporter))
        {
            return;
        }

        _modelImporter = modelImporter;
        ClearImportedAssetCache();
    }

    public void SetModelImporters(IEnumerable<IVideraModelImporter>? modelImporters)
    {
        var next = modelImporters?.ToArray() ?? Array.Empty<IVideraModelImporter>();
        if (_modelImporters.SequenceEqual(next))
        {
            return;
        }

        _modelImporters = next;
        ClearImportedAssetCache();
    }

    public async Task<ImportedAssetLoadResult> ImportSingleAsync(string path, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.StartNew();
        try
        {
            var import = await ResolveImportedAssetAsync(path, cancellationToken).ConfigureAwait(false);
            return new ImportedAssetLoadResult(
                import.Asset,
                import.Failure,
                startedAt.Elapsed,
                import.Diagnostics,
                import.ImportDuration);
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
                startedAt.Elapsed,
                CreateErrorDiagnostics(ex),
                ImportDuration: TimeSpan.Zero);
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

        return new ImportedAssetBatchLoadResult(imports, startedAt.Elapsed);
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
                var import = await ResolveImportedAssetAsync(path, cancellationToken).ConfigureAwait(false);
                return new ImportedAssetBatchResult(
                    path,
                    import.Asset,
                    import.Failure,
                    import.Diagnostics,
                    import.ImportDuration);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ImportedAssetBatchResult(
                    path,
                    Asset: null,
                    new ModelLoadFailure(path, ex),
                    CreateErrorDiagnostics(ex),
                    ImportDuration: TimeSpan.Zero);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private static ImportedAssetBatchIndexResult CreateBatchAssetResult(int index, ImportedAssetBatchResult importedAsset)
    {
        return new ImportedAssetBatchIndexResult(
            index,
            importedAsset.Path,
            importedAsset.Asset,
            importedAsset.Failure,
            importedAsset.Diagnostics,
            importedAsset.ImportDuration);
    }

    private async Task<ImportedAssetResolveResult> ResolveImportedAssetAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var registeredImporter = _modelImporters.FirstOrDefault(importer => importer.CanImport(path));
        if (registeredImporter is not null)
        {
            return await ResolveWithRegisteredImporterAsync(path, registeredImporter, cancellationToken).ConfigureAwait(false);
        }

        if (_modelImporter is not null)
        {
            return await ResolveWithDelegateImporterAsync(path, _modelImporter, cancellationToken).ConfigureAwait(false);
        }

        var message = _modelImporters.Count > 0
            ? $"No registered model importer can import '{path}'. Add a matching Videra.Import.* importer to VideraViewOptions.ModelImporters."
            : "No model importer is configured for this VideraView. Add the required Videra.Import.* package(s) and assign VideraViewOptions.ModelImporter or register VideraViewOptions.ModelImporters before calling LoadModelAsync(...) or LoadModelsAsync(...).";
        var exception = new InvalidOperationException(message);
        return ImportedAssetResolveResult.Failed(path, exception, CreateErrorDiagnostics(exception), importDuration: TimeSpan.Zero);
    }

    private async Task<ImportedAssetResolveResult> ResolveWithDelegateImporterAsync(
        string path,
        Func<string, ImportedSceneAsset> importer,
        CancellationToken cancellationToken)
    {
        var key = CreateReuseKey(path, RuntimeHelpers.GetHashCode(importer));
        if (TryGetImportedAsset(key, out var cachedAsset))
        {
            return ImportedAssetResolveResult.Success(path, cachedAsset, Array.Empty<ModelImportDiagnostic>(), importDuration: TimeSpan.Zero);
        }

        var startedAt = Stopwatch.StartNew();
        try
        {
            var asset = await Task.Run(() => importer(path), cancellationToken).ConfigureAwait(false);
            StoreImportedAsset(key, asset);
            return ImportedAssetResolveResult.Success(path, asset, Array.Empty<ModelImportDiagnostic>(), startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ImportedAssetResolveResult.Failed(path, ex, CreateErrorDiagnostics(ex), startedAt.Elapsed);
        }
    }

    private async Task<ImportedAssetResolveResult> ResolveWithRegisteredImporterAsync(
        string path,
        IVideraModelImporter importer,
        CancellationToken cancellationToken)
    {
        var key = CreateReuseKey(path, RuntimeHelpers.GetHashCode(importer));
        if (TryGetImportedAsset(key, out var cachedAsset))
        {
            return ImportedAssetResolveResult.Success(path, cachedAsset, Array.Empty<ModelImportDiagnostic>(), importDuration: TimeSpan.Zero);
        }

        var startedAt = Stopwatch.StartNew();
        try
        {
            var result = await importer.ImportAsync(
                new ModelImportRequest(path, Path.GetFileName(path)),
                cancellationToken).ConfigureAwait(false);
            var importDuration = result.ImportDuration == TimeSpan.Zero ? startedAt.Elapsed : result.ImportDuration;
            if (result.Asset is null)
            {
                var exception = new InvalidOperationException(
                    result.Diagnostics.LastOrDefault(static diagnostic => diagnostic.Severity == ModelImportDiagnosticSeverity.Error)?.Message
                    ?? $"Model importer '{importer.GetType().Name}' did not return an asset for '{path}'.");
                return ImportedAssetResolveResult.Failed(
                    path,
                    exception,
                    EnsureErrorDiagnostic(result.Diagnostics, exception),
                    importDuration);
            }

            StoreImportedAsset(key, result.Asset);
            return ImportedAssetResolveResult.Success(path, result.Asset, result.Diagnostics, importDuration);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ImportedAssetResolveResult.Failed(path, ex, CreateErrorDiagnostics(ex), startedAt.Elapsed);
        }
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

    private void ClearImportedAssetCache()
    {
        lock (_importedAssetGate)
        {
            _importedAssetCache.Clear();
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

    private static ImportedSceneAssetReuseKey CreateReuseKey(string path, int importerIdentity)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return new ImportedSceneAssetReuseKey(
                importerIdentity,
                path,
                fullPath,
                File.Exists(fullPath)
                    ? File.GetLastWriteTimeUtc(fullPath)
                    : DateTime.MinValue);
        }
        catch
        {
            return new ImportedSceneAssetReuseKey(importerIdentity, path, path, DateTime.MinValue);
        }
    }

    private static IReadOnlyList<ModelImportDiagnostic> CreateErrorDiagnostics(Exception exception)
    {
        return [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Error, exception.Message)];
    }

    private static IReadOnlyList<ModelImportDiagnostic> EnsureErrorDiagnostic(
        IReadOnlyList<ModelImportDiagnostic> diagnostics,
        Exception exception)
    {
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == ModelImportDiagnosticSeverity.Error))
        {
            return diagnostics;
        }

        return diagnostics
            .Append(new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Error, exception.Message))
            .ToArray();
    }
}

internal sealed record ImportedAssetLoadResult(
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure,
    TimeSpan Duration,
    IReadOnlyList<ModelImportDiagnostic> Diagnostics,
    TimeSpan ImportDuration);

internal sealed class ImportedAssetBatchLoadResult
{
    public ImportedAssetBatchLoadResult(
        IReadOnlyList<ImportedAssetBatchIndexResult> results,
        TimeSpan duration)
    {
        Results = results ?? throw new ArgumentNullException(nameof(results));
        Assets = Results.Where(static result => result.Asset is not null).Select(static result => result.Asset!).ToArray();
        Failures = Results.Where(static result => result.Failure is not null).Select(static result => result.Failure!).ToArray();
        Duration = duration;
    }

    public IReadOnlyList<ImportedAssetBatchIndexResult> Results { get; }

    public IReadOnlyList<ImportedSceneAsset> Assets { get; }

    public IReadOnlyList<ModelLoadFailure> Failures { get; }

    public TimeSpan Duration { get; }
}

internal sealed record ImportedAssetBatchIndexResult(
    int Index,
    string Path,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure,
    IReadOnlyList<ModelImportDiagnostic> Diagnostics,
    TimeSpan ImportDuration);

internal readonly record struct IndexedImportPath(
    int Index,
    string Path);

internal sealed record ImportedAssetBatchResult(
    string Path,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure,
    IReadOnlyList<ModelImportDiagnostic> Diagnostics,
    TimeSpan ImportDuration);

internal sealed record ImportedAssetResolveResult(
    string Path,
    ImportedSceneAsset? Asset,
    ModelLoadFailure? Failure,
    IReadOnlyList<ModelImportDiagnostic> Diagnostics,
    TimeSpan ImportDuration)
{
    public static ImportedAssetResolveResult Success(
        string path,
        ImportedSceneAsset asset,
        IReadOnlyList<ModelImportDiagnostic> diagnostics,
        TimeSpan importDuration)
    {
        return new ImportedAssetResolveResult(path, asset, Failure: null, diagnostics, importDuration);
    }

    public static ImportedAssetResolveResult Failed(
        string path,
        Exception exception,
        IReadOnlyList<ModelImportDiagnostic> diagnostics,
        TimeSpan importDuration)
    {
        return new ImportedAssetResolveResult(path, Asset: null, new ModelLoadFailure(path, exception), diagnostics, importDuration);
    }
}

internal readonly record struct ImportedSceneAssetReuseKey(
    int ImporterIdentity,
    string RequestPath,
    string FullPath,
    DateTime LastWriteTimeUtc);
