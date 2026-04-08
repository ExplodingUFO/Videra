using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;

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
}
