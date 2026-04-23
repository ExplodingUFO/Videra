using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Runtime;
using Videra.Avalonia.Runtime.Scene;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Inspection;
using Videra.Core.Overlay;
using Videra.Core.Selection;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;

namespace Videra.Avalonia.Controls;

internal sealed class VideraViewSessionBridge
{
    private const RenderFeatureSet BridgeAdditionalFeatures =
        RenderFeatureSet.Picking |
        RenderFeatureSet.Screenshot;

    private static readonly Color PrimarySelectionColor = Colors.Green;
    private static readonly Color SecondarySelectionColor = Colors.Black;

    private readonly RenderSession _session;
    private readonly Func<bool> _isPreferredBackendOverrideSet;
    private readonly Func<GraphicsBackendPreference> _preferredBackendValue;
    private readonly Func<VideraBackendOptions?> _backendOptionsAccessor;
    private readonly Func<VideraDiagnosticsOptions?> _diagnosticsOptionsAccessor;
    private readonly OverlayProjectionService _overlayProjectionService = new();
    private readonly MeasurementOverlayProjector _measurementOverlayProjector = new();
    private static readonly Color MeasurementColor = Colors.Gold;

    public VideraViewSessionBridge(
        RenderSession session,
        Func<bool> isPreferredBackendOverrideSet,
        Func<GraphicsBackendPreference> preferredBackendValue,
        Func<VideraBackendOptions?> backendOptionsAccessor,
        Func<VideraDiagnosticsOptions?> diagnosticsOptionsAccessor)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _isPreferredBackendOverrideSet = isPreferredBackendOverrideSet ?? throw new ArgumentNullException(nameof(isPreferredBackendOverrideSet));
        _preferredBackendValue = preferredBackendValue ?? throw new ArgumentNullException(nameof(preferredBackendValue));
        _backendOptionsAccessor = backendOptionsAccessor ?? throw new ArgumentNullException(nameof(backendOptionsAccessor));
        _diagnosticsOptionsAccessor = diagnosticsOptionsAccessor ?? throw new ArgumentNullException(nameof(diagnosticsOptionsAccessor));
    }

    public bool WantsNativeBackend()
    {
        var backendOptions = CreateBackendOptionsSnapshot();
        if (backendOptions.PreferredBackend == GraphicsBackendPreference.Software)
        {
            return false;
        }

        if (GraphicsBackendFactory.ResolveRequestedPreference(new GraphicsBackendRequest(
                backendOptions.PreferredBackend,
                backendOptions.EnvironmentOverrideMode,
                backendOptions.AllowSoftwareFallback)) == GraphicsBackendPreference.Software)
        {
            return false;
        }

        return OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    }

    public bool OnViewAttached()
    {
        RuntimeTraceLog.Write("VideraViewSessionBridge.OnViewAttached");
        var wasReady = _session.IsReady;
        var backendOptions = CreateBackendOptionsSnapshot();
        _session.Attach(backendOptions.PreferredBackend, backendOptions);
        return !wasReady && _session.IsReady;
    }

    public void OnViewDetached()
    {
        RuntimeTraceLog.Write("VideraViewSessionBridge.OnViewDetached");
        _session.Dispose();
    }

    public bool OnSizeChanged(uint widthPx, uint heightPx, float renderScale)
    {
        return SynchronizeSession(widthPx, heightPx, renderScale);
    }

    public bool OnBackendOptionsChanged(uint widthPx, uint heightPx, float renderScale)
    {
        return SynchronizeSession(widthPx, heightPx, renderScale);
    }

    public bool OnNativeHandleCreated(
        IntPtr handle,
        string? resolvedDisplayServer,
        bool fallbackUsed,
        string? fallbackReason,
        uint widthPx,
        uint heightPx,
        float renderScale)
    {
        RuntimeTraceLog.Write($"VideraViewSessionBridge.OnNativeHandleCreated handle=0x{handle.ToInt64():X}");
        var wasReady = _session.IsReady;
        _session.SetDisplayServerDiagnostics(resolvedDisplayServer, fallbackUsed, fallbackReason);
        _session.BindHandle(handle);

        var backendOptions = CreateBackendOptionsSnapshot();
        _session.Attach(backendOptions.PreferredBackend, backendOptions);
        _session.Resize(widthPx, heightPx, renderScale);

        return !wasReady && _session.IsReady;
    }

    public void OnNativeHandleBoundWithoutSize(
        IntPtr handle,
        string? resolvedDisplayServer,
        bool fallbackUsed,
        string? fallbackReason)
    {
        RuntimeTraceLog.Write($"VideraViewSessionBridge.OnNativeHandleBoundWithoutSize handle=0x{handle.ToInt64():X}");
        _session.SetDisplayServerDiagnostics(resolvedDisplayServer, fallbackUsed, fallbackReason);
        _session.BindHandle(handle);
    }

    public void OnNativeHandleDestroyed()
    {
        RuntimeTraceLog.Write("VideraViewSessionBridge.OnNativeHandleDestroyed");
        _session.SetDisplayServerDiagnostics(null, fallbackUsed: false, fallbackReason: null);
        _session.BindHandle(IntPtr.Zero);
    }

    public VideraViewOverlayState CreateOverlayState(
        VideraSelectionState? selectionState,
        IReadOnlyList<VideraAnnotation>? annotations,
        IReadOnlyList<VideraMeasurement>? measurements,
        Vector2 viewportSize)
    {
        var effectiveSelectionState = selectionState ?? new VideraSelectionState();
        var effectiveAnnotations = annotations ?? Array.Empty<VideraAnnotation>();
        var effectiveMeasurements = measurements ?? Array.Empty<VideraMeasurement>();
        var sceneObjects = _session.Engine.SceneObjects;
        var camera = _session.Engine.Camera;

        var selectionOutlines = _overlayProjectionService
            .ProjectSelectionOutlines(
                effectiveSelectionState.ObjectIds,
                effectiveSelectionState.PrimaryObjectId,
                sceneObjects,
                camera,
                viewportSize)
            .Select(projection => new VideraSelectionOutline(
                projection.ObjectId,
                new Rect(
                    projection.ScreenBounds.MinX,
                    projection.ScreenBounds.MinY,
                    Math.Max(1d, projection.ScreenBounds.Width),
                    Math.Max(1d, projection.ScreenBounds.Height)),
                projection.IsPrimary ? PrimarySelectionColor : SecondarySelectionColor,
                projection.IsPrimary))
            .ToArray();

        var visibleAnnotations = effectiveAnnotations
            .Where(annotation => annotation is { IsVisible: true })
            .ToArray();
        var measurementAnchors = effectiveMeasurements
            .Where(static measurement => measurement is { IsVisible: true })
            .SelectMany(static measurement => new[]
            {
                new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: true), AnnotationAnchorDescriptor.ForWorldPoint(measurement.Start.WorldPoint)),
                new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: false), AnnotationAnchorDescriptor.ForWorldPoint(measurement.End.WorldPoint))
            })
            .ToArray();
        var overlayState = new AnnotationOverlayRenderState(
            visibleAnnotations
                .Select(annotation => new AnnotationOverlayAnchor(annotation.Id, annotation.Anchor))
                .Concat(measurementAnchors)
                .ToArray(),
            markerColor: new RgbaFloat(1f, 0f, 0f, 1f));
        var annotationLookup = visibleAnnotations.ToDictionary(annotation => annotation.Id);
        var labels = _overlayProjectionService
            .ProjectAnnotationLabels(_session.Engine, overlayState, viewportSize)
            .Where(projection => annotationLookup.ContainsKey(projection.AnnotationId))
            .Select(projection =>
            {
                var annotation = annotationLookup[projection.AnnotationId];
                return new VideraOverlayLabel(
                    annotation.Id,
                    annotation.Text,
                    annotation.Color,
                    projection.ScreenPosition,
                    projection.Anchor,
                    projection.ResolvedObjectId);
            })
            .ToArray();
        var measurementOverlays = _measurementOverlayProjector
            .Project(effectiveMeasurements, camera, viewportSize)
            .Select(projection => new VideraOverlayMeasurement(
                projection.MeasurementId,
                projection.StartScreenPosition,
                projection.EndScreenPosition,
                projection.Text,
                MeasurementColor))
            .ToArray();

        return new VideraViewOverlayState(selectionOutlines, labels, measurementOverlays);
    }

    public VideraViewOverlayState CreateOverlayState(
        VideraSelectionState? selectionState,
        IReadOnlyList<VideraAnnotation>? annotations,
        Vector2 viewportSize)
    {
        return CreateOverlayState(selectionState, annotations, measurements: null, viewportSize);
    }

    public VideraBackendDiagnostics CreateDiagnosticsSnapshot(
        string? lastInitializationError,
        SceneResidencyDiagnostics sceneDiagnostics,
        InspectionWorkflowDiagnostics inspectionDiagnostics)
    {
        var backendOptions = CreateBackendOptionsSnapshot();
        var diagnosticsOptions = _diagnosticsOptionsAccessor() ?? new VideraDiagnosticsOptions();
        var snapshot = _session.OrchestrationSnapshot;
        var capabilities = _session.RenderCapabilities;
        var resolution = snapshot.LastBackendResolution;
        var pipelineSnapshot = snapshot.LastPipelineSnapshot ?? capabilities.LastPipelineSnapshot;

        return new VideraBackendDiagnostics
        {
            RequestedBackend = backendOptions.PreferredBackend,
            ResolvedBackend = resolution?.ResolvedPreference ?? capabilities.ActiveBackendPreference ?? backendOptions.PreferredBackend,
            IsReady = _session.IsReady,
            IsUsingSoftwareFallback = resolution?.IsUsingSoftwareFallback ?? false,
            FallbackReason = resolution?.FallbackReason,
            NativeHostBound = snapshot.HandleState.IsBound,
            RenderLoopMode = diagnosticsOptions.RenderLoopMode,
            EnvironmentOverrideApplied = resolution?.EnvironmentOverrideApplied ?? false,
            LastInitializationError = lastInitializationError ?? snapshot.LastInitializationError?.Message,
            ResolvedDisplayServer = snapshot.ResolvedDisplayServer,
            DisplayServerFallbackUsed = snapshot.DisplayServerFallbackUsed,
            DisplayServerFallbackReason = snapshot.DisplayServerFallbackReason,
            RenderPipelineProfile = pipelineSnapshot?.Profile.ToString(),
            LastFrameStageNames = pipelineSnapshot?.StageNames?.ToArray() ?? Array.Empty<string>(),
            LastFrameFeatureNames = pipelineSnapshot?.FeatureNames?.ToArray() ?? Array.Empty<string>(),
            LastFrameObjectCount = pipelineSnapshot?.FrameObjectCount ?? 0,
            LastFrameOpaqueObjectCount = pipelineSnapshot?.OpaqueObjectCount ?? 0,
            LastFrameTransparentObjectCount = pipelineSnapshot?.TransparentObjectCount ?? 0,
            SupportedRenderFeatureNames = MergeSupportedFeatures(capabilities.SupportedFeatures, BridgeAdditionalFeatures).ToFeatureNames(),
            TransparentFeatureStatus = VideraBackendDiagnostics.CurrentTransparentFeatureStatus,
            UsesSoftwarePresentationCopy = snapshot.UsesSoftwarePresentationCopy,
            SceneDocumentVersion = sceneDiagnostics.SceneDocumentVersion,
            PendingSceneUploads = sceneDiagnostics.PendingUploads,
            PendingSceneUploadBytes = sceneDiagnostics.PendingUploadBytes,
            ResidentSceneObjects = sceneDiagnostics.ResidentObjects,
            DirtySceneObjects = sceneDiagnostics.DirtyObjects,
            FailedSceneUploads = sceneDiagnostics.FailedUploads,
            LastFrameUploadedObjects = sceneDiagnostics.LastUploadedObjects,
            LastFrameUploadedBytes = sceneDiagnostics.LastUploadedBytes,
            LastFrameUploadFailures = sceneDiagnostics.LastUploadFailures,
            LastFrameUploadDuration = sceneDiagnostics.LastUploadDuration,
            ResolvedUploadBudgetObjects = sceneDiagnostics.LastBudgetMaxObjects,
            ResolvedUploadBudgetBytes = sceneDiagnostics.LastBudgetMaxBytes,
            IsClippingActive = inspectionDiagnostics.IsClippingActive,
            ActiveClippingPlaneCount = inspectionDiagnostics.ActiveClippingPlaneCount,
            MeasurementCount = inspectionDiagnostics.MeasurementCount,
            LastSnapshotExportPath = inspectionDiagnostics.LastSnapshotExportPath,
            LastSnapshotExportStatus = inspectionDiagnostics.LastSnapshotExportStatus,
            SupportsPassContributors = capabilities.SupportsPassContributors,
            SupportsPassReplacement = capabilities.SupportsPassReplacement,
            SupportsFrameHooks = capabilities.SupportsFrameHooks,
            SupportsPipelineSnapshots = capabilities.SupportsPipelineSnapshots
        };
    }

    public VideraBackendDiagnostics CreateDiagnosticsSnapshot(string? lastInitializationError, SceneResidencyDiagnostics sceneDiagnostics)
    {
        return CreateDiagnosticsSnapshot(lastInitializationError, sceneDiagnostics, InspectionWorkflowDiagnostics.Empty);
    }

    private bool SynchronizeSession(uint widthPx, uint heightPx, float renderScale)
    {
        var wasReady = _session.IsReady;
        var backendOptions = CreateBackendOptionsSnapshot();
        _session.Attach(backendOptions.PreferredBackend, backendOptions);
        _session.Resize(widthPx, heightPx, renderScale);
        return !wasReady && _session.IsReady;
    }

    private VideraBackendOptions CreateBackendOptionsSnapshot()
    {
        var source = _backendOptionsAccessor() ?? new VideraBackendOptions();
        return new VideraBackendOptions
        {
            PreferredBackend = _isPreferredBackendOverrideSet() ? _preferredBackendValue() : source.PreferredBackend,
            EnvironmentOverrideMode = source.EnvironmentOverrideMode,
            AllowSoftwareFallback = source.AllowSoftwareFallback
        };
    }

    private static Guid CreateMeasurementAnchorId(Guid measurementId, bool isStart)
    {
        var bytes = measurementId.ToByteArray();
        bytes[15] ^= isStart ? (byte)0x41 : (byte)0x82;
        return new Guid(bytes);
    }

    private static RenderFeatureSet MergeSupportedFeatures(RenderFeatureSet engineFeatures, RenderFeatureSet bridgeFeatures)
    {
        return engineFeatures | bridgeFeatures;
    }
}
