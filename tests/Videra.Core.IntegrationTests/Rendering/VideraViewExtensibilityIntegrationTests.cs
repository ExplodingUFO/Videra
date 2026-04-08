using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewExtensibilityIntegrationTests
{
    [Fact]
    public void VideraView_ExposesPublicEngineExtensibilityAndCapabilityQuery()
    {
        var view = new VideraView();
        try
        {
            using var backend = new SoftwareBackend();
            backend.Initialize(IntPtr.Zero, 200, 200);

            var observed = new List<string>();
            view.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameBegin, context => observed.Add(context.HookPoint.ToString()));
            view.Engine.RegisterPassContributor(
                RenderPassSlot.SolidGeometry,
                new RecordingContributor(context => observed.Add(context.Slot.ToString())));

            view.Engine.Initialize(backend);
            view.Engine.Resize(200, 200);
            view.Engine.Grid.IsVisible = false;
            view.Engine.ShowAxis = false;
            view.Engine.AddObject(DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory()));

            view.Engine.Draw();

            observed.Should().Equal("FrameBegin", "SolidGeometry");
            view.RenderCapabilities.SupportsPassContributors.Should().BeTrue();
            view.RenderCapabilities.SupportsPassReplacement.Should().BeTrue();
            view.RenderCapabilities.SupportsFrameHooks.Should().BeTrue();
            view.RenderCapabilities.SupportsPipelineSnapshots.Should().BeTrue();
            view.RenderCapabilities.ActiveBackendPreference.Should().Be(GraphicsBackendPreference.Software);
            view.RenderCapabilities.LastPipelineSnapshot.Should().NotBeNull();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void BackendDiagnostics_ExposeCapabilityProjectionFields()
    {
        var view = new VideraView();
        try
        {
            view.BackendDiagnostics.SupportsPassContributors.Should().BeTrue();
            view.BackendDiagnostics.SupportsPassReplacement.Should().BeTrue();
            view.BackendDiagnostics.SupportsFrameHooks.Should().BeTrue();
            view.BackendDiagnostics.SupportsPipelineSnapshots.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void RenderCapabilities_AndBackendDiagnostics_AreQueryableBeforeInitialization()
    {
        var view = new VideraView();
        try
        {
            view.RenderCapabilities.IsInitialized.Should().BeFalse();
            view.RenderCapabilities.SupportsPassContributors.Should().BeTrue();
            view.RenderCapabilities.SupportsPassReplacement.Should().BeTrue();
            view.RenderCapabilities.SupportsFrameHooks.Should().BeTrue();
            view.RenderCapabilities.SupportsPipelineSnapshots.Should().BeTrue();
            view.RenderCapabilities.LastPipelineSnapshot.Should().BeNull();

            view.BackendDiagnostics.IsReady.Should().BeFalse();
            view.BackendDiagnostics.IsUsingSoftwareFallback.Should().BeFalse();
            view.BackendDiagnostics.FallbackReason.Should().BeNull();
            view.BackendDiagnostics.SupportsPassContributors.Should().BeTrue();
            view.BackendDiagnostics.SupportsPassReplacement.Should().BeTrue();
            view.BackendDiagnostics.SupportsFrameHooks.Should().BeTrue();
            view.BackendDiagnostics.SupportsPipelineSnapshots.Should().BeTrue();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    private sealed class RecordingContributor(Action<RenderPassContributionContext> onContribute) : IRenderPassContributor
    {
        public void Contribute(RenderPassContributionContext context)
        {
            onContribute(context);
        }
    }
}
