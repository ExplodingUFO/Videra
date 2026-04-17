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
        _runtime.AttachInteractionRouter();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _runtime.DetachInteractionRouter();
        OnViewDetached();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _runtime.RoutePointerPressed(e, VideraPointerRoute.View);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _runtime.RoutePointerReleased(e, VideraPointerRoute.View);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _runtime.RoutePointerMoved(e, VideraPointerRoute.View);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _runtime.RoutePointerWheel(e, VideraPointerRoute.View);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _runtime.RoutePointerCaptureLost(e);
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _runtime.RoutePointerPressed(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _runtime.RoutePointerReleased(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerMoved(object? sender, PointerEventArgs e)
    {
        _runtime.RoutePointerMoved(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        _runtime.RoutePointerWheel(e, VideraPointerRoute.Overlay);
    }

    private void OnNativePointer(NativePointerEvent e)
    {
        _runtime.RouteNativePointer(e);
    }
}
