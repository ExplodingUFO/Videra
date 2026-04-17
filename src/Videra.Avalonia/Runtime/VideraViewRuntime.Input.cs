using Avalonia.Input;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    internal void AttachInteractionRouter()
    {
        _interactionRouter.Attach();
    }

    internal void DetachInteractionRouter()
    {
        _interactionRouter.Detach();
    }

    internal void RoutePointerPressed(PointerPressedEventArgs e, VideraPointerRoute route)
    {
        _interactionRouter.RoutePressed(e, route);
    }

    internal void RoutePointerReleased(PointerReleasedEventArgs e, VideraPointerRoute route)
    {
        _interactionRouter.RouteReleased(e, route);
    }

    internal void RoutePointerMoved(PointerEventArgs e, VideraPointerRoute route)
    {
        _interactionRouter.RouteMoved(e, route);
    }

    internal void RoutePointerWheel(PointerWheelEventArgs e, VideraPointerRoute route)
    {
        _interactionRouter.RouteWheel(e, route);
    }

    internal void RoutePointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _interactionRouter.RouteCaptureLost(e);
    }

    internal void RouteNativePointer(NativePointerEvent e)
    {
        _interactionRouter.RouteNativePointer(e);
    }
}
