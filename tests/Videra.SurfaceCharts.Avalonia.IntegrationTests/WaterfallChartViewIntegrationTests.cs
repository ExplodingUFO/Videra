using System.Reflection;
using System.Numerics;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class WaterfallChartViewIntegrationTests
{
    [Fact]
    public Task WaterfallChartView_PublishesReadySoftwareSnapshot_AndUsesWaterfallRenderer()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 6f);
            var view = new WaterfallChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            view.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderingStatus.IsReady.Should().BeTrue();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));

            var renderHost = (SurfaceChartRenderHost)typeof(SurfaceChartView)
                .GetField("_renderHost", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            var tile = renderHost.SoftwareScene!.Tiles.Should().ContainSingle().Subject;
            tile.Vertices[tile.Geometry.SampleWidth].Position.Z.Should().BeApproximately(0.35f, 0.001f);
        });
    }

    [Fact]
    public void WaterfallChartView_FitToData_And_ResetCamera_PreserveInheritedViewStateWorkflow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var source = new RecordingSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata());
            var dataWindow = new SurfaceDataWindow(128d, 64d, 256d, 128d);
            var view = new WaterfallChartView
            {
                Source = source,
                ViewState = new SurfaceViewState(
                    dataWindow,
                    new SurfaceCameraPose(new Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d))
            };

            view.ResetCamera();
            view.ViewState.DataWindow.Should().Be(dataWindow);
            view.ViewState.Camera.Should().Be(SurfaceCameraPose.CreateDefault(source.Metadata, dataWindow));

            view.FitToData();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));
        });
    }
}
