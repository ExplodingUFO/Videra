using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceProbeResultTests
{
    public static TheoryData<double> NonFiniteValues => new()
    {
        double.NaN,
        double.PositiveInfinity,
        double.NegativeInfinity
    };

    [Fact]
    public void RequestAndResult_ExposeStableSampleCoordinatesAndValue()
    {
        var request = new SurfaceProbeRequest(12.5, 48.25);
        var result = new SurfaceProbeResult(12.0, 48.0, -13.75);

        request.SampleX.Should().Be(12.5);
        request.SampleY.Should().Be(48.25);
        result.SampleX.Should().Be(12.0);
        result.SampleY.Should().Be(48.0);
        result.Value.Should().Be(-13.75);
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void RequestCtor_RejectsNonFiniteSampleX(double sampleX)
    {
        var act = () => new SurfaceProbeRequest(sampleX, 12.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "sampleX");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void RequestCtor_RejectsNonFiniteSampleY(double sampleY)
    {
        var act = () => new SurfaceProbeRequest(12.0, sampleY);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "sampleY");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void ResultCtor_RejectsNonFiniteSampleX(double sampleX)
    {
        var act = () => new SurfaceProbeResult(sampleX, 48.0, -13.75);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "sampleX");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void ResultCtor_RejectsNonFiniteSampleY(double sampleY)
    {
        var act = () => new SurfaceProbeResult(12.0, sampleY, -13.75);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "sampleY");
    }

    [Theory]
    [MemberData(nameof(NonFiniteValues))]
    public void ResultCtor_RejectsNonFiniteValue(double value)
    {
        var act = () => new SurfaceProbeResult(12.0, 48.0, value);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "value");
    }
}
