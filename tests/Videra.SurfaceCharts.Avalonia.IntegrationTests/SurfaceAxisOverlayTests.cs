using System.Globalization;
using System.Numerics;
using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceAxisOverlayTests
{
    [Fact]
    public Task AxisOverlay_UsesMetadataLabelsAndLegendRange()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 64,
                height: 48,
                new SurfaceAxisDescriptor("Time", "s", 0d, 180d),
                new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 24d),
                new SurfaceValueRange(-8d, 32d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7f);
            var view = new VideraChartView();
            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-20d, 20d),
                new SurfaceColorMapPalette(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u));

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            var axisState = GetAxisOverlayState(view);
            var axisTitles = GetAxisTitles(axisState);
            axisTitles.Should().Contain("Time (s)");
            axisTitles.Should().Contain("Frequency (kHz)");
            axisTitles.Should().Contain("Value");

            var legendState = GetLegendOverlayState(view);
            GetStringProperty(legendState, "MinimumText").Should().Contain("-20");
            GetStringProperty(legendState, "MaximumText").Should().Contain("20");
        });
    }

    [Fact]
    public Task AxisOverlay_SwitchesDisplayedEdges_WhenCameraYawChanges()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 5f);
            var view = new VideraChartView();

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            var initialState = GetAxisOverlayState(view);
            var initialYAxisStart = GetAxisLineStart(initialState, "Y");

            UpdateProjectionSettings(view, new SurfaceChartProjectionSettings(180d, 0d));

            var rotatedState = GetAxisOverlayState(view);
            var rotatedYAxisStart = GetAxisLineStart(rotatedState, "Y");

            rotatedYAxisStart.Should().NotBe(initialYAxisStart);
        });
    }

    [Fact]
    public Task AxisOverlay_TicksStayMonotonicAndLegendTracksColorMapRange_AfterViewChanges()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 64,
                height: 48,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "kHz", 100d, 220d),
                new SurfaceValueRange(-80d, 160d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7f);
            var view = new VideraChartView
            {
                ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(-32d, -24d, 160d, 120d)),
            };
            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-20d, 40d),
                new SurfaceColorMapPalette(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u));

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            AssertAxisTicksMonotonic(GetAxisOverlayState(view), "X");
            AssertAxisTicksMonotonic(GetAxisOverlayState(view), "Y");
            AssertAxisTicksMonotonic(GetAxisOverlayState(view), "Z");
            AssertLegendMatchesColorMapRange(view, GetLegendOverlayState(view));

            view.ViewState = new SurfaceViewState((new SurfaceViewport(16d, 12d, 24d, 18d)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            UpdateProjectionSettings(view, new SurfaceChartProjectionSettings(225d, 15d));

            var updatedAxisState = GetAxisOverlayState(view);
            AssertAxisTicksMonotonic(updatedAxisState, "X");
            AssertAxisTicksMonotonic(updatedAxisState, "Y");
            AssertAxisTicksMonotonic(updatedAxisState, "Z");
            AssertLegendMatchesColorMapRange(view, GetLegendOverlayState(view));
        });
    }

    [Fact]
    public Task AxisOverlay_LegendTracksUpdatedColorMapRange_AfterColorMapChange()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 32,
                height: 24,
                new SurfaceAxisDescriptor("Time", "s", 0d, 31d),
                new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 23d),
                new SurfaceValueRange(-8d, 32d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7f);
            var view = new VideraChartView();
            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-20d, 20d),
                new SurfaceColorMapPalette(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u));

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-5d, 45d),
                new SurfaceColorMapPalette(0xFF401020u, 0xFF40E0D0u));

            await WaitForConditionAsync(
                () =>
                {
                    var legendState = GetLegendOverlayState(view);
                    return GetStringProperty(legendState, "MinimumText") == "-5"
                        && GetStringProperty(legendState, "MaximumText") == "45";
                },
                "legend should refresh to the active color-map range after color-map changes");

            AssertLegendMatchesColorMapRange(view, GetLegendOverlayState(view));
        });
    }

    [Fact]
    public Task AxisOverlay_UsesChartLocalOverlayOptions_ForFormatterTitlesMinorTicksAndLegend()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 64,
                height: 48,
                new SurfaceAxisDescriptor("Time", "s", 0d, 180d),
                new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 24d),
                new SurfaceValueRange(-20d, 40d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7f);
            var view = new VideraChartView();
            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-20d, 40d),
                new SurfaceColorMapPalette(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u));
            view.Plot.OverlayOptions = new SurfaceChartOverlayOptions
            {
                ShowMinorTicks = true,
                MinorTickDivisions = 3,
                LabelFormatter = static (axisKey, value) => $"{axisKey}:{value:0.0}",
                HorizontalAxisTitleOverride = "Elapsed",
                HorizontalAxisUnitOverride = "ms",
                ValueAxisTitleOverride = "Amplitude",
                ValueAxisUnitOverride = "dB",
                DepthAxisTitleOverride = "Band",
                DepthAxisUnitOverride = "Hz",
                LegendTitleOverride = "Amplitude",
            };

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            var axisState = GetAxisOverlayState(view);
            GetAxisTitles(axisState).Should().Contain(["Elapsed (ms)", "Amplitude (dB)", "Band (Hz)"]);
            GetAxisTickLabels(axisState, "X").Should().OnlyContain(static label => label.StartsWith("X:", StringComparison.Ordinal));
            GetAxisTickLabels(axisState, "Y").Should().OnlyContain(static label => label.StartsWith("Y:", StringComparison.Ordinal));
            GetMinorTickCount(axisState, "X").Should().BeGreaterThan(0);

            var legendState = GetLegendOverlayState(view);
            GetStringProperty(legendState, "TitleText").Should().Be("Amplitude");
            GetStringProperty(legendState, "MinimumText").Should().Be("Legend:-20.0");
            GetStringProperty(legendState, "MaximumText").Should().Be("Legend:40.0");
        });
    }

    [Fact]
    public Task AxisOverlay_CullsDenseLabels_WhenFormatterProducesLongLabels()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 96,
                height: 64,
                new SurfaceAxisDescriptor("Time", "s", 0d, 180d),
                new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 24d),
                new SurfaceValueRange(-20d, 40d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7f);
            var view = new VideraChartView();
            view.Plot.OverlayOptions = new SurfaceChartOverlayOptions
            {
                LabelFormatter = static (axisKey, value) => $"{axisKey}-label-{value:000.000}",
            };

            view.Measure(new Size(260, 180));
            view.Arrange(new Rect(0, 0, 260, 180));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            var xAxisState = GetAxisState(GetAxisOverlayState(view), "X");
            GetIntProperty(xAxisState, "MajorTickCount").Should().BeGreaterThan(GetCollectionCount(xAxisState, "Ticks"));
        });
    }

    [Fact]
    public Task AxisOverlay_AppliesNumericPresetPrecision_ForAxisAndLegend()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 4f);
            var view = new VideraChartView();
            view.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Fixed(2);
            view.Plot.ColorMap = new SurfaceColorMap(
                new SurfaceValueRange(-20d, 40d),
                SurfaceColorMapPresets.CreateGrayscale());

            view.Measure(new Size(340, 220));
            view.Arrange(new Rect(0, 0, 340, 220));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [4f]);

            var axisState = GetAxisOverlayState(view);
            var xLabels = GetAxisTickLabels(axisState, "X");
            xLabels.Should().NotBeEmpty();
            xLabels.Should().OnlyContain(label => label.Contains('.', StringComparison.Ordinal) && label.EndsWith("00", StringComparison.Ordinal));
            xLabels.Should().OnlyContain(label => char.IsDigit(label[0]) || label[0] == '-');

            var yLabels = GetAxisTickLabels(axisState, "Y");
            yLabels.Should().NotBeEmpty();
            yLabels.Should().OnlyContain(label => label.Contains('.', StringComparison.Ordinal) && label.EndsWith("00", StringComparison.Ordinal));

            var zLabels = GetAxisTickLabels(axisState, "Z");
            zLabels.Should().NotBeEmpty();
            zLabels.Should().OnlyContain(label => label.Contains('.', StringComparison.Ordinal) && label.EndsWith("00", StringComparison.Ordinal));

            var legendState = GetLegendOverlayState(view);
            GetStringProperty(legendState, "MinimumText").Should().Be("-20.00");
            GetStringProperty(legendState, "MaximumText").Should().Be("40.00");
        });
    }

    [Fact]
    public void SurfaceChartOverlayOptions_FormatLabel_UsesDistinctAxisAndLegendPresetStyles()
    {
        var options = new SurfaceChartOverlayOptions
        {
            TickLabelFormat = SurfaceChartNumericLabelFormat.Scientific,
            TickLabelPrecision = 2,
            LegendLabelFormat = SurfaceChartNumericLabelFormat.Fixed,
            LegendLabelPrecision = 4,
        };

        options.FormatLabel("X", 1234.567).Should().Be("1.23E+3");
        options.FormatLabel("Y", -1234.567).Should().Be("-1.23E+3");
        options.FormatLabel("Legend", 1234.567).Should().Be("1234.5670");
        options.FormatLabel("Legend", -1234.567).Should().Be("-1234.5670");
    }

    [Fact]
    public Task AxisOverlay_UsesConfiguredGridPlane_AndPinnedAxisSideMode()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new VideraChartView();
            view.Plot.OverlayOptions = new SurfaceChartOverlayOptions
            {
                GridPlane = SurfaceChartGridPlane.YZ,
                AxisSideMode = SurfaceChartAxisSideMode.MaximumBounds,
            };

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            var initialState = GetAxisOverlayState(view);
            GetStringProperty(initialState, "GridPlaneKey").Should().Be("YZ");
            GetDoubleProperty(initialState, "AnchorCornerX").Should().Be(metadata.HorizontalAxis.Maximum);
            GetDoubleProperty(initialState, "AnchorCornerZ").Should().Be(metadata.VerticalAxis.Maximum);
            GetCollectionCount(initialState, "GridLines").Should().BeGreaterThan(0);

            UpdateProjectionSettings(view, new SurfaceChartProjectionSettings(180d, 0d));

            var rotatedState = GetAxisOverlayState(view);
            GetStringProperty(rotatedState, "GridPlaneKey").Should().Be("YZ");
            GetDoubleProperty(rotatedState, "AnchorCornerX").Should().Be(metadata.HorizontalAxis.Maximum);
            GetDoubleProperty(rotatedState, "AnchorCornerZ").Should().Be(metadata.VerticalAxis.Maximum);
        });
    }

    [Fact]
    public void SurfaceChartProjection_UsesSharedCameraFrameMath()
    {
        var metadata = new SurfaceMetadata(
            width: 8,
            height: 8,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "kHz", 100d, 220d),
            new SurfaceValueRange(-8d, 32d));
        var tile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 7f);
        var scene = new SurfaceRenderer().BuildScene(
            metadata,
            [tile],
            new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF102030u, 0xFFFFE080u)));
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, 320d, 200d, 1f);
        var projection = SurfaceChartProjection.Create(
            scene,
            cameraFrame,
            SurfaceChartProjection.CreateChartBoundsPoints(metadata, metadata.ValueRange));
        var plotPoint = new Vector3(15f, 12f, 150f);

        projection.Should().NotBeNull();
        var projectedPoint = projection!.Project(plotPoint);
        var sharedMathPoint = SurfaceProjectionMath.ProjectToScreen(plotPoint, cameraFrame);

        projectedPoint.X.Should().BeApproximately(sharedMathPoint.X, 0.001d);
        projectedPoint.Y.Should().BeApproximately(sharedMathPoint.Y, 0.001d);
    }

    private static object GetAxisOverlayState(VideraChartView view)
    {
        return SurfaceChartTestHelpers.GetOverlayCoordinator(view).AxisState;
    }

    private static object GetLegendOverlayState(VideraChartView view)
    {
        return SurfaceChartTestHelpers.GetOverlayCoordinator(view).LegendState;
    }

    private static string[] GetAxisTitles(object axisOverlayState)
    {
        var axes = GetPropertyValue(axisOverlayState, "Axes");
        axes.Should().BeAssignableTo<System.Collections.IEnumerable>();

        List<string> titles = [];
        foreach (var axis in (System.Collections.IEnumerable)axes!)
        {
            titles.Add(GetStringProperty(axis!, "TitleText")!);
        }

        return titles.ToArray();
    }

    private static Point GetAxisLineStart(object axisOverlayState, string axisKey)
    {
        var axisState = GetAxisState(axisOverlayState, axisKey);
        var axisLine = GetPropertyValue(axisState, "AxisLine")!;
        return (Point)GetPropertyValue(axisLine, "Start")!;
    }

    private static void AssertAxisTicksMonotonic(object axisOverlayState, string axisKey)
    {
        GetAxisTickValues(axisOverlayState, axisKey).Should().BeInAscendingOrder();
    }

    private static double[] GetAxisTickValues(object axisOverlayState, string axisKey)
    {
        var axis = GetAxisState(axisOverlayState, axisKey);
        var ticks = (System.Collections.IEnumerable)GetPropertyValue(axis, "Ticks")!;
        return ticks
            .Cast<object>()
            .Select(tick => (double)GetPropertyValue(tick, "Value")!)
            .ToArray();
    }

    private static string[] GetAxisTickLabels(object axisOverlayState, string axisKey)
    {
        var axis = GetAxisState(axisOverlayState, axisKey);
        var ticks = (System.Collections.IEnumerable)GetPropertyValue(axis, "Ticks")!;
        return ticks
            .Cast<object>()
            .Select(tick => GetStringProperty(tick, "LabelText")!)
            .ToArray();
    }

    private static int GetMinorTickCount(object axisOverlayState, string axisKey)
    {
        var axis = GetAxisState(axisOverlayState, axisKey);
        return GetCollectionCount(axis, "MinorTicks");
    }

    private static void AssertLegendMatchesColorMapRange(VideraChartView view, object legendOverlayState)
    {
        view.Plot.ColorMap.Should().NotBeNull();
        GetStringProperty(legendOverlayState, "MinimumText").Should().Be(view.Plot.ColorMap!.Range.Minimum.ToString("0.###", CultureInfo.InvariantCulture));
        GetStringProperty(legendOverlayState, "MaximumText").Should().Be(view.Plot.ColorMap.Range.Maximum.ToString("0.###", CultureInfo.InvariantCulture));
    }

    private static void UpdateProjectionSettings(VideraChartView view, SurfaceChartProjectionSettings settings)
    {
        var method = typeof(VideraChartView).GetMethod(
            "UpdateProjectionSettings",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should().NotBeNull("Task 2 needs a narrow control-side way to update projection settings for overlay rendering.");
        method!.Invoke(view, [settings]);
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return property!.GetValue(instance);
    }

    private static string? GetStringProperty(object instance, string propertyName)
    {
        return (string?)GetPropertyValue(instance, propertyName);
    }

    private static int GetIntProperty(object instance, string propertyName)
    {
        return (int)GetPropertyValue(instance, propertyName)!;
    }

    private static double GetDoubleProperty(object instance, string propertyName)
    {
        return (double)GetPropertyValue(instance, propertyName)!;
    }

    private static int GetCollectionCount(object instance, string propertyName)
    {
        return ((System.Collections.ICollection)GetPropertyValue(instance, propertyName)!).Count;
    }

    private static object GetAxisState(object axisOverlayState, string axisKey)
    {
        var axes = (System.Collections.IEnumerable)GetPropertyValue(axisOverlayState, "Axes")!;

        foreach (var axis in axes)
        {
            if (GetStringProperty(axis!, "AxisKey") == axisKey)
            {
                return axis!;
            }
        }

        throw new Xunit.Sdk.XunitException($"Axis '{axisKey}' was not found.");
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, string because, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(condition);

        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = DateTime.UtcNow + timeout.Value;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(true);
        }

        condition().Should().BeTrue(because);
    }
}
