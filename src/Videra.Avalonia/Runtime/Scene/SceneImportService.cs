using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.IO;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneImportService
{
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
            var asset = await Task.Run(() => ModelImporter.Import(path), cancellationToken).ConfigureAwait(false);
            var sceneObject = SceneObjectFactory.CreateDeferred(asset);
            var entry = _mutator.CreateImportedEntry(sceneObject, asset);
            return new ImportedSceneResult(entry, sceneObject, Failure: null, startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ImportedSceneResult(
                Entry: null,
                SceneObject: null,
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

        var imports = await Task.WhenAll(pathArray.Select(
                (path, index) => ImportBatchEntryAsync(path, index, gate, cancellationToken)))
            .ConfigureAwait(false);

        Array.Sort(imports, static (left, right) => left.Index.CompareTo(right.Index));

        return new ImportedSceneBatchResult(
            imports.Where(static result => result.Entry is not null).Select(static result => result.Entry!).ToArray(),
            imports.Where(static result => result.SceneObject is not null).Select(static result => result.SceneObject!).ToArray(),
            imports.Where(static result => result.Failure is not null).Select(static result => result.Failure!).ToArray(),
            startedAt.Elapsed);
    }

    private async Task<ImportedSceneBatchEntryResult> ImportBatchEntryAsync(
        string path,
        int index,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var result = await ImportSingleAsync(path, cancellationToken).ConfigureAwait(false);
            return new ImportedSceneBatchEntryResult(index, result.Entry, result.SceneObject, result.Failure);
        }
        finally
        {
            gate.Release();
        }
    }
}

internal sealed record ImportedSceneResult(
    SceneDocumentEntry? Entry,
    Object3D? SceneObject,
    ModelLoadFailure? Failure,
    TimeSpan Duration);

internal sealed record ImportedSceneBatchResult(
    IReadOnlyList<SceneDocumentEntry> Entries,
    IReadOnlyList<Object3D> SceneObjects,
    IReadOnlyList<ModelLoadFailure> Failures,
    TimeSpan Duration);

internal sealed record ImportedSceneBatchEntryResult(
    int Index,
    SceneDocumentEntry? Entry,
    Object3D? SceneObject,
    ModelLoadFailure? Failure);
