using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using System.Diagnostics;

namespace Videra.Avalonia.Runtime.Scene;

/** Uploads primitive-level runtime objects (<see cref="Videra.Core.Graphics.Object3D"/>)
 *  to the GPU. Budgets are counted per-primitive, not per-entry, because one entry may
 *  own multiple independently uploadable objects. */
internal sealed class SceneUploadQueue
{
    private readonly Dictionary<SceneEntryId, long> _pendingIds = [];
    private long _nextSequence;

    public void Enqueue(IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        foreach (var entry in entries)
        {
            Enqueue(entry.Id);
        }
    }

    public void Enqueue(IEnumerable<SceneResidencyRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        foreach (var record in records)
        {
            Enqueue(record.Id);
        }
    }

    public int PendingCount => _pendingIds.Count;

    public SceneUploadFlushResult Drain(
        IResourceFactory? factory,
        SceneUploadBudget budget,
        ulong resourceEpoch,
        SceneResidencyRegistry registry,
        ILogger logger,
        bool preferAttachedEntries)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(logger);

        if (factory is null || budget.MaxObjectsPerFrame <= 0 || budget.MaxBytesPerFrame <= 0)
        {
            return SceneUploadFlushResult.Empty with
            {
                ResolvedBudget = budget
            };
        }

        var stopwatch = Stopwatch.StartNew();
        var uploaded = new List<SceneResidencyRecord>();
        var failed = 0;
        var uploadedBytes = 0L;
        var processedObjects = 0;

        while (_pendingIds.Count > 0)
        {
            if (processedObjects >= budget.MaxObjectsPerFrame)
            {
                break;
            }

            if (!TrySelectNextCandidate(registry, preferAttachedEntries, out var id, out var record))
            {
                break;
            }

            var uploadBytes = Math.Max(record.ApproximateUploadBytes, 1L);
            if (processedObjects > 0 && uploadedBytes + uploadBytes > budget.MaxBytesPerFrame)
            {
                break;
            }

            _pendingIds.Remove(id);
            processedObjects++;
            uploadedBytes += uploadBytes;

            try
            {
                registry.MarkUploading(id);
                foreach (var runtimeObject in record.RuntimeObjects)
                {
                    SceneObjectUploader.Upload(runtimeObject, factory, logger);
                }

                registry.MarkResident(id, resourceEpoch);
                if (registry.TryGet(id, out var resident))
                {
                    uploaded.Add(resident);
                }
            }
            catch (Exception ex)
            {
                registry.MarkFailed(id, ex);
                failed++;
            }
        }

        stopwatch.Stop();
        return new SceneUploadFlushResult(uploaded, failed, uploadedBytes, budget, stopwatch.Elapsed);
    }

    private void Enqueue(SceneEntryId id)
    {
        if (!_pendingIds.ContainsKey(id))
        {
            _pendingIds[id] = _nextSequence++;
        }
    }

    private bool TrySelectNextCandidate(
        SceneResidencyRegistry registry,
        bool preferAttachedEntries,
        out SceneEntryId id,
        out SceneResidencyRecord record)
    {
        foreach (var candidate in _pendingIds
                     .OrderByDescending(pair => ResolvePriority(pair.Key, registry, preferAttachedEntries))
                     .ThenBy(pair => pair.Value)
                     .Select(pair => pair.Key)
                     .ToArray())
        {
            if (!registry.TryGet(candidate, out var resolved) ||
                (resolved.State != SceneResidencyState.PendingUpload &&
                 resolved.State != SceneResidencyState.Dirty))
            {
                _pendingIds.Remove(candidate);
                continue;
            }

            id = candidate;
            record = resolved;
            return true;
        }

        id = default;
        record = default!;
        return false;
    }

    private static int ResolvePriority(
        SceneEntryId id,
        SceneResidencyRegistry registry,
        bool preferAttachedEntries)
    {
        if (!registry.TryGet(id, out var record))
        {
            return int.MinValue;
        }

        if (preferAttachedEntries && record.IsAttachedToEngine && record.State == SceneResidencyState.Dirty)
        {
            return 2;
        }

        if (record.State == SceneResidencyState.Dirty)
        {
            return 1;
        }

        return 0;
    }
}

internal sealed record SceneUploadFlushResult(
    IReadOnlyList<SceneResidencyRecord> UploadedRecords,
    int FailedCount,
    long UploadedBytes,
    SceneUploadBudget ResolvedBudget,
    TimeSpan Duration)
{
    public static SceneUploadFlushResult Empty { get; } = new(
        Array.Empty<SceneResidencyRecord>(),
        0,
        0,
        SceneUploadBudget.None,
        TimeSpan.Zero);

    public bool HasChanges => UploadedRecords.Count > 0 || FailedCount > 0;
}
