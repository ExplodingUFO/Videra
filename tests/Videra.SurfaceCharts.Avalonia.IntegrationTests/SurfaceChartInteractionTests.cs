using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using FluentAssertions.Extensions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartInteractionTests
{
    [Fact]
    public Task LeftDrag_Orbits_ViewStateCamera()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var start = new Point(128d, 96d);
            var end = new Point(168d, 120d);

            view.RoutePointerPressed(pointer, start, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            view.ViewState.DataWindow.Should().Be(initialState.DataWindow);
            view.ViewState.Camera.Target.Should().Be(initialState.Camera.Target);
            view.ViewState.Camera.YawDegrees.Should().BeLessThan(initialState.Camera.YawDegrees);
            view.ViewState.Camera.PitchDegrees.Should().BeGreaterThan(initialState.Camera.PitchDegrees);
        });
    }

    [Fact]
    public Task RightDrag_HorizontalMotion_Pans_DepthWindowStart()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(256d, 256d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var start = new Point(120d, 84d);
            var end = new Point(168d, 84d);

            view.RoutePointerPressed(pointer, start, RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.RightMouseButton);
            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.RightButtonReleased, MouseButton.Right);

            view.ViewState.DataWindow.Width.Should().BeApproximately(initialState.DataWindow.Width, 0.0001d);
            view.ViewState.DataWindow.Height.Should().BeApproximately(initialState.DataWindow.Height, 0.0001d);
            view.ViewState.DataWindow.StartX.Should().BeApproximately(initialState.DataWindow.StartX, 0.0001d);
            view.ViewState.DataWindow.StartY.Should().BeLessThan(initialState.DataWindow.StartY);
            view.ViewState.Camera.YawDegrees.Should().BeApproximately(initialState.Camera.YawDegrees, 0.0001d);
            view.ViewState.Camera.PitchDegrees.Should().BeApproximately(initialState.Camera.PitchDegrees, 0.0001d);
        });
    }

    [Fact]
    public Task RightDrag_VerticalMotion_Pans_HorizontalWindowStart()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(256d, 256d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var start = new Point(120d, 84d);
            var end = new Point(120d, 132d);

            view.RoutePointerPressed(pointer, start, RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.RightMouseButton);
            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.RightButtonReleased, MouseButton.Right);

            view.ViewState.DataWindow.Width.Should().BeApproximately(initialState.DataWindow.Width, 0.0001d);
            view.ViewState.DataWindow.Height.Should().BeApproximately(initialState.DataWindow.Height, 0.0001d);
            view.ViewState.DataWindow.StartX.Should().BeGreaterThan(initialState.DataWindow.StartX);
            view.ViewState.DataWindow.StartY.Should().BeApproximately(initialState.DataWindow.StartY, 0.0001d);
            view.ViewState.Camera.YawDegrees.Should().BeApproximately(initialState.Camera.YawDegrees, 0.0001d);
            view.ViewState.Camera.PitchDegrees.Should().BeApproximately(initialState.Camera.PitchDegrees, 0.0001d);
        });
    }

    [Fact]
    public Task MouseWheel_Dollies_AroundHoveredProbeOrWindowCenter()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var wheelPoint = new Point(192d, 48d);

            view.RoutePointerMoved(pointer, wheelPoint, RawInputModifiers.None);
            view.RoutePointerWheel(pointer, wheelPoint, RawInputModifiers.None, new Vector(0d, 1d));

            var nextState = view.ViewState;
            nextState.DataWindow.Width.Should().BeLessThan(initialState.DataWindow.Width);
            nextState.DataWindow.Height.Should().BeLessThan(initialState.DataWindow.Height);
            GetCenterX(nextState.DataWindow).Should().BeGreaterThan(GetCenterX(initialState.DataWindow));
            GetCenterY(nextState.DataWindow).Should().BeLessThan(GetCenterY(initialState.DataWindow));
            nextState.Camera.Distance.Should().BeLessThan(initialState.Camera.Distance);
        });
    }

    [Fact]
    public Task CtrlLeftDrag_BoxZoom_UpdatesViewStateDataWindow()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var start = new Point(64d, 48d);
            var end = new Point(192d, 144d);

            view.RoutePointerPressed(
                pointer,
                start,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Control);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton | RawInputModifiers.Control, KeyModifiers.Control);
            view.RoutePointerReleased(
                pointer,
                end,
                RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Control);

            var nextState = view.ViewState;
            nextState.DataWindow.Width.Should().BeLessThan(initialState.DataWindow.Width);
            nextState.DataWindow.Height.Should().BeLessThan(initialState.DataWindow.Height);
            nextState.DataWindow.StartX.Should().BeGreaterThan(initialState.DataWindow.StartX);
            nextState.DataWindow.StartY.Should().BeGreaterThan(initialState.DataWindow.StartY);
        });
    }

    [Fact]
    public Task BoxZoom_BelowThreshold_DoesNotCreateDegenerateWindow()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));
            view.ResetCamera();

            var initialState = view.ViewState;
            var start = new Point(96d, 72d);
            var end = new Point(98d, 74d);

            view.RoutePointerPressed(
                pointer,
                start,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Control);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton | RawInputModifiers.Control, KeyModifiers.Control);
            view.RoutePointerReleased(
                pointer,
                end,
                RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Control);

            view.ViewState.Should().Be(initialState);
        });
    }

    [Fact]
    public Task NavigationGesture_TransitionsBetweenInteractiveAndRefineQuality()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            ReadInteractionQuality(view).Should().Be("Refine");

            var start = new Point(128d, 96d);
            var end = new Point(168d, 120d);
            view.RoutePointerPressed(pointer, start, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton);

            ReadInteractionQuality(view).Should().Be("Interactive");

            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            await WaitForConditionAsync(
                () => string.Equals(ReadInteractionQuality(view), "Refine", StringComparison.Ordinal),
                "the chart should settle back to refine quality after input stops.",
                3.Seconds()).ConfigureAwait(true);
        });
    }

    [Fact]
    public Task NavigationGesture_RaisesInteractionQualityChanged_ForInteractiveAndRefine()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var observedQualities = new List<SurfaceChartInteractionQuality>();

            view.InteractionQualityChanged += (_, _) => observedQualities.Add(view.InteractionQuality);
            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            var start = new Point(128d, 96d);
            var end = new Point(168d, 120d);
            view.RoutePointerPressed(pointer, start, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            await WaitForConditionAsync(
                () => observedQualities.Contains(SurfaceChartInteractionQuality.Interactive)
                    && observedQualities.Contains(SurfaceChartInteractionQuality.Refine)
                    && observedQualities[^1] == SurfaceChartInteractionQuality.Refine,
                "interaction diagnostics should report both the active and settled states.",
                3.Seconds()).ConfigureAwait(true);
        });
    }

    private static double GetCenterX(SurfaceDataWindow dataWindow)
    {
        return dataWindow.StartX + (dataWindow.Width * 0.5d);
    }

    private static double GetCenterY(SurfaceDataWindow dataWindow)
    {
        return dataWindow.StartY + (dataWindow.Height * 0.5d);
    }

    private static string? ReadInteractionQuality(VideraChartView view)
    {
        var property = typeof(VideraChartView).GetProperty("InteractionQuality");
        return property?.GetValue(view)?.ToString();
    }

    private static async Task WaitForConditionAsync(
        Func<bool> condition,
        string because,
        TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

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

    private sealed class RoutedInteractionTestView : VideraChartView
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

        public void RoutePointerWheel(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            Vector delta,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerWheelEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, delta);
            base.OnPointerWheelChanged(args);
        }
    }
}
