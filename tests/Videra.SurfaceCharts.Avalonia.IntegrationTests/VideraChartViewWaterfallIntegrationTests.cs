using System.Reflection;
using System.Numerics;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class VideraChartViewWaterfallIntegrationTests
{
    [Fact]
    public Task VideraChartViewWaterfall_PublishesReadySoftwareSnapshot_AndUsesExplicitWaterfallSpacing()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(CreateWaterfallMetadata(), defaultTileValue: 6f);
            var view = new VideraChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            view.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderingStatus.IsReady.Should().BeTrue();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));

            var renderHost = (SurfaceChartRenderHost)typeof(VideraChartView)
                .GetField("_renderHost", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            var tile = renderHost.SoftwareScene!.Tiles.Should().ContainSingle().Subject;
            tile.Vertices[tile.Geometry.SampleWidth].Position.Z.Should().BeApproximately(0.15f, 0.001f);
            tile.Vertices[tile.Geometry.SampleWidth * 3].Position.Z.Should().BeApproximately(1f, 0.001f);
        });
    }

    [Fact]
    public void VideraChartViewWaterfall_FitToData_And_ResetCamera_PreserveInheritedViewStateWorkflow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var source = new RecordingSurfaceTileSource(CreateWaterfallMetadata());
            var dataWindow = new SurfaceDataWindow(128d, 64d, 256d, 128d);
            var view = new VideraChartView
            {
                ViewState = new SurfaceViewState(
                    dataWindow,
                    new SurfaceCameraPose(new Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d))
            };
            SurfaceChartTestHelpers.LoadWaterfall(view, source);

            view.ResetCamera();
            view.ViewState.DataWindow.Should().Be(dataWindow);
            view.ViewState.Camera.Should().Be(SurfaceCameraPose.CreateDefault(source.Metadata, dataWindow));

            view.FitToData();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));
        });
    }

    private static SurfaceMetadata CreateWaterfallMetadata()
    {
        const int width = 16;
        const int stripCount = 4;
        const int rowsPerStrip = 3;
        const double signalRowOffset = 0.15d;
        const double trailingBaselineOffset = 0.3d;
        var height = stripCount * rowsPerStrip;
        var horizontalCoordinates = new double[width];
        var verticalCoordinates = new double[height];

        for (var x = 0; x < width; x++)
        {
            horizontalCoordinates[x] = (10d * x) / (width - 1d);
        }

        for (var strip = 0; strip < stripCount; strip++)
        {
            var baselineTop = strip * rowsPerStrip;
            verticalCoordinates[baselineTop] = strip;
            verticalCoordinates[baselineTop + 1] = strip + signalRowOffset;
            verticalCoordinates[baselineTop + 2] = strip + trailingBaselineOffset;
        }

        return new SurfaceMetadata(
            new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates),
            new SurfaceAxisDescriptor("Time", "s", horizontalCoordinates[0], horizontalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Sweep", unit: null, minimum: verticalCoordinates[0], maximum: verticalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(0d, 100d));
    }
}
