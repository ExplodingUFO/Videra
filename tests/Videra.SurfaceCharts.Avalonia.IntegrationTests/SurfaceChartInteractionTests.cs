using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartInteractionTests
{
    [Fact]
    public void OrbitDrag_UpdatesViewStateCameraAngles()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = CreateArrangedView();
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);
            var initialViewState = view.ViewState;

            view.RoutePointerPressed(pointer, new Point(100, 100), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, new Point(136, 118), RawInputModifiers.LeftMouseButton);

            SurfaceChartTestHelpers.GetRuntime(view).CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Interactive);
            view.RoutePointerReleased(pointer, new Point(136, 118), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            view.ViewState.DataWindow.Should().Be(initialViewState.DataWindow);
            view.ViewState.Camera.Yaw.Should().NotBe(initialViewState.Camera.Yaw);
            view.ViewState.Camera.Pitch.Should().NotBe(initialViewState.Camera.Pitch);
        });
    }

    [Fact]
    public void PanDrag_UpdatesViewStateDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = CreateArrangedView();
            var pointer = new Pointer(2, PointerType.Mouse, isPrimary: true);
            var initialViewState = view.ViewState;

            view.RoutePointerPressed(pointer, new Point(120, 110), RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed);
            view.RoutePointerMoved(pointer, new Point(150, 90), RawInputModifiers.RightMouseButton);
            view.RoutePointerReleased(pointer, new Point(150, 90), RawInputModifiers.None, PointerUpdateKind.RightButtonReleased, MouseButton.Right);

            view.ViewState.DataWindow.Should().NotBe(initialViewState.DataWindow);
            view.ViewState.Camera.Yaw.Should().Be(initialViewState.Camera.Yaw);
            view.ViewState.Camera.Pitch.Should().Be(initialViewState.Camera.Pitch);
        });
    }

    [Fact]
    public void WheelDolly_UpdatesViewStateCameraDistance()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = CreateArrangedView();
            var pointer = new Pointer(3, PointerType.Mouse, isPrimary: true);
            var initialDistance = view.ViewState.Camera.Distance;

            view.RoutePointerWheel(pointer, new Point(128, 96), RawInputModifiers.None, new global::Avalonia.Vector(0, 1));

            SurfaceChartTestHelpers.GetRuntime(view).CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Interactive);
            view.ViewState.Camera.Distance.Should().NotBe(initialDistance);
        });
    }

    private static RoutedSurfaceChartView CreateArrangedView()
    {
        var dataWindow = new Videra.SurfaceCharts.Core.SurfaceDataWindow(0, 256, 0, 192);
        var view = new RoutedSurfaceChartView
        {
            ViewState = new Videra.SurfaceCharts.Core.SurfaceViewState(
                dataWindow,
                Videra.SurfaceCharts.Avalonia.Controls.Interaction.SurfaceChartRuntime.CreateDefaultCameraPose(dataWindow))
        };

        view.Measure(new Size(256, 192));
        view.Arrange(new Rect(0, 0, 256, 192));
        return view;
    }

    private sealed class RoutedSurfaceChartView : SurfaceChartView
    {
        public void RoutePointerPressed(Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
        }

        public void RoutePointerMoved(Pointer pointer, Point position, RawInputModifiers rawModifiers, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, this, pointer, this, position, timestamp: 0, properties, keyModifiers);
            base.OnPointerMoved(args);
        }

        public void RoutePointerReleased(Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, MouseButton mouseButton, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerReleasedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, mouseButton);
            base.OnPointerReleased(args);
        }

        public void RoutePointerWheel(Pointer pointer, Point position, RawInputModifiers rawModifiers, global::Avalonia.Vector delta, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerWheelEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, delta);
            base.OnPointerWheelChanged(args);
        }
    }
}
