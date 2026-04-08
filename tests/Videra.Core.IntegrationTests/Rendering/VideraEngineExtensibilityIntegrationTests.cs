using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Software;
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

        var observedSlots = new List<string>();
        engine.RegisterPassContributor(
            RenderPassSlot.SolidGeometry,
            new RecordingContributor(context => observedSlots.Add(context.Slot.ToString())));

        engine.Draw();

        observedSlots.Should().ContainSingle().Which.Should().Be("SolidGeometry");
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
    public void RegisterFrameHook_RunsHooksAtStableHookPointsInDeterministicOrder()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        var hookOrder = new List<string>();
        engine.RegisterFrameHook(RenderFrameHookPoint.FrameBegin, context => hookOrder.Add(context.HookPoint.ToString()));
        engine.RegisterFrameHook(RenderFrameHookPoint.SceneSubmit, context => hookOrder.Add(context.HookPoint.ToString()));
        engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context => hookOrder.Add(context.HookPoint.ToString()));

        engine.Draw();

        hookOrder.Should().Equal("FrameBegin", "SceneSubmit", "FrameEnd");
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
        capabilities.LastPipelineSnapshot.Should().NotBeNull();
        capabilities.LastPipelineSnapshot!.StageNames.Should().Contain("PrepareFrame");
        capabilities.LastPipelineSnapshot.StageNames.Should().Contain("PresentFrame");
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
