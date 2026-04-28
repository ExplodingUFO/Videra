using System.Collections;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartPinnedProbeTests
{
    [Fact]
    public Task PinnedProbe_ShiftClick_TogglesProbeBubble()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedProbeTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var point = new Point(64, 32);

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.ViewState = new SurfaceViewState((new SurfaceViewport(128, 64, 256, 128)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);

            var initialState = GetOverlayState(view);
            GetPropertyValue(initialState, "HoveredProbe").Should().NotBeNull();
            GetPinnedProbes(initialState).Should().BeEmpty();

            view.RoutePointerPressed(
                pointer,
                point,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Shift);
            view.RoutePointerReleased(
                pointer,
                point,
                RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Shift);

            var pinnedState = GetOverlayState(view);
            GetPinnedProbes(pinnedState).Should().HaveCount(1);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);
            view.RoutePointerPressed(
                pointer,
                point,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Shift);
            view.RoutePointerReleased(
                pointer,
                point,
                RawInputModifiers.Shift,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Shift);

            var unpinnedState = GetOverlayState(view);
            GetPinnedProbes(unpinnedState).Should().BeEmpty();
        });
    }

    [Fact]
    public Task PinnedProbe_SurvivesViewportAndProjectionChanges_WithStableAxisTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 5,
                height: 5,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
                new SurfaceValueRange(-2d, 2d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 11f);
            var view = new RoutedProbeTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var point = new Point(100, 100);

            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));
            view.ViewState = new SurfaceViewState((new SurfaceViewport(0, 0, 4, 4)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);
            TogglePinnedProbe(view, pointer, point);

            var initialPinnedProbe = GetSinglePinnedProbe(GetOverlayState(view));
            AssertProbeTruth(initialPinnedProbe, sampleX: 2d, sampleY: 2d, axisX: 15d, axisY: 150d, value: 11d, isApproximate: false);

            view.ViewState = new SurfaceViewState((new SurfaceViewport(3d, 3d, 1d, 1d)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            UpdateProjectionSettings(view, new SurfaceChartProjectionSettings(210d, 15d));

            await SurfaceChartTestHelpers.AssertLoadedTileValuesStayAsync(view, [11f]);

            var persistedPinnedProbe = GetSinglePinnedProbe(GetOverlayState(view));
            AssertProbeTruth(persistedPinnedProbe, sampleX: 2d, sampleY: 2d, axisX: 15d, axisY: 150d, value: 11d, isApproximate: false);
        });
    }

    [Fact]
    public Task PinnedProbe_ShiftClick_RemainsAvailable_AfterOrbitGesture()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedProbeTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var point = new Point(96, 72);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.ViewState = new SurfaceViewState((new SurfaceViewport(128, 64, 256, 192)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.RoutePointerPressed(pointer, point, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, new Point(point.X + 36d, point.Y + 18d), RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, new Point(point.X + 36d, point.Y + 18d), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            view.RoutePointerMoved(pointer, point, RawInputModifiers.None);
            TogglePinnedProbe(view, pointer, point);

            GetPinnedProbes(GetOverlayState(view)).Should().HaveCount(1);
        });
    }

    private static object GetOverlayState(VideraChartView view)
    {
        return SurfaceChartTestHelpers.GetOverlayCoordinator(view).ProbeState;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.Should().NotBeNull($"Expected property '{propertyName}' on {instance.GetType().FullName}.");
        return property!.GetValue(instance);
    }

    private static object[] GetPinnedProbes(object overlayState)
    {
        var pinned = GetPropertyValue(overlayState, "PinnedProbes");
        pinned.Should().BeAssignableTo<IEnumerable>();
        return ((IEnumerable)pinned!).Cast<object>().ToArray();
    }

    private static object GetSinglePinnedProbe(object overlayState)
    {
        return GetPinnedProbes(overlayState).Should().ContainSingle().Subject;
    }

    private static void AssertProbeTruth(
        object probe,
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double value,
        bool isApproximate)
    {
        GetDoubleProperty(probe, "SampleX").Should().BeApproximately(sampleX, 0.0001d);
        GetDoubleProperty(probe, "SampleY").Should().BeApproximately(sampleY, 0.0001d);
        GetDoubleProperty(probe, "AxisX").Should().BeApproximately(axisX, 0.0001d);
        GetDoubleProperty(probe, "AxisY").Should().BeApproximately(axisY, 0.0001d);
        GetDoubleProperty(probe, "Value").Should().Be(value);
        GetBooleanProperty(probe, "IsApproximate").Should().Be(isApproximate);
    }

    private static void TogglePinnedProbe(RoutedProbeTestView view, Pointer pointer, Point position)
    {
        view.RoutePointerPressed(
            pointer,
            position,
            RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift,
            PointerUpdateKind.LeftButtonPressed,
            KeyModifiers.Shift);
        view.RoutePointerReleased(
            pointer,
            position,
            RawInputModifiers.Shift,
            PointerUpdateKind.LeftButtonReleased,
            MouseButton.Left,
            KeyModifiers.Shift);
    }

    private static void UpdateProjectionSettings(VideraChartView view, SurfaceChartProjectionSettings settings)
    {
        var method = typeof(VideraChartView).GetMethod(
            "UpdateProjectionSettings",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should().NotBeNull("Phase 15 keeps camera/projection changes behind a narrow chart-local seam.");
        method!.Invoke(view, [settings]);
    }

    private static double GetDoubleProperty(object instance, string propertyName)
    {
        return (double)GetPropertyValue(instance, propertyName)!;
    }

    private static bool GetBooleanProperty(object instance, string propertyName)
    {
        return (bool)GetPropertyValue(instance, propertyName)!;
    }

    private sealed class RoutedProbeTestView : VideraChartView
    {
        public void RoutePointerPressed(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
        }

        public void RoutePointerMoved(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, this, pointer, this, position, timestamp: 0, properties, keyModifiers);
            base.OnPointerMoved(args);
        }

        public void RoutePointerReleased(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            MouseButton mouseButton,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerReleasedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, mouseButton);
            base.OnPointerReleased(args);
        }
    }
}
