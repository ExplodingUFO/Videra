using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Videra.Core.Graphics;

namespace Videra.Avalonia.Controls.Interaction;

internal interface IVideraInteractionHost
{
    VideraEngine Engine { get; }

    VideraInteractionMode InteractionMode { get; }

    VideraInteractionOptions InteractionOptions { get; }

    IReadOnlyList<Object3D> SceneObjects { get; }

    IInputElement PointerCaptureTarget { get; }

    TopLevel? ResolveTopLevel();

    Vector2 GetInteractionViewportSize();

    bool IsPointWithinHost(Point position);

    void FocusHost();

    void RaiseSelectionRequested(SelectionRequestedEventArgs e);

    void RaiseAnnotationRequested(AnnotationRequestedEventArgs e);
}
