using FluentAssertions;
using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.Software;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Presets;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraEnginePipelineContractTests
{
    [Fact]
    public void Draw_CapturesStandardPipelineSnapshot()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.Profile.Should().Be(RenderPipelineProfile.Standard);
        engine.LastPipelineSnapshot.ActiveFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Overlay);
        engine.LastPipelineSnapshot.FeatureNames.Should().Equal("Opaque", "Overlay");
        engine.LastPipelineSnapshot.FrameObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.OpaqueObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.TransparentObjectCount.Should().Be(0);
        engine.LastPipelineSnapshot.Stages.Should().Equal(
            RenderPipelineStage.PrepareFrame,
            RenderPipelineStage.BindSharedFrameState,
            RenderPipelineStage.GridPass,
            RenderPipelineStage.SolidGeometryPass,
            RenderPipelineStage.AxisPass,
            RenderPipelineStage.PresentFrame);
        engine.LastPipelineSnapshot.Stages.Should().NotContain(RenderPipelineStage.WireframePass);
    }

    [Fact]
    public void Draw_WithExplicitWireframeOverlay_CapturesWireframePass()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));
        engine.Wireframe.Mode = WireframeMode.AllEdges;

        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.Profile.Should().Be(RenderPipelineProfile.StandardWithWireframeOverlay);
        engine.LastPipelineSnapshot.ActiveFeatures.Should().Be(RenderFeatureSet.Opaque | RenderFeatureSet.Overlay);
        engine.LastPipelineSnapshot.Stages.Should().ContainInOrder(
            RenderPipelineStage.SolidGeometryPass,
            RenderPipelineStage.WireframePass,
            RenderPipelineStage.AxisPass,
            RenderPipelineStage.PresentFrame);
    }

    [Fact]
    public void Draw_WithStyleDrivenWireframeOnly_CapturesWireframeOnlyProfile()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));
        engine.StyleService.ApplyPreset(RenderStylePreset.Wireframe);

        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.Profile.Should().Be(RenderPipelineProfile.WireframeOnly);
        engine.LastPipelineSnapshot.ActiveFeatures.Should().Be(RenderFeatureSet.Overlay);
        engine.LastPipelineSnapshot.FeatureNames.Should().Equal("Overlay");
        engine.LastPipelineSnapshot.EffectiveWireframeMode.Should().Be(WireframeMode.WireframeOnly);
        engine.LastPipelineSnapshot.FrameObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.OpaqueObjectCount.Should().Be(0);
        engine.LastPipelineSnapshot.TransparentObjectCount.Should().Be(0);
        engine.LastPipelineSnapshot.Stages.Should().Contain(RenderPipelineStage.WireframePass);
        engine.LastPipelineSnapshot.Stages.Should().NotContain(RenderPipelineStage.SolidGeometryPass);
    }

    [Fact]
    public void Draw_WithTransparentGeometry_CapturesTransparentFeature()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;
        engine.AddObject(DemoMeshFactory.CreateBlendedQuad(new RgbaFloat(1f, 0f, 0f, 0.5f), Vector3.Zero));

        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.ActiveFeatures.Should().Be(RenderFeatureSet.Transparent);
        engine.LastPipelineSnapshot.FeatureNames.Should().Equal("Transparent");
        engine.LastPipelineSnapshot.FrameObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.OpaqueObjectCount.Should().Be(0);
        engine.LastPipelineSnapshot.TransparentObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.Stages.Should().Contain(RenderPipelineStage.SolidGeometryPass);
    }

    [Fact]
    public void Draw_WithDeferredTransparentGeometry_DoesNotCountSkippedObjects()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;
        engine.AddObject(DemoMeshFactory.CreateBlendedQuad(new RgbaFloat(1f, 0f, 0f, 0.5f), Vector3.Zero), uploadIfPossible: false);

        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.ActiveFeatures.Should().Be(RenderFeatureSet.Transparent);
        engine.LastPipelineSnapshot.FrameObjectCount.Should().Be(1);
        engine.LastPipelineSnapshot.OpaqueObjectCount.Should().Be(0);
        engine.LastPipelineSnapshot.TransparentObjectCount.Should().Be(0);
    }
}
