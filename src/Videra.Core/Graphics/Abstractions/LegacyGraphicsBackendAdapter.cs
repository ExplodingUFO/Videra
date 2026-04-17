using System.Numerics;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Bridges the v1 monolithic backend contract into the internal device/surface split.
/// This keeps platform backends unchanged while the orchestrator and engine migrate.
/// </summary>
internal sealed class LegacyGraphicsBackendAdapter : IGraphicsDevice
{
    private readonly IGraphicsBackend _backend;

    public LegacyGraphicsBackendAdapter(IGraphicsBackend backend, GraphicsBackendPreference? activeBackendPreference = null)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        ActiveBackendPreference = activeBackendPreference ?? InferPreference(backend);
    }

    public GraphicsBackendPreference? ActiveBackendPreference { get; }

    public bool IsSoftwareBackend => _backend is ISoftwareBackend;

    public IResourceFactory ResourceFactory => _backend.GetResourceFactory();

    public ICommandExecutor CommandExecutor => _backend.GetCommandExecutor();

    internal IGraphicsBackend LegacyBackend => _backend;

    public IRenderSurface CreateRenderSurface()
    {
        return new LegacyRenderSurface(_backend);
    }

    public void Dispose()
    {
        _backend.Dispose();
    }

    private static GraphicsBackendPreference? InferPreference(IGraphicsBackend backend)
    {
        if (backend is ISoftwareBackend)
        {
            return GraphicsBackendPreference.Software;
        }

        var typeName = backend.GetType().Name;
        if (typeName.Contains("D3D11", StringComparison.OrdinalIgnoreCase))
        {
            return GraphicsBackendPreference.D3D11;
        }

        if (typeName.Contains("Vulkan", StringComparison.OrdinalIgnoreCase))
        {
            return GraphicsBackendPreference.Vulkan;
        }

        if (typeName.Contains("Metal", StringComparison.OrdinalIgnoreCase))
        {
            return GraphicsBackendPreference.Metal;
        }

        return null;
    }

    private sealed class LegacyRenderSurface : IRenderSurface
    {
        private readonly IGraphicsBackend _backend;

        public LegacyRenderSurface(IGraphicsBackend backend)
        {
            _backend = backend;
        }

        public bool IsInitialized => _backend.IsInitialized;

        public bool UsesSoftwarePresentationCopy => _backend is ISoftwareBackend;

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            _backend.Initialize(windowHandle, width, height);
        }

        public void Resize(int width, int height)
        {
            _backend.Resize(width, height);
        }

        public IFrameContext BeginFrame(Vector4 clearColor)
        {
            _backend.SetClearColor(clearColor);
            _backend.BeginFrame();
            return new LegacyFrameContext(_backend);
        }

        public void Dispose()
        {
        }
    }

    private sealed class LegacyFrameContext : IFrameContext
    {
        private readonly IGraphicsBackend _backend;
        private bool _disposed;

        public LegacyFrameContext(IGraphicsBackend backend)
        {
            _backend = backend;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _backend.EndFrame();
        }
    }
}
