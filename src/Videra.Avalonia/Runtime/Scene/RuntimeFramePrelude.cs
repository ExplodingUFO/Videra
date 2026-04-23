using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class RuntimeFramePrelude
{
    private readonly SceneUploadQueue _uploadQueue;
    private readonly SceneResidencyRegistry _residencyRegistry;
    private readonly VideraEngine _engine;
    private readonly Func<IResourceFactory?> _resourceFactoryAccessor;
    private readonly Func<bool> _isInteractiveAccessor;
    private readonly Func<ulong> _resourceEpochAccessor;
    private readonly Action _afterSceneApplied;
    private readonly ILogger _logger;

    public RuntimeFramePrelude(
        SceneUploadQueue uploadQueue,
        SceneResidencyRegistry residencyRegistry,
        VideraEngine engine,
        Func<IResourceFactory?> resourceFactoryAccessor,
        Func<bool> isInteractiveAccessor,
        Func<ulong> resourceEpochAccessor,
        Action afterSceneApplied,
        ILogger logger)
    {
        _uploadQueue = uploadQueue ?? throw new ArgumentNullException(nameof(uploadQueue));
        _residencyRegistry = residencyRegistry ?? throw new ArgumentNullException(nameof(residencyRegistry));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _resourceFactoryAccessor = resourceFactoryAccessor ?? throw new ArgumentNullException(nameof(resourceFactoryAccessor));
        _isInteractiveAccessor = isInteractiveAccessor ?? throw new ArgumentNullException(nameof(isInteractiveAccessor));
        _resourceEpochAccessor = resourceEpochAccessor ?? throw new ArgumentNullException(nameof(resourceEpochAccessor));
        _afterSceneApplied = afterSceneApplied ?? throw new ArgumentNullException(nameof(afterSceneApplied));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SceneUploadFlushResult Execute()
    {
        var pendingCount = _uploadQueue.PendingCount;
        var pendingBytes = _residencyRegistry.GetPendingUploadBytes();
        var budget = SceneUploadBudget.Resolve(
            _isInteractiveAccessor(),
            pendingCount,
            pendingBytes);
        var result = _uploadQueue.Drain(
            _resourceFactoryAccessor(),
            budget,
            _resourceEpochAccessor(),
            _residencyRegistry,
            _logger,
            preferAttachedEntries: _isInteractiveAccessor());

        if (result.UploadedRecords.Count > 0)
        {
            SceneEngineApplicator.ApplyReadyAdds(_engine, result.UploadedRecords, _residencyRegistry);
        }

        _afterSceneApplied();
        return result;
    }
}
