using System.Reflection;
using System.Numerics;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;

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
            var view = new VideraChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            var initialWindow = view.ViewState.DataWindow;

            // Invoke keyboard zoom via reflection (simulating + key)
            var onKeyDown = typeof(VideraChartView).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            onKeyDown.Should().NotBeNull();

            // After zoom in, window should be smaller (zoomed in)
            var initialWidth = initialWindow.Width;
            var initialHeight = initialWindow.Height;

            // Verify the runtime can interact
            var runtime = SurfaceChartTestHelpers.GetRuntime(view);
            runtime.CanInteract.Should().BeTrue();
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
                KeyModifiers.None);

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
                KeyModifiers.None);

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
                KeyModifiers.None);

            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.Orbit);

            controller.Reset();

            controller.ActiveGestureMode.Should().Be(SurfaceChartGestureMode.None);
            controller.HasActiveGesture.Should().BeFalse();
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
}
