using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

#pragma warning disable CA2007

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceMetadataTests
{
    public static TheoryData<double> NonFiniteValues => new()
    {
        double.NaN,
        double.PositiveInfinity,
        double.NegativeInfinity
    };

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

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void Ctor_RejectsNonFiniteAxisMinimum(double minimum)
    {
        var act = () => new SurfaceAxisDescriptor("Time", "s", minimum, 1.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "minimum");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void Ctor_RejectsNonFiniteAxisMaximum(double maximum)
    {
        var act = () => new SurfaceAxisDescriptor("Time", "s", 0.0, maximum);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "maximum");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void Ctor_RejectsNonFiniteValueRangeMinimum(double minimum)
    {
        var act = () => new SurfaceValueRange(minimum, 1.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "minimum");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void Ctor_RejectsNonFiniteValueRangeMaximum(double maximum)
    {
        var act = () => new SurfaceValueRange(0.0, maximum);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "maximum");
    }

    [Fact]
    public void Ctor_RejectsTileWithInvalidBoundsShape()
    {
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);
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
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);
        var valueRange = new SurfaceValueRange(0.0, 1.0);
        var values = new float[3];
        var bounds = new SurfaceTileBounds(4, 6, 2, 2);

        var act = () => new SurfaceTile(tileKey, 2, 2, bounds, values, valueRange);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "values");
    }

    [Fact]
    public async Task SourceExtensions_RejectNullSource()
    {
        ISurfaceTileSource? source = null;

        var metadataAct = () => source!.GetRequiredMetadata();
        var tileAct = async () => await InvokeRequiredTileAsync(source!, new SurfaceTileKey(0, 0, 0, 0));

        metadataAct.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "source");
        var tileAssertion = await tileAct.Should().ThrowAsync<ArgumentNullException>();
        tileAssertion.Where(ex => ex.ParamName == "source");
    }

    [Fact]
    public async Task GetRequiredTileAsync_ThrowsWhenSourceDoesNotHaveTile()
    {
        var source = new MissingTileSource();

        var act = async () => await InvokeRequiredTileAsync(source, new SurfaceTileKey(1, 1, 2, 3));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetRequiredTileAsync_PreservesSourceFailures()
    {
        var source = new FaultingTileSource();

        var act = async () => await InvokeRequiredTileAsync(source, new SurfaceTileKey(1, 1, 2, 3));

        var assertion = await act.Should().ThrowAsync<InvalidOperationException>();
        assertion.WithMessage("Backend unavailable.");
    }

    [Fact]
    public async Task GetRequiredTileAsync_RejectsMismatchedTileKey()
    {
        var source = new MismatchedTileSource();

        var act = async () => await InvokeRequiredTileAsync(source, new SurfaceTileKey(1, 1, 2, 3));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void TileSourceContract_UsesAsyncTileRetrieval()
    {
        typeof(ISurfaceTileSource).GetMethod("TryGetTile").Should().BeNull();

        var tileMethod = typeof(ISurfaceTileSource).GetMethod("GetTileAsync");
        tileMethod.Should().NotBeNull();
        tileMethod!.ReturnType.Should().Be(typeof(ValueTask<SurfaceTile>));

        var tileParameters = tileMethod.GetParameters();
        tileParameters.Should().HaveCount(2);
        tileParameters[0].ParameterType.Should().Be(typeof(SurfaceTileKey));
        tileParameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        tileParameters[1].IsOptional.Should().BeTrue();

        typeof(SurfaceTileSourceExtensions).GetMethod("GetRequiredTile").Should().BeNull();

        var helperMethod = typeof(SurfaceTileSourceExtensions).GetMethod("GetRequiredTileAsync");
        helperMethod.Should().NotBeNull();
        helperMethod!.ReturnType.Should().Be(typeof(ValueTask<SurfaceTile>));

        var helperParameters = helperMethod.GetParameters();
        helperParameters.Should().HaveCount(3);
        helperParameters[0].ParameterType.Should().Be(typeof(ISurfaceTileSource));
        helperParameters[1].ParameterType.Should().Be(typeof(SurfaceTileKey));
        helperParameters[2].ParameterType.Should().Be(typeof(CancellationToken));
        helperParameters[2].IsOptional.Should().BeTrue();
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

    [Fact]
    public void Ctor_AcceptsExplicitGridAndCentralizedCoordinateMapping()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d },
                verticalCoordinates: new double[] { 100d, 130d, 190d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 80d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 190d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-3.5, 9.25));

        metadata.Width.Should().Be(4);
        metadata.Height.Should().Be(3);
        metadata.MapHorizontalCoordinate(0d).Should().Be(10d);
        metadata.MapHorizontalCoordinate(2.5d).Should().BeApproximately(60d, 0.0001d);
        metadata.MapVerticalCoordinate(1.5d).Should().BeApproximately(160d, 0.0001d);
    }

    [Fact]
    public void AxisCtor_RejectsNonPositiveLogRange()
    {
        var act = () => new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 10d, SurfaceAxisScaleKind.Log);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "minimum");
    }

    private sealed class MissingTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(16, 8);

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }
    }

    private sealed class FaultingTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(16, 8);

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Backend unavailable.");
        }
    }

    private sealed class MismatchedTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(16, 8);

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            var mismatchedKey = new SurfaceTileKey(tileKey.LevelX, tileKey.LevelY, tileKey.TileX + 1, tileKey.TileY);
            var mismatchedTile = new SurfaceTile(
                mismatchedKey,
                2,
                2,
                new SurfaceTileBounds(0, 0, 2, 2),
                new float[] { 1, 2, 3, 4 },
                new SurfaceValueRange(1, 4));

            return ValueTask.FromResult<SurfaceTile?>(mismatchedTile);
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

    private static ValueTask<SurfaceTile> InvokeRequiredTileAsync(ISurfaceTileSource source, SurfaceTileKey tileKey)
    {
        var method = typeof(SurfaceTileSourceExtensions).GetMethod("GetRequiredTileAsync");
        method.Should().NotBeNull("the async tile helper should exist");

        var result = method!.Invoke(null, new object?[] { source, tileKey, CancellationToken.None });
        result.Should().NotBeNull("the async tile helper should return a ValueTask");

        return (ValueTask<SurfaceTile>)result!;
    }
}

#pragma warning restore CA2007
