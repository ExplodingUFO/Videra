using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraEngineExtensibilityIntegrationTests
{
    [Fact]
    public void RegisterPassContributor_AllowsCustomContributorToObserveSolidGeometryPass()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        RenderPassSlot? observedSlot = null;
        RenderFeatureSet observedActiveFeatures = RenderFeatureSet.None;
        RenderFeatureSet observedSlotFeatures = RenderFeatureSet.None;
        engine.RegisterPassContributor(
            RenderPassSlot.SolidGeometry,
            new RecordingContributor(context =>
            {
                observedSlot = context.Slot;
                observedActiveFeatures = context.ActiveFeatures;
                observedSlotFeatures = context.SlotFeatures;
            }));

        engine.Draw();

        observedSlot.Should().Be(RenderPassSlot.SolidGeometry);
        observedActiveFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Overlay);
        observedSlotFeatures.Should().Be(RenderFeatureSet.Opaque);
    }

    [Fact]
    public void RegisterPassContributor_ExposesOverlayTruthThroughContributionContext()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        var sceneObject = DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory());
        engine.AddObject(sceneObject);
        engine.SetSelectionOverlayState(new SelectionOverlayRenderState(
            selectedObjectIds: [sceneObject.Id],
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

        SelectionOverlayRenderState? observedSelection = null;
        AnnotationOverlayRenderState? observedAnnotation = null;
        engine.RegisterPassContributor(
            RenderPassSlot.Wireframe,
            new RecordingContributor(context =>
            {
                observedSelection = context.SelectionOverlay;
                observedAnnotation = context.AnnotationOverlay;
            }));

        engine.Draw();

        observedSelection.Should().NotBeNull();
        observedSelection!.SelectedObjectIds.Should().ContainSingle().Which.Should().Be(sceneObject.Id);
        observedSelection.HoverObjectId.Should().Be(sceneObject.Id);
        observedAnnotation.Should().NotBeNull();
        observedAnnotation!.Anchors.Should().ContainSingle();
        observedAnnotation.Anchors[0].Anchor.Should().Be(AnnotationAnchorDescriptor.ForObject(sceneObject.Id));
    }

    [Fact]
    public void ReplacePassContributor_AllowsBuiltInSolidGeometryContributorToBeReplaced()
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
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        engine.ReplacePassContributor(RenderPassSlot.SolidGeometry, new NoOpContributor());

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Blue).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ReplacePassContributor_ForWireframe_SuppressesBuiltInOverlayContributors()
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
        engine.AddObject(sceneObject);
        engine.SetSelectionOverlayState(new SelectionOverlayRenderState(
            selectedObjectIds: [sceneObject.Id],
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
        engine.ReplacePassContributor(RenderPassSlot.Wireframe, new NoOpContributor());

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Black).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Green).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Red).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().BeGreaterThan(0);
    }

    [Fact]
    public void RegisterFrameHook_RunsHooksAtStableHookPointsInDeterministicOrder()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        var hookOrder = new List<string>();
        var hookFeatures = new List<RenderFeatureSet>();
        engine.RegisterFrameHook(RenderFrameHookPoint.FrameBegin, context =>
        {
            hookOrder.Add(context.HookPoint.ToString());
            hookFeatures.Add(context.ActiveFeatures);
        });
        engine.RegisterFrameHook(RenderFrameHookPoint.SceneSubmit, context =>
        {
            hookOrder.Add(context.HookPoint.ToString());
            hookFeatures.Add(context.ActiveFeatures);
        });
        engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context =>
        {
            hookOrder.Add(context.HookPoint.ToString());
            hookFeatures.Add(context.ActiveFeatures);
        });

        engine.Draw();

        hookOrder.Should().Equal("FrameBegin", "SceneSubmit", "FrameEnd");
        hookFeatures.Should().OnlyContain(static features => features == (RenderFeatureSet.Opaque | RenderFeatureSet.Overlay));
    }

    [Fact]
    public void GetRenderCapabilities_ExposesPipelineAndBackendTruthAfterRender()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        engine.Draw();
        var capabilities = engine.GetRenderCapabilities();

        capabilities.IsInitialized.Should().BeTrue();
        capabilities.ActiveBackendPreference.Should().Be(GraphicsBackendPreference.Software);
        capabilities.SupportsPassContributors.Should().BeTrue();
        capabilities.SupportsPassReplacement.Should().BeTrue();
        capabilities.SupportsFrameHooks.Should().BeTrue();
        capabilities.SupportsPipelineSnapshots.Should().BeTrue();
        capabilities.SupportedFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Transparent | RenderFeatureSet.Overlay);
        capabilities.SupportedFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay");
        capabilities.LastPipelineSnapshot.Should().NotBeNull();
        capabilities.LastPipelineSnapshot!.StageNames.Should().Contain("PrepareFrame");
        capabilities.LastPipelineSnapshot.StageNames.Should().Contain("PresentFrame");
    }

    [Fact]
    public void DisposedOverlayContributors_AreIgnoredHarmlessly()
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
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        using var selectionContributor = new SelectionOverlayContributor();
        using var annotationContributor = new AnnotationAnchorOverlayContributor();
        selectionContributor.Dispose();
        annotationContributor.Dispose();
        engine.RegisterPassContributor(RenderPassSlot.Wireframe, selectionContributor);
        engine.RegisterPassContributor(RenderPassSlot.Wireframe, annotationContributor);

        var act = () => engine.Draw();

        act.Should().NotThrow();
        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Red).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Green).Should().Be(0);
    }

    [Fact]
    public void DisposedEngine_IgnoresAdditionalRegistrations_AndKeepsCapabilitiesQueryable()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        engine.Dispose();

        var act = () =>
        {
            engine.RegisterPassContributor(RenderPassSlot.SolidGeometry, new NoOpContributor());
            engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, _ => { });
        };

        act.Should().NotThrow();

        var capabilities = engine.GetRenderCapabilities();
        capabilities.IsInitialized.Should().BeFalse();
        capabilities.SupportsPassContributors.Should().BeTrue();
        capabilities.SupportsPassReplacement.Should().BeTrue();
        capabilities.SupportsFrameHooks.Should().BeTrue();
        capabilities.SupportsPipelineSnapshots.Should().BeTrue();
        capabilities.SupportedFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Transparent | RenderFeatureSet.Overlay);
        capabilities.SupportedFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay");
        capabilities.LastPipelineSnapshot.Should().BeNull();
    }

    [Fact]
    public void GetRenderCapabilities_RemainsCallableAfterDispose_AndRetainsLastPipelineSnapshot()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        engine.Draw();
        engine.Dispose();

        var capabilities = engine.GetRenderCapabilities();

        capabilities.IsInitialized.Should().BeFalse();
        capabilities.SupportsPassContributors.Should().BeTrue();
        capabilities.SupportsPassReplacement.Should().BeTrue();
        capabilities.SupportsFrameHooks.Should().BeTrue();
        capabilities.SupportsPipelineSnapshots.Should().BeTrue();
        capabilities.SupportedFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Transparent | RenderFeatureSet.Overlay);
        capabilities.SupportedFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay");
        capabilities.LastPipelineSnapshot.Should().NotBeNull();
        capabilities.LastPipelineSnapshot!.StageNames.Should().Contain("PrepareFrame");
        capabilities.LastPipelineSnapshot.StageNames.Should().Contain("PresentFrame");
    }

    private sealed class RecordingContributor(Action<RenderPassContributionContext> onContribute) : IRenderPassContributor
    {
        public void Contribute(RenderPassContributionContext context)
        {
            onContribute(context);
        }
    }

    private sealed class NoOpContributor : IRenderPassContributor
    {
        public void Contribute(RenderPassContributionContext context)
        {
            _ = context;
        }
    }
}
