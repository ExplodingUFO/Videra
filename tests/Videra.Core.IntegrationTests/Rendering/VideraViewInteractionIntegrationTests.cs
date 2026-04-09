using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Input;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewInteractionIntegrationTests
{
    [Fact]
    public void NavigateMode_RoutesCameraGesturesThroughInteractionController()
    {
        var view = CreateInteractiveView();
        try
        {
            view.InteractionMode = VideraInteractionMode.Navigate;
            var controller = ReadInteractionController(view);
            var route = ParsePointerRoute("View");
            var yawBefore = view.Engine.Camera.Yaw;
            var pitchBefore = view.Engine.Camera.Pitch;
            var radiusBefore = view.Engine.Camera.Radius;

            InvokeController(controller, "HandlePressed", CreatePointerSnapshot(new Point(100, 100), isLeftButtonPressed: true), route, new object());
            InvokeController(controller, "HandleMoved", CreatePointerSnapshot(new Point(132, 118), isLeftButtonPressed: true), route, new object());
            InvokeController(controller, "HandleReleased", CreatePointerSnapshot(new Point(132, 118), initialPressMouseButton: MouseButton.Left), route, new object());
            InvokeController(controller, "HandleWheel", CreatePointerSnapshot(new Point(132, 118), wheelDeltaY: 1f), route, new object());

            view.Engine.Camera.Yaw.Should().NotBeApproximately(yawBefore, 0.0001f);
            view.Engine.Camera.Pitch.Should().NotBeApproximately(pitchBefore, 0.0001f);
            view.Engine.Camera.Radius.Should().BeLessThan(radiusBefore);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void Controller_DeduplicatesTopLevelAndOverlayPointerPaths()
    {
        var view = CreateInteractiveView();
        try
        {
            var controller = ReadInteractionController(view);
            var wheelSnapshot = CreatePointerSnapshot(new Point(100, 100), wheelDeltaY: 1f);
            var token = new object();
            var radiusBefore = view.Engine.Camera.Radius;

            InvokeController(controller, "HandleWheel", wheelSnapshot, ParsePointerRoute("TopLevel"), token);
            InvokeController(controller, "HandleWheel", wheelSnapshot, ParsePointerRoute("Overlay"), token);

            view.Engine.Camera.Radius.Should().BeApproximately(radiusBefore - 0.25f, 0.0001f);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void SelectMode_EmitsSelectionIntent_WithoutMutatingHostOwnedSelectionState()
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

            SelectionRequestedEventArgs? requested = null;
            view.SelectionRequested += (_, e) => requested = e;

            var controller = ReadInteractionController(view);
            var route = ParsePointerRoute("View");
            var targetPoint = ProjectPoint(view, Vector3.Zero);

            InvokeController(controller, "HandlePressed", CreatePointerSnapshot(targetPoint, isLeftButtonPressed: true), route, new object());
            InvokeController(controller, "HandleReleased", CreatePointerSnapshot(targetPoint, initialPressMouseButton: MouseButton.Left), route, new object());

            requested.Should().NotBeNull();
            requested!.ObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
            requested.PrimaryObjectId.Should().Be(sceneObject.Id);
            view.SelectionState.ObjectIds.Should().ContainSingle().Which.Should().Be(originalSelectionId);
            view.SelectionState.PrimaryObjectId.Should().Be(originalSelectionId);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void AnnotateMode_EmitsObjectAndWorldPointAnnotationRequests()
    {
        var view = CreateInteractiveView();
        try
        {
            var sceneObject = AddCenteredQuad(view);
            view.InteractionMode = VideraInteractionMode.Annotate;

            var requests = new List<AnnotationRequestedEventArgs>();
            view.AnnotationRequested += (_, e) => requests.Add(e);

            var controller = ReadInteractionController(view);
            var route = ParsePointerRoute("View");
            var objectPoint = ProjectPoint(view, Vector3.Zero);
            var emptyPoint = new Point(12, 12);

            InvokeController(controller, "HandlePressed", CreatePointerSnapshot(objectPoint, isLeftButtonPressed: true), route, new object());
            InvokeController(controller, "HandleReleased", CreatePointerSnapshot(objectPoint, initialPressMouseButton: MouseButton.Left), route, new object());
            InvokeController(controller, "HandlePressed", CreatePointerSnapshot(emptyPoint, isLeftButtonPressed: true), route, new object());
            InvokeController(controller, "HandleReleased", CreatePointerSnapshot(emptyPoint, initialPressMouseButton: MouseButton.Left), route, new object());

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
    public void Controller_Dispose_IsIdempotent_AndStopsFurtherInteractionRouting()
    {
        var view = CreateInteractiveView();
        try
        {
            var controller = ReadInteractionController(view);
            var radiusBefore = view.Engine.Camera.Radius;
            var disposeMethod = controller.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public);

            disposeMethod.Should().NotBeNull();
            disposeMethod!.Invoke(controller, Array.Empty<object>());
            disposeMethod.Invoke(controller, Array.Empty<object>());

            InvokeController(controller, "HandleWheel", CreatePointerSnapshot(new Point(100, 100), wheelDeltaY: 1f), ParsePointerRoute("View"), new object());

            view.Engine.Camera.Radius.Should().BeApproximately(radiusBefore, 0.0001f);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    private static VideraView CreateInteractiveView()
    {
        var view = new VideraView(nativeHostFactory: null, bitmapFactory: static (_, _) => null);
        view.Measure(new Size(200, 200));
        view.Arrange(new Rect(0, 0, 200, 200));

        var renderSession = ReadPrivateField<RenderSession>(view, "_renderSession");
        renderSession.Attach(GraphicsBackendPreference.Software);
        renderSession.Resize(200, 200, 1f);

        view.Engine.Camera.SetOrbit(Vector3.Zero, 5f, 0f, 0f);
        view.Engine.Camera.UpdateProjection(200, 200);
        return view;
    }

    private static Object3D AddCenteredQuad(VideraView view)
    {
        var renderSession = ReadPrivateField<RenderSession>(view, "_renderSession");
        renderSession.ResourceFactory.Should().NotBeNull();
        var sceneObject = DemoMeshFactory.CreateWhiteQuad(renderSession.ResourceFactory!);
        view.AddObject(sceneObject);
        return sceneObject;
    }

    private static Point ProjectPoint(VideraView view, Vector3 worldPoint)
    {
        view.Engine.Camera.TryProjectWorldPoint(worldPoint, new Vector2(200f, 200f), out var screenPoint).Should().BeTrue();
        return new Point(screenPoint.X, screenPoint.Y);
    }

    private static object ReadInteractionController(VideraView view)
    {
        var field = typeof(VideraView).GetField("_interactionController", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull("Task 5 extracts VideraView input state into a dedicated interaction controller");

        var controller = field!.GetValue(view);
        controller.Should().NotBeNull();
        return controller!;
    }

    private static object CreatePointerSnapshot(
        Point position,
        RawInputModifiers modifiers = RawInputModifiers.None,
        bool isLeftButtonPressed = false,
        bool isRightButtonPressed = false,
        MouseButton initialPressMouseButton = MouseButton.Left,
        float wheelDeltaY = 0f)
    {
        var snapshotType = GetInteractionType("VideraPointerGestureSnapshot");
        var snapshot = Activator.CreateInstance(
            snapshotType,
            position,
            modifiers,
            isLeftButtonPressed,
            isRightButtonPressed,
            initialPressMouseButton,
            wheelDeltaY);

        snapshot.Should().NotBeNull();
        return snapshot!;
    }

    private static object ParsePointerRoute(string routeName)
    {
        var routeType = GetInteractionType("VideraPointerRoute");
        return Enum.Parse(routeType, routeName);
    }

    private static void InvokeController(object controller, string methodName, object snapshot, object route, object token)
    {
        var method = controller.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [snapshot.GetType(), route.GetType(), typeof(object)],
            modifiers: null);

        method.Should().NotBeNull($"controller method {methodName} should exist for the normalized interaction path");
        method!.Invoke(controller, [snapshot, route, token]);
    }

    private static Type GetInteractionType(string name)
    {
        var type = typeof(VideraView).Assembly.GetType($"Videra.Avalonia.Controls.Interaction.{name}");
        type.Should().NotBeNull($"interaction type {name} should exist for the extracted controller boundary");
        return type!;
    }

    private static T ReadPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull($"field {fieldName} should exist on {instance.GetType().FullName}");

        var value = field!.GetValue(instance);
        value.Should().BeAssignableTo<T>();
        return (T)value!;
    }
}
