using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneResidencyRegistry
{
    private readonly Dictionary<SceneEntryId, SceneResidencyRecord> _records = [];

    public void Apply(SceneDelta delta, ulong resourceEpoch)
    {
        ArgumentNullException.ThrowIfNull(delta);

        foreach (var removed in delta.Removed)
        {
            _records.Remove(removed.Id);
        }

        foreach (var added in delta.Added)
        {
            _records[added.Id] = CreateRecord(added, resourceEpoch);
        }

        foreach (var retained in delta.Retained)
        {
            if (_records.TryGetValue(retained.Id, out var existing))
            {
                _records[retained.Id] = existing with
                {
                    SceneObject = retained.SceneObject,
                    RuntimeObjects = retained.RuntimeObjects,
                    ImportedAsset = retained.ImportedAsset,
                    Ownership = retained.Ownership,
                    ApproximateUploadBytes = ResolveApproximateUploadBytes(retained)
                };
            }
        }

        foreach (var reupload in delta.ReuploadRequired)
        {
            if (_records.TryGetValue(reupload.Id, out var existing))
            {
                _records[reupload.Id] = existing with
                {
                    SceneObject = reupload.SceneObject,
                    RuntimeObjects = reupload.RuntimeObjects,
                    ImportedAsset = reupload.ImportedAsset,
                    Ownership = reupload.Ownership,
                    ApproximateUploadBytes = ResolveApproximateUploadBytes(reupload),
                    State = CanUpload(reupload) ? SceneResidencyState.Dirty : existing.State,
                    LastError = CanUpload(reupload)
                        ? null
                        : existing.LastError,
                    ResourceEpoch = resourceEpoch
                };
            }
        }
    }

    public void MarkUploading(SceneEntryId id)
    {
        if (_records.TryGetValue(id, out var record))
        {
            _records[id] = record with
            {
                State = SceneResidencyState.Uploading,
                LastError = null
            };
        }
    }

    public void MarkResident(SceneEntryId id, ulong resourceEpoch)
    {
        if (_records.TryGetValue(id, out var record))
        {
            _records[id] = record with
            {
                State = SceneResidencyState.Resident,
                ResourceEpoch = resourceEpoch,
                LastError = null
            };
        }
    }

    public void MarkFailed(SceneEntryId id, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (_records.TryGetValue(id, out var record))
        {
            _records[id] = record with
            {
                State = SceneResidencyState.Failed,
                LastError = exception
            };
        }
    }

    public void MarkAttached(SceneEntryId id)
    {
        if (_records.TryGetValue(id, out var record))
        {
            _records[id] = record with { IsAttachedToEngine = true };
        }
    }

    public void MarkDetached(SceneEntryId id)
    {
        if (_records.TryGetValue(id, out var record))
        {
            _records[id] = record with { IsAttachedToEngine = false };
        }
    }

    public IReadOnlyList<SceneResidencyRecord> MarkDirtyForResourceEpoch(ulong resourceEpoch)
    {
        var dirtyRecords = new List<SceneResidencyRecord>();
        foreach (var pair in _records.ToArray())
        {
            var record = pair.Value;
            if (!CanUpload(record))
            {
                continue;
            }

            if (record.State is SceneResidencyState.PendingUpload or SceneResidencyState.Uploading or SceneResidencyState.Dirty)
            {
                _records[pair.Key] = record with { ResourceEpoch = resourceEpoch };
                continue;
            }

            var dirty = record with
            {
                State = SceneResidencyState.Dirty,
                ResourceEpoch = resourceEpoch,
                LastError = null
            };
            _records[pair.Key] = dirty;
            dirtyRecords.Add(dirty);
        }

        return dirtyRecords;
    }

    public IReadOnlyList<SceneResidencyRecord> MarkDirty(IEnumerable<SceneDocumentEntry> entries, ulong resourceEpoch)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var dirtyRecords = new List<SceneResidencyRecord>();
        foreach (var entry in entries)
        {
            if (!_records.TryGetValue(entry.Id, out var record) || !CanUpload(record))
            {
                continue;
            }

            if (record.State is SceneResidencyState.PendingUpload or SceneResidencyState.Uploading or SceneResidencyState.Dirty)
            {
                _records[entry.Id] = record with
                {
                    ResourceEpoch = resourceEpoch,
                    ApproximateUploadBytes = ResolveApproximateUploadBytes(entry),
                    LastError = null
                };
                continue;
            }

            var dirty = record with
            {
                SceneObject = entry.SceneObject,
                RuntimeObjects = entry.RuntimeObjects,
                ImportedAsset = entry.ImportedAsset,
                Ownership = entry.Ownership,
                ApproximateUploadBytes = ResolveApproximateUploadBytes(entry),
                State = SceneResidencyState.Dirty,
                ResourceEpoch = resourceEpoch,
                LastError = null
            };
            _records[entry.Id] = dirty;
            dirtyRecords.Add(dirty);
        }

        return dirtyRecords;
    }

    public bool TryGet(SceneEntryId id, out SceneResidencyRecord record)
    {
        return _records.TryGetValue(id, out record!);
    }

    public IReadOnlyList<SceneResidencyRecord> GetReadyAdds(IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var ready = new List<SceneResidencyRecord>();
        foreach (var entry in entries)
        {
            if (!_records.TryGetValue(entry.Id, out var record))
            {
                continue;
            }

            if (record.State == SceneResidencyState.Resident && !record.IsAttachedToEngine)
            {
                ready.Add(record);
            }
        }

        return ready;
    }

    public IReadOnlyList<SceneResidencyRecord> GetPendingCandidates()
    {
        return _records.Values
            .Where(static record =>
                record.State == SceneResidencyState.PendingUpload ||
                record.State == SceneResidencyState.Dirty)
            .ToArray();
    }

    public long GetPendingUploadBytes()
    {
        return _records.Values
            .Where(static record =>
                record.State == SceneResidencyState.PendingUpload ||
                record.State == SceneResidencyState.Dirty)
            .Sum(static record => Math.Max(record.ApproximateUploadBytes, 1L));
    }

    public SceneResidencyDiagnostics CreateDiagnostics(
        int sceneDocumentVersion,
        SceneUploadFlushResult? lastFlush = null,
        SceneUploadBudget? lastResolvedBudget = null)
    {
        var flush = lastFlush ?? SceneUploadFlushResult.Empty;
        var budget = lastResolvedBudget ?? flush.ResolvedBudget;
        return new SceneResidencyDiagnostics(
            sceneDocumentVersion,
            _records.Values.Count(static record => record.State == SceneResidencyState.PendingUpload || record.State == SceneResidencyState.Uploading),
            GetPendingUploadBytes(),
            _records.Values.Count(static record => record.State == SceneResidencyState.Resident),
            _records.Values.Count(static record => record.State == SceneResidencyState.Dirty),
            _records.Values.Count(static record => record.State == SceneResidencyState.Failed),
            flush.UploadedRecords.Count,
            flush.UploadedBytes,
            flush.FailedCount,
            flush.Duration,
            budget.MaxObjectsPerFrame,
            budget.MaxBytesPerFrame);
    }

    private static SceneResidencyRecord CreateRecord(SceneDocumentEntry entry, ulong resourceEpoch)
    {
        var state = ResolveInitialState(entry);
        return new SceneResidencyRecord(
            entry.Id,
            entry.SceneObject,
            entry.RuntimeObjects,
            entry.ImportedAsset,
            entry.Ownership,
            state,
            ResolveApproximateUploadBytes(entry),
            resourceEpoch,
            IsAttachedToEngine: false,
            LastError: state == SceneResidencyState.Failed
                ? new InvalidOperationException("Scene object is neither resident nor rehydratable.")
                : null);
    }

    private static SceneResidencyState ResolveInitialState(SceneDocumentEntry entry)
    {
        if (entry.RuntimeObjects.All(static runtimeObject =>
                runtimeObject.VertexBuffer != null &&
                runtimeObject.IndexBuffer != null &&
                runtimeObject.WorldBuffer != null))
        {
            return SceneResidencyState.Resident;
        }

        return CanUpload(entry)
            ? SceneResidencyState.PendingUpload
            : SceneResidencyState.Failed;
    }

    private static bool CanUpload(SceneDocumentEntry entry)
    {
        return entry.ImportedAsset is not null || entry.RuntimeObjects.Any(static runtimeObject => runtimeObject.CanRecreateGraphicsResources);
    }

    private static bool CanUpload(SceneResidencyRecord record)
    {
        return record.ImportedAsset is not null || record.RuntimeObjects.Any(static runtimeObject => runtimeObject.CanRecreateGraphicsResources);
    }

    private static long ResolveApproximateUploadBytes(SceneDocumentEntry entry)
    {
        return entry.RuntimeObjects.Sum(static runtimeObject => runtimeObject.ApproximateGpuBytes);
    }
}
