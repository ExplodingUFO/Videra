using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering.Software;
using Videra.Core.Graphics;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartRenderHost
{
    private readonly ISurfaceChartRenderBackend _softwareBackend;
    private readonly ISurfaceChartRenderBackend? _gpuBackend;
    private readonly string? _gpuBackendResolutionFailure;
    private readonly SurfaceChartRenderState _renderState;

    public SurfaceChartRenderHost(
        SurfaceChartRenderState renderState,
        ISurfaceChartRenderBackend? softwareBackend = null,
        ISurfaceChartRenderBackend? gpuBackend = null)
    {
        _renderState = renderState ?? throw new ArgumentNullException(nameof(renderState));
        var defaultGpuBackend = gpuBackend is null ? CreateDefaultGpuBackend() : default;
        _softwareBackend = softwareBackend ?? new SurfaceChartSoftwareRenderBackend(_renderState.Renderer);
        _gpuBackend = gpuBackend ?? defaultGpuBackend.Backend;
        _gpuBackendResolutionFailure = gpuBackend is null ? defaultGpuBackend.Diagnostic : null;
        Inputs = new SurfaceChartRenderInputs();
        LastChangeSet = new SurfaceChartRenderChangeSet();
        Snapshot = new SurfaceChartRenderSnapshot
        {
            ActiveBackend = SurfaceChartRenderBackendKind.Software,
            IsReady = false,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = false,
            ResidentTileCount = 0,
            VisibleTileCount = 0,
            ResidentTileBytes = 0,
        };
        RenderingStatus = SurfaceChartRenderingStatus.FromSnapshot(Snapshot);
    }

    public SurfaceChartRenderHost(
        ISurfaceChartRenderBackend? softwareBackend = null,
        ISurfaceChartRenderBackend? gpuBackend = null)
        : this(
            new SurfaceChartRenderState(),
            softwareBackend,
            gpuBackend)
    {
    }

    public SurfaceChartRenderInputs Inputs { get; private set; }

    public SurfaceChartRenderSnapshot Snapshot { get; private set; }

    public SurfaceChartRenderingStatus RenderingStatus { get; private set; }

    public SurfaceChartRenderChangeSet LastChangeSet { get; private set; }

    public SurfaceChartRenderState RenderState => _renderState;

    public bool HasGpuBackend => _gpuBackend is not null;

    public SurfaceRenderScene? SoftwareScene => Snapshot.ActiveBackend == SurfaceChartRenderBackendKind.Gpu
        ? _gpuBackend?.SoftwareScene
        : _softwareBackend.SoftwareScene;

    public SurfaceChartRenderSnapshot UpdateInputs(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        Inputs = inputs;
        LastChangeSet = _renderState.Update(inputs);
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
                return _gpuBackend.Render(inputs, _renderState, LastChangeSet);
            }
            catch (Exception ex)
            {
                return CreateGpuNotReadySnapshot(ex.Message);
            }
        }

        if (inputs.HandleBound && _gpuBackendResolutionFailure is not null)
        {
            return CreateGpuNotReadySnapshot(_gpuBackendResolutionFailure, usesNativeSurface: false);
        }

        return _softwareBackend.Render(
            inputs with
            {
                NativeHandle = IntPtr.Zero,
                HandleBound = false,
            },
            _renderState,
            LastChangeSet);
    }

    private SurfaceChartRenderSnapshot CreateGpuNotReadySnapshot(string diagnostic, bool usesNativeSurface = true)
    {
        return new SurfaceChartRenderSnapshot
        {
            ActiveBackend = SurfaceChartRenderBackendKind.Gpu,
            IsReady = false,
            IsFallback = false,
            FallbackReason = diagnostic,
            UsesNativeSurface = usesNativeSurface,
            ResidentTileCount = _renderState.ResidentTileCount,
            VisibleTileCount = 0,
            ResidentTileBytes = _renderState.EstimatedResidentTileBytes,
        };
    }

    private static (ISurfaceChartRenderBackend? Backend, string? Diagnostic) CreateDefaultGpuBackend()
    {
        try
        {
            var resolution = GraphicsBackendFactory.ResolveBackend(
                new GraphicsBackendRequest(
                    GraphicsBackendPreference.Auto,
                    BackendEnvironmentOverrideMode.PreferOverrides,
                    AllowSoftwareFallback: false));

            return resolution.ResolvedPreference == GraphicsBackendPreference.Software
                ? (null, "Resolved graphics backend is software; native SurfaceCharts rendering is unavailable.")
                : (new SurfaceChartGpuRenderBackend(resolution.Backend), null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }
}
