using System.Numerics;
using BindingFlags = System.Reflection.BindingFlags;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Remote.Protocol;
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
    private const int MkLButton = 0x0001;
    private const int MkShift = 0x0004;
    private const int MkControl = 0x0008;

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
    public void SelectMode_RoutedCtrlDrag_EmitsAdditiveBoxSelectionRequest()
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

            view.RoutePointerPressed(
                pointer,
                start,
                RawInputModifiers.LeftMouseButton | RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonPressed,
                KeyModifiers.Control);
            view.RoutePointerMoved(pointer, end, RawInputModifiers.LeftMouseButton | RawInputModifiers.Control, KeyModifiers.Control);
            view.RoutePointerReleased(
                pointer,
                end,
                RawInputModifiers.Control,
                PointerUpdateKind.LeftButtonReleased,
                MouseButton.Left,
                KeyModifiers.Control);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Add);
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
    public void SelectMode_EmptySpaceDrag_EmitsClearRequest_WhenConfigured()
    {
        var view = CreateInteractiveView();
        try
        {
            AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Select;
            view.InteractionOptions = new VideraInteractionOptions
            {
                EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.ClearSelection
            };
            var pointer = CreateMousePointer();

            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            view.RoutePointerPressed(pointer, new Point(12, 12), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, new Point(40, 40), RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, new Point(40, 40), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

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
    public void SelectMode_EmptySpaceDrag_DoesNotEmitRequest_WhenConfiguredToPreserveSelection()
    {
        var view = CreateInteractiveView();
        try
        {
            AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Select;
            view.InteractionOptions = new VideraInteractionOptions
            {
                EmptySpaceSelectionBehavior = VideraEmptySpaceSelectionBehavior.PreserveSelection
            };
            var pointer = CreateMousePointer();

            var requests = new List<SelectionRequestedEventArgs>();
            view.SelectionRequested += (_, e) => requests.Add(e);

            view.RoutePointerPressed(pointer, new Point(12, 12), RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            view.RoutePointerMoved(pointer, new Point(40, 40), RawInputModifiers.LeftMouseButton);
            view.RoutePointerReleased(pointer, new Point(40, 40), RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

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
    public void SelectMode_TopLevelRoutedPointerInput_EmitsSelectionRequest_AfterAttach()
    {
        var view = CreateInteractiveView();
        var topLevel = CreateOffscreenTopLevel();
        try
        {
            AddCenteredQuad(view);
            topLevel.Content = view;
            topLevel.Measure(new Size(200, 200));
            topLevel.Arrange(new Rect(0, 0, 200, 200));
            view.AttachToVisualTree(topLevel);

            view.InteractionMode = VideraInteractionMode.Select;
            var pointer = CreateMousePointer();
            var point = ProjectPoint(view, Vector3.Zero);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            RouteTopLevelPointerPressed(topLevel, pointer, point, RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            RouteTopLevelPointerReleased(topLevel, pointer, point, RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased, MouseButton.Left);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Replace);
            request.ObjectIds.Should().ContainSingle();
        }
        finally
        {
            view.DetachFromVisualTree(topLevel);
            topLevel.Content = null;
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_NativePointerInput_EmitsSelectionRequest()
    {
        var nativeHostFactory = new RecordingNativeHostFactory();
        var view = CreateInteractiveView(nativeHostFactory);
        try
        {
            AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Select;
            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            var point = ProjectPoint(view, Vector3.Zero);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            nativeHostFactory.LastCreatedHost!.RaiseNativePointer(new NativePointerEvent(NativePointerKind.LeftDown, (int)Math.Round(point.X), (int)Math.Round(point.Y), wheelDelta: 0));
            nativeHostFactory.LastCreatedHost.RaiseNativePointer(new NativePointerEvent(NativePointerKind.LeftUp, (int)Math.Round(point.X), (int)Math.Round(point.Y), wheelDelta: 0));

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Replace);
            request.ObjectIds.Should().ContainSingle();
        }
        finally
        {
            InvokeNonPublicMethod(view, "ReleaseNativeHost");
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_NativeCtrlClick_EmitsToggleSelectionRequest()
    {
        var nativeHostFactory = new RecordingNativeHostFactory();
        var view = CreateInteractiveView(nativeHostFactory);
        try
        {
            var sceneObject = AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Select;
            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            var point = ProjectPoint(view, Vector3.Zero);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            nativeHostFactory.LastCreatedHost!.RaiseNativePointer(new NativePointerEvent(
                NativePointerKind.LeftDown,
                (int)Math.Round(point.X),
                (int)Math.Round(point.Y),
                wheelDelta: 0,
                modifiers: RawInputModifiers.Control));
            nativeHostFactory.LastCreatedHost.RaiseNativePointer(new NativePointerEvent(
                NativePointerKind.LeftUp,
                (int)Math.Round(point.X),
                (int)Math.Round(point.Y),
                wheelDelta: 0,
                modifiers: RawInputModifiers.Control));

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Toggle);
            request.ObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            request.PrimaryObjectId.Should().Be(sceneObject.Id);
        }
        finally
        {
            InvokeNonPublicMethod(view, "ReleaseNativeHost");
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_NativeCtrlDrag_EmitsAdditiveBoxSelectionRequest()
    {
        var nativeHostFactory = new RecordingNativeHostFactory();
        var view = CreateInteractiveView(nativeHostFactory);
        try
        {
            var first = AddQuad(view, new Vector3(-1.1f, 0f, 0f));
            var second = AddQuad(view, new Vector3(1.1f, 0f, 0f));
            view.InteractionMode = VideraInteractionMode.Select;
            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            var firstPoint = ProjectPoint(view, first.WorldBounds!.Value.Center);
            var secondPoint = ProjectPoint(view, second.WorldBounds!.Value.Center);
            var start = new Point(Math.Min(firstPoint.X, secondPoint.X) - 30d, Math.Min(firstPoint.Y, secondPoint.Y) - 30d);
            var end = new Point(Math.Max(firstPoint.X, secondPoint.X) + 30d, Math.Max(firstPoint.Y, secondPoint.Y) + 30d);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            nativeHostFactory.LastCreatedHost!.RaiseNativePointer(new NativePointerEvent(
                NativePointerKind.LeftDown,
                (int)Math.Round(start.X),
                (int)Math.Round(start.Y),
                wheelDelta: 0,
                modifiers: RawInputModifiers.Control));
            nativeHostFactory.LastCreatedHost.RaiseNativePointer(new NativePointerEvent(
                NativePointerKind.Move,
                (int)Math.Round(end.X),
                (int)Math.Round(end.Y),
                wheelDelta: 0,
                modifiers: RawInputModifiers.Control));
            nativeHostFactory.LastCreatedHost.RaiseNativePointer(new NativePointerEvent(
                NativePointerKind.LeftUp,
                (int)Math.Round(end.X),
                (int)Math.Round(end.Y),
                wheelDelta: 0,
                modifiers: RawInputModifiers.Control));

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Add);
            request.ObjectIds.Should().Equal(first.Id, second.Id);
            request.PrimaryObjectId.Should().Be(first.Id);
        }
        finally
        {
            InvokeNonPublicMethod(view, "ReleaseNativeHost");
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_NativeWin32CtrlClick_EmitsToggleSelectionRequest()
    {
        var nativeHostFactory = new TranslatingNativeHostFactory();
        var view = CreateInteractiveView(nativeHostFactory);
        try
        {
            var sceneObject = AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Select;
            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            var point = ProjectPoint(view, Vector3.Zero);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            nativeHostFactory.LastCreatedHost!.RaiseWin32Pointer(NativePointerKind.LeftDown, point, MkLButton | MkControl);
            nativeHostFactory.LastCreatedHost.RaiseWin32Pointer(NativePointerKind.LeftUp, point, MkControl);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Toggle);
            request.ObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            request.PrimaryObjectId.Should().Be(sceneObject.Id);
        }
        finally
        {
            InvokeNonPublicMethod(view, "ReleaseNativeHost");
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_NativeWin32ShiftDrag_EmitsAdditiveBoxSelectionRequest()
    {
        var nativeHostFactory = new TranslatingNativeHostFactory();
        var view = CreateInteractiveView(nativeHostFactory);
        try
        {
            var first = AddQuad(view, new Vector3(-1.1f, 0f, 0f));
            var second = AddQuad(view, new Vector3(1.1f, 0f, 0f));
            view.InteractionMode = VideraInteractionMode.Select;
            InvokeNonPublicMethod(view, "EnsureNativeHost");

            nativeHostFactory.LastCreatedHost.Should().NotBeNull();
            var firstPoint = ProjectPoint(view, first.WorldBounds!.Value.Center);
            var secondPoint = ProjectPoint(view, second.WorldBounds!.Value.Center);
            var start = new Point(Math.Min(firstPoint.X, secondPoint.X) - 30d, Math.Min(firstPoint.Y, secondPoint.Y) - 30d);
            var end = new Point(Math.Max(firstPoint.X, secondPoint.X) + 30d, Math.Max(firstPoint.Y, secondPoint.Y) + 30d);
            SelectionRequestedEventArgs? request = null;
            view.SelectionRequested += (_, e) => request = e;

            nativeHostFactory.LastCreatedHost!.RaiseWin32Pointer(NativePointerKind.LeftDown, start, MkLButton | MkShift);
            nativeHostFactory.LastCreatedHost.RaiseWin32Pointer(NativePointerKind.Move, end, MkLButton | MkShift);
            nativeHostFactory.LastCreatedHost.RaiseWin32Pointer(NativePointerKind.LeftUp, end, MkShift);

            request.Should().NotBeNull();
            request!.Operation.Should().Be(VideraSelectionOperation.Add);
            request.ObjectIds.Should().Equal(first.Id, second.Id);
            request.PrimaryObjectId.Should().Be(first.Id);
        }
        finally
        {
            InvokeNonPublicMethod(view, "ReleaseNativeHost");
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

    private static RoutedInteractionTestView CreateInteractiveView(INativeHostFactory? nativeHostFactory = null)
    {
        var view = new RoutedInteractionTestView(nativeHostFactory, bitmapFactory: static (_, _) => null);
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

    private static TopLevel CreateOffscreenTopLevel()
    {
        EnsureTopLevelLocatorServicesRegistered();

        var controlsAssembly = typeof(TopLevel).Assembly;
        var implType = controlsAssembly.GetType("Avalonia.Controls.Remote.RemoteServer+EmbeddableRemoteServerTopLevelImpl");
        var topLevelType = controlsAssembly.GetType("Avalonia.Controls.Embedding.Offscreen.OffscreenTopLevel");
        implType.Should().NotBeNull();
        topLevelType.Should().NotBeNull();

        var implConstructor = implType!.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, [typeof(IAvaloniaRemoteTransportConnection)], modifiers: null);
        implConstructor.Should().NotBeNull();
        var impl = implConstructor!.Invoke([new RemoteTransportConnectionStub()]);
        impl.Should().NotBeNull();

        var constructor = topLevelType!.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, [implType!], modifiers: null);
        constructor.Should().NotBeNull();

        var topLevel = constructor!.Invoke([impl]);
        topLevel.Should().BeAssignableTo<TopLevel>();
        return (TopLevel)topLevel!;
    }

    private static void EnsureTopLevelLocatorServicesRegistered()
    {
        var avaloniaAssembly = typeof(AvaloniaObject).Assembly;
        var locatorType = avaloniaAssembly.GetType("Avalonia.AvaloniaLocator");
        var renderTimerType = avaloniaAssembly.GetType("Avalonia.Rendering.IRenderTimer");
        var defaultRenderTimerType = avaloniaAssembly.GetType("Avalonia.Rendering.DefaultRenderTimer");
        var keyboardDeviceType = avaloniaAssembly.GetType("Avalonia.Input.IKeyboardDevice");
        var keyboardDeviceImplType = avaloniaAssembly.GetType("Avalonia.Input.KeyboardDevice");
        var mouseDeviceType = avaloniaAssembly.GetType("Avalonia.Input.IMouseDevice");
        var mouseDeviceImplType = avaloniaAssembly.GetType("Avalonia.Input.MouseDevice");
        locatorType.Should().NotBeNull();
        renderTimerType.Should().NotBeNull();
        defaultRenderTimerType.Should().NotBeNull();
        keyboardDeviceType.Should().NotBeNull();
        keyboardDeviceImplType.Should().NotBeNull();
        mouseDeviceType.Should().NotBeNull();
        mouseDeviceImplType.Should().NotBeNull();

        var currentResolver = locatorType!.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null);
        currentResolver.Should().NotBeNull();
        var getService = currentResolver!.GetType().GetMethod("GetService", BindingFlags.Public | BindingFlags.Instance);
        getService.Should().NotBeNull();

        var currentMutable = locatorType.GetProperty("CurrentMutable", BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null);
        currentMutable.Should().NotBeNull();
        var bindMethod = locatorType.GetMethod("Bind", BindingFlags.Public | BindingFlags.Instance);
        bindMethod.Should().NotBeNull();

        RegisterLocatorServiceIfMissing(
            currentResolver,
            currentMutable!,
            getService!,
            bindMethod!,
            renderTimerType!,
            () =>
            {
                var timerConstructor = defaultRenderTimerType!.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, [typeof(int)], modifiers: null);
                timerConstructor.Should().NotBeNull();
                return timerConstructor!.Invoke([60]);
            },
            defaultRenderTimerType!);

        RegisterLocatorServiceIfMissing(
            currentResolver,
            currentMutable!,
            getService!,
            bindMethod!,
            keyboardDeviceType!,
            () =>
            {
                var keyboardConstructor = keyboardDeviceImplType!.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, Type.EmptyTypes, modifiers: null);
                keyboardConstructor.Should().NotBeNull();
                return keyboardConstructor!.Invoke(Array.Empty<object>());
            },
            keyboardDeviceImplType!);

        RegisterLocatorServiceIfMissing(
            currentResolver,
            currentMutable!,
            getService!,
            bindMethod!,
            mouseDeviceType!,
            () =>
            {
                var mouseConstructor = mouseDeviceImplType!.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, [typeof(Pointer)], modifiers: null);
                mouseConstructor.Should().NotBeNull();
                return mouseConstructor!.Invoke([new Pointer(0, PointerType.Mouse, isPrimary: true)]);
            },
            mouseDeviceImplType!);
    }

    private static void RegisterLocatorServiceIfMissing(
        object currentResolver,
        object currentMutable,
        System.Reflection.MethodInfo getService,
        System.Reflection.MethodInfo bindMethod,
        Type serviceType,
        Func<object> createInstance,
        Type implementationType)
    {
        if (getService.Invoke(currentResolver, [serviceType]) is not null)
        {
            return;
        }

        var bindHelper = bindMethod.MakeGenericMethod(serviceType).Invoke(currentMutable, parameters: null);
        bindHelper.Should().NotBeNull();

        var instance = createInstance();
        var toConstantMethod = bindHelper!.GetType().GetMethod("ToConstant", BindingFlags.Public | BindingFlags.Instance);
        toConstantMethod.Should().NotBeNull();
        _ = toConstantMethod!.MakeGenericMethod(implementationType).Invoke(bindHelper, [instance]);
    }

    private static void RouteTopLevelPointerPressed(TopLevel topLevel, Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, KeyModifiers keyModifiers = KeyModifiers.None)
    {
        var properties = new PointerPointProperties(rawModifiers, updateKind);
        var args = new PointerPressedEventArgs(topLevel, pointer, topLevel, position, timestamp: 0, properties, keyModifiers, clickCount: 1);
        topLevel.RaiseEvent(args);
    }

    private static void RouteTopLevelPointerReleased(TopLevel topLevel, Pointer pointer, Point position, RawInputModifiers rawModifiers, PointerUpdateKind updateKind, MouseButton mouseButton, KeyModifiers keyModifiers = KeyModifiers.None)
    {
        var properties = new PointerPointProperties(rawModifiers, updateKind);
        var args = new PointerReleasedEventArgs(topLevel, pointer, topLevel, position, timestamp: 0, properties, keyModifiers, mouseButton);
        topLevel.RaiseEvent(args);
    }

    private static void InvokeNonPublicMethod(object target, string methodName)
    {
        var type = target.GetType();
        System.Reflection.MethodInfo? method = null;
        while (type is not null && method is null)
        {
            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            type = type.BaseType;
        }

        method.Should().NotBeNull($"method {methodName} should exist on {target.GetType().FullName}");
        method!.Invoke(target, Array.Empty<object>());
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

        public void AttachToVisualTree(TopLevel topLevel)
        {
            var args = new VisualTreeAttachmentEventArgs(topLevel, topLevel);
            base.OnAttachedToVisualTree(args);
        }

        public void DetachFromVisualTree(TopLevel topLevel)
        {
            var args = new VisualTreeAttachmentEventArgs(topLevel, topLevel);
            base.OnDetachedFromVisualTree(args);
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

    private sealed class RecordingNativeHostFactory : INativeHostFactory
    {
        public RecordingNativeHost? LastCreatedHost { get; private set; }

        public IVideraNativeHost? CreateHost()
        {
            LastCreatedHost = new RecordingNativeHost();
            return LastCreatedHost;
        }
    }

    private sealed class RecordingNativeHost : NativeControlHost, IVideraNativeHost
    {
        public event Action<IntPtr>? HandleCreated;
        public event Action? HandleDestroyed;
        public event Action<NativePointerEvent>? NativePointer;

        public void RaiseNativePointer(NativePointerEvent e)
        {
            NativePointer?.Invoke(e);
        }
    }

    private sealed class TranslatingNativeHostFactory : INativeHostFactory
    {
        public TranslatingNativeHost? LastCreatedHost { get; private set; }

        public IVideraNativeHost? CreateHost()
        {
            LastCreatedHost = new TranslatingNativeHost();
            return LastCreatedHost;
        }
    }

    private sealed class TranslatingNativeHost : NativeControlHost, IVideraNativeHost
    {
        private static readonly System.Reflection.MethodInfo RaisePointerMethod =
            typeof(VideraNativeHost).GetMethod(
                "RaisePointer",
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                [typeof(NativePointerKind), typeof(IntPtr), typeof(IntPtr), typeof(int)],
                modifiers: null)!;

        private readonly VideraNativeHost _translator = new();

        public TranslatingNativeHost()
        {
            _translator.NativePointer += OnTranslatedPointer;
        }

        public event Action<IntPtr>? HandleCreated;
        public event Action? HandleDestroyed;
        public event Action<NativePointerEvent>? NativePointer;

        public void RaiseWin32Pointer(NativePointerKind kind, Point position, int wParamState, int wheelDelta = 0)
        {
            RaisePointerMethod.Invoke(_translator, [kind, new IntPtr(wParamState), CreateLParam(position), wheelDelta]);
        }

        private void OnTranslatedPointer(NativePointerEvent e)
        {
            NativePointer?.Invoke(e);
        }

        private static IntPtr CreateLParam(Point position)
        {
            var x = unchecked((ushort)(short)Math.Round(position.X));
            var y = unchecked((ushort)(short)Math.Round(position.Y));
            var value = (y << 16) | x;
            return new IntPtr(value);
        }
    }

    private sealed class RemoteTransportConnectionStub : IAvaloniaRemoteTransportConnection
    {
        public event Action<IAvaloniaRemoteTransportConnection, object>? OnMessage;
        public event Action<IAvaloniaRemoteTransportConnection, Exception>? OnException;

        public Task Send(object data)
        {
            _ = data;
            return Task.CompletedTask;
        }

        public void Start()
        {
        }

        public void Dispose()
        {
        }
    }
}
