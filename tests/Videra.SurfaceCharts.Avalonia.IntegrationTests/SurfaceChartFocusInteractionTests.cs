using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartFocusInteractionTests
{
    [Fact]
    public void BoxZoomFocusGesture_UpdatesDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = CreateArrangedView();

            view.RoutePointerPressed(new Point(64, 48), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed, KeyModifiers.Control);
            view.RoutePointerMoved(new Point(192, 144), RawInputModifiers.LeftMouseButton, KeyModifiers.Control);
            view.RoutePointerReleased(new Point(192, 144), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left, KeyModifiers.Control);

            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(64, 192, 48, 144));
        });
    }

    [Fact]
    public void BoxZoomFocusGesture_FitToData_RestoresFullDatasetWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var view = CreateArrangedView(new RecordingSurfaceTileSource(metadata));

            view.RoutePointerPressed(new Point(32, 24), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed, KeyModifiers.Control);
            view.RoutePointerMoved(new Point(224, 168), RawInputModifiers.LeftMouseButton, KeyModifiers.Control);
            view.RoutePointerReleased(new Point(224, 168), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left, KeyModifiers.Control);

            view.FitToData();

            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0, metadata.Width, 0, metadata.Height));
        });
    }

    [Fact]
    public void BoxZoomFocusGesture_ResetCamera_RestoresDefaultCameraForFocusedWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = CreateArrangedView();

            view.RoutePointerPressed(new Point(48, 24), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed, KeyModifiers.Control);
            view.RoutePointerMoved(new Point(208, 160), RawInputModifiers.LeftMouseButton, KeyModifiers.Control);
            view.RoutePointerReleased(new Point(208, 160), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left, KeyModifiers.Control);
            var focusedWindow = view.ViewState.DataWindow;

            view.RoutePointerPressed(new Point(120, 90), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(new Point(152, 112), RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(new Point(152, 112), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            view.ResetCamera();

            view.ViewState.DataWindow.Should().Be(focusedWindow);
            view.ViewState.Camera.Should().Be(CreateCamera(focusedWindow));
        });
    }

    private static RoutedSurfaceChartView CreateArrangedView(ISurfaceTileSource? source = null)
    {
        var dataWindow = new SurfaceDataWindow(0, 256, 0, 192);
        var view = new RoutedSurfaceChartView
        {
            Source = source,
            ViewState = new SurfaceViewState(
                dataWindow,
                CreateCamera(dataWindow))
        };

        view.Measure(new Size(256, 192));
        view.Arrange(new Rect(0, 0, 256, 192));
        return view;
    }

    private static SurfaceCameraPose CreateCamera(SurfaceDataWindow dataWindow)
    {
        return new SurfaceCameraPose(
            new Vector3(
                (float)((dataWindow.XMin + dataWindow.XMax) / 2.0),
                (float)((dataWindow.YMin + dataWindow.YMax) / 2.0),
                0f),
            yaw: 45.0,
            pitch: 35.264,
            distance: Math.Max(dataWindow.Width, dataWindow.Height) * 2.0,
            fieldOfView: 45.0,
            projectionMode: SurfaceProjectionMode.Perspective);
    }

    private sealed class RoutedSurfaceChartView : SurfaceChartView
    {
        private readonly Pointer _pointer = new(100, PointerType.Mouse, isPrimary: true);

        public void RoutePointerPressed(Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, _pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
        }

        public void RoutePointerMoved(Point position, RawInputModifiers rawModifiers, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, this, _pointer, this, position, timestamp: 0, properties, keyModifiers);
            base.OnPointerMoved(args);
        }

        public void RoutePointerReleased(Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, MouseButton mouseButton, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerReleasedEventArgs(this, _pointer, this, position, timestamp: 0, properties, keyModifiers, mouseButton);
            base.OnPointerReleased(args);
        }
    }
}
