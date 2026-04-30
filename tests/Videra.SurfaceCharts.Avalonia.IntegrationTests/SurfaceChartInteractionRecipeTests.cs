using System;
using System.IO;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartInteractionRecipeTests
{
    [Fact]
    public Task TryResolveProbe_ReportsPointerProbeWithoutChangingPinnedState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 13f);
            var view = new VideraChartView();

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [13f]);

            view.TryResolveProbe(new Point(128d, 64d), out var probe).Should().BeTrue();

            probe.Value.Should().Be(13d);
            SurfaceChartTestHelpers.GetOverlayCoordinator(view).ProbeState.PinnedProbes.Should().BeEmpty();
        });
    }

    [Fact]
    public Task SelectionReports_ClickAndRectangle_InSampleAndAxisSpace()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 5,
                height: 5,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
                new SurfaceValueRange(-2d, 2d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 3f);
            var view = new VideraChartView();

            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));
            view.ViewState = new SurfaceViewState((new SurfaceViewport(0, 0, 4, 4)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [3f]);

            view.TryCreateSelectionReport(new Point(100d, 100d), out var clickReport).Should().BeTrue();
            clickReport.Kind.Should().Be(SurfaceChartSelectionKind.Click);
            clickReport.SampleStartX.Should().BeApproximately(2d, 0.0001d);
            clickReport.SampleStartY.Should().BeApproximately(2d, 0.0001d);
            clickReport.AxisStartX.Should().BeApproximately(15d, 0.0001d);
            clickReport.AxisStartY.Should().BeApproximately(150d, 0.0001d);
            clickReport.DataWindow.Should().BeNull();

            view.TryCreateSelectionReport(new Point(50d, 50d), new Point(150d, 100d), out var rectangleReport).Should().BeTrue();
            rectangleReport.Kind.Should().Be(SurfaceChartSelectionKind.Rectangle);
            rectangleReport.DataWindow.Should().NotBeNull();
            rectangleReport.DataWindow!.Value.StartX.Should().BeApproximately(1d, 0.0001d);
            rectangleReport.DataWindow.Value.StartY.Should().BeApproximately(1d, 0.0001d);
            rectangleReport.DataWindow.Value.Width.Should().BeApproximately(2d, 0.0001d);
            rectangleReport.DataWindow.Value.Height.Should().BeApproximately(1d, 0.0001d);
        });
    }

    [Fact]
    public void DraggableOverlayRecipes_ClampMarkerAndRangeWithoutMutatingInput()
    {
        var metadata = new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(-2d, 2d));
        var requestedWindow = new SurfaceDataWindow(3d, 3d, 4d, 4d);

        var marker = SurfaceChartDraggableOverlayRecipes.CreateMarker(metadata, 9d, -3d);
        var range = SurfaceChartDraggableOverlayRecipes.CreateRange(metadata, requestedWindow);
        var draggedRange = SurfaceChartDraggableOverlayRecipes.DragRangeBy(metadata, range, 10d, 10d);

        marker.SampleX.Should().Be(4d);
        marker.SampleY.Should().Be(0d);
        marker.AxisX.Should().Be(20d);
        marker.AxisY.Should().Be(100d);

        requestedWindow.StartX.Should().Be(3d);
        requestedWindow.StartY.Should().Be(3d);
        range.DataWindow.StartX.Should().Be(1d);
        range.DataWindow.StartY.Should().Be(1d);
        draggedRange.DataWindow.StartX.Should().Be(1d);
        draggedRange.DataWindow.StartY.Should().Be(1d);
    }

    [Fact]
    public Task VideraChartView_CreatesBoundedDraggableOverlayRecipesFromScreenPositions()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = new SurfaceMetadata(
                width: 5,
                height: 5,
                new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
                new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
                new SurfaceValueRange(-2d, 2d));
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 3f);
            var view = new VideraChartView();

            view.Measure(new Size(200, 200));
            view.Arrange(new Rect(0, 0, 200, 200));
            view.ViewState = new SurfaceViewState((new SurfaceViewport(0, 0, 4, 4)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);
            SurfaceChartTestHelpers.LoadSurface(view, source);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [3f]);

            view.TryCreateDraggableMarkerOverlay(new Point(100d, 100d), out var marker).Should().BeTrue();
            view.TryCreateDraggableRangeOverlay(new Point(50d, 50d), new Point(150d, 150d), out var range).Should().BeTrue();

            marker.SampleX.Should().BeApproximately(2d, 0.0001d);
            marker.SampleY.Should().BeApproximately(2d, 0.0001d);
            range.DataWindow.StartX.Should().BeApproximately(1d, 0.0001d);
            range.DataWindow.StartY.Should().BeApproximately(1d, 0.0001d);
            range.DataWindow.Width.Should().BeApproximately(2d, 0.0001d);
            range.DataWindow.Height.Should().BeApproximately(2d, 0.0001d);
        });
    }

    [Fact]
    public void InteractionRecipeEvidenceFormatter_ProducesDeterministicSupportEvidence()
    {
        var evidence = SurfaceChartInteractionRecipeEvidenceFormatter.CreateSupported();

        SurfaceChartInteractionRecipeEvidenceFormatter.Format(evidence).Should().Be(
            "EvidenceKind: surface-chart-interaction-recipes\n" +
            "ProbeResolution: Supported\n" +
            "SelectionReporting: Supported\n" +
            "DraggableMarkerOverlay: Supported\n" +
            "DraggableRangeOverlay: Supported\n" +
            "StateOwnership: HostOwned");
    }

    [Fact]
    public void SurfaceChartsReadme_DocumentsMinimalInteractionHostWiring()
    {
        var readme = ReadSurfaceChartsReadme();
        var section = ExtractInteractionHandoffSection(readme);

        section.Should().Contain("using Videra.SurfaceCharts.Avalonia.Controls;");
        section.Should().Contain("using Videra.SurfaceCharts.Avalonia.Controls.Interaction;");
        section.Should().Contain("SurfaceChartInteractionProfile");
        section.Should().Contain("SurfaceChartCommand.ZoomIn");
        section.Should().Contain("TryExecuteChartCommand(SurfaceChartCommand.ResetCamera)");
        section.Should().Contain("host-owned buttons and context-menu items");
        section.Should().Contain("EnabledCommands");
    }

    [Fact]
    public void SurfaceChartsReadme_DocumentsProbeSelectionAndDraggableHostOwnership()
    {
        var readme = ReadSurfaceChartsReadme();
        var section = ExtractInteractionHandoffSection(readme);

        section.Should().Contain("TryResolveProbe");
        section.Should().Contain("SurfaceChartProbeEvidenceFormatter.Create");
        section.Should().Contain("EvidenceKind: surface-chart-probe");
        section.Should().Contain("ProbeStatus");
        section.Should().Contain("PinnedCount");
        section.Should().Contain("TryCreateSelectionReport");
        section.Should().Contain("TryCreateDraggableMarkerOverlay");
        section.Should().Contain("TryCreateDraggableRangeOverlay");
        section.Should().Contain("SurfaceChartInteractionRecipeEvidenceFormatter");
        section.Should().Contain("StateOwnership: HostOwned");
        section.Should().Contain("host-owned");
        section.Should().Contain("do not add chart-owned product selection");
        section.Should().Contain("built-in drag editor");
        section.Should().NotContain("chart owns product selection");
        section.Should().NotContain("chart-owned selection state");
        section.Should().NotContain("provides a built-in drag editor");
    }

    private static string ReadSurfaceChartsReadme()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Videra.SurfaceCharts.Avalonia", "README.md");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate src/Videra.SurfaceCharts.Avalonia/README.md from the test output directory.");
    }

    private static string ExtractInteractionHandoffSection(string readme)
    {
        const string heading = "### Interaction Handoff Recipes";
        const string nextHeading = "Hosts currently own:";

        var start = readme.IndexOf(heading, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);

        var end = readme.IndexOf(nextHeading, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);

        return readme[start..end];
    }
}
