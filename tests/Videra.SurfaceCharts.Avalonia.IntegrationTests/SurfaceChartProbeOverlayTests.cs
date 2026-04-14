using System.Linq;
using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartProbeOverlayTests
{
    [Fact]
    public Task ProbeUpdate_WithLoadedTiles_ProducesReadoutState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 7f);
            var view = new SurfaceChartView();

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.Viewport = new SurfaceViewport(128, 64, 256, 128);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(64, 32));

            var overlayState = GetOverlayState(view);

            GetBooleanProperty(overlayState, "HasNoData").Should().BeFalse();
            GetStringProperty(overlayState, "NoDataText").Should().BeNull();
            GetStringProperty(overlayState, "ReadoutText").Should().Contain("7");

            var probeResult = GetPropertyValue(overlayState, "ProbeResult");
            probeResult.Should().NotBeNull();
            GetDoubleProperty(probeResult!, "SampleX").Should().BeApproximately(192d, 0.0001d);
            GetDoubleProperty(probeResult!, "SampleY").Should().BeApproximately(96d, 0.0001d);
            GetDoubleProperty(probeResult!, "Value").Should().Be(7d);
        });
    }

    [Fact]
    public Task SourceWithoutCommittedTiles_PresentsNoDataOverlayState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 9f);
            var view = new SurfaceChartView();

            source.EnqueuePendingResponse(started, completion, observeCancellation: true);

            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));
            view.Source = source;

            await started.Task;

            var overlayState = GetOverlayState(view);

            GetBooleanProperty(overlayState, "HasNoData").Should().BeTrue();
            GetStringProperty(overlayState, "NoDataText").Should().Be("No data");
            GetStringProperty(overlayState, "ReadoutText").Should().BeNull();
            GetPropertyValue(overlayState, "ProbeResult").Should().BeNull();

            view.Source = null;
        });
    }

    [Fact]
    public Task ViewportAndSourceChanges_RecomputeAndClearOverlayState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 5f);
            var view = new SurfaceChartView();

            view.Measure(new Size(200, 100));
            view.Arrange(new Rect(0, 0, 200, 100));
            view.Viewport = new SurfaceViewport(100, 50, 200, 100);
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            UpdateProbeScreenPosition(view, new Point(100, 50));

            var initialState = GetOverlayState(view);
            GetDoubleProperty(GetPropertyValue(initialState, "ProbeResult")!, "SampleX").Should().BeApproximately(200d, 0.0001d);
            GetDoubleProperty(GetPropertyValue(initialState, "ProbeResult")!, "SampleY").Should().BeApproximately(100d, 0.0001d);

            view.Viewport = new SurfaceViewport(300, 250, 200, 100);

            var updatedState = GetOverlayState(view);
            GetBooleanProperty(updatedState, "HasNoData").Should().BeFalse();
            GetDoubleProperty(GetPropertyValue(updatedState, "ProbeResult")!, "SampleX").Should().BeApproximately(400d, 0.0001d);
            GetDoubleProperty(GetPropertyValue(updatedState, "ProbeResult")!, "SampleY").Should().BeApproximately(300d, 0.0001d);

            view.Source = null;

            var clearedState = GetOverlayState(view);
            GetBooleanProperty(clearedState, "HasNoData").Should().BeTrue();
            GetStringProperty(clearedState, "ReadoutText").Should().BeNull();
            GetPropertyValue(clearedState, "ProbeResult").Should().BeNull();
        });
    }

    [Fact]
    public void CoarseTileProbe_MapsSampleCoordinatesThroughClampedViewportAndTileBoundsSpan()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var coarseTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 1024, 1024),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-50, -40, 200, 200),
            new Size(100, 100),
            [coarseTile],
            new Point(50, 50));

        state.HasNoData.Should().BeFalse();
        state.ProbeResult.Should().NotBeNull();
        state.ProbeResult!.Value.SampleX.Should().BeApproximately(100d, 0.0001d);
        state.ProbeResult.Value.SampleY.Should().BeApproximately(100d, 0.0001d);
        state.ProbeResult.Value.Value.Should().Be(10d);
    }

    [Fact]
    public void ProbeOverlay_ConvertsSampleCoordinatesToAxisValues()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-1d, 1d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 5,
            height: 5,
            new SurfaceTileBounds(0, 0, 5, 5),
            Enumerable.Repeat(3f, 25).ToArray(),
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(0, 0, 4, 4),
            new Size(200, 200),
            [tile],
            new Point(100, 100));

        var hoveredProbe = GetHoveredProbe(state);
        GetDoubleProperty(hoveredProbe, "SampleX").Should().BeApproximately(2d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "SampleY").Should().BeApproximately(2d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisX").Should().BeApproximately(15d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "AxisY").Should().BeApproximately(150d, 0.0001d);
        GetDoubleProperty(hoveredProbe, "Value").Should().Be(3d);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeFalse();
    }

    [Fact]
    public void ProbeOverlay_FlagsApproximateWhenTileDensityIsCoarse()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var coarseTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 1024, 1024),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            metadata.ValueRange);

        var state = SurfaceProbeOverlayPresenter.CreateState(
            new StaticTileSource(metadata),
            new SurfaceViewport(-50, -40, 200, 200),
            new Size(100, 100),
            [coarseTile],
            new Point(50, 50));

        var hoveredProbe = GetHoveredProbe(state);
        GetBooleanProperty(hoveredProbe, "IsApproximate").Should().BeTrue();
        GetDoubleProperty(hoveredProbe, "Value").Should().Be(10d);
    }

    private static void UpdateProbeScreenPosition(SurfaceChartView view, Point probeScreenPosition)
    {
        var method = typeof(SurfaceChartView).GetMethod(
            "UpdateProbeScreenPosition",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should().NotBeNull("Task 9 requires a narrow control-side probe update path.");
        method!.Invoke(view, [probeScreenPosition]);
    }

    private static object GetOverlayState(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField(
            "_overlayState",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field.Should().NotBeNull("Task 9 requires SurfaceChartView to retain overlay state separately from scheduling.");
        return field!.GetValue(view)!;
    }

    private static object GetHoveredProbe(object overlayState)
    {
        var hoveredProbe = GetPropertyValue(overlayState, "HoveredProbe");
        hoveredProbe.Should().NotBeNull("the overlay state should keep the currently hovered probe.");
        return hoveredProbe!;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return property!.GetValue(instance);
    }

    private static bool GetBooleanProperty(object instance, string propertyName)
    {
        return (bool)GetPropertyValue(instance, propertyName)!;
    }

    private static double GetDoubleProperty(object instance, string propertyName)
    {
        return (double)GetPropertyValue(instance, propertyName)!;
    }

    private static string? GetStringProperty(object instance, string propertyName)
    {
        return (string?)GetPropertyValue(instance, propertyName);
    }

    private sealed class StaticTileSource : ISurfaceTileSource
    {
        public StaticTileSource(SurfaceMetadata metadata)
        {
            Metadata = metadata;
        }

        public SurfaceMetadata Metadata { get; }

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
