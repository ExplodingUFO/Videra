using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
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
        snapshot.VisibleTileCount.Should().Be(0);
        snapshot.ResidentTileBytes.Should().Be(0);
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
        snapshot.VisibleTileCount.Should().Be(1);
        snapshot.ResidentTileBytes.Should().BeGreaterThan(0);
        host.SoftwareScene.Should().NotBeNull();
        host.SoftwareScene!.Metadata.Should().BeSameAs(metadata);
        host.SoftwareScene.Tiles.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateInputs_WithViewStateAndCameraFrame_PersistsCameraTruthOnHost()
    {
        var host = new SurfaceChartRenderHost();
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, 320d, 180d, 1f);
        var inputs = new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            ViewState = viewState,
            CameraFrame = cameraFrame,
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
            RenderScale = 1f,
        };

        host.UpdateInputs(inputs);

        host.Inputs.ViewState.Should().Be(viewState);
        host.Inputs.CameraFrame.Should().Be(cameraFrame);
        host.Snapshot.IsReady.Should().BeTrue();
    }

    [Fact]
    public void UpdateInputs_WithCustomRenderer_UsesInjectedTranslationForSoftwareScene()
    {
        var renderState = new SurfaceChartRenderState(new OffsetSurfaceRenderer());
        var host = new SurfaceChartRenderHost(renderState);
        var metadata = CreateMetadata(width: 2, height: 2);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var inputs = new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height)),
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
            RenderScale = 1f,
        };

        host.UpdateInputs(inputs);

        host.RenderState.Should().BeSameAs(renderState);
        host.Snapshot.IsReady.Should().BeTrue();
        host.SoftwareScene.Should().NotBeNull();
        host.SoftwareScene!.Tiles.Should().ContainSingle();
        host.SoftwareScene.Tiles[0].Vertices[0].Should().Be(new SurfaceRenderVertex(new(100f, 17f, 7f), 0xFF112233u));
        host.RenderState.TryGetResidentTile(tile.Key, out var residentTile).Should().BeTrue();
        residentTile.SoftwareRenderTile.Vertices[0].Should().Be(new SurfaceRenderVertex(new(100f, 17f, 7f), 0xFF112233u));
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

    private sealed class OffsetSurfaceRenderer : SurfaceRenderer
    {
        public override SurfaceRenderTile BuildTile(SurfaceMetadata metadata, SurfaceTile tile, SurfaceColorMap colorMap)
        {
            var baseTile = base.BuildTile(metadata, tile, colorMap);
            var shiftedVertices = baseTile.Vertices
                .Select(static vertex => new SurfaceRenderVertex(
                    vertex.Position + new Vector3(100f, 5f, 7f),
                    0xFF112233u))
                .ToArray();

            return new SurfaceRenderTile(baseTile.Key, baseTile.Bounds, baseTile.Geometry, shiftedVertices);
        }
    }
}
