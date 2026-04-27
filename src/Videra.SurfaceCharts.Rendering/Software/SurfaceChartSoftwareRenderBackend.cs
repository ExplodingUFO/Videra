using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering.Software;

public sealed class SurfaceChartSoftwareRenderBackend : ISurfaceChartRenderBackend
{
    private readonly SurfaceRenderer _renderer;

    public SurfaceChartSoftwareRenderBackend(SurfaceRenderer? renderer = null)
    {
        _renderer = renderer ?? new SurfaceRenderer();
    }

    public SurfaceChartRenderBackendKind Kind => SurfaceChartRenderBackendKind.Software;

    public bool UsesNativeSurface => false;

    public SurfaceRenderScene? SoftwareScene { get; private set; }

    public SurfaceChartRenderSnapshot Render(
        SurfaceChartRenderInputs inputs,
        SurfaceChartRenderState state,
        SurfaceChartRenderChangeSet changeSet)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(changeSet);

        if (changeSet.FullResetRequired && state.ResidentTileCount == 0)
        {
            SoftwareScene = null;
        }

        if (state.Metadata is null
            || state.ResidentTileCount == 0
            || inputs.ViewWidth <= 0d
            || inputs.ViewHeight <= 0d)
        {
            SoftwareScene = null;
            return new SurfaceChartRenderSnapshot
            {
                ActiveBackend = Kind,
                IsReady = false,
                IsFallback = false,
                FallbackReason = null,
                UsesNativeSurface = UsesNativeSurface,
                ResidentTileCount = 0,
                VisibleTileCount = 0,
                ResidentTileBytes = 0,
            };
        }

        if (SoftwareScene is null
            || changeSet.FullResetRequired
            || changeSet.ResidencyDirty
            || changeSet.ColorDirty)
        {
            if (inputs.ColorMap is null)
            {
                SoftwareScene = null;
                return new SurfaceChartRenderSnapshot
                {
                    ActiveBackend = Kind,
                    IsReady = false,
                    IsFallback = false,
                    FallbackReason = null,
                    UsesNativeSurface = UsesNativeSurface,
                    ResidentTileCount = 0,
                    VisibleTileCount = 0,
                    ResidentTileBytes = 0,
                };
            }

            SoftwareScene = new SurfaceRenderScene(
                state.Metadata,
                state.ResidentTiles.Select(residentTile => _renderer.BuildTile(state.Metadata, residentTile.SourceTile, inputs.ColorMap)).ToArray());
        }

        return new SurfaceChartRenderSnapshot
        {
            ActiveBackend = Kind,
            IsReady = true,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = UsesNativeSurface,
            ResidentTileCount = state.ResidentTileCount,
            VisibleTileCount = state.ResidentTileCount,
            ResidentTileBytes = state.EstimatedResidentTileBytes,
        };
    }
}
