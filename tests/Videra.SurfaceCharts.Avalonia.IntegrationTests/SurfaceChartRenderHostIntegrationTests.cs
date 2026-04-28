using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartRenderHostIntegrationTests
{
    [Fact]
    public void VideraChartView_DefaultRenderSnapshot_IsSoftwareAndNotReady()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.RenderSnapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderSnapshot.IsReady.Should().BeFalse();
            view.RenderSnapshot.IsFallback.Should().BeFalse();
            view.RenderSnapshot.UsesNativeSurface.Should().BeFalse();
            view.RenderSnapshot.ResidentTileCount.Should().Be(0);
        });
    }

    [Fact]
    public Task VideraChartView_LoadedTiles_PublishSoftwareSnapshotFromRenderHost()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new VideraChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            view.RenderSnapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderSnapshot.IsReady.Should().BeTrue();
            view.RenderSnapshot.IsFallback.Should().BeFalse();
            view.RenderSnapshot.UsesNativeSurface.Should().BeFalse();
            view.RenderSnapshot.ResidentTileCount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public Task SurfaceChartRuntime_ViewStateChangesStillPublishRenderHostSnapshot()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new VideraChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            var runtime = SurfaceChartTestHelpers.GetRuntime(view);
            runtime.UpdateViewState(SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(256, 128, 256, 256)));

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            runtime.CurrentViewport.Should().Be(new SurfaceViewport(256, 128, 256, 256));
            view.RenderSnapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderSnapshot.IsReady.Should().BeTrue();
            view.RenderSnapshot.ResidentTileCount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public Task VideraChartView_LoadedTiles_PublishViewStateAndCameraFrameToRenderHost()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new VideraChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            var renderHost = (SurfaceChartRenderHost)typeof(VideraChartView)
                .GetField("_renderHost", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            var runtime = SurfaceChartTestHelpers.GetRuntime(view);

            renderHost.Inputs.ViewState.Should().Be(runtime.ViewState);
            renderHost.Inputs.CameraFrame.Should().NotBeNull();
            renderHost.Inputs.CameraFrame!.Value.ProjectionSettings.Should().Be(runtime.ViewState.Camera.ToProjectionSettings());
        });
    }
}
