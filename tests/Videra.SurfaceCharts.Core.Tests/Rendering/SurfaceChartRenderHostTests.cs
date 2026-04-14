using FluentAssertions;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public sealed class SurfaceChartRenderHostTests
{
    [Fact]
    public void DefaultHost_ExposesSoftwareSnapshotSurfaceWithoutAvalonia()
    {
        var host = new SurfaceChartRenderHost();

        typeof(ISurfaceChartRenderBackend).IsInterface.Should().BeTrue();

        SurfaceChartRenderSnapshot snapshot = host.Snapshot;
        snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        snapshot.IsReady.Should().BeFalse();
        snapshot.ResidentTileCount.Should().Be(0);
        snapshot.UsesNativeSurface.Should().BeFalse();
        host.SoftwareScene.Should().BeNull();
    }

    [Fact]
    public void UpdateInputs_WithMetadataTilesViewportProjectionAndViewSize_ProducesSoftwareReadyState()
    {
        var host = new SurfaceChartRenderHost();
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var inputs = new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            Viewport = new SurfaceViewport(0, 0, 4, 4),
            ProjectionSettings = default,
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
            RenderScale = 1f,
        };

        host.UpdateInputs(inputs);

        SurfaceChartRenderSnapshot snapshot = host.Snapshot;
        snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        snapshot.IsReady.Should().BeTrue();
        snapshot.IsFallback.Should().BeFalse();
        snapshot.FallbackReason.Should().BeNull();
        snapshot.UsesNativeSurface.Should().BeFalse();
        snapshot.ResidentTileCount.Should().Be(1);
        host.SoftwareScene.Should().NotBeNull();
        host.SoftwareScene!.Metadata.Should().BeSameAs(metadata);
        host.SoftwareScene.Tiles.Should().HaveCount(1);
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

    private static SurfaceTile CreateTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var width = endX - startX;
        var height = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, width, height);
        var values = new float[width * height];
        Array.Fill(values, tileValue);
        return new SurfaceTile(key, width, height, bounds, values, metadata.ValueRange);
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
    }
}
