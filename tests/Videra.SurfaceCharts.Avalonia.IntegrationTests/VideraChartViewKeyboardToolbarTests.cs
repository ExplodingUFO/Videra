using System.Numerics;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;
using Pointer = Avalonia.Input.Pointer;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class VideraChartViewKeyboardToolbarTests
{
    [Fact]
    public void ToolbarOverlayState_DefaultsToEmpty()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var coordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);

            coordinator.ToolbarState.Should().BeSameAs(SurfaceChartToolbarOverlayState.Empty);
        });
    }

    [Fact]
    public void ToolbarOverlayState_WithSourceAndSize_ShowsButtons()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();

            view.Measure(new Size(400, 300));
            view.Arrange(new Rect(0, 0, 400, 300));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            var coordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);

            // Toolbar should be visible after source load and layout
            coordinator.ToolbarState.IsVisible.Should().BeTrue();
            coordinator.ToolbarState.Buttons.Should().HaveCount(4);
        });
    }

    [Fact]
    public void ToolbarHitTest_WithinZoomInButton_ReturnsZoomInAction()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var viewSize = new Size(400, 300);
            var state = SurfaceChartToolbarOverlayPresenter.CreateState(viewSize, null, canInteract: true);

            // Zoom in button should be at bottom-right
            var zoomInButton = state.Buttons.First(b => b.Action == SurfaceChartToolbarAction.ZoomIn);
            var hitResult = SurfaceChartToolbarOverlayPresenter.HitTest(state, new Point(
                zoomInButton.ScreenBounds.X + 14,
                zoomInButton.ScreenBounds.Y + 14));

            hitResult.Should().Be(SurfaceChartToolbarAction.ZoomIn);
        });
    }

    [Fact]
    public void ToolbarHitTest_OutsideButtons_ReturnsNull()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var viewSize = new Size(400, 300);
            var state = SurfaceChartToolbarOverlayPresenter.CreateState(viewSize, null, canInteract: true);

            // Top-left corner should not hit any button
            var hitResult = SurfaceChartToolbarOverlayPresenter.HitTest(state, new Point(10, 10));

            hitResult.Should().BeNull();
        });
    }

    [Fact]
    public void ToolbarState_WithoutInteraction_ReturnsEmpty()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var viewSize = new Size(400, 300);
            var state = SurfaceChartToolbarOverlayPresenter.CreateState(viewSize, null, canInteract: false);

            state.IsVisible.Should().BeFalse();
            state.Buttons.Should().BeEmpty();
        });
    }

    [Fact]
    public void KeyboardZoomIn_ChangesDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new RoutedKeyboardTestView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));

            var initialWindow = view.ViewState.DataWindow;
            var args = view.RouteKeyDown(Key.Add);

            args.Handled.Should().BeTrue();
            view.ViewState.DataWindow.Width.Should().BeLessThan(initialWindow.Width);
            view.ViewState.DataWindow.Height.Should().BeLessThan(initialWindow.Height);
        });
    }

    [Fact]
    public void InteractionController_ActiveGestureMode_WhenNoGesture_ReturnsNone()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var controller = new SurfaceChartInteractionController();

            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.None);
            controller.HasActiveGesture.Should().BeFalse();
        });
    }

    [Fact]
    public void InteractionController_ActiveGestureMode_AfterPress_ReturnsGestureMode()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var controller = new SurfaceChartInteractionController();

            // Left button press should start orbit
            var handled = controller.HandlePointerPressed(
                PointerUpdateKind.LeftButtonPressed,
                new Point(100, 100),
                KeyModifiers.None,
                SurfaceChartInteractionProfile.Default);

            handled.Should().BeTrue();
            controller.HasActiveGesture.Should().BeTrue();
            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.Orbit);
        });
    }

    [Fact]
    public void InteractionController_ActiveGestureMode_AfterRightPress_ReturnsPan()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var controller = new SurfaceChartInteractionController();

            // Right button press should start pan
            var handled = controller.HandlePointerPressed(
                PointerUpdateKind.RightButtonPressed,
                new Point(100, 100),
                KeyModifiers.None,
                SurfaceChartInteractionProfile.Default);

            handled.Should().BeTrue();
            controller.HasActiveGesture.Should().BeTrue();
            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.Pan);
        });
    }

    [Fact]
    public void InteractionController_ActiveGestureMode_AfterReset_ReturnsNone()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var controller = new SurfaceChartInteractionController();

            controller.HandlePointerPressed(
                PointerUpdateKind.LeftButtonPressed,
                new Point(100, 100),
                KeyModifiers.None,
                SurfaceChartInteractionProfile.Default);

            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.Orbit);

            controller.Reset();

            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.None);
            controller.HasActiveGesture.Should().BeFalse();
        });
    }

    [Fact]
    public void InteractionController_DisabledProfile_RejectsBuiltInGestures()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var controller = new SurfaceChartInteractionController();

            controller.HandlePointerPressed(
                    PointerUpdateKind.LeftButtonPressed,
                    new Point(100, 100),
                    KeyModifiers.None,
                    SurfaceChartInteractionProfile.Disabled)
                .Should()
                .BeFalse();

            controller.HandlePointerPressed(
                    PointerUpdateKind.RightButtonPressed,
                    new Point(100, 100),
                    KeyModifiers.None,
                    SurfaceChartInteractionProfile.Disabled)
                .Should()
                .BeFalse();

            controller.HasActiveGesture.Should().BeFalse();
        });
    }

    [Fact]
    public void CommandSurface_DisabledZoom_ReturnsFalseAndKeepsViewState()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView
            {
                InteractionProfile = new SurfaceChartInteractionProfile
                {
                    IsDollyEnabled = false,
                },
            };

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));

            var initialState = view.ViewState;

            view.TryExecuteChartCommand(SurfaceChartCommand.ZoomIn).Should().BeFalse();
            view.ViewState.Should().Be(initialState);
        });
    }

    [Fact]
    public void CommandSurface_DefaultZoom_ChangesDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));

            var initialWindow = view.ViewState.DataWindow;

            view.TryExecuteChartCommand(SurfaceChartCommand.ZoomIn).Should().BeTrue();
            view.ViewState.DataWindow.Width.Should().BeLessThan(initialWindow.Width);
            view.ViewState.DataWindow.Height.Should().BeLessThan(initialWindow.Height);
        });
    }

    [Fact]
    public void KeyboardShortcuts_DisabledProfile_DoesNotHandleOrChangeViewState()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new RoutedKeyboardTestView
            {
                InteractionProfile = new SurfaceChartInteractionProfile
                {
                    IsKeyboardShortcutsEnabled = false,
                },
            };

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));

            var initialState = view.ViewState;
            var args = view.RouteKeyDown(Key.Add);

            args.Handled.Should().BeFalse();
            view.ViewState.Should().Be(initialState);
        });
    }

    [Fact]
    public void ToolbarCommands_DisabledProfile_ConsumesToolbarHitWithoutChangingViewState()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new RoutedKeyboardTestView
            {
                InteractionProfile = new SurfaceChartInteractionProfile
                {
                    IsToolbarEnabled = false,
                },
            };
            var pointer = new Pointer(1, PointerType.Mouse, isPrimary: true);

            view.Measure(new Size(400, 300));
            view.Arrange(new Rect(0, 0, 400, 300));
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.ZoomTo(new SurfaceDataWindow(128d, 128d, 256d, 256d));

            var coordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);
            var zoomInButton = coordinator.ToolbarState.Buttons.First(b => b.Action == SurfaceChartToolbarAction.ZoomIn);
            var clickPoint = new Point(
                zoomInButton.ScreenBounds.X + (zoomInButton.ScreenBounds.Width * 0.5d),
                zoomInButton.ScreenBounds.Y + (zoomInButton.ScreenBounds.Height * 0.5d));
            var initialState = view.ViewState;

            var handled = view.RoutePointerPressed(
                pointer,
                clickPoint,
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed);

            handled.Should().BeTrue();
            view.ViewState.Should().Be(initialState);
        });
    }

    [Fact]
    public void ToolbarPresenter_AllFourActions_HaveUniqueScreenBounds()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var viewSize = new Size(400, 300);
            var state = SurfaceChartToolbarOverlayPresenter.CreateState(viewSize, null, canInteract: true);

            state.Buttons.Should().HaveCount(4);

            var bounds = state.Buttons.Select(b => b.ScreenBounds).ToList();
            bounds.Distinct().Should().HaveCount(4, "each button should have unique bounds");

            // All buttons should have the same size
            var sizes = bounds.Select(b => (b.Width, b.Height)).Distinct().ToList();
            sizes.Should().HaveCount(1);
        });
    }

    [Fact]
    public void ToolbarPresenter_ButtonsPositionedInBottomRight()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var viewSize = new Size(400, 300);
            var state = SurfaceChartToolbarOverlayPresenter.CreateState(viewSize, null, canInteract: true);

            // All buttons should be in the right portion of the view
            foreach (var button in state.Buttons)
            {
                button.ScreenBounds.X.Should().BeGreaterThan(viewSize.Width / 2,
                    "toolbar buttons should be in the right half");
                button.ScreenBounds.Y.Should().BeGreaterThan(viewSize.Height / 2,
                    "toolbar buttons should be in the bottom half");
            }
        });
    }

    private sealed class RoutedKeyboardTestView : VideraChartView
    {
        public KeyEventArgs RouteKeyDown(Key key)
        {
            var args = new KeyEventArgs
            {
                Key = key,
                RoutedEvent = KeyDownEvent,
                Source = this,
            };
            base.OnKeyDown(args);
            return args;
        }

        public bool RoutePointerPressed(
            Pointer pointer,
            Point position,
            RawInputModifiers rawModifiers,
            PointerUpdateKind updateKind,
            KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
            return args.Handled;
        }
    }
}
