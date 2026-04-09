using System.Numerics;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Selection.Annotations;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewInteractionIntegrationTests
{
    [Fact]
    public void NavigateMode_RoutedPointerInput_UpdatesCamera()
    {
        var view = CreateInteractiveView();
        try
        {
            view.InteractionMode = VideraInteractionMode.Navigate;
            var pointer = CreateMousePointer();
            var yawBefore = view.Engine.Camera.Yaw;
            var pitchBefore = view.Engine.Camera.Pitch;
            var radiusBefore = view.Engine.Camera.Radius;

            view.RoutePointerPressed(pointer, new Point(100, 100), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            pointer.Captured.Should().BeSameAs(view);

            view.RoutePointerMoved(pointer, new Point(134, 116), RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, new Point(134, 116), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);
            view.RoutePointerWheel(pointer, new Point(134, 116), RawInputModifiers.None, new global::Avalonia.Vector(0, 1));

            view.Engine.Camera.Yaw.Should().NotBeApproximately(yawBefore, 0.0001f);
            view.Engine.Camera.Pitch.Should().NotBeApproximately(pitchBefore, 0.0001f);
            view.Engine.Camera.Radius.Should().BeLessThan(radiusBefore);
            pointer.Captured.Should().BeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_RoutedCtrlClick_EmitsToggleSelectionRequest_WithoutMutatingHostState()
    {
        var view = CreateInteractiveView();
        try
        {
            var sceneObject = AddCenteredQuad(view);
            var originalSelectionId = Guid.NewGuid();
            view.SelectionState = new VideraSelectionState
            {
                ObjectIds = [originalSelectionId],
                PrimaryObjectId = originalSelectionId
            };
            view.InteractionMode = VideraInteractionMode.Select;
            var pointer = CreateMousePointer();
            var point = ProjectPoint(view, Vector3.Zero);

            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            view.RoutePointerPressed(
                pointer,
                point,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Control);
            view.RoutePointerReleased(
                pointer,
                point,
                RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Control);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Toggle);
            request.ObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            request.PrimaryObjectId.Should().Be(sceneObject.Id);
            request.EmptySpaceSelectionBehavior.Should().Be(VideraEmptySpaceSelectionBehavior.ClearSelection);
            view.SelectionState.ObjectIds.Should().ContainSingle().Which.Should().Be(originalSelectionId);
            view.SelectionState.PrimaryObjectId.Should().Be(originalSelectionId);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_RoutedDrag_EmitsBoxSelectionRequest()
    {
        var view = CreateInteractiveView();
        try
        {
            var first = AddQuad(view, new Vector3(-1.1f, 0f, 0f));
            var second = AddQuad(view, new Vector3(1.1f, 0f, 0f));
            view.InteractionMode = VideraInteractionMode.Select;
            var pointer = CreateMousePointer();

            var firstPoint = ProjectPoint(view, first.WorldBounds!.Value.Center);
            var secondPoint = ProjectPoint(view, second.WorldBounds!.Value.Center);
            var start = new Point(Math.Min(firstPoint.X, secondPoint.X) - 30d, Math.Min(firstPoint.Y, secondPoint.Y) - 30d);
            var end = new Point(Math.Max(firstPoint.X, secondPoint.X) + 30d, Math.Max(firstPoint.Y, secondPoint.Y) + 30d);

            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            view.RoutePointerPressed(pointer, start, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, end, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Replace);
            request.ObjectIds.Should().Equal(first.Id, second.Id);
            request.PrimaryObjectId.Should().Be(first.Id);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_EmptySpaceClick_EmitsClearRequest_WhenConfigured()
    {
        var view = CreateInteractiveView();
        try
        {
            view.InteractionMode = VideraInteractionMode.Select;
            view.InteractionOptions = new VideraInteractionOptions
            {
                EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.ClearSelection
            };
            var pointer = CreateMousePointer();

            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            view.RoutePointerPressed(pointer, new Point(12, 12), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerReleased(pointer, new Point(12, 12), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Replace);
            request.ObjectIds.Should().BeEmpty();
            request.PrimaryObjectId.Should().BeNull();
            request.EmptySpaceSelectionBehavior.Should().Be(VideraEmptySpaceSelectionBehavior.ClearSelection);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_EmptySpaceClick_DoesNotEmitRequest_WhenConfiguredToPreserveSelection()
    {
        var view = CreateInteractiveView();
        try
        {
            view.InteractionMode = VideraInteractionMode.Select;
            view.InteractionOptions = new VideraInteractionOptions
            {
                EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.PreserveSelection
            };
            var pointer = CreateMousePointer();

            var requests = new List<SelectionRequestedEventArgs>();
            view.SelectionRequested += (_, e) => requests.Add(e);

            view.RoutePointerPressed(pointer, new Point(12, 12), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerReleased(pointer, new Point(12, 12), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            requests.Should().BeEmpty();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void AnnotateMode_RoutedClicks_EmitObjectAndWorldPointAnnotationRequests()
    {
        var view = CreateInteractiveView();
        try
        {
            var sceneObject = AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Annotate;
            var pointer = CreateMousePointer();
            var objectPoint = ProjectPoint(view, Vector3.Zero);
            var emptyPoint = new Point(12, 12);
            var requests = new List<AnnotationRequestedEventArgs>();
            view.AnnotationRequested += (_, e) => requests.Add(e);

            view.RoutePointerPressed(pointer, objectPoint, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerReleased(pointer, objectPoint, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);
            view.RoutePointerPressed(pointer, emptyPoint, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerReleased(pointer, emptyPoint, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            requests.Should().HaveCount(2);
            requests[0].Anchor.Kind.Should().Be(AnnotationAnchorKind.Object);
            requests[0].Anchor.ObjectId.Should().Be(sceneObject.Id);
            requests[1].Anchor.Kind.Should().Be(AnnotationAnchorKind.WorldPoint);
            requests[1].Anchor.WorldPoint.Should().NotBeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RoutedCaptureLifecycle_PreservesCaptureUntilAllButtonsRelease_AndResetsOnCaptureLost()
    {
        var view = CreateInteractiveView();
        try
        {
            view.InteractionMode = VideraInteractionMode.Navigate;
            var pointer = CreateMousePointer();
            var targetBeforeCaptureLoss = view.Engine.Camera.Target;

            view.RoutePointerPressed(pointer, new Point(100, 100), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            pointer.Captured.Should().BeSameAs(view);

            view.RoutePointerPressed(
                pointer,
                new Point(100, 100),
                RawInputModifiers.LeftMouseButton | RawInputModifiers.RightMouseButton,
                PointerUpdateKind.RightButtonPressed);

            view.RoutePointerReleased(
                pointer,
                new Point(100, 100),
                RawInputModifiers.RightMouseButton,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left);
            pointer.Captured.Should().BeSameAs(view);

            pointer.Capture(null);
            view.RoutePointerCaptureLost(pointer);
            view.RoutePointerMoved(pointer, new Point(130, 120), RawInputModifiers.RightMouseButton);

            view.Engine.Camera.Target.Should().Be(targetBeforeCaptureLoss);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    private static RoutedInteractionTestView CreateInteractiveView()
    {
        var view = new RoutedInteractionTestView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        view.Measure(new Size(200, 200));
        view.Arrange(new Rect(0, 0, 200, 200));

        var renderSession = view.ReadPrivateField<RenderSession>("_renderSession");
        renderSession.Attach(GraphicsBackendPreference.Software);
        renderSession.Resize(200, 200, 1f);

        view.Engine.Camera.SetOrbit(Vector3.Zero, 5f, 0f, 0f);
        view.Engine.Camera.UpdateProjection(200, 200);
        return view;
    }

    private static Object3D AddCenteredQuad(RoutedInteractionTestView view)
    {
        return AddQuad(view, Vector3.Zero);
    }

    private static Object3D AddQuad(RoutedInteractionTestView view, Vector3 position)
    {
        var renderSession = view.ReadPrivateField<RenderSession>("_renderSession");
        renderSession.ResourceFactory.Should().NotBeNull();
        var sceneObject = DemoMeshFactory.CreateWhiteQuad(renderSession.ResourceFactory!);
        sceneObject.Position = position;
        view.AddObject(sceneObject);
        return sceneObject;
    }

    private static Pointer CreateMousePointer()
    {
        return new Pointer(1, PointerType.Mouse, true);
    }

    private static Point ProjectPoint(RoutedInteractionTestView view, Vector3 worldPoint)
    {
        view.Engine.Camera.TryProjectWorldPoint(worldPoint, new Vector2(200f, 200f), out var screenPoint).Should().BeTrue();
        return new Point(screenPoint.X, screenPoint.Y);
    }

    private sealed class RoutedInteractionTestView : VideraView
    {
        public RoutedInteractionTestView(INativeHostFactory? nativeHostFactory, Func<uint, uint, global::Avalonia.Media.Imaging.WriteableBitmap?>? bitmapFactory)
            : base(nativeHostFactory, bitmapFactory)
        {
        }

        public void RoutePointerPressed(Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerPressedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
            base.OnPointerPressed(args);
        }

        public void RoutePointerMoved(Pointer pointer, Point position, RawInputModifiers rawModifiers, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerEventArgs(InputElement.PointerMovedEvent, this, pointer, this, position, timestamp: 0, properties, keyModifiers);
            base.OnPointerMoved(args);
        }

        public void RoutePointerReleased(Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, MouseButton mouseButton, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, updateKind);
            var args = new PointerReleasedEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, mouseButton);
            base.OnPointerReleased(args);
        }

        public void RoutePointerWheel(Pointer pointer, Point position, RawInputModifiers rawModifiers, global::Avalonia.Vector delta, KeyModifiers keyModifiers = KeyModifiers.None)
        {
            var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
            var args = new PointerWheelEventArgs(this, pointer, this, position, timestamp: 0, properties, keyModifiers, delta);
            base.OnPointerWheelChanged(args);
        }

        public void RoutePointerCaptureLost(Pointer pointer)
        {
            var args = new PointerCaptureLostEventArgs(this, pointer);
            base.OnPointerCaptureLost(args);
        }

        public T ReadPrivateField<T>(string fieldName)
        {
            var field = typeof(VideraView).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.Should().NotBeNull($"field {fieldName} should exist on {typeof(VideraView).FullName}");

            var value = field!.GetValue(this);
            value.Should().BeAssignableTo<T>();
            return (T)value!;
        }
    }
}
