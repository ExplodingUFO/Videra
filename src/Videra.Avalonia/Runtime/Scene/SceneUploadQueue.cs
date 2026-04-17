using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneUploadQueue
{
    private readonly Queue<SceneEntryId> _pendingIds = new();
    private readonly HashSet<SceneEntryId> _enqueuedIds = [];

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

    public SceneUploadFlushResult Drain(
        IResourceFactory? factory,
        SceneUploadBudget budget,
        ulong resourceEpoch,
        SceneResidencyRegistry registry,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(logger);

        if (factory is null || budget.MaxObjectsPerFrame <= 0 || budget.MaxBytesPerFrame <= 0)
        {
            return SceneUploadFlushResult.Empty;
        }

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

            var id = _pendingIds.Peek();
            if (!registry.TryGet(id, out var record) ||
                (record.State != SceneResidencyState.PendingUpload &&
                 record.State != SceneResidencyState.Dirty))
            {
                _pendingIds.Dequeue();
                _enqueuedIds.Remove(id);
                continue;
            }

            var uploadBytes = Math.Max(record.ApproximateUploadBytes, 1L);
            if (processedObjects > 0 && uploadedBytes + uploadBytes > budget.MaxBytesPerFrame)
            {
                break;
            }

            _pendingIds.Dequeue();
            _enqueuedIds.Remove(id);
            processedObjects++;
            uploadedBytes += uploadBytes;

            try
            {
                registry.MarkUploading(id);
                SceneObjectUploader.Upload(record.SceneObject, factory, logger);
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

        return new SceneUploadFlushResult(uploaded, failed, uploadedBytes);
    }

    private void Enqueue(SceneEntryId id)
    {
        if (_enqueuedIds.Add(id))
        {
            _pendingIds.Enqueue(id);
        }
    }
}

internal sealed record SceneUploadFlushResult(
    IReadOnlyList<SceneResidencyRecord> UploadedRecords,
    int FailedCount,
    long UploadedBytes)
{
    public static SceneUploadFlushResult Empty { get; } = new(
        Array.Empty<SceneResidencyRecord>(),
        0,
        0);

    public bool HasChanges => UploadedRecords.Count > 0 || FailedCount > 0;
}
