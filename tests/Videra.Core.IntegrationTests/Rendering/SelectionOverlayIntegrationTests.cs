using System.Numerics;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class SelectionOverlayIntegrationTests
{
    [Fact]
    public void SelectedObjectOverlay_RendersStableHighlightWithoutGlobalWireframe()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var selectedObject = DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory());
        engine.AddObject(selectedObject);
        engine.SetSelectionOverlayState(new SelectionOverlayRenderState(
            selectedObjectIds: [selectedObject.Id],
            hoverObjectId: null,
            selectedLineColor: RgbaFloat.Black,
            hoverLineColor: RgbaFloat.Green));

        engine.Draw();
        var firstFrame = DemoMeshFactory.CaptureFrame(backend);
        engine.Draw();
        var secondFrame = DemoMeshFactory.CaptureFrame(backend);

        DemoMeshFactory.CountPixels(firstFrame, DemoMeshFactory.PixelColor.Black).Should().BeGreaterThan(0);
        DemoMeshFactory.CountPixels(firstFrame, DemoMeshFactory.PixelColor.White).Should().BeGreaterThan(0);
        firstFrame.Should().Equal(secondFrame);
        engine.LastPipelineSnapshot!.StageNames.Should().Contain("WireframePass");
    }

    [Fact]
    public void HoverAndAnchorOverlayMarkers_RenderWithoutMutatingSceneState()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var sceneObject = DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory());
        var originalBounds = sceneObject.WorldBounds;
        var originalPosition = sceneObject.Position;
        engine.AddObject(sceneObject);

        engine.SetSelectionOverlayState(new SelectionOverlayRenderState(
            selectedObjectIds: Array.Empty<Guid>(),
            hoverObjectId: sceneObject.Id,
            selectedLineColor: RgbaFloat.Black,
            hoverLineColor: RgbaFloat.Green));
        engine.SetAnnotationOverlayState(new AnnotationOverlayRenderState(
            anchors:
            [
                new AnnotationOverlayAnchor(Guid.NewGuid(), AnnotationAnchorDescriptor.ForObject(sceneObject.Id))
            ],
            markerColor: RgbaFloat.Red,
            markerWorldSize: 0.08f));

        engine.Draw();
        var frame = DemoMeshFactory.CaptureFrame(backend);

        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Green).Should().BeGreaterThan(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Red).Should().BeGreaterThan(0);
        engine.SceneObjects.Should().ContainSingle().Which.Should().BeSameAs(sceneObject);
        sceneObject.WorldBounds.Should().Be(originalBounds);
        sceneObject.Position.Should().Be(originalPosition);
    }

    [Fact]
    public void SessionBridge_CanDerive2DOverlayStateFromProjectedAnchors()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new SoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.Software
        };
        var bridge = new VideraViewSessionBridge(
            session,
            isPreferredBackendOverrideSet: static () => false,
            preferredBackendValue: static () => GraphicsBackendPreference.Auto,
            backendOptionsAccessor: () => backendOptions,
            diagnosticsOptionsAccessor: static () => new VideraDiagnosticsOptions());

        bridge.OnSizeChanged(200, 200, 1f);

        var sceneObject = DemoMeshFactory.CreateWhiteQuad(session.ResourceFactory!);
        engine.AddObject(sceneObject);

        var selectionState = new VideraSelectionState
        {
            ObjectIds = [sceneObject.Id],
            PrimaryObjectId = sceneObject.Id
        };
        var annotation = new VideraNodeAnnotation
        {
            Id = Guid.NewGuid(),
            Text = "Selected",
            ObjectId = sceneObject.Id
        };

        var overlay = bridge.CreateOverlayState(
            selectionState,
            [annotation],
            viewportSize: new Vector2(200f, 200f));

        overlay.SelectionOutlines.Should().ContainSingle();
        overlay.SelectionOutlines[0].ScreenBounds.Width.Should().BeGreaterThan(0d);
        overlay.SelectionOutlines[0].ScreenBounds.Height.Should().BeGreaterThan(0d);
        overlay.Labels.Should().ContainSingle();
        overlay.Labels[0].AnnotationId.Should().Be(annotation.Id);
        overlay.Labels[0].Text.Should().Be("Selected");
        overlay.Labels[0].ScreenPosition.X.Should().BeGreaterThan(0f);
        overlay.Labels[0].ScreenPosition.Y.Should().BeGreaterThan(0f);
    }
}
