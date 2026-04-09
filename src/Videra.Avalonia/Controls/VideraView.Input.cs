using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Videra.Avalonia.Controls.Interaction;

namespace Videra.Avalonia.Controls;

/// <summary>
/// VideraView 的输入转发部分。
/// </summary>
public partial class VideraView
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnViewAttached();
        _interactionRouter.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _interactionRouter.Detach();
        OnViewDetached();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _interactionRouter.RoutePressed(e, VideraPointerRoute.View);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _interactionRouter.RouteReleased(e, VideraPointerRoute.View);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _interactionRouter.RouteMoved(e, VideraPointerRoute.View);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _interactionRouter.RouteWheel(e, VideraPointerRoute.View);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _interactionRouter.RouteCaptureLost(e);
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _interactionRouter.RoutePressed(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _interactionRouter.RouteReleased(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerMoved(object? sender, PointerEventArgs e)
    {
        _interactionRouter.RouteMoved(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        _interactionRouter.RouteWheel(e, VideraPointerRoute.Overlay);
    }

    private void OnNativePointer(NativePointerEvent e)
    {
        _interactionRouter.RouteNativePointer(e);
    }
}
