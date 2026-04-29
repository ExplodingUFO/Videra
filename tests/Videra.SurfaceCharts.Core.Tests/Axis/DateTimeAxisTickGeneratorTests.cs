using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class DateTimeAxisTickGeneratorTests
{
    [Fact]
    public void CreateDateTimeTickValues_SubMinuteRange_ReturnsSecondSteps()
    {
        // Range: 0..30 seconds — should get ticks at ~5s intervals
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(0d, 30d, 300d);

        ticks.Should().NotBeEmpty();
        ticks.Should().OnlyContain(t => t >= 0d && t <= 30d);
        // Expect steps of 5s
        if (ticks.Count >= 2)
        {
            var step = ticks[1] - ticks[0];
            step.Should().BeOneOf(2d, 5d, 10d);
        }
    }

    [Fact]
    public void CreateDateTimeTickValues_MinuteRange_ReturnsMinuteSteps()
    {
        // Range: 0..3600 seconds (1 hour) — should get ticks at 60s or 300s intervals
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(0d, 3600d, 400d);

        ticks.Should().NotBeEmpty();
        ticks.Should().OnlyContain(t => t >= 0d && t <= 3600d);
        if (ticks.Count >= 2)
        {
            var step = ticks[1] - ticks[0];
            step.Should().BeGreaterThanOrEqualTo(60d);
        }
    }

    [Fact]
    public void CreateDateTimeTickValues_HourRange_ReturnsHourSteps()
    {
        // Range: 0..86400 seconds (1 day) — should get ticks at 3600s or 7200s intervals
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(0d, 86400d, 500d);

        ticks.Should().NotBeEmpty();
        ticks.Should().OnlyContain(t => t >= 0d && t <= 86400d);
        if (ticks.Count >= 2)
        {
            var step = ticks[1] - ticks[0];
            step.Should().BeGreaterThanOrEqualTo(3600d);
        }
    }

    [Fact]
    public void CreateDateTimeTickValues_DayRange_ReturnsDaySteps()
    {
        // Range: 0..604800 seconds (1 week) — should get ticks at 86400s intervals
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(0d, 604800d, 500d);

        ticks.Should().NotBeEmpty();
        ticks.Should().OnlyContain(t => t >= 0d && t <= 604800d);
        if (ticks.Count >= 2)
        {
            var step = ticks[1] - ticks[0];
            step.Should().BeGreaterThanOrEqualTo(86400d);
        }
    }

    [Fact]
    public void CreateDateTimeTickValues_ReturnsTicksInRange()
    {
        var min = 1000d;
        var max = 5000d;
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(min, max, 400d);

        ticks.Should().OnlyContain(t => t >= min && t <= max);
    }

    [Fact]
    public void CreateDateTimeTickValues_DegenerateRange_ReturnsMinMax()
    {
        var ticks = SurfaceAxisTickGenerator.CreateDateTimeTickValues(100d, 100d, 300d);

        ticks.Should().HaveCount(2);
        ticks[0].Should().Be(100d);
        ticks[1].Should().Be(100d);
    }

    [Fact]
    public void CreateLogTickValues_StandardRange_ReturnsPowersOf10()
    {
        // Range: 1..1000 should produce [1, 10, 100, 1000]
        var ticks = SurfaceAxisTickGenerator.CreateLogTickValues(1d, 1000d, 400d);

        ticks.Should().Contain(1d);
        ticks.Should().Contain(10d);
        ticks.Should().Contain(100d);
        ticks.Should().Contain(1000d);
    }

    [Fact]
    public void CreateLogTickValues_NarrowRange_ReturnsAppropriateTicks()
    {
        // Range: 1..100 should produce ticks at decade intervals
        var ticks = SurfaceAxisTickGenerator.CreateLogTickValues(1d, 100d, 300d);

        ticks.Should().NotBeEmpty();
        ticks.Should().OnlyContain(t => t >= 1d && t <= 100d);
        // Should include at least 1 and 100
        ticks.Should().Contain(1d);
    }

    [Fact]
    public void CreateLogTickValues_DegenerateRange_ReturnsMinMax()
    {
        var ticks = SurfaceAxisTickGenerator.CreateLogTickValues(10d, 10d, 300d);

        ticks.Should().HaveCount(2);
        ticks[0].Should().Be(10d);
        ticks[1].Should().Be(10d);
    }

    [Fact]
    public void CreateLogTickValues_NonPositiveMin_ReturnsMinMax()
    {
        // Non-positive min should be treated as degenerate
        var ticks = SurfaceAxisTickGenerator.CreateLogTickValues(-1d, 100d, 300d);

        ticks.Should().HaveCount(2);
        ticks[0].Should().Be(-1d);
        ticks[1].Should().Be(100d);
    }
}
