using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Tests.Scene;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Avalonia.Tests.Interaction;

public sealed class VideraInteractionControllerTests
{
    [Fact]
    public void MeasureMode_TwoResolvedClicks_AppendsMeasurement()
    {
        using var engine = new VideraEngine();
        var sceneObject = SceneTestMeshes.CreateDeferredObject();
        engine.AddObject(sceneObject);
        engine.Camera.SetOrbit(new Vector3(0.5f, 0.5f, 0f), 5f, 0f, 0f);
        engine.Camera.UpdateProjection(200, 200);

        var host = new FakeInteractionHost(engine)
        {
            InteractionMode = VideraInteractionMode.Measure
        };
        using var controller = new VideraInteractionController(host, NullLogger.Instance);

        engine.Camera.TryProjectWorldPoint(new Vector3(0.5f, 0.5f, 0f), new Vector2(200f, 200f), out var center).Should().BeTrue();

        controller.HandlePressed(
            new VideraPointerGestureSnapshot(new Point(center.X, center.Y), RawInputModifiers.LeftMouseButton, true, false, MouseButton.Left, 0f),
            VideraPointerRoute.View).Should().BeTrue();
        controller.HandleReleased(
            new VideraPointerGestureSnapshot(new Point(center.X, center.Y), RawInputModifiers.None, false, false, MouseButton.Left, 0f),
            VideraPointerRoute.View).Should().BeTrue();

        controller.HandlePressed(
            new VideraPointerGestureSnapshot(new Point(18d, 18d), RawInputModifiers.LeftMouseButton, true, false, MouseButton.Left, 0f),
            VideraPointerRoute.View).Should().BeTrue();
        controller.HandleReleased(
            new VideraPointerGestureSnapshot(new Point(18d, 18d), RawInputModifiers.None, false, false, MouseButton.Left, 0f),
            VideraPointerRoute.View).Should().BeTrue();

        host.Measurements.Should().ContainSingle();
        host.Measurements[0].Distance.Should().BeGreaterThan(0f);
    }

    private sealed class FakeInteractionHost : IVideraInteractionHost
    {
        private IReadOnlyList<VideraMeasurement> _measurements = Array.Empty<VideraMeasurement>();

        public FakeInteractionHost(VideraEngine engine)
        {
            Engine = engine;
            PointerCaptureTarget = new Border();
        }

        public VideraEngine Engine { get; }

        public VideraInteractionMode InteractionMode { get; set; } = VideraInteractionMode.Navigate;

        public VideraInteractionOptions InteractionOptions { get; } = new();

        public IReadOnlyList<Object3D> SceneObjects => Engine.SceneObjects;

        public IReadOnlyList<VideraMeasurement> Measurements
        {
            get => _measurements;
            set => _measurements = value ?? Array.Empty<VideraMeasurement>();
        }

        public IInputElement PointerCaptureTarget { get; }

        public TopLevel? ResolveTopLevel() => null;

        public Vector2 GetInteractionViewportSize() => new(200f, 200f);

        public bool IsPointWithinHost(Point position) => position.X >= 0d && position.Y >= 0d && position.X <= 200d && position.Y <= 200d;

        public void FocusHost()
        {
        }

        public void RaiseSelectionRequested(SelectionRequestedEventArgs e)
        {
            _ = e;
        }

        public void RaiseAnnotationRequested(AnnotationRequestedEventArgs e)
        {
            _ = e;
        }

        public InteractiveFrameLease BeginInteractiveFrameLease()
        {
            return new InteractiveFrameLease(() => { });
        }

        public void InvalidateRender(RenderInvalidationKinds flags)
        {
            _ = flags;
        }
    }
}
