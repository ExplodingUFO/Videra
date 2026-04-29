using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public class CustomFormatterIntegrationTests
{
    [Fact]
    public void FormatLabel_WithXAxisFormatter_UsesXFormatter()
    {
        var options = new SurfaceChartOverlayOptions
        {
            XAxisFormatter = value => $"X-CUSTOM:{value:0.0}",
        };

        options.FormatLabel("X", 42d).Should().Be("X-CUSTOM:42.0");
    }

    [Fact]
    public void FormatLabel_WithYAxisFormatter_UsesYFormatter()
    {
        var options = new SurfaceChartOverlayOptions
        {
            YAxisFormatter = value => $"Y-CUSTOM:{value:0.0}",
        };

        options.FormatLabel("Y", 42d).Should().Be("Y-CUSTOM:42.0");
    }

    [Fact]
    public void FormatLabel_PerAxisFormatter_TakesPriorityOverLabelFormatter()
    {
        var options = new SurfaceChartOverlayOptions
        {
            XAxisFormatter = value => $"PER-AXIS:{value}",
            LabelFormatter = (axisKey, value) => $"GLOBAL:{axisKey}:{value}",
        };

        options.FormatLabel("X", 42d).Should().Be("PER-AXIS:42");
        // Y should fall through to LabelFormatter since YAxisFormatter is not set
        options.FormatLabel("Y", 42d).Should().Be("GLOBAL:Y:42");
    }

    [Fact]
    public void FormatLabel_NoFormatter_FallsBackToNumeric()
    {
        var options = new SurfaceChartOverlayOptions
        {
            TickLabelFormat = SurfaceChartNumericLabelFormat.General,
            TickLabelPrecision = 1,
        };

        options.FormatLabel("X", 42.5d).Should().Be("42.5");
    }

    [Fact]
    public void FormatLabel_LabelFormatter_UsedWhenNoPerAxis()
    {
        var options = new SurfaceChartOverlayOptions
        {
            LabelFormatter = (axisKey, value) => $"LABEL:{axisKey}:{value:0.0}",
        };

        options.FormatLabel("X", 42d).Should().Be("LABEL:X:42.0");
        options.FormatLabel("Y", 42d).Should().Be("LABEL:Y:42.0");
        options.FormatLabel("Z", 42d).Should().Be("LABEL:Z:42.0");
    }

    [Fact]
    public void FormatDateTimeLabel_SubHourRange_ShowsTimeOnly()
    {
        // UTC seconds for a sub-hour span
        var label = SurfaceChartOverlayOptions.FormatDateTimeLabel(3661d, 1800d);

        // Should contain HH:mm:ss format (time only)
        label.Should().MatchRegex(@"\d{2}:\d{2}:\d{2}");
        label.Should().NotContain("-");
    }

    [Fact]
    public void FormatDateTimeLabel_SubDayRange_ShowsDateAndTime()
    {
        // UTC seconds for a sub-day span (but > 1 hour)
        var label = SurfaceChartOverlayOptions.FormatDateTimeLabel(86400d, 7200d);

        // Should contain MM-dd HH:mm format
        label.Should().MatchRegex(@"\d{2}-\d{2} \d{2}:\d{2}");
    }

    [Fact]
    public void FormatDateTimeLabel_LongRange_ShowsDateOnly()
    {
        // UTC seconds for a multi-day span
        var label = SurfaceChartOverlayOptions.FormatDateTimeLabel(86400d, 172800d);

        // Should contain yyyy-MM-dd format
        label.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}");
    }
}
