using Videra.Avalonia.Controls;
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

    private IGraphicsBackend? _backend;
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
    }

    public RenderSessionHandle HandleState => _handleState;

    public bool IsInitialized => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsReady => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsSoftwareBackend => _backend is ISoftwareBackend;

    public IResourceFactory? ResourceFactory => _backend?.GetResourceFactory();

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

    internal ISoftwareBackend? SoftwareBackend => _backend as ISoftwareBackend;

    public bool Attach(GraphicsBackendPreference preference, VideraBackendOptions? backendOptions = null)
    {
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

        _backend?.Resize((int)width, (int)height);
        _engine.Resize(width, height);
        return false;
    }

    public RenderSessionRenderResult RenderOnce()
    {
        if (!IsReady || _backend == null)
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
                _backend as ISoftwareBackend);
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
                _backend?.Dispose();
            }

            _backend = null;
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
        _backend = null;
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

        if (RequiresNativeHandle(request) && !_handleState.IsBound)
        {
            _state = RenderSessionState.WaitingForHandle;
            return false;
        }

        IGraphicsBackend? backend = null;
        try
        {
            var resolution = _backendResolutionFactory(request);
            backend = resolution.Backend;
            var handle = resolution.ResolvedPreference == GraphicsBackendPreference.Software
                ? IntPtr.Zero
                : _handleState.Handle;

            backend.Initialize(handle, (int)_inputs.Width, (int)_inputs.Height);

            _backend = backend;
            LastBackendResolution = resolution;
            LastInitializationError = null;
            _engine.Initialize(backend);
            _engine.Resize(_inputs.Width, _inputs.Height);
            _state = RenderSessionState.Ready;
            return true;
        }
        catch (Exception ex)
        {
            LastInitializationError = ex;
            _state = RenderSessionState.Faulted;

            if (_engine.IsInitialized)
            {
                _engine.Suspend();
            }
            else
            {
                backend?.Dispose();
            }

            _backend = null;
            throw;
        }
    }

    private void Suspend()
    {
        if (_engine.IsInitialized)
        {
            _engine.Suspend();
        }

        _backend = null;
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

    private static bool RequiresNativeHandle(GraphicsBackendRequest request)
    {
        var preference = GraphicsBackendFactory.ResolveRequestedPreference(request);
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
        if (_inputs.Width == 0 || _inputs.Height == 0)
        {
            _state = RenderSessionState.WaitingForSize;
            return;
        }

        _state = RequiresNativeHandle(request) && !_handleState.IsBound
            ? RenderSessionState.WaitingForHandle
            : RenderSessionState.Detached;
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
