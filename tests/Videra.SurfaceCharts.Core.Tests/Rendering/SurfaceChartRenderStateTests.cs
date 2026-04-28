using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public sealed class SurfaceChartRenderStateTests
{
    [Fact]
    public void ViewportChanges_MarkProjectionDirty_WithoutRebuildingUnchangedResidentGeometry()
    {
        var metadata = CreateMetadata(width: 8, height: 8);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 6f);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 8, 8)));
        state.TryGetResidentTile(tile.Key, out var initialResident).Should().BeTrue();

        var viewportChange = state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(2, 1, 4, 4)));

        viewportChange.FullResetRequired.Should().BeFalse();
        viewportChange.ResidencyDirty.Should().BeFalse();
        viewportChange.ColorDirty.Should().BeFalse();
        viewportChange.ProjectionDirty.Should().BeTrue();
        viewportChange.AddedResidentKeys.Should().BeEmpty();
        viewportChange.RemovedResidentKeys.Should().BeEmpty();
        state.TryGetResidentTile(tile.Key, out var viewportResident).Should().BeTrue();
        viewportResident.SoftwareRenderTile.Should().BeSameAs(initialResident.SoftwareRenderTile);
        viewportResident.SampleValues.Should().BeSameAs(initialResident.SampleValues);
        viewportResident.SampleValueMemory.Equals(initialResident.SampleValueMemory).Should().BeTrue();
    }

    [Fact]
    public void CameraFrameChanges_MarkProjectionDirty_WithoutRebuildingUnchangedResidentGeometry()
    {
        var metadata = CreateMetadata(width: 8, height: 8);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 6f);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 8d, 8d));
        var rotatedViewState = new SurfaceViewState(
            viewState.DataWindow,
            new SurfaceCameraPose(
                viewState.Camera.Target,
                yawDegrees: 210d,
                pitchDegrees: 15d,
                distance: viewState.Camera.Distance,
                fieldOfViewDegrees: viewState.Camera.FieldOfViewDegrees));
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 8, 8)));
        state.TryGetResidentTile(tile.Key, out var initialResident).Should().BeTrue();

        var cameraChange = state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 8, 8), rotatedViewState));

        cameraChange.FullResetRequired.Should().BeFalse();
        cameraChange.ResidencyDirty.Should().BeFalse();
        cameraChange.ColorDirty.Should().BeFalse();
        cameraChange.ProjectionDirty.Should().BeTrue();
        state.TryGetResidentTile(tile.Key, out var rotatedResident).Should().BeTrue();
        rotatedResident.SoftwareRenderTile.Should().BeSameAs(initialResident.SoftwareRenderTile);
        rotatedResident.SampleValues.Should().BeSameAs(initialResident.SampleValues);
        rotatedResident.SampleValueMemory.Equals(initialResident.SampleValueMemory).Should().BeTrue();
    }

    [Fact]
    public void ColorMapChanges_UpdateColorTruth_WithoutChangingSamplePositionMapping()
    {
        var metadata = CreateMetadata(width: 8, height: 8);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 9f);
        var initialColorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var updatedColorMap = CreateColorMap(metadata, startColor: 0xFFB03020u, endColor: 0xFF20FFD0u);
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [tile], initialColorMap, new SurfaceViewport(0, 0, 8, 8)));
        state.TryGetResidentTile(tile.Key, out var initialResident).Should().BeTrue();

        var colorChange = state.Update(CreateInputs(metadata, [tile], updatedColorMap, new SurfaceViewport(0, 0, 8, 8)));

        colorChange.FullResetRequired.Should().BeFalse();
        colorChange.ResidencyDirty.Should().BeFalse();
        colorChange.ColorDirty.Should().BeTrue();
        colorChange.ProjectionDirty.Should().BeFalse();
        colorChange.ColorChangedKeys.Should().BeEmpty();
        state.TryGetResidentTile(tile.Key, out var colorResident).Should().BeTrue();
        colorResident.SoftwareRenderTile.Should().BeSameAs(initialResident.SoftwareRenderTile);
        colorResident.SampleValues.Should().BeSameAs(initialResident.SampleValues);
        colorResident.SampleValueMemory.Equals(initialResident.SampleValueMemory).Should().BeTrue();
    }

    [Fact]
    public void IndependentColorField_CachesColorScalarSamplesForResidentTile()
    {
        var metadata = CreateMetadata(width: 2, height: 2, valueMaximum: 40d);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: metadata.ValueRange),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 40f, 30f, 20f, 10f },
                range: metadata.ValueRange));
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 2, 2)));

        state.TryGetResidentTile(tile.Key, out var residentTile).Should().BeTrue();
        residentTile.SampleValues.Should().Equal(40f, 30f, 20f, 10f);
        residentTile.SampleValueMemory.Equals(tile.ColorField!.Values).Should().BeTrue();
    }

    [Fact]
    public void ResidentTileCtor_WithExplicitSampleValues_PreservesLegacySequenceContract()
    {
        var metadata = CreateMetadata(width: 2, height: 2, valueMaximum: 40d);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 10f);
        var renderTile = new SurfaceRenderer().BuildTile(
            metadata,
            tile,
            CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu));
        var sampleValues = new float[] { 4f, 3f, 2f, 1f };

        var residentTile = new SurfaceChartResidentTile(tile, renderTile, sampleValues);
        sampleValues[0] = 99f;

        residentTile.SampleValues.Should().Equal(4f, 3f, 2f, 1f);
        residentTile.SampleValueMemory.Span.ToArray().Should().Equal(4f, 3f, 2f, 1f);
    }

    [Fact]
    public void ResidencyChanges_OnlyTouchChangedKeys()
    {
        var metadata = CreateMetadata(width: 8, height: 8);
        var overviewTile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 3f);
        var detailTile = CreateTile(metadata, new SurfaceTileKey(1, 1, 0, 0), tileValue: 7f);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [overviewTile], colorMap, new SurfaceViewport(0, 0, 8, 8)));

        var residencyChange = state.Update(CreateInputs(metadata, [detailTile], colorMap, new SurfaceViewport(0, 0, 8, 8)));

        residencyChange.FullResetRequired.Should().BeFalse();
        residencyChange.ResidencyDirty.Should().BeTrue();
        residencyChange.ColorDirty.Should().BeFalse();
        residencyChange.ProjectionDirty.Should().BeFalse();
        residencyChange.AddedResidentKeys.Should().Equal(detailTile.Key);
        residencyChange.RemovedResidentKeys.Should().Equal(overviewTile.Key);
        residencyChange.ColorChangedKeys.Should().BeEmpty();
    }

    [Fact]
    public void MetadataReplacement_TriggersFullReset()
    {
        var metadata = CreateMetadata(width: 8, height: 8);
        var replacementMetadata = CreateMetadata(width: 8, height: 8, valueMaximum: 250d);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 11f);
        var replacementTile = CreateTile(replacementMetadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 22f);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var replacementColorMap = CreateColorMap(replacementMetadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var state = new SurfaceChartRenderState();

        state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 8, 8)));

        var fullReset = state.Update(CreateInputs(replacementMetadata, [replacementTile], replacementColorMap, new SurfaceViewport(0, 0, 8, 8)));

        fullReset.FullResetRequired.Should().BeTrue();
        state.ResidentTileCount.Should().Be(1);
        state.TryGetResidentTile(replacementTile.Key, out _).Should().BeTrue();
    }

    [Fact]
    public void InjectedRenderer_BuildsResidentTilesWithCustomTranslation()
    {
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var colorMap = CreateColorMap(metadata, startColor: 0xFF203040u, endColor: 0xFFE0F0FFu);
        var state = new SurfaceChartRenderState(new OffsetSurfaceRenderer());

        state.Update(CreateInputs(metadata, [tile], colorMap, new SurfaceViewport(0, 0, 4, 4)));

        state.TryGetResidentTile(tile.Key, out var residentTile).Should().BeTrue();
        residentTile.SoftwareRenderTile.Vertices[0].Should().Be(new SurfaceRenderVertex(new(100f, 17f, 7f), 0xFF112233u));
        residentTile.SoftwareRenderTile.Vertices[3].Should().Be(new SurfaceRenderVertex(new(103f, 17f, 7f), 0xFF112233u));
    }

    private static SurfaceChartRenderInputs CreateInputs(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> tiles,
        SurfaceColorMap colorMap,
        SurfaceViewport viewport,
        SurfaceViewState? viewState = null)
    {
        var resolvedViewState = viewState ?? SurfaceViewState.CreateDefault(metadata, viewport.ToDataWindow());

        return new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = tiles,
            ColorMap = colorMap,
            ViewState = resolvedViewState,
            CameraFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, resolvedViewState, 320d, 180d, 1f),
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
            RenderScale = 1f,
        };
    }

    private static SurfaceMetadata CreateMetadata(int width, int height, double valueMaximum = 100d)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0d, maximum: width - 1d),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0d, maximum: height - 1d),
            new SurfaceValueRange(0d, valueMaximum));
    }

    private static SurfaceTile CreateTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var tileWidth = endX - startX;
        var tileHeight = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, tileWidth, tileHeight);
        var values = new float[tileWidth * tileHeight];
        Array.Fill(values, tileValue);
        return new SurfaceTile(key, tileWidth, tileHeight, bounds, values, metadata.ValueRange);
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata, uint startColor, uint endColor)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(startColor, endColor));
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
