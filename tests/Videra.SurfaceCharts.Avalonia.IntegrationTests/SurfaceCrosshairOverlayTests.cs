using System.Numerics;
using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceCrosshairOverlayTests
{
    [Fact]
    public Task Crosshair_FollowsPointerPosition()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new VideraChartView();

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            // Move pointer to first position
            var position1 = new Point(100, 80);
            UpdateProbeScreenPosition(view, position1);

            var state1 = GetCrosshairState(view);
            GetBoolProperty(state1, "IsVisible").Should().BeTrue();

            // Move pointer to second position
            var position2 = new Point(200, 120);
            UpdateProbeScreenPosition(view, position2);

            var state2 = GetCrosshairState(view);
            GetBoolProperty(state2, "IsVisible").Should().BeTrue();

            // Guideline endpoints should differ between positions
            var xStart1 = GetPointProperty(state1, "XGuidelineStart");
            var xStart2 = GetPointProperty(state2, "XGuidelineStart");
            xStart2.Should().NotBe(xStart1, "crosshair should follow pointer position");
        });
    }

    [Fact]
    public Task Crosshair_DisplaysAxisValuePills()
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

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(160, 100));

            var state = GetCrosshairState(view);
            GetBoolProperty(state, "IsVisible").Should().BeTrue();

            var xValueText = GetStringProperty(state, "XValueText");
            var zValueText = GetStringProperty(state, "ZValueText");

            xValueText.Should().NotBeNullOrEmpty("X axis value pill should have text");
            zValueText.Should().NotBeNullOrEmpty("Z axis value pill should have text");

            // Pill positions should be set
            var xPillPos = GetPointProperty(state, "XPillPosition");
            var zPillPos = GetPointProperty(state, "ZPillPosition");
            xPillPos.Should().NotBe(default(Point));
            zPillPos.Should().NotBe(default(Point));
        });
    }

    [Fact]
    public Task Crosshair_CanBeToggledOff()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new VideraChartView();
            view.Plot.OverlayOptions = new SurfaceChartOverlayOptions
            {
                ShowCrosshair = false,
            };

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            UpdateProbeScreenPosition(view, new Point(160, 100));

            var state = GetCrosshairState(view);
            GetBoolProperty(state, "IsVisible").Should().BeFalse("crosshair should be invisible when ShowCrosshair is false");
        });
    }

    [Fact]
    public Task Crosshair_DefaultsToVisible()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5f);
            var view = new VideraChartView();
            // Use default options (ShowCrosshair should default to true)

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            UpdateProbeScreenPosition(view, new Point(160, 100));

            var state = GetCrosshairState(view);
            GetBoolProperty(state, "IsVisible").Should().BeTrue("crosshair should default to visible");
        });
    }

    [Fact]
    public Task Crosshair_ProjectsGroundPlaneGuidelines()
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

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(160, 100));

            var state = GetCrosshairState(view);
            GetBoolProperty(state, "IsVisible").Should().BeTrue();

            // Guidelines should have distinct start/end points (not degenerate)
            var xStart = GetPointProperty(state, "XGuidelineStart");
            var xEnd = GetPointProperty(state, "XGuidelineEnd");
            var zStart = GetPointProperty(state, "ZGuidelineStart");
            var zEnd = GetPointProperty(state, "ZGuidelineEnd");

            xStart.Should().NotBe(xEnd, "X guideline should have non-zero length");
            zStart.Should().NotBe(zEnd, "Z guideline should have non-zero length");
        });
    }

    [Fact]
    public Task Crosshair_UsesCustomFormatter()
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
            view.Plot.OverlayOptions = new SurfaceChartOverlayOptions
            {
                ShowCrosshair = true,
                XAxisFormatter = static value => $"X={value:0.0}",
                ZAxisFormatter = static value => $"Z={value:0.0}",
            };

            view.Measure(new Size(320, 200));
            view.Arrange(new Rect(0, 0, 320, 200));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);

            UpdateProbeScreenPosition(view, new Point(160, 100));

            var state = GetCrosshairState(view);
            var xValueText = GetStringProperty(state, "XValueText");
            var zValueText = GetStringProperty(state, "ZValueText");

            xValueText.Should().StartWith("X=", "X value should use custom formatter");
            zValueText.Should().StartWith("Z=", "Z value should use custom formatter");
        });
    }

    private static object GetCrosshairState(VideraChartView view)
    {
        return SurfaceChartTestHelpers.GetOverlayCoordinator(view).CrosshairState;
    }

    private static void UpdateProbeScreenPosition(VideraChartView view, Point position)
    {
        var method = typeof(VideraChartView).GetMethod(
            "UpdateProbeScreenPosition",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should().NotBeNull();
        method!.Invoke(view, [position]);
    }

    private static bool GetBoolProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return (bool)property!.GetValue(instance)!;
    }

    private static string? GetStringProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return (string?)property!.GetValue(instance);
    }

    private static Point GetPointProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return (Point)property!.GetValue(instance)!;
    }
}
