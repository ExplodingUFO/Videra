using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Inspection;

namespace Videra.Avalonia.Controls;

public partial class VideraView : IVideraInteractionHost
{
    TopLevel? IVideraInteractionHost.ResolveTopLevel() => TopLevel.GetTopLevel(this);

    VideraInteractionMode IVideraInteractionHost.InteractionMode => InteractionMode;

    VideraInteractionOptions IVideraInteractionHost.InteractionOptions => _runtime.InteractionOptions;

    IReadOnlyList<Object3D> IVideraInteractionHost.SceneObjects => Engine.SceneObjects;

    IReadOnlyList<VideraMeasurement> IVideraInteractionHost.Measurements
    {
        get => _runtime.Measurements;
        set => _runtime.Measurements = value;
    }

    IInputElement IVideraInteractionHost.PointerCaptureTarget => this;

    Vector2 IVideraInteractionHost.GetInteractionViewportSize() => _runtime.GetInteractionViewportSize();

    bool IVideraInteractionHost.IsPointWithinHost(Point position)
    {
        var viewport = ((IVideraInteractionHost)this).GetInteractionViewportSize();
        return position.X >= 0 &&
               position.Y >= 0 &&
               position.X < viewport.X &&
               position.Y < viewport.Y;
    }

    void IVideraInteractionHost.FocusHost()
    {
        Focus();
    }

    void IVideraInteractionHost.RaiseSelectionRequested(SelectionRequestedEventArgs e)
    {
        RaiseSelectionRequested(e);
    }

    void IVideraInteractionHost.RaiseAnnotationRequested(AnnotationRequestedEventArgs e)
    {
        RaiseAnnotationRequested(e);
    }

    InteractiveFrameLease IVideraInteractionHost.BeginInteractiveFrameLease() => _runtime.RenderSession.AcquireInteractiveLease();

    void IVideraInteractionHost.InvalidateRender(RenderInvalidationKinds flags)
    {
        _runtime.RenderSession.Invalidate(flags);
    }
}
