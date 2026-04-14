using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
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
            var view = new SurfaceChartView
            {
                ColorMap = new SurfaceColorMap(
                    new SurfaceValueRange(-20d, 20d),
                    new SurfaceColorMapPalette(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u)),
            };

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
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 5f);
            var view = new SurfaceChartView();

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

    private static object GetAxisOverlayState(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField(
            "_axisOverlayState",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field.Should().NotBeNull("Task 2 should keep a dedicated axis overlay state on the control.");
        return field!.GetValue(view)!;
    }

    private static object GetLegendOverlayState(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField(
            "_legendOverlayState",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field.Should().NotBeNull("Task 2 should keep a dedicated legend overlay state on the control.");
        return field!.GetValue(view)!;
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
        var axes = (System.Collections.IEnumerable)GetPropertyValue(axisOverlayState, "Axes")!;

        foreach (var axis in axes)
        {
            if (GetStringProperty(axis!, "AxisKey") != axisKey)
            {
                continue;
            }

            var axisLine = GetPropertyValue(axis!, "AxisLine")!;
            return (Point)GetPropertyValue(axisLine, "Start")!;
        }

        throw new Xunit.Sdk.XunitException($"Axis '{axisKey}' was not found.");
    }

    private static void UpdateProjectionSettings(SurfaceChartView view, SurfaceChartProjectionSettings settings)
    {
        var method = typeof(SurfaceChartView).GetMethod(
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
}
