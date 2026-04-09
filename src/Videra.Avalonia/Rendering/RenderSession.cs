using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

namespace Videra.Avalonia.Rendering;

internal sealed partial class RenderSession : IDisposable
{
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(16);
    private readonly object _sync = new();

    private readonly Action? _beforeRender;
    private readonly Action? _requestRender;
    private readonly ILogger _logger;
    private readonly Func<IRenderLoopDriver> _renderLoopFactory;
    private readonly Func<uint, uint, WriteableBitmap?> _bitmapFactory;
    private readonly RenderSessionOrchestrator _orchestrator;

    private IRenderLoopDriver? _renderLoop;
    private WriteableBitmap? _bitmap;
    private bool _renderLoopRunning;

    public RenderSession(
        VideraEngine engine,
        Func<GraphicsBackendPreference, IGraphicsBackend>? backendFactory = null,
        Action? beforeRender = null,
        Action? requestRender = null,
        ILogger? logger = null,
        Func<IRenderLoopDriver>? renderLoopFactory = null,
        Func<GraphicsBackendRequest, GraphicsBackendResolution>? backendResolutionFactory = null,
        Func<uint, uint, WriteableBitmap?>? bitmapFactory = null)
    {
        _beforeRender = beforeRender;
        _requestRender = requestRender;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<RenderSession>();
        _renderLoopFactory = renderLoopFactory ?? (() => new NullRenderLoopDriver());
        _bitmapFactory = bitmapFactory ?? CreateWriteableBitmap;
        _orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory,
            backendResolutionFactory);
    }

    public event EventHandler? BackendReady;

    public WriteableBitmap? Bitmap => _bitmap;

    public RenderSessionHandle HandleState => _orchestrator.HandleState;

    public bool IsInitialized => _orchestrator.IsInitialized;

    public bool IsReady => _orchestrator.IsReady;

    public bool IsSoftwareBackend => _orchestrator.IsSoftwareBackend;

    public IResourceFactory? ResourceFactory => _orchestrator.ResourceFactory;

    internal VideraEngine Engine => _orchestrator.Engine;

    internal GraphicsBackendResolution? LastBackendResolution => _orchestrator.LastBackendResolution;

    internal Exception? LastInitializationError => _orchestrator.LastInitializationError;

    internal string? LastResolvedDisplayServer => _orchestrator.LastResolvedDisplayServer;

    internal bool LastDisplayServerFallbackUsed => _orchestrator.LastDisplayServerFallbackUsed;

    internal string? LastDisplayServerFallbackReason => _orchestrator.LastDisplayServerFallbackReason;

    internal RenderPipelineSnapshot? LastPipelineSnapshot => _orchestrator.LastPipelineSnapshot;

    internal RenderSessionSnapshot OrchestrationSnapshot => _orchestrator.Snapshot;

    internal RenderCapabilitySnapshot RenderCapabilities => _orchestrator.RenderCapabilities;

    internal void SetDisplayServerDiagnostics(string? resolvedDisplayServer, bool fallbackUsed, string? fallbackReason)
    {
        lock (_sync)
        {
            _orchestrator.SetDisplayServerDiagnostics(resolvedDisplayServer, fallbackUsed, fallbackReason);
        }
    }

    public void Attach(GraphicsBackendPreference preference)
    {
        Attach(preference, backendOptions: null);
    }

    internal void Attach(GraphicsBackendPreference preference, VideraBackendOptions? backendOptions)
    {
        var shouldRaiseBackendReady = false;

        lock (_sync)
        {
            try
            {
                shouldRaiseBackendReady = _orchestrator.Attach(preference, backendOptions);
            }
            finally
            {
                SyncRuntimeShellUnsafe();
            }
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
            try
            {
                shouldRaiseBackendReady = _orchestrator.BindHandle(handle);
            }
            finally
            {
                SyncRuntimeShellUnsafe();
            }
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
            try
            {
                shouldRaiseBackendReady = _orchestrator.Resize(width, height, renderScale);
            }
            finally
            {
                SyncRuntimeShellUnsafe();
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
            _beforeRender?.Invoke();
            var result = _orchestrator.RenderOnce();
            if (result.Faulted)
            {
                if (result.Error != null)
                {
                    Log.RenderFrameFailed(_logger, result.Error.Message, result.Error);
                }

                StopRenderLoopUnsafe();
                _bitmap?.Dispose();
                _bitmap = null;
                return;
            }

            if (result.SoftwareBackend != null)
            {
                var snapshot = _orchestrator.Snapshot;
                EnsureBitmapUnsafe(snapshot.Inputs.Width, snapshot.Inputs.Height);
                if (_bitmap != null)
                {
                    using var locked = _bitmap.Lock();
                    result.SoftwareBackend.CopyFrameTo(locked.Address, locked.RowBytes);
                }
            }

            _requestRender?.Invoke();
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_orchestrator.Snapshot.State == RenderSessionState.Disposed)
            {
                return;
            }

            StopRenderLoopUnsafe();
            _renderLoop?.Dispose();
            _renderLoop = null;
            _renderLoopRunning = false;
            _bitmap?.Dispose();
            _bitmap = null;
            _orchestrator.Dispose();
        }
    }

    private void SyncRuntimeShellUnsafe()
    {
        var snapshot = _orchestrator.Snapshot;

        if (_orchestrator.IsReady)
        {
            StartRenderLoopUnsafe();
        }
        else
        {
            StopRenderLoopUnsafe();
        }

        if (!snapshot.UsesSoftwarePresentationCopy || snapshot.Inputs.Width == 0 || snapshot.Inputs.Height == 0)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            return;
        }

        EnsureBitmapUnsafe(snapshot.Inputs.Width, snapshot.Inputs.Height);
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
        _bitmap = _bitmapFactory(width, height);
    }

    private void StartRenderLoopUnsafe()
    {
        if (_renderLoopRunning)
        {
            return;
        }

        _renderLoop ??= _renderLoopFactory();
        _renderLoop.Start(RenderInterval, OnRenderLoopTick);
        _renderLoopRunning = true;
    }

    private void StopRenderLoopUnsafe()
    {
        if (!_renderLoopRunning)
        {
            return;
        }

        _renderLoop?.Stop();
        _renderLoopRunning = false;
    }

    private void OnRenderLoopTick(object? sender, EventArgs e)
    {
        RenderOnce();
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

    private static WriteableBitmap CreateWriteableBitmap(uint width, uint height)
    {
        return new WriteableBitmap(
            new PixelSize((int)width, (int)height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }
}
