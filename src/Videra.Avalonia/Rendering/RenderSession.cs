using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Avalonia.Rendering;

internal sealed partial class RenderSession : IDisposable
{
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(16);
    private readonly object _sync = new();

    private readonly VideraEngine _engine;
    private readonly Func<GraphicsBackendPreference, IGraphicsBackend> _backendFactory;
    private readonly Func<GraphicsBackendRequest, GraphicsBackendResolution> _backendResolutionFactory;
    private readonly Action? _requestRender;
    private readonly ILogger _logger;
    private readonly Func<IRenderLoopDriver> _renderLoopFactory;

    private IGraphicsBackend? _backend;
    private IRenderLoopDriver? _renderLoop;
    private WriteableBitmap? _bitmap;
    private GraphicsBackendPreference _preference = GraphicsBackendPreference.Auto;
    private VideraBackendOptions? _backendOptions;
    private uint _width;
    private uint _height;
    private RenderSessionState _state = RenderSessionState.Detached;

    public RenderSession(
        VideraEngine engine,
        Func<GraphicsBackendPreference, IGraphicsBackend>? backendFactory = null,
        Action? requestRender = null,
        ILogger? logger = null,
        Func<IRenderLoopDriver>? renderLoopFactory = null,
        Func<GraphicsBackendRequest, GraphicsBackendResolution>? backendResolutionFactory = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _backendFactory = backendFactory ?? (preference => GraphicsBackendFactory.CreateBackend(preference));
        _backendResolutionFactory = backendResolutionFactory
            ?? (backendFactory == null
                ? GraphicsBackendFactory.ResolveBackend
                : CreateResolutionFromFactory);
        _requestRender = requestRender;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<RenderSession>();
        _renderLoopFactory = renderLoopFactory ?? (() => new NullRenderLoopDriver());
    }

    public event EventHandler? BackendReady;

    public WriteableBitmap? Bitmap => _bitmap;

    public RenderSessionHandle HandleState { get; private set; } = RenderSessionHandle.Unbound;

    public bool IsInitialized => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsReady => _state == RenderSessionState.Ready && _engine.IsInitialized;

    public bool IsSoftwareBackend => _backend is ISoftwareBackend;

    public IResourceFactory? ResourceFactory => _backend?.GetResourceFactory();

    internal GraphicsBackendResolution? LastBackendResolution { get; private set; }

    internal Exception? LastInitializationError { get; private set; }

    public void Attach(GraphicsBackendPreference preference)
    {
        Attach(preference, backendOptions: null);
    }

    internal void Attach(GraphicsBackendPreference preference, VideraBackendOptions? backendOptions)
    {
        var shouldRaiseBackendReady = false;

        lock (_sync)
        {
            if (_state == RenderSessionState.Disposed)
            {
                return;
            }

            if (_preference != preference && IsReady)
            {
                SuspendUnsafe();
            }

            _preference = preference;
            _backendOptions = backendOptions;
            shouldRaiseBackendReady = TryInitializeUnsafe();
        }

        if (shouldRaiseBackendReady)
        {
            BackendReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public void BindHandle(IntPtr handle)
    {
        var shouldRaiseBackendReady = false;

        lock (_sync)
        {
            if (_state == RenderSessionState.Disposed)
            {
                return;
            }

            if (handle == IntPtr.Zero)
            {
                if (HandleState.IsBound)
                {
                    HandleState = HandleState.Clear();
                    if (IsReady && !IsSoftwareBackend)
                    {
                        SuspendUnsafe();
                    }
                }

                TransitionToWaitingStateUnsafe(CreateBackendRequest());
                return;
            }

            if (HandleState.Generation == 0)
            {
                HandleState = RenderSessionHandle.Create(handle);
            }
            else if (HandleState.Handle != handle)
            {
                HandleState = HandleState.Rebind(handle);
                if (IsReady && !IsSoftwareBackend)
                {
                    SuspendUnsafe();
                }
            }

            shouldRaiseBackendReady = TryInitializeUnsafe();
        }

        if (shouldRaiseBackendReady)
        {
            BackendReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Resize(uint width, uint height, float renderScale)
    {
        var shouldRaiseBackendReady = false;

        lock (_sync)
        {
            if (_state == RenderSessionState.Disposed || width == 0 || height == 0)
            {
                return;
            }

            _width = width;
            _height = height;
            _engine.RenderScale = renderScale;

            if (!IsReady)
            {
                shouldRaiseBackendReady = TryInitializeUnsafe();
            }
            else
            {
                _backend?.Resize((int)width, (int)height);
                _engine.Resize(width, height);
                EnsureBitmapUnsafe(width, height);
            }
        }

        if (shouldRaiseBackendReady)
        {
            BackendReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RenderOnce()
    {
        lock (_sync)
        {
            if (!IsReady || _backend == null)
            {
                return;
            }

            try
            {
                _engine.Draw();

                if (_backend is ISoftwareBackend softwareBackend)
                {
                    EnsureBitmapUnsafe(_width, _height);
                    if (_bitmap != null)
                    {
                        using var locked = _bitmap.Lock();
                        softwareBackend.CopyFrameTo(locked.Address, locked.RowBytes);
                    }

                    _requestRender?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.RenderFrameFailed(_logger, ex.Message, ex);
                LastInitializationError = ex;
                StopRenderLoopUnsafe();
                _bitmap?.Dispose();
                _bitmap = null;
                _engine.Suspend();
                _backend = null;
                _state = RenderSessionState.Faulted;
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_state == RenderSessionState.Disposed)
            {
                return;
            }

            _state = RenderSessionState.Disposed;
            StopRenderLoopUnsafe();
            _renderLoop?.Dispose();
            _renderLoop = null;
            _bitmap?.Dispose();
            _bitmap = null;
            _engine.Dispose();
            _backend = null;
            HandleState = RenderSessionHandle.Unbound;
        }
    }

    private bool TryInitializeUnsafe()
    {
        if (_state == RenderSessionState.Disposed || IsReady)
        {
            return false;
        }

        var request = CreateBackendRequest();
        if (_width == 0 || _height == 0)
        {
            _state = RenderSessionState.WaitingForSize;
            return false;
        }

        if (RequiresNativeHandle(request) && !HandleState.IsBound)
        {
            _state = RenderSessionState.WaitingForHandle;
            return false;
        }

        IGraphicsBackend? backend = null;
        try
        {
            var resolution = _backendResolutionFactory(request);
            backend = resolution.Backend;
            var handle = resolution.ResolvedPreference == GraphicsBackendPreference.Software ? IntPtr.Zero : HandleState.Handle;
            backend.Initialize(handle, (int)_width, (int)_height);

            _backend = backend;
            LastBackendResolution = resolution;
            LastInitializationError = null;
            _engine.Initialize(backend);
            _engine.Resize(_width, _height);
            EnsureBitmapUnsafe(_width, _height);
            StartRenderLoopUnsafe();
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

    private void SuspendUnsafe()
    {
        StopRenderLoopUnsafe();
        _bitmap?.Dispose();
        _bitmap = null;

        if (_engine.IsInitialized)
        {
            _engine.Suspend();
        }

        _backend = null;
        TransitionToWaitingStateUnsafe(CreateBackendRequest());
    }

    private void EnsureBitmapUnsafe(uint width, uint height)
    {
        if (width == 0 || height == 0)
        {
            return;
        }

        if (!IsSoftwareBackend)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            return;
        }

        if (_bitmap != null &&
            _bitmap.PixelSize.Width == width &&
            _bitmap.PixelSize.Height == height)
        {
            return;
        }

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(
            new PixelSize((int)width, (int)height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    private void StartRenderLoopUnsafe()
    {
        _renderLoop ??= _renderLoopFactory();
        _renderLoop.Start(RenderInterval, OnRenderLoopTick);
    }

    private void StopRenderLoopUnsafe()
    {
        _renderLoop?.Stop();
    }

    private void OnRenderLoopTick(object? sender, EventArgs e)
    {
        RenderOnce();
    }

    private GraphicsBackendRequest CreateBackendRequest()
    {
        if (_backendOptions == null)
        {
            return new GraphicsBackendRequest(
                _preference,
                BackendEnvironmentOverrideMode.PreferOverrides,
                AllowSoftwareFallback: true);
        }

        return new GraphicsBackendRequest(
            _preference,
            _backendOptions.EnvironmentOverrideMode,
            _backendOptions.AllowSoftwareFallback);
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

    private void TransitionToWaitingStateUnsafe(GraphicsBackendRequest request)
    {
        if (_state == RenderSessionState.Disposed)
        {
            return;
        }

        if (_width == 0 || _height == 0)
        {
            _state = RenderSessionState.WaitingForSize;
            return;
        }

        _state = RequiresNativeHandle(request) && !HandleState.IsBound
            ? RenderSessionState.WaitingForHandle
            : RenderSessionState.Detached;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Render session frame failed: {Error}")]
        public static partial void RenderFrameFailed(ILogger logger, string error, Exception exception);
    }

    internal interface IRenderLoopDriver : IDisposable
    {
        void Start(TimeSpan interval, EventHandler tick);

        void Stop();
    }

    private sealed class NullRenderLoopDriver : IRenderLoopDriver
    {
        public void Start(TimeSpan interval, EventHandler tick)
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class DispatcherRenderLoopDriver : IRenderLoopDriver
    {
        private readonly DispatcherTimer _timer = new(DispatcherPriority.Render);
        private EventHandler? _tick;

        public void Start(TimeSpan interval, EventHandler tick)
        {
            Stop();
            _tick = tick;
            _timer.Interval = interval;
            _timer.Tick += tick;
            _timer.Start();
        }

        public void Stop()
        {
            if (_tick == null)
            {
                return;
            }

            _timer.Stop();
            _timer.Tick -= _tick;
            _tick = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }

    private enum RenderSessionState
    {
        Detached = 0,
        WaitingForSize,
        WaitingForHandle,
        Ready,
        Faulted,
        Disposed
    }
}
