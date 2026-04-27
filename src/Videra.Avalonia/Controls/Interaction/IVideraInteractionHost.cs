using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;

namespace Videra.Avalonia.Controls.Interaction;

internal interface IVideraInteractionHost
{
    VideraEngine Engine { get; }

    VideraInteractionMode InteractionMode { get; }

    VideraInteractionOptions InteractionOptions { get; }

    IReadOnlyList<Object3D> SceneObjects { get; }

    IReadOnlyList<InstanceBatchEntry> InstanceBatches { get; }

    IReadOnlyList<VideraMeasurement> Measurements { get; set; }

    IInputElement PointerCaptureTarget { get; }

    TopLevel? ResolveTopLevel();

    Vector2 GetInteractionViewportSize();

    bool IsPointWithinHost(Point position);

    void FocusHost();

    void RaiseSelectionRequested(SelectionRequestedEventArgs e);

    void RaiseAnnotationRequested(AnnotationRequestedEventArgs e);

    InteractiveFrameLease BeginInteractiveFrameLease();

    void InvalidateRender(RenderInvalidationKinds flags);
}
