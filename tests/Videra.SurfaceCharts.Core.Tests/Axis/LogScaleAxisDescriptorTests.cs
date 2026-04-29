using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class LogScaleAxisDescriptorTests
{
    [Fact]
    public void Constructor_LogScale_WithPositiveRange_Succeeds()
    {
        var act = () => new SurfaceAxisDescriptor("Y", "dB", 1d, 1000d, SurfaceAxisScaleKind.Log);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_LogScale_WithZeroMinimum_ThrowsArgumentOutOfRange()
    {
        var act = () => new SurfaceAxisDescriptor("Y", "dB", 0d, 1000d, SurfaceAxisScaleKind.Log);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "minimum")
            .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_LogScale_WithNegativeMinimum_ThrowsArgumentOutOfRange()
    {
        var act = () => new SurfaceAxisDescriptor("Y", "dB", -1d, 1000d, SurfaceAxisScaleKind.Log);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "minimum")
            .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_LogScale_WithZeroMaximum_ThrowsArgumentOutOfRange()
    {
        var act = () => new SurfaceAxisDescriptor("Y", "dB", 1d, 0d, SurfaceAxisScaleKind.Log);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "maximum")
            .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_LogScale_WithNegativeMaximum_ThrowsArgumentOutOfRange()
    {
        var act = () => new SurfaceAxisDescriptor("Y", "dB", 1d, -10d, SurfaceAxisScaleKind.Log);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "maximum")
            .WithMessage("*positive*");
    }

    [Fact]
    public void Constructor_LogScale_SetsScaleKind()
    {
        var descriptor = new SurfaceAxisDescriptor("Y", "dB", 1d, 1000d, SurfaceAxisScaleKind.Log);

        descriptor.ScaleKind.Should().Be(SurfaceAxisScaleKind.Log);
    }

    [Fact]
    public void Constructor_LogScale_MinimumEqualsMaximum_Succeeds()
    {
        var act = () => new SurfaceAxisDescriptor("Y", null, 1d, 1d, SurfaceAxisScaleKind.Log);

        act.Should().NotThrow();
    }
}
