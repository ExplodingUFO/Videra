using System.Numerics;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartViewViewStateTests
{
    [Fact]
    public void ViewState_UpdatePreservesDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            var viewport = new SurfaceViewport(64d, 32d, 128d, 96d);

            view.ViewState = new SurfaceViewState((viewport).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

            view.ViewState.DataWindow.Should().Be(viewport.ToDataWindow());
        });
    }

    [Fact]
    public void ViewState_ExposesDataWindowAsViewport()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            var viewState = new SurfaceViewState(
                new SurfaceDataWindow(10d, 20d, 30d, 40d),
                new SurfaceCameraPose(new Vector3(1f, 2f, 3f), 210d, 15d, 24d, 45d));

            view.ViewState = viewState;

            view.ViewState.DataWindow.ToViewport().Should().Be(new SurfaceViewport(10d, 20d, 30d, 40d));
        });
    }

    [Fact]
    public void FitToData_ResetsDataWindowToActiveMetadataBounds()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var source = new RecordingSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata());
            var view = new SurfaceChartView
            {
                Source = source,
                ViewState = new SurfaceViewState(
                    new SurfaceDataWindow(128d, 64d, 256d, 128d),
                    new SurfaceCameraPose(new Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d))
            };

            view.FitToData();

            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));
            view.ViewState.DataWindow.ToViewport().Should().Be(new SurfaceViewport(0d, 0d, source.Metadata.Width, source.Metadata.Height));
        });
    }

    [Fact]
    public void ResetCamera_RestoresDefaultCameraPoseWithoutChangingDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var source = new RecordingSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata());
            var dataWindow = new SurfaceDataWindow(128d, 64d, 256d, 128d);
            var view = new SurfaceChartView
            {
                Source = source,
                ViewState = new SurfaceViewState(
                    dataWindow,
                    new SurfaceCameraPose(new Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d))
            };

            view.ResetCamera();

            view.ViewState.DataWindow.Should().Be(dataWindow);
            view.ViewState.Camera.Should().Be(SurfaceCameraPose.CreateDefault(source.Metadata, dataWindow));
            view.ViewState.DataWindow.ToViewport().Should().Be(dataWindow.ToViewport());
        });
    }

    [Fact]
    public void ZoomTo_Updates_DataWindow_ThroughPublicApi()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var source = new RecordingSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata());
            var view = new SurfaceChartView
            {
                Source = source
            };
            var dataWindow = new SurfaceDataWindow(96d, 48d, 128d, 96d);

            view.ZoomTo(dataWindow);

            view.ViewState.DataWindow.Should().Be(dataWindow);
            view.ViewState.DataWindow.ToViewport().Should().Be(dataWindow.ToViewport());
        });
    }

    [Fact]
    public void FitToData_ResetCamera_AndZoomTo_NoOp_WhenSourceIsNull()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            var initialState = view.ViewState;

            view.FitToData();
            view.ResetCamera();
            view.ZoomTo(new SurfaceDataWindow(10d, 20d, 30d, 40d));

            view.ViewState.Should().Be(initialState);
            view.ViewState.DataWindow.ToViewport().Should().Be(initialState.ToViewport());
        });
    }

    [Fact]
    public Task FitToData_And_ResetCamera_RemainDeterministic_AfterBuiltInFocus()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 11f);
            var view = new RoutedInteractionTestView
            {
                Source = source
            };
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(256, 192));
            view.Arrange(new Rect(0, 0, 256, 192));

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [11f]);

            view.RoutePointerPressed(
                pointer,
                new Point(64d, 48d),
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Control);
            view.RoutePointerMoved(pointer, new Point(192d, 144d), RawInputModifiers.LeftMouseButton | RawInputModifiers.Control, KeyModifiers.Control);
            view.RoutePointerReleased(
                pointer,
                new Point(192d, 144d),
                RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Control);

            var focusedWindow = view.ViewState.DataWindow;
            focusedWindow.Width.Should().BeLessThan(source.Metadata.Width);
            focusedWindow.Height.Should().BeLessThan(source.Metadata.Height);

            view.ViewState = new SurfaceViewState(
                focusedWindow,
                new SurfaceCameraPose(new Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d));

            view.ResetCamera();
            view.ViewState.DataWindow.Should().Be(focusedWindow);
            view.ViewState.Camera.Should().Be(SurfaceCameraPose.CreateDefault(source.Metadata, focusedWindow));

            view.FitToData();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));
        });
    }

    private sealed class RoutedInteractionTestView : SurfaceChartView
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
