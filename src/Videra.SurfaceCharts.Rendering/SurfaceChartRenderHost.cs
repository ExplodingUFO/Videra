using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering.Software;
using Videra.Core.Graphics;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartRenderHost
{
    private readonly ISurfaceChartRenderBackend _softwareBackend;
    private readonly ISurfaceChartRenderBackend? _gpuBackend;
    private readonly bool _allowSoftwareFallback;

    public SurfaceChartRenderHost(
        ISurfaceChartRenderBackend? softwareBackend = null,
        ISurfaceChartRenderBackend? gpuBackend = null,
        bool allowSoftwareFallback = true)
    {
        _softwareBackend = softwareBackend ?? new SurfaceChartSoftwareRenderBackend();
        _gpuBackend = gpuBackend ?? CreateDefaultGpuBackend();
        _allowSoftwareFallback = allowSoftwareFallback;
        Inputs = new SurfaceChartRenderInputs();
        Snapshot = new SurfaceChartRenderSnapshot
        {
            ActiveBackend = SurfaceChartRenderBackendKind.Software,
            IsReady = false,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = false,
            ResidentTileCount = 0,
        };
        RenderingStatus = SurfaceChartRenderingStatus.FromSnapshot(Snapshot);
    }

    public SurfaceChartRenderInputs Inputs { get; private set; }

    public SurfaceChartRenderSnapshot Snapshot { get; private set; }

    public SurfaceChartRenderingStatus RenderingStatus { get; private set; }

    public bool HasGpuBackend => _gpuBackend is not null;

    public SurfaceRenderScene? SoftwareScene => Snapshot.ActiveBackend == SurfaceChartRenderBackendKind.Gpu
        ? _gpuBackend?.SoftwareScene
        : _softwareBackend.SoftwareScene;

    public SurfaceChartRenderSnapshot UpdateInputs(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        Inputs = inputs;
        Snapshot = Render(inputs);
        RenderingStatus = SurfaceChartRenderingStatus.FromSnapshot(Snapshot);
        return Snapshot;
    }

    private SurfaceChartRenderSnapshot Render(SurfaceChartRenderInputs inputs)
    {
        if (inputs.HandleBound && _gpuBackend is not null)
        {
            try
            {
                return _gpuBackend.Render(inputs);
            }
            catch (Exception ex) when (_allowSoftwareFallback)
            {
                var fallbackSnapshot = _softwareBackend.Render(
                    inputs with
                    {
                        NativeHandle = IntPtr.Zero,
                        HandleBound = false,
                    });

                return fallbackSnapshot with
                {
                    ActiveBackend = SurfaceChartRenderBackendKind.Software,
                    IsFallback = true,
                    FallbackReason = ex.Message,
                    UsesNativeSurface = false,
                };
            }
        }

        return _softwareBackend.Render(
            inputs with
            {
                NativeHandle = IntPtr.Zero,
                HandleBound = false,
            });
    }

    private static ISurfaceChartRenderBackend? CreateDefaultGpuBackend()
    {
        try
        {
            var resolution = GraphicsBackendFactory.ResolveBackend(
                new GraphicsBackendRequest(
                    GraphicsBackendPreference.Auto,
                    BackendEnvironmentOverrideMode.PreferOverrides,
                    AllowSoftwareFallback: false));

            return resolution.ResolvedPreference == GraphicsBackendPreference.Software
                ? null
                : new SurfaceChartGpuRenderBackend(resolution.Backend);
        }
        catch
        {
            return null;
        }
    }
}
