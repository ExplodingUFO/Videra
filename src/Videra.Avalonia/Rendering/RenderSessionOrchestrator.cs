using Videra.Avalonia.Controls;
using Videra.Avalonia.Runtime;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

namespace Videra.Avalonia.Rendering;

internal sealed class RenderSessionOrchestrator : IDisposable
{
    private readonly VideraEngine _engine;
    private readonly Func<GraphicsBackendPreference, IGraphicsBackend> _backendFactory;
    private readonly Func<GraphicsBackendRequest, GraphicsBackendResolution> _backendResolutionFactory;
    private readonly bool _usesCustomBackendFactoryResolution;

    private IGraphicsDevice? _device;
    private IRenderSurface? _renderSurface;
    private RenderSessionInputs _inputs = new();
    private RenderSessionHandle _handleState = RenderSessionHandle.Unbound;
    private RenderSessionState _state = RenderSessionState.Detached;

    public RenderSessionOrchestrator(
        VideraEngine engine,
        Func<GraphicsBackendPreference, IGraphicsBackend>? backendFactory = null,
        Func<GraphicsBackendRequest, GraphicsBackendResolution>? backendResolutionFactory = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _backendFactory = backendFactory ?? (preference => GraphicsBackendFactory.CreateBackend(preference));
        _backendResolutionFactory = backendResolutionFactory
            ?? (backendFactory == null
                ? GraphicsBackendFactory.ResolveBackend
                : CreateResolutionFromFactory);
        _usesCustomBackendFactoryResolution = backendFactory != null && backendResolutionFactory == null;
    }

    public RenderSessionHandle HandleState => _handleState;

    public bool IsInitialized => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsReady => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsSoftwareBackend => _device?.IsSoftwareBackend == true;

    public IResourceFactory? ResourceFactory => _device?.ResourceFactory;

    internal VideraEngine Engine => _engine;

    public GraphicsBackendResolution? LastBackendResolution { get; private set; }

    public Exception? LastInitializationError { get; private set; }

    public string? LastResolvedDisplayServer { get; private set; }

    public bool LastDisplayServerFallbackUsed { get; private set; }

    public string? LastDisplayServerFallbackReason { get; private set; }

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; private set; }

    internal RenderCapabilitySnapshot RenderCapabilities => _engine.GetRenderCapabilities();

    public RenderSessionSnapshot Snapshot => new()
    {
        State = _state,
        Inputs = _inputs with { },
        HandleState = _handleState,
        LastBackendResolution = LastBackendResolution,
        LastInitializationError = LastInitializationError,
        ResolvedDisplayServer = LastResolvedDisplayServer,
        DisplayServerFallbackUsed = LastDisplayServerFallbackUsed,
        DisplayServerFallbackReason = LastDisplayServerFallbackReason,
        LastPipelineSnapshot = LastPipelineSnapshot,
        UsesSoftwarePresentationCopy = IsSoftwareBackend
    };

    internal ISoftwareBackend? SoftwareBackend => _device switch
    {
        ISoftwareBackend softwareBackend => softwareBackend,
        LegacyGraphicsBackendAdapter adapter => adapter.LegacyBackend as ISoftwareBackend,
        _ => null
    };

    public bool Attach(GraphicsBackendPreference preference, VideraBackendOptions? backendOptions = null)
    {
        RuntimeTraceLog.Write($"RenderSessionOrchestrator.Attach preference={preference} state={_state} handleBound={_handleState.IsBound}");
        if (_state == RenderSessionState.Disposed)
        {
            return false;
        }

        if (_inputs.RequestedBackend != preference && IsReady)
        {
            Suspend();
        }

        _inputs = _inputs with
        {
            RequestedBackend = preference,
            BackendOptions = backendOptions
        };

        return TryInitialize();
    }

    public bool BindHandle(IntPtr handle)
    {
        RuntimeTraceLog.Write($"RenderSessionOrchestrator.BindHandle handle=0x{handle.ToInt64():X} state={_state} currentlyBound={_handleState.IsBound}");
        if (_state == RenderSessionState.Disposed)
        {
            return false;
        }

        if (handle == IntPtr.Zero)
        {
            if (_handleState.IsBound)
            {
                _handleState = _handleState.Clear();
                _inputs = _inputs with
                {
                    Handle = IntPtr.Zero,
                    HandleBound = false
                };

                if (IsReady && !IsSoftwareBackend)
                {
                    Suspend();
                }
            }

            TransitionToWaitingState();
            return false;
        }

        if (_handleState.Generation == 0)
        {
            _handleState = RenderSessionHandle.Create(handle);
        }
        else if (_handleState.Handle != handle)
        {
            _handleState = _handleState.Rebind(handle);
            if (IsReady && !IsSoftwareBackend)
            {
                Suspend();
            }
        }

        _inputs = _inputs with
        {
            Handle = handle,
            HandleBound = true
        };

        return TryInitialize();
    }

    public bool Resize(uint width, uint height, float renderScale)
    {
        RuntimeTraceLog.Write($"RenderSessionOrchestrator.Resize {width}x{height} scale={renderScale} state={_state} handleBound={_handleState.IsBound}");
        if (_state == RenderSessionState.Disposed || width == 0 || height == 0)
        {
            return false;
        }

        _inputs = _inputs with
        {
            Width = width,
            Height = height,
            RenderScale = renderScale
        };
        _engine.RenderScale = renderScale;

        if (!IsReady)
        {
            return TryInitialize();
        }

        _engine.Resize(width, height);
        return false;
    }

    public RenderSessionRenderResult RenderOnce()
    {
        if (!IsReady || _device == null || _renderSurface == null)
        {
            return RenderSessionRenderResult.NotReady;
        }

        try
        {
            _engine.Draw();
            LastPipelineSnapshot = _engine.LastPipelineSnapshot;
            return new RenderSessionRenderResult(
                false,
                null,
                SoftwareBackend);
        }
        catch (Exception ex)
        {
            LastInitializationError = ex;
            if (_engine.IsInitialized)
            {
                _engine.Suspend();
            }
            else
            {
                _renderSurface?.Dispose();
                _device?.Dispose();
            }

            _renderSurface = null;
            _device = null;
            _state = RenderSessionState.Faulted;
            return new RenderSessionRenderResult(
                true,
                ex,
                null);
        }
    }

    public void SetDisplayServerDiagnostics(string? resolvedDisplayServer, bool fallbackUsed, string? fallbackReason)
    {
        LastResolvedDisplayServer = resolvedDisplayServer;
        LastDisplayServerFallbackUsed = fallbackUsed;
        LastDisplayServerFallbackReason = fallbackReason;
    }

    public void Dispose()
    {
        if (_state == RenderSessionState.Disposed)
        {
            return;
        }

        _state = RenderSessionState.Disposed;
        _engine.Dispose();
        _renderSurface = null;
        _device = null;
        _handleState = RenderSessionHandle.Unbound;
        _inputs = _inputs with
        {
            Handle = IntPtr.Zero,
            HandleBound = false
        };
        LastResolvedDisplayServer = null;
        LastDisplayServerFallbackUsed = false;
        LastDisplayServerFallbackReason = null;
        LastPipelineSnapshot = null;
    }

    private bool TryInitialize()
    {
        RuntimeTraceLog.Write($"RenderSessionOrchestrator.TryInitialize state={_state} width={_inputs.Width} height={_inputs.Height} handleBound={_handleState.IsBound}");
        if (_state == RenderSessionState.Disposed || IsReady)
        {
            return false;
        }

        var request = CreateBackendRequest();
        if (_inputs.Width == 0 || _inputs.Height == 0)
        {
            _state = RenderSessionState.WaitingForSize;
            return false;
        }

        if (_usesCustomBackendFactoryResolution)
        {
            if (RequiresNativeHandleForCustomFactory(request) && !_handleState.IsBound)
            {
                _state = RenderSessionState.WaitingForHandle;
                return false;
            }
        }
        else if (RequiresNativeHandleWithOverrides(request) && !_handleState.IsBound)
        {
            _state = RenderSessionState.WaitingForHandle;
            return false;
        }

        IGraphicsDevice? device = null;
        IRenderSurface? renderSurface = null;
        try
        {
            var resolution = _backendResolutionFactory(request);
            device = resolution.Backend as IGraphicsDevice
                ?? new LegacyGraphicsBackendAdapter(resolution.Backend, resolution.ResolvedPreference);
            renderSurface = device.CreateRenderSurface();
            var handle = resolution.ResolvedPreference == GraphicsBackendPreference.Software
                ? IntPtr.Zero
                : _handleState.Handle;

            renderSurface.Initialize(handle, (int)_inputs.Width, (int)_inputs.Height);

            _device = device;
            _renderSurface = renderSurface;
            LastBackendResolution = resolution;
            LastInitializationError = null;
            _engine.Initialize(device, renderSurface);
            _engine.Resize(_inputs.Width, _inputs.Height);
            _state = RenderSessionState.Ready;
            RuntimeTraceLog.Write($"RenderSessionOrchestrator.TryInitialize -> Ready backend={resolution.ResolvedPreference}");
            return true;
        }
        catch (Exception ex)
        {
            RuntimeTraceLog.Write($"RenderSessionOrchestrator.TryInitialize faulted: {ex.Message}");
            LastInitializationError = ex;
            _state = RenderSessionState.Faulted;

            if (_engine.IsInitialized)
            {
                _engine.Suspend();
            }
            else
            {
                renderSurface?.Dispose();
                device?.Dispose();
            }

            _renderSurface = null;
            _device = null;
            throw;
        }
    }

    private void Suspend()
    {
        RuntimeTraceLog.Write($"RenderSessionOrchestrator.Suspend state={_state} handleBound={_handleState.IsBound}");
        if (_engine.IsInitialized)
        {
            _engine.Suspend();
        }

        _renderSurface = null;
        _device = null;
        LastPipelineSnapshot = null;
        TransitionToWaitingState();
    }

    private GraphicsBackendRequest CreateBackendRequest()
    {
        if (_inputs.BackendOptions == null)
        {
            return new GraphicsBackendRequest(
                _inputs.RequestedBackend,
                BackendEnvironmentOverrideMode.PreferOverrides,
                AllowSoftwareFallback: true);
        }

        return new GraphicsBackendRequest(
            _inputs.RequestedBackend,
            _inputs.BackendOptions.EnvironmentOverrideMode,
            _inputs.BackendOptions.AllowSoftwareFallback);
    }

    private GraphicsBackendResolution CreateResolutionFromFactory(GraphicsBackendRequest request)
    {
        var backend = _backendFactory(request.RequestedPreference);
        var resolvedPreference = backend is ISoftwareBackend
            ? GraphicsBackendPreference.Software
            : request.RequestedPreference;

        return new GraphicsBackendResolution(
            backend,
            request.RequestedPreference,
            resolvedPreference);
    }

    private static bool RequiresNativeHandleForCustomFactory(GraphicsBackendRequest request)
    {
        var preference = request.RequestedPreference;
        if (preference == GraphicsBackendPreference.Software)
        {
            return false;
        }

        return OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    }

    private void TransitionToWaitingState()
    {
        if (_state == RenderSessionState.Disposed)
        {
            return;
        }

        var request = CreateBackendRequest();
        var requiresNativeHandle = _usesCustomBackendFactoryResolution
            ? RequiresNativeHandleForCustomFactory(request)
            : RequiresNativeHandleWithOverrides(request);
        if (_inputs.Width == 0 || _inputs.Height == 0)
        {
            _state = RenderSessionState.WaitingForSize;
            return;
        }

        _state = requiresNativeHandle && !_handleState.IsBound
            ? RenderSessionState.WaitingForHandle
            : RenderSessionState.Detached;
    }

    private static bool RequiresNativeHandleWithOverrides(GraphicsBackendRequest request)
    {
        var preference = GraphicsBackendFactory.ResolveRequestedPreference(request);
        if (preference == GraphicsBackendPreference.Software)
        {
            return false;
        }

        return OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    }

    internal readonly record struct RenderSessionRenderResult(
        bool Faulted,
        Exception? Error,
        ISoftwareBackend? SoftwareBackend)
    {
        public static RenderSessionRenderResult NotReady => new(
            false,
            null,
            null);
    }
}
