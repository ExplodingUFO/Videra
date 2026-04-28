using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartIncrementalRenderingTests
{
    [Fact]
    public void RepeatedViewportChanges_ReuseResidentGeometryInsteadOfRebuildingFullResidentState()
    {
        var backend = new CountingRenderBackend(SurfaceChartRenderBackendKind.Software, usesNativeSurface: false);
        var host = new SurfaceChartRenderHost(
            softwareBackend: backend,
            gpuBackend: null,
            allowSoftwareFallback: true);
        var metadata = CreateMetadata(width: 8, height: 8);
        var residentTile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var colorMap = CreateColorMap(metadata);

        host.UpdateInputs(CreateInputs(metadata, [residentTile], colorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: false));

        host.UpdateInputs(CreateInputs(metadata, [residentTile], colorMap, new SurfaceViewport(0, 0, 6, 6), handleBound: false));
        host.UpdateInputs(CreateInputs(metadata, [residentTile], colorMap, new SurfaceViewport(1, 1, 6, 6), handleBound: false));
        host.UpdateInputs(CreateInputs(metadata, [residentTile], colorMap, new SurfaceViewport(2, 2, 4, 4), handleBound: false));

        backend.FullResetRenderCount.Should().Be(1);
        backend.ProjectionOnlyRenderCount.Should().Be(3);
        backend.ChangeSets.Skip(1).Should().OnlyContain(static changeSet =>
            !changeSet.FullResetRequired
            && !changeSet.ResidencyDirty
            && !changeSet.ColorDirty
            && changeSet.ProjectionDirty);
    }

    [Fact]
    public void DetailResidentReplacement_UpdatesOnlyChangedResidentKeys()
    {
        var backend = new CountingRenderBackend(SurfaceChartRenderBackendKind.Software, usesNativeSurface: false);
        var host = new SurfaceChartRenderHost(
            softwareBackend: backend,
            gpuBackend: null,
            allowSoftwareFallback: true);
        var metadata = CreateMetadata(width: 8, height: 8);
        var overviewTile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 4f);
        var detailTile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(1, 1, 0, 0), tileValue: 9f);
        var colorMap = CreateColorMap(metadata);

        host.UpdateInputs(CreateInputs(metadata, [overviewTile], colorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: false));
        host.UpdateInputs(CreateInputs(metadata, [detailTile], colorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: false));

        var delta = backend.ChangeSets[^1];
        delta.ResidencyDirty.Should().BeTrue();
        delta.AddedResidentKeys.Should().Equal(detailTile.Key);
        delta.RemovedResidentKeys.Should().Equal(overviewTile.Key);
        delta.ColorChangedKeys.Should().BeEmpty();
    }

    [Fact]
    public void GpuAndSoftwarePaths_ShareTheSameResidentDeltaTruth()
    {
        var softwareBackend = new CountingRenderBackend(SurfaceChartRenderBackendKind.Software, usesNativeSurface: false);
        var gpuBackend = new CountingRenderBackend(SurfaceChartRenderBackendKind.Gpu, usesNativeSurface: true);
        var softwareHost = new SurfaceChartRenderHost(
            softwareBackend: softwareBackend,
            gpuBackend: null,
            allowSoftwareFallback: true);
        var gpuHost = new SurfaceChartRenderHost(
            softwareBackend: new CountingRenderBackend(SurfaceChartRenderBackendKind.Software, usesNativeSurface: false),
            gpuBackend: gpuBackend,
            allowSoftwareFallback: true);
        var metadata = CreateMetadata(width: 8, height: 8);
        var tile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 7f);
        var initialColorMap = CreateColorMap(metadata);
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));

        softwareHost.UpdateInputs(CreateInputs(metadata, [tile], initialColorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: false));
        softwareHost.UpdateInputs(CreateInputs(metadata, [tile], replacementColorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: false));
        softwareHost.UpdateInputs(CreateInputs(metadata, [tile], replacementColorMap, new SurfaceViewport(2, 2, 4, 4), handleBound: false));

        gpuHost.UpdateInputs(CreateInputs(metadata, [tile], initialColorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: true));
        gpuHost.UpdateInputs(CreateInputs(metadata, [tile], replacementColorMap, new SurfaceViewport(0, 0, 8, 8), handleBound: true));
        gpuHost.UpdateInputs(CreateInputs(metadata, [tile], replacementColorMap, new SurfaceViewport(2, 2, 4, 4), handleBound: true));

        softwareBackend.ChangeSets.Select(ToDirtySignature).Should().Equal(gpuBackend.ChangeSets.Select(ToDirtySignature));
        softwareBackend.ChangeSets.Select(changeSet => changeSet.ColorChangedKeys.ToArray()).Should()
            .BeEquivalentTo(gpuBackend.ChangeSets.Select(changeSet => changeSet.ColorChangedKeys.ToArray()));
    }

    private static SurfaceChartRenderInputs CreateInputs(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> tiles,
        SurfaceColorMap colorMap,
        SurfaceViewport viewport,
        bool handleBound)
    {
        var viewState = SurfaceViewState.CreateDefault(metadata, viewport.ToDataWindow());

        return new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = tiles,
            ColorMap = colorMap,
            ViewState = viewState,
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = handleBound ? new IntPtr(0x1234) : IntPtr.Zero,
            HandleBound = handleBound,
            RenderScale = 1f,
        };
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0d, maximum: width - 1d),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0d, maximum: height - 1d),
            new SurfaceValueRange(0d, 100d));
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
    }

    private static string ToDirtySignature(SurfaceChartRenderChangeSet changeSet)
    {
        return $"{changeSet.FullResetRequired}:{changeSet.ResidencyDirty}:{changeSet.ColorDirty}:{changeSet.ProjectionDirty}";
    }

    private sealed class CountingRenderBackend : ISurfaceChartRenderBackend
    {
        public CountingRenderBackend(SurfaceChartRenderBackendKind kind, bool usesNativeSurface)
        {
            Kind = kind;
            UsesNativeSurface = usesNativeSurface;
        }

        public SurfaceChartRenderBackendKind Kind { get; }

        public bool UsesNativeSurface { get; }

        public SurfaceRenderScene? SoftwareScene => null;

        public List<SurfaceChartRenderChangeSet> ChangeSets { get; } = [];

        public int FullResetRenderCount { get; private set; }

        public int ProjectionOnlyRenderCount { get; private set; }

        public SurfaceChartRenderSnapshot Render(
            SurfaceChartRenderInputs inputs,
            SurfaceChartRenderState state,
            SurfaceChartRenderChangeSet changeSet)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(changeSet);

            ChangeSets.Add(changeSet);
            if (changeSet.FullResetRequired)
            {
                FullResetRenderCount++;
            }

            if (!changeSet.FullResetRequired
                && !changeSet.ResidencyDirty
                && !changeSet.ColorDirty
                && changeSet.ProjectionDirty)
            {
                ProjectionOnlyRenderCount++;
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
}
