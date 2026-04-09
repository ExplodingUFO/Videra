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
        _interactionController.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _interactionController.Detach();
        OnViewDetached();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _interactionController.HandlePressed(e, VideraPointerRoute.View);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _interactionController.HandleReleased(e, VideraPointerRoute.View);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _interactionController.HandleMoved(e, VideraPointerRoute.View);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _interactionController.HandleWheel(e, VideraPointerRoute.View);
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _interactionController.HandlePressed(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _interactionController.HandleReleased(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerMoved(object? sender, PointerEventArgs e)
    {
        _interactionController.HandleMoved(e, VideraPointerRoute.Overlay);
    }

    private void OnOverlayPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        _interactionController.HandleWheel(e, VideraPointerRoute.Overlay);
    }

    private void OnNativePointer(NativePointerEvent e)
    {
        _interactionController.HandleNativePointer(e);
    }
}
