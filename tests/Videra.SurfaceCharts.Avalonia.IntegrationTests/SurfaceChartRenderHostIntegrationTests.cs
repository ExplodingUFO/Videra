using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;
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
            SurfaceChartTestHelpers.LoadSurface(view, source);

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
            SurfaceChartTestHelpers.LoadSurface(view, source);

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
            SurfaceChartTestHelpers.LoadSurface(view, source);

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

    [Fact]
    public void SurfaceChartRenderHost_GpuResolutionFailure_PublishesNotReadyDiagnosticInsteadOfSoftwareDownshift()
    {
        var metadata = VideraChartViewLifecycleTests.CreateMetadata();
        var tile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 6f);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: null);

        var snapshot = host.UpdateInputs(new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = new SurfaceColorMap(metadata.ValueRange, SurfaceColorMapPresets.CreateDefault()),
            ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceViewport(0, 0, metadata.Width, metadata.Height).ToDataWindow()),
            ViewWidth = 240d,
            ViewHeight = 160d,
            NativeHandle = new IntPtr(0x1234),
            HandleBound = true,
            RenderScale = 1f,
        });

        snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        snapshot.IsReady.Should().BeFalse();
        snapshot.IsFallback.Should().BeFalse();
        snapshot.FallbackReason.Should().NotBeNullOrWhiteSpace();
        snapshot.UsesNativeSurface.Should().BeFalse();
        host.SoftwareScene.Should().BeNull();
    }
}
