using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class SurfaceScalarFieldTests
{
    [Fact]
    public void Ctor_RejectsValueCountThatDoesNotMatchShape()
    {
        var act = () => new SurfaceScalarField(
            width: 2,
            height: 2,
            values: new float[] { 1f, 2f, 3f },
            range: new SurfaceValueRange(1d, 3d));

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "values");
    }

    [Fact]
    public void SurfaceTileCtor_AcceptsIndependentColorFieldWithoutBreakingHeightAliases()
    {
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 100f, 200f, 300f, 400f },
                range: new SurfaceValueRange(100d, 400d)));

        tile.Width.Should().Be(2);
        tile.Height.Should().Be(2);
        tile.Values.ToArray().Should().Equal(10f, 20f, 30f, 40f);
        tile.ValueRange.Should().Be(new SurfaceValueRange(10d, 40d));
        tile.HeightField.Range.Should().Be(new SurfaceValueRange(10d, 40d));
        tile.ColorField.Should().NotBeNull();
        tile.ColorField!.Values.ToArray().Should().Equal(100f, 200f, 300f, 400f);
        tile.ColorField.Range.Should().Be(new SurfaceValueRange(100d, 400d));
    }

    [Fact]
    public void SurfaceTileCtor_DerivesMaskFromNonFiniteHeightAndColorSamples()
    {
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, float.NaN, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 100f, 200f, float.PositiveInfinity, 400f },
                range: new SurfaceValueRange(100d, 400d)));

        tile.Mask.Should().NotBeNull();
        tile.Mask!.Values.ToArray().Should().Equal(true, false, false, true);
    }

    [Fact]
    public void SurfaceTileLegacyCtor_DerivesMaskFromNonFiniteHeightSamples()
    {
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 2, 2),
            values: new float[] { 10f, float.NaN, 30f, 40f },
            valueRange: new SurfaceValueRange(10d, 40d));

        tile.Mask.Should().NotBeNull();
        tile.Mask!.Values.ToArray().Should().Equal(true, false, true, true);
    }

    [Fact]
    public void SurfaceMatrixCtor_AcceptsIndependentColorFieldWithoutBreakingHeightAliases()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 0d, 1d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 1d),
            new SurfaceValueRange(10d, 40d));
        var matrix = new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 1f, 2f, 3f, 4f },
                range: new SurfaceValueRange(1d, 4d)));

        matrix.Values.ToArray().Should().Equal(10f, 20f, 30f, 40f);
        matrix.HeightField.Range.Should().Be(metadata.ValueRange);
        matrix.ColorField.Should().NotBeNull();
        matrix.ColorField!.Values.ToArray().Should().Equal(1f, 2f, 3f, 4f);
    }

    [Fact]
    public void SurfaceMatrixCtor_CombinesExplicitMaskWithNonFiniteSamples()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 0d, 1d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 1d),
            new SurfaceValueRange(10d, 40d));
        var matrix = new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, float.NaN, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            colorField: null,
            mask: new SurfaceMask(
                width: 2,
                height: 2,
                values: new bool[] { true, true, false, true }));

        matrix.Mask.Should().NotBeNull();
        matrix.Mask!.Values.ToArray().Should().Equal(true, false, false, true);
    }

    [Fact]
    public void SurfaceTileCtor_RejectsMismatchedMaskShape()
    {
        var act = () => new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            colorField: null,
            mask: new SurfaceMask(
                width: 1,
                height: 2,
                values: new bool[] { true, false }));

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "mask");
    }

    [Fact]
    public void SurfaceMatrixCtor_RejectsMismatchedMaskShape()
    {
        var metadata = new SurfaceMetadata(
            width: 2,
            height: 2,
            new SurfaceAxisDescriptor("Time", "s", 0d, 1d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 1d),
            new SurfaceValueRange(10d, 40d));

        var act = () => new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: new SurfaceValueRange(10d, 40d)),
            colorField: null,
            mask: new SurfaceMask(
                width: 1,
                height: 2,
                values: new bool[] { true, false }));

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "mask");
    }
}
