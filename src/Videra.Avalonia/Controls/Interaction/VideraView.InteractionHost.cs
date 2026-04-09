using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Graphics;

namespace Videra.Avalonia.Controls;

public partial class VideraView : IVideraInteractionHost
{
    private VideraInteractionController _interactionController = null!;
    private VideraInteractionRouter _interactionRouter = null!;

    private void InitializeInteractionController()
    {
        _interactionController = new VideraInteractionController(this, _logger);
        _interactionRouter = new VideraInteractionRouter(this, _interactionController);
    }

    TopLevel? IVideraInteractionHost.ResolveTopLevel() => TopLevel.GetTopLevel(this);

    VideraInteractionMode IVideraInteractionHost.InteractionMode => InteractionMode;

    VideraInteractionOptions IVideraInteractionHost.InteractionOptions => InteractionOptions;

    IReadOnlyList<Object3D> IVideraInteractionHost.SceneObjects => Engine.SceneObjects;

    IInputElement IVideraInteractionHost.PointerCaptureTarget => this;

    Vector2 IVideraInteractionHost.GetInteractionViewportSize()
    {
        var width = (float)Math.Max(0d, Bounds.Width);
        var height = (float)Math.Max(0d, Bounds.Height);
        if (width > 0f && height > 0f)
        {
            return new Vector2(width, height);
        }

        var snapshot = _renderSession.OrchestrationSnapshot;
        return snapshot.Inputs.Width > 0 && snapshot.Inputs.Height > 0
            ? new Vector2(snapshot.Inputs.Width, snapshot.Inputs.Height)
            : Vector2.Zero;
    }

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
}
