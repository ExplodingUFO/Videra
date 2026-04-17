using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Runtime.Scene;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Overlay;
using Videra.Core.Selection;
using Videra.Core.Selection.Rendering;

namespace Videra.Avalonia.Controls;

internal sealed class VideraViewSessionBridge
{
    private static readonly Color PrimarySelectionColor = Colors.Green;
    private static readonly Color SecondarySelectionColor = Colors.Black;

    private readonly RenderSession _session;
    private readonly Func<bool> _isPreferredBackendOverrideSet;
    private readonly Func<GraphicsBackendPreference> _preferredBackendValue;
    private readonly Func<VideraBackendOptions?> _backendOptionsAccessor;
    private readonly Func<VideraDiagnosticsOptions?> _diagnosticsOptionsAccessor;
    private readonly OverlayProjectionService _overlayProjectionService = new();

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
        var wasReady = _session.IsReady;
        var backendOptions = CreateBackendOptionsSnapshot();
        _session.Attach(backendOptions.PreferredBackend, backendOptions);
        return !wasReady && _session.IsReady;
    }

    public void OnViewDetached()
    {
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
        var wasReady = _session.IsReady;
        _session.SetDisplayServerDiagnostics(resolvedDisplayServer, fallbackUsed, fallbackReason);
        _session.BindHandle(handle);

        var backendOptions = CreateBackendOptionsSnapshot();
        _session.Attach(backendOptions.PreferredBackend, backendOptions);
        _session.Resize(widthPx, heightPx, renderScale);

        return !wasReady && _session.IsReady;
    }

    public void OnNativeHandleDestroyed()
    {
        _session.SetDisplayServerDiagnostics(null, fallbackUsed: false, fallbackReason: null);
        _session.BindHandle(IntPtr.Zero);
    }

    public VideraViewOverlayState CreateOverlayState(
        VideraSelectionState? selectionState,
        IReadOnlyList<VideraAnnotation>? annotations,
        Vector2 viewportSize)
    {
        var effectiveSelectionState = selectionState ?? new VideraSelectionState();
        var effectiveAnnotations = annotations ?? Array.Empty<VideraAnnotation>();
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
        var overlayState = new AnnotationOverlayRenderState(
            visibleAnnotations
                .Select(annotation => new AnnotationOverlayAnchor(annotation.Id, annotation.Anchor))
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

        return new VideraViewOverlayState(selectionOutlines, labels);
    }

    public VideraBackendDiagnostics CreateDiagnosticsSnapshot(string? lastInitializationError, SceneResidencyDiagnostics sceneDiagnostics)
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
            UsesSoftwarePresentationCopy = snapshot.UsesSoftwarePresentationCopy,
            SceneDocumentVersion = sceneDiagnostics.SceneDocumentVersion,
            PendingSceneUploads = sceneDiagnostics.PendingUploads,
            ResidentSceneObjects = sceneDiagnostics.ResidentObjects,
            DirtySceneObjects = sceneDiagnostics.DirtyObjects,
            FailedSceneUploads = sceneDiagnostics.FailedUploads,
            SupportsPassContributors = capabilities.SupportsPassContributors,
            SupportsPassReplacement = capabilities.SupportsPassReplacement,
            SupportsFrameHooks = capabilities.SupportsFrameHooks,
            SupportsPipelineSnapshots = capabilities.SupportsPipelineSnapshots
        };
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
}
