using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartRenderHostIntegrationTests
{
    [Fact]
    public void SurfaceChartView_UsesRenderHostInsteadOfControlOwnedRendererState()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var renderHostField = typeof(SurfaceChartView).GetField("_renderHost", BindingFlags.Instance | BindingFlags.NonPublic);
            renderHostField.Should().NotBeNull();
            renderHostField!.FieldType.Should().Be(typeof(SurfaceChartRenderHost));

            typeof(SurfaceChartView).GetField("_renderer", BindingFlags.Instance | BindingFlags.NonPublic).Should().BeNull();

            var view = new SurfaceChartView();
            SurfaceChartRenderSnapshot snapshot = view.RenderSnapshot;
            snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            snapshot.IsReady.Should().BeFalse();
        });
    }

    [Fact]
    public Task SurfaceChartView_LoadedTiles_PublishSoftwareSnapshotFromRenderHost()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new SurfaceChartView();

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
}
