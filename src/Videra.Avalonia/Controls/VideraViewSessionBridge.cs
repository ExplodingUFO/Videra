using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Selection.Rendering;

namespace Videra.Avalonia.Controls;

internal sealed class VideraViewSessionBridge
{
    private readonly RenderSession _session;
    private readonly Func<bool> _isPreferredBackendOverrideSet;
    private readonly Func<GraphicsBackendPreference> _preferredBackendValue;
    private readonly Func<VideraBackendOptions?> _backendOptionsAccessor;
    private readonly Func<VideraDiagnosticsOptions?> _diagnosticsOptionsAccessor;

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

        var selectionOutlines = effectiveSelectionState.ObjectIds
            .Distinct()
            .Select(objectId => TryCreateSelectionOutline(objectId, effectiveSelectionState.PrimaryObjectId, sceneObjects, camera, viewportSize))
            .Where(outline => outline is not null)
            .Select(outline => outline!)
            .ToArray();

        var visibleAnnotations = effectiveAnnotations
            .Where(annotation => annotation is { IsVisible: true })
            .ToArray();
        var overlayState = new AnnotationOverlayRenderState(
            visibleAnnotations
                .Select(annotation => new AnnotationOverlayAnchor(annotation.Id, annotation.Anchor))
                .ToArray(),
            markerColor: new RgbaFloat(1f, 0f, 0f, 1f));
        var projectedAnchors = _session.Engine.ProjectAnnotationAnchors(overlayState, viewportSize);
        var annotationLookup = visibleAnnotations.ToDictionary(annotation => annotation.Id);
        var labels = projectedAnchors
            .Where(projection => projection.Projection.IsVisible && annotationLookup.ContainsKey(projection.AnnotationId))
            .Select(projection =>
            {
                var annotation = annotationLookup[projection.AnnotationId];
                return new VideraOverlayLabel(
                    annotation.Id,
                    annotation.Text,
                    annotation.Color,
                    projection.Projection.ScreenPosition,
                    projection.Anchor,
                    projection.Projection.ResolvedObjectId);
            })
            .ToArray();

        return new VideraViewOverlayState(selectionOutlines, labels);
    }

    public VideraBackendDiagnostics CreateDiagnosticsSnapshot(string? lastInitializationError)
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

    private static VideraSelectionOutline? TryCreateSelectionOutline(
        Guid objectId,
        Guid? primaryObjectId,
        IReadOnlyList<Object3D> sceneObjects,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return null;
        }

        var sceneObject = sceneObjects.FirstOrDefault(obj => obj.Id == objectId);
        if (sceneObject?.WorldBounds is not BoundingBox3 bounds)
        {
            return null;
        }

        var projectedPoints = GetProjectedBoundsCorners(bounds, camera, viewportSize);
        if (projectedPoints.Count == 0)
        {
            return null;
        }

        var minX = projectedPoints.Min(point => point.X);
        var minY = projectedPoints.Min(point => point.Y);
        var maxX = projectedPoints.Max(point => point.X);
        var maxY = projectedPoints.Max(point => point.Y);
        var outlineColor = objectId == primaryObjectId ? Colors.Green : Colors.Black;

        return new VideraSelectionOutline(
            objectId,
            new Rect(minX, minY, Math.Max(1d, maxX - minX), Math.Max(1d, maxY - minY)),
            outlineColor,
            objectId == primaryObjectId);
    }

    private static List<Vector2> GetProjectedBoundsCorners(
        BoundingBox3 bounds,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        var projectedPoints = new List<Vector2>(8);
        foreach (var corner in CreateCorners(bounds))
        {
            if (camera.TryProjectWorldPoint(corner, viewportSize, out var screenPoint))
            {
                projectedPoints.Add(screenPoint);
            }
        }

        return projectedPoints;
    }

    private static Vector3[] CreateCorners(BoundingBox3 bounds)
    {
        return
        [
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Max.Z)
        ];
    }
}
