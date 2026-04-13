using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartTileSchedulingTests
{
    [Fact]
    public async Task ArrangedViewport_RequestsAdditionalTilesThroughScheduler()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var source = new RecordingSurfaceTileSource(metadata);
        var view = new SurfaceChartView
        {
            Source = source
        };

        await source.WaitForRequestCountAsync(1);

        view.Measure(new Size(256, 128));
        view.Arrange(new Rect(0, 0, 256, 128));

        var viewport = new SurfaceViewport(256, 128, 512, 256);
        var expectedSelection = SurfaceLodPolicy.Default.Select(
            new SurfaceViewportRequest(metadata, viewport, outputWidth: 256, outputHeight: 128));
        var expectedKeys = expectedSelection.EnumerateTileKeys().ToArray();

        view.Viewport = viewport;

        await source.WaitForRequestCountAsync(1 + expectedKeys.Length);

        source.RequestLog[0].Should().Be(new SurfaceTileKey(0, 0, 0, 0));
        source.RequestLog.Skip(1).Should().ContainInOrder(expectedKeys);
    }
}
