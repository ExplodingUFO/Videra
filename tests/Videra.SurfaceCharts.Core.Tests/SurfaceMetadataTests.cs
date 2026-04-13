using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceMetadataTests
{
    [Fact]
    public void Ctor_RejectsNonPositiveWidth()
    {
        var act = () => CreateMetadata(0, 8);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "width");
    }

    [Fact]
    public void Ctor_RejectsNonPositiveHeight()
    {
        var act = () => CreateMetadata(8, 0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "height");
    }

    [Fact]
    public void Ctor_RejectsInvalidValueRange()
    {
        var act = () => new SurfaceValueRange(9.0, 1.0);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "maximum");
    }

    [Fact]
    public void Ctor_RejectsTileWithInvalidBoundsShape()
    {
        var tileKey = new SurfaceTileKey(0, 0, 0);
        var valueRange = new SurfaceValueRange(0.0, 1.0);
        var values = new float[3];
        var bounds = new SurfaceTileBounds(4, 6, 1, 3);

        var act = () => new SurfaceTile(tileKey, 2, 2, bounds, values, valueRange);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "bounds");
    }

    [Fact]
    public void Ctor_RejectsTileWithInvalidValueCount()
    {
        var tileKey = new SurfaceTileKey(0, 0, 0);
        var valueRange = new SurfaceValueRange(0.0, 1.0);
        var values = new float[3];
        var bounds = new SurfaceTileBounds(4, 6, 2, 2);

        var act = () => new SurfaceTile(tileKey, 2, 2, bounds, values, valueRange);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "values");
    }

    [Fact]
    public void SourceExtensions_RejectNullSource()
    {
        ISurfaceTileSource? source = null;

        var metadataAct = () => source!.GetRequiredMetadata();
        var tileAct = () => source!.GetRequiredTile(new SurfaceTileKey(0, 0, 0));

        metadataAct.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "source");
        tileAct.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "source");
    }

    [Fact]
    public void GetRequiredTile_ThrowsWhenSourceDoesNotHaveTile()
    {
        var source = new MissingTileSource();

        var act = () => source.GetRequiredTile(new SurfaceTileKey(1, 2, 3));

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetRequiredTile_ThrowsWhenSourceReturnsNullTileForSuccess()
    {
        var source = new NullTileSource();

        var act = () => source.GetRequiredTile(new SurfaceTileKey(1, 2, 3));

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Ctor_ExposesStableMetadata()
    {
        var metadata = CreateMetadata(128, 64);

        metadata.Width.Should().Be(128);
        metadata.Height.Should().Be(64);
        metadata.ValueRange.Should().Be(new SurfaceValueRange(-3.5, 9.25));
        metadata.HorizontalAxis.Label.Should().Be("Time");
        metadata.VerticalAxis.Label.Should().Be("Frequency");
    }

    private sealed class MissingTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(16, 8);

        public bool TryGetTile(SurfaceTileKey tileKey, [NotNullWhen(true)] out SurfaceTile? tile)
        {
            tile = null;
            return false;
        }
    }

    private sealed class NullTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(16, 8);

        public bool TryGetTile(SurfaceTileKey tileKey, [NotNullWhen(true)] out SurfaceTile? tile)
        {
            tile = default!;
            return true;
        }
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(-3.5, 9.25));
    }
}
