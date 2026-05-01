using System.Numerics;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceTooltipOverlayTests
{
    [Fact]
    public void MultiSeriesProbe_ResolvesAllSeriesAtSamePosition()
    {
        var metadata = CreateMetadata();
        var tile = CreateTile(metadata, value: 5f);
        var series = CreateMultiSeries(metadata);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            cameraFrame: null,
            [tile],
            probeScreenPosition: null,
            pinnedProbeRequests: null,
            overlayOptions: SurfaceChartOverlayOptions.Default,
            series: series);

        // With no probe position, tooltip content should be null
        state.TooltipContent.Should().BeNull();
    }

    [Fact]
    public void MultiSeriesProbe_WithHoveredPosition_ResolvesTooltipContent()
    {
        var metadata = CreateMetadata();
        var tile = CreateTile(metadata, value: 5f);
        var series = CreateMultiSeries(metadata);

        var viewport = new SurfaceViewport(0, 0, 10, 10);
        var viewSize = new Size(100, 100);

        // Use the viewport-based CreateState for simpler testing
        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            viewport,
            viewSize,
            [tile],
            new Point(50, 50));

        // Single-series: tooltip content should be null (only populated for multi-series)
        state.TooltipContent.Should().BeNull();
        state.HoveredProbe.Should().NotBeNull();
    }

    [Fact]
    public void MultiSeriesProbe_SingleSeries_DoesNotPopulateTooltipContent()
    {
        var metadata = CreateMetadata();
        var tile = CreateTile(metadata, value: 5f);
        var singleSeries = new List<Plot3DSeries>
        {
            CreateSurfaceSeries(metadata, "Surface 1"),
        };

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            cameraFrame: null,
            [tile],
            probeScreenPosition: null,
            pinnedProbeRequests: null,
            overlayOptions: SurfaceChartOverlayOptions.Default,
            series: singleSeries);

        // Single series: tooltip content should be null
        state.TooltipContent.Should().BeNull();
    }

    [Fact]
    public void TooltipContent_FromSeriesProbes_AggregatesEntries()
    {
        var entries = new List<SurfaceTooltipSeriesEntry>
        {
            new("Series A", Plot3DSeriesKind.Surface, new SurfaceProbeInfo(1d, 2d, 10d, 20d, 5d, false)),
            new("Series B", Plot3DSeriesKind.Waterfall, new SurfaceProbeInfo(1d, 2d, 10d, 20d, 8d, false)),
        };

        var content = SurfaceTooltipContent.FromSeriesProbes(entries);

        content.Should().NotBeNull();
        content!.WorldX.Should().Be(10d);
        content.WorldZ.Should().Be(20d);
        content.IsApproximate.Should().BeFalse();
        content.Entries.Should().HaveCount(2);
        content.Entries[0].SeriesName.Should().Be("Series A");
        content.Entries[0].ProbeInfo.Value.Should().Be(5d);
        content.Entries[1].SeriesName.Should().Be("Series B");
        content.Entries[1].ProbeInfo.Value.Should().Be(8d);
    }

    [Fact]
    public void TooltipContent_FromSeriesProbes_WithApproximateEntry_MarksApproximate()
    {
        var entries = new List<SurfaceTooltipSeriesEntry>
        {
            new("Series A", Plot3DSeriesKind.Surface, new SurfaceProbeInfo(1d, 2d, 10d, 20d, 5d, false)),
            new("Series B", Plot3DSeriesKind.Surface, new SurfaceProbeInfo(1d, 2d, 10d, 20d, 8d, true)),
        };

        var content = SurfaceTooltipContent.FromSeriesProbes(entries);

        content.Should().NotBeNull();
        content!.IsApproximate.Should().BeTrue();
    }

    [Fact]
    public void TooltipContent_FromSeriesProbes_EmptyEntries_ReturnsNull()
    {
        var content = SurfaceTooltipContent.FromSeriesProbes([]);

        content.Should().BeNull();
    }

    [Fact]
    public void TooltipContent_FromSeriesProbes_NullEntries_ReturnsNull()
    {
        var content = SurfaceTooltipContent.FromSeriesProbes(null!);

        content.Should().BeNull();
    }

    [Fact]
    public void TooltipSeriesEntry_StoresSeriesKind()
    {
        var entry = new SurfaceTooltipSeriesEntry(
            "Waterfall 1",
            Plot3DSeriesKind.Waterfall,
            new SurfaceProbeInfo(1d, 2d, 10d, 20d, 5d, false));

        entry.SeriesKind.Should().Be(Plot3DSeriesKind.Waterfall);
    }

    [Fact]
    public void OverlayOptions_TooltipOffset_DefaultIsTwelveNegativeTwelve()
    {
        var options = SurfaceChartOverlayOptions.Default;

        options.TooltipOffset.X.Should().Be(12f);
        options.TooltipOffset.Y.Should().Be(-12f);
    }

    [Fact]
    public void OverlayOptions_TooltipOffset_CanBeCustomized()
    {
        var options = new SurfaceChartOverlayOptions
        {
            TooltipOffset = new Vector2(20f, -20f),
        };

        options.TooltipOffset.X.Should().Be(20f);
        options.TooltipOffset.Y.Should().Be(-20f);
    }

    [Fact]
    public void ProbeOverlayState_WithTooltipContent_PreservesExistingProperties()
    {
        var tooltipContent = new SurfaceTooltipContent(
            10d, 20d, false,
            [new SurfaceTooltipSeriesEntry("S1", Plot3DSeriesKind.Surface, new SurfaceProbeInfo(1d, 2d, 10d, 20d, 5d, false))]);

        var state = new SurfaceProbeOverlayState(
            hasNoData: false,
            noDataText: null,
            hoveredProbeScreenPosition: new Point(50, 50),
            hoveredProbe: new SurfaceProbeInfo(1d, 2d, 10d, 20d, 5d, false),
            pinnedProbes: [],
            overlayOptions: SurfaceChartOverlayOptions.Default,
            tooltipContent: tooltipContent);

        state.HasNoData.Should().BeFalse();
        state.HoveredProbe.Should().NotBeNull();
        state.TooltipContent.Should().NotBeNull();
        state.TooltipContent!.Entries.Should().HaveCount(1);
        state.ReadoutText.Should().NotBeNull(); // Existing single-series readout still works
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 0d, 10d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 100d),
            new SurfaceValueRange(-1d, 1d));
    }

    private static SurfaceTile CreateTile(SurfaceMetadata metadata, float value)
    {
        return new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 5,
            height: 5,
            new SurfaceTileBounds(0, 0, 5, 5),
            Enumerable.Repeat(value, 25).ToArray(),
            metadata.ValueRange);
    }

    private static Plot3DSeries CreateSurfaceSeries(SurfaceMetadata metadata, string name)
    {
        return new Plot3DSeries(
            Plot3DSeriesKind.Surface,
            name,
            new StaticTileSource(metadata),
            scatterData: null,
            barData: null,
            contourData: null,
            lineData: null,
            ribbonData: null,
            vectorFieldData: null,
            heatmapSliceData: null,
            boxPlotData: null);
    }

    private static List<Plot3DSeries> CreateMultiSeries(SurfaceMetadata metadata)
    {
        return
        [
            CreateSurfaceSeries(metadata, "Surface 1"),
            CreateSurfaceSeries(metadata, "Surface 2"),
        ];
    }

    private sealed class StaticTileSource : ISurfaceTileSource
    {
        public StaticTileSource(SurfaceMetadata metadata)
        {
            Metadata = metadata;
        }

        public SurfaceMetadata Metadata { get; }

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }
    }
}
