using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Demo.Views;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsDemoViewportBehaviorTests
{
    [Fact]
    public Task DemoWindow_UsesBuiltInInteractionWorkflowAndRemovesViewportSelector()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var interactionQualityText = window.FindControl<TextBlock>("InteractionQualityText")
                ?? throw new InvalidOperationException("InteractionQualityText is missing.");

            var initialSource = GetActiveSurfaceSource(chartView);
            window.FindControl<ComboBox>("ViewportSelector").Should().BeNull();
            chartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, initialSource.Metadata.Width, initialSource.Metadata.Height));
            statusText.Text.Should().Contain("Start here: In-memory first chart");
            statusText.Text.Should().Contain("baseline");
            interactionQualityText.Text.Should().Contain("Refine");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming"));

            await WaitForConditionAsync(
                () => HasActiveSurfaceSource(chartView) &&
                      statusText.Text?.Contains("Explore next: Cache-backed streaming", StringComparison.Ordinal) == true,
                "switching data paths should keep the built-in interaction workflow active on the new source.")
                .ConfigureAwait(true);

            var cacheSource = GetActiveSurfaceSource(chartView);
            chartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, cacheSource.Metadata.Width, cacheSource.Metadata.Height));
            statusText.Text.Should().Contain("Explore next: Cache-backed streaming");
            statusText.Text.Should().Contain("Advanced follow-up");
            statusText.Text.Should().Contain("lazy");
        });
    }

    [Fact]
    public Task DemoWindow_AnalyticsProof_SwitchesToExplicitColorFieldSurfacePath()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var datasetText = window.FindControl<TextBlock>("DatasetText")
                ?? throw new InvalidOperationException("DatasetText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Analytics proof"));

            await WaitForConditionAsync(
                () => HasActiveSurfaceSource(chartView) &&
                      statusText.Text?.Contains("Try next: Analytics proof", StringComparison.Ordinal) == true &&
                      GetActiveSurfaceSource(chartView).Metadata.HorizontalAxis.ScaleKind == SurfaceAxisScaleKind.ExplicitCoordinates,
                "the analytics proof should switch to an explicit-coordinate source and update the onboarding status.")
                .ConfigureAwait(true);

            var surfaceSource = GetActiveSurfaceSource(chartView).Should().BeOfType<InMemorySurfaceTileSource>().Subject;
            surfaceSource.Metadata.Geometry.Should().BeOfType<SurfaceExplicitGrid>();
            surfaceSource.Metadata.HorizontalAxis.ScaleKind.Should().Be(SurfaceAxisScaleKind.ExplicitCoordinates);
            surfaceSource.Metadata.VerticalAxis.ScaleKind.Should().Be(SurfaceAxisScaleKind.ExplicitCoordinates);

            surfaceSource.Levels.Should().NotBeEmpty();
            var matrix = surfaceSource.Levels[0].Matrix;
            matrix.ColorField.Should().NotBeNull();
            matrix.ColorField!.Values.ToArray().Should().NotEqual(matrix.HeightField.Values.ToArray());

            statusText.Text.Should().Contain("explicit");
            statusText.Text.Should().Contain("ColorField");
            datasetText.Text.Should().Contain("explicit");
            datasetText.Text.Should().Contain("color");
        });
    }

    [Fact]
    public Task DemoWindow_AnalyticsProof_TextProjectsProbeAndColorTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Analytics proof"));

            await WaitForConditionAsync(
                () => supportSummaryText.Text?.Contains("Try next: Analytics proof", StringComparison.Ordinal) == true &&
                      supportSummaryText.Text?.Contains("Shift + LeftClick", StringComparison.Ordinal) == true &&
                      supportSummaryText.Text?.Contains("ColorField", StringComparison.Ordinal) == true,
                "switching to the analytics proof should project explicit-coordinate and independent color scalar truth into the summary text.")
                .ConfigureAwait(true);

            supportSummaryText.Text.Should().Contain("Plot path:");
            supportSummaryText.Text.Should().Contain("Plot details:");
            supportSummaryText.Text.Should().Contain("Dataset:");
            supportSummaryText.Text.Should().Contain("Shift + LeftClick");
        });
    }

    [Fact]
    public Task DemoWindow_ViewStateContractButtonsDrivePublicApi()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var fitToDataButton = window.FindControl<Button>("FitToDataButton")
                ?? throw new InvalidOperationException("FitToDataButton is missing.");
            var resetCameraButton = window.FindControl<Button>("ResetCameraButton")
                ?? throw new InvalidOperationException("ResetCameraButton is missing.");
            var viewStateText = window.FindControl<TextBlock>("ViewStateText")
                ?? throw new InvalidOperationException("ViewStateText is missing.");
            var interactionQualityText = window.FindControl<TextBlock>("InteractionQualityText")
                ?? throw new InvalidOperationException("InteractionQualityText is missing.");

            var focusedWindow = new SurfaceDataWindow(8d, 6d, 32d, 24d);
            chartView.ZoomTo(focusedWindow);
            chartView.ViewState.DataWindow.Should().Be(focusedWindow);

            chartView.ViewState = new SurfaceViewState(
                chartView.ViewState.DataWindow,
                new SurfaceCameraPose(new System.Numerics.Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d));

            ClickButton(resetCameraButton);

            await WaitForConditionAsync(
                () => chartView.ViewState.Camera == SurfaceCameraPose.CreateDefault(GetActiveSurfaceSource(chartView).Metadata, chartView.ViewState.DataWindow),
                "Reset camera should restore the default pose for the active data window.")
                .ConfigureAwait(true);

            ClickButton(fitToDataButton);

            await WaitForConditionAsync(
                () =>
                {
                    var activeSource = GetActiveSurfaceSource(chartView);
                    return chartView.ViewState.DataWindow == new SurfaceDataWindow(0d, 0d, activeSource.Metadata.Width, activeSource.Metadata.Height);
                },
                "Fit to data should restore the full dataset window through the public API.")
                .ConfigureAwait(true);

            viewStateText.Text.Should().Contain("ViewState");
            viewStateText.Text.Should().Contain("Data window");
            viewStateText.Text.Should().Contain("Camera");
            interactionQualityText.Text.Should().Contain("Refine");
        });
    }

    [Fact]
    public Task DemoWindow_InteractionGuidancePanels_ProjectBuiltInTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var interactionQualityText = window.FindControl<TextBlock>("InteractionQualityText")
                ?? throw new InvalidOperationException("InteractionQualityText is missing.");
            var viewStateText = window.FindControl<TextBlock>("ViewStateText")
                ?? throw new InvalidOperationException("ViewStateText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(statusText.Text) &&
                      !string.IsNullOrWhiteSpace(interactionQualityText.Text) &&
                      !string.IsNullOrWhiteSpace(viewStateText.Text),
                "the demo should project the built-in interaction contract into visible onboarding text.")
                .ConfigureAwait(true);

            statusText.Text.Should().Contain("Start here: In-memory first chart");
            statusText.Text.Should().Contain("first chart");
            statusText.Text.Should().Contain("Ctrl + Left drag");
            interactionQualityText.Text.Should().Contain("Interactive");
            interactionQualityText.Text.Should().Contain("Refine");
            viewStateText.Text.Should().Contain("ViewState");
        });
    }

    [Fact]
    public Task DemoWindow_OverlayOptionsPanelProjectsChartLocalCustomizationTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var overlayOptionsText = window.FindControl<TextBlock>("OverlayOptionsText")
                ?? throw new InvalidOperationException("OverlayOptionsText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(overlayOptionsText.Text),
                "the demo should project overlay customization truth into visible onboarding text.")
                .ConfigureAwait(true);

            chartView.Plot.OverlayOptions.ShowMinorTicks.Should().BeTrue();
            chartView.Plot.OverlayOptions.MinorTickDivisions.Should().Be(4);
            chartView.Plot.OverlayOptions.GridPlane.Should().Be(SurfaceChartGridPlane.XZ);
            chartView.Plot.OverlayOptions.AxisSideMode.Should().Be(SurfaceChartAxisSideMode.Auto);
            overlayOptionsText.Text.Should().Contain("OverlayOptions");
            overlayOptionsText.Text.Should().Contain("Minor ticks");
            overlayOptionsText.Text.Should().Contain("Grid plane");
            overlayOptionsText.Text.Should().Contain("Axis side");
        });
    }

    [Fact]
    public Task DemoWindow_RenderingPathProjectsRenderingStatusIntoVisibleText()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var renderingPathText = window.FindControl<TextBlock>("RenderingPathText")
                ?? throw new InvalidOperationException("RenderingPathText is missing.");
            var renderingDiagnosticsText = window.FindControl<TextBlock>("RenderingDiagnosticsText")
                ?? throw new InvalidOperationException("RenderingDiagnosticsText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(renderingPathText.Text) &&
                      !string.IsNullOrWhiteSpace(renderingDiagnosticsText.Text),
                "the demo should project rendering status into visible onboarding text.")
                .ConfigureAwait(true);

            renderingPathText.Text.Should().Contain("Active backend");
            renderingPathText.Text.Should().Contain(chartView.RenderingStatus.ActiveBackend.ToString());
            renderingPathText.Text.Should().Contain("Resident tiles");
            renderingDiagnosticsText.Text.Should().Contain("ActiveBackend:");
            renderingDiagnosticsText.Text.Should().Contain(chartView.RenderingStatus.ActiveBackend.ToString());
            renderingDiagnosticsText.Text.Should().Contain("IsReady:");
            renderingDiagnosticsText.Text.Should().Contain("IsFallback:");
            renderingDiagnosticsText.Text.Should().Contain("FallbackReason:");
            renderingDiagnosticsText.Text.Should().Contain("UsesNativeSurface:");
            renderingDiagnosticsText.Text.Should().Contain("ResidentTileCount:");
            renderingDiagnosticsText.Text.Should().Contain("Host path:");
        });
    }

    [Fact]
    public Task DemoWindow_WaterfallProof_SwitchesToWaterfallChartPath_AndPreservesViewStateButtons()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallPlotView = window.FindControl<VideraChartView>("WaterfallPlotView")
                ?? throw new InvalidOperationException("WaterfallPlotView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var fitToDataButton = window.FindControl<Button>("FitToDataButton")
                ?? throw new InvalidOperationException("FitToDataButton is missing.");
            var resetCameraButton = window.FindControl<Button>("ResetCameraButton")
                ?? throw new InvalidOperationException("ResetCameraButton is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Waterfall proof"));

            await WaitForConditionAsync(
                () => waterfallPlotView.IsVisible &&
                      HasActiveSurfaceSource(waterfallPlotView) &&
                      statusText.Text?.Contains("Try next: Waterfall proof", StringComparison.Ordinal) == true,
                "switching data paths should activate the thin Waterfall proof path.")
                .ConfigureAwait(true);

            chartView.IsVisible.Should().BeFalse();
            var waterfallSource = GetActiveSurfaceSource(waterfallPlotView);
            waterfallPlotView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, waterfallSource.Metadata.Width, waterfallSource.Metadata.Height));

            var requestedWindow = new SurfaceDataWindow(-12d, -6d, 120d, 60d);
            var expectedWindow = requestedWindow.ClampTo(waterfallSource.Metadata);
            waterfallPlotView.ZoomTo(requestedWindow);
            waterfallPlotView.ViewState.DataWindow.Should().Be(expectedWindow);

            var camera = new SurfaceCameraPose(new System.Numerics.Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d);
            waterfallPlotView.ViewState = new SurfaceViewState(
                expectedWindow,
                camera);

            ClickButton(resetCameraButton);

            await WaitForConditionAsync(
                () => waterfallPlotView.ViewState.Camera == SurfaceCameraPose.CreateDefault(GetActiveSurfaceSource(waterfallPlotView).Metadata, waterfallPlotView.ViewState.DataWindow),
                "reset camera should keep working on the Waterfall proof path.")
                .ConfigureAwait(true);

            ClickButton(fitToDataButton);

            await WaitForConditionAsync(
                () =>
                {
                    var activeSource = GetActiveSurfaceSource(waterfallPlotView);
                    return waterfallPlotView.ViewState.DataWindow == new SurfaceDataWindow(0d, 0d, activeSource.Metadata.Width, activeSource.Metadata.Height);
                },
                "fit to data should still use the inherited ViewState workflow on the Waterfall proof path.")
                .ConfigureAwait(true);
        });
    }

    [Fact]
    public Task DemoWindow_ScatterProof_SwitchesToScatterChartPath_AndProjectsScatterTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallPlotView = window.FindControl<VideraChartView>("WaterfallPlotView")
                ?? throw new InvalidOperationException("WaterfallPlotView is missing.");
            var scatterPlotView = window.FindControl<VideraChartView>("ScatterPlotView")
                ?? throw new InvalidOperationException("ScatterPlotView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var renderingPathText = window.FindControl<TextBlock>("RenderingPathText")
                ?? throw new InvalidOperationException("RenderingPathText is missing.");
            var renderingDiagnosticsText = window.FindControl<TextBlock>("RenderingDiagnosticsText")
                ?? throw new InvalidOperationException("RenderingDiagnosticsText is missing.");
            var builtInInteractionText = window.FindControl<TextBlock>("BuiltInInteractionText")
                ?? throw new InvalidOperationException("BuiltInInteractionText is missing.");
            var interactionQualityText = window.FindControl<TextBlock>("InteractionQualityText")
                ?? throw new InvalidOperationException("InteractionQualityText is missing.");
            var overlayOptionsText = window.FindControl<TextBlock>("OverlayOptionsText")
                ?? throw new InvalidOperationException("OverlayOptionsText is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Scatter proof"));

            await WaitForConditionAsync(
                () => scatterPlotView.IsVisible &&
                      scatterPlotView.Plot.Series.Any(series => series.Kind == Plot3DSeriesKind.Scatter) &&
                      statusText.Text?.Contains("Try next: Scatter proof", StringComparison.Ordinal) == true,
                "switching data paths should activate the direct scatter proof path.")
                .ConfigureAwait(true);

            chartView.IsVisible.Should().BeFalse();
            waterfallPlotView.IsVisible.Should().BeFalse();
            scatterPlotView.IsVisible.Should().BeTrue();

            builtInInteractionText.Text.Should().Contain("Left drag orbit");
            builtInInteractionText.Text.Should().Contain("Wheel dolly");
            builtInInteractionText.Text.Should().Contain("does not expose right-drag pan");

            interactionQualityText.Text.Should().Contain("Current mode:");
            interactionQualityText.Text.Should().Contain("Refine");
            overlayOptionsText.Text.Should().Contain("shared by");
            statusText.Text.Should().Contain("Plot.Add.Scatter");
            statusText.Text.Should().Contain("Current scene:");
            renderingPathText.Text.Should().Contain("Plot path");
            renderingPathText.Text.Should().Contain("Series:");
            renderingPathText.Text.Should().Contain("Points:");
            renderingDiagnosticsText.Text.Should().Contain("PlotRevision:");
            renderingDiagnosticsText.Text.Should().Contain("InteractionQuality:");
            renderingDiagnosticsText.Text.Should().Contain("SeriesCount:");
            renderingDiagnosticsText.Text.Should().Contain("PointCount:");

            supportSummaryText.Text.Should().Contain("VideraChartView");
            supportSummaryText.Text.Should().Contain("ChartControl: VideraChartView");
            supportSummaryText.Text.Should().Contain("EnvironmentRuntime:");
            supportSummaryText.Text.Should().Contain("AssemblyIdentity:");
            supportSummaryText.Text.Should().Contain("BackendDisplayEnvironment:");
            supportSummaryText.Text.Should().Contain("CacheLoadFailure: none");
            supportSummaryText.Text.Should().Contain("RenderingStatus:");
            supportSummaryText.Text.Should().Contain("SeriesCount");
            supportSummaryText.Text.Should().Contain("PointCount");
            supportSummaryText.Text.Should().Contain("InteractionQuality: Refine");
            supportSummaryText.Text.Should().Contain("OverlayOptions: VideraChartView.Plot.OverlayOptions");
            supportSummaryText.Text.Should().NotContain("ViewState:");
        });
    }

    [Fact]
    public Task DemoWindow_CompletedCacheLoad_DoesNotOverrideNewerWaterfallSelection()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallPlotView = window.FindControl<VideraChartView>("WaterfallPlotView")
                ?? throw new InvalidOperationException("WaterfallPlotView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");

            var cacheSourceTaskField = typeof(MainWindow).GetField("_cacheSourceTask", BindingFlags.Instance | BindingFlags.NonPublic);
            var inMemorySourceField = typeof(MainWindow).GetField("_inMemorySource", BindingFlags.Instance | BindingFlags.NonPublic);
            cacheSourceTaskField.Should().NotBeNull();
            inMemorySourceField.Should().NotBeNull();

            var cacheSourceCompletion = new TaskCompletionSource<ISurfaceTileSource>(TaskCreationOptions.RunContinuationsAsynchronously);
            var inMemorySource = (ISurfaceTileSource)inMemorySourceField!.GetValue(window)!;
            cacheSourceTaskField!.SetValue(window, cacheSourceCompletion.Task);

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming"));
            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Waterfall proof"));

            await WaitForConditionAsync(
                () => waterfallPlotView.IsVisible &&
                      HasActiveSurfaceSource(waterfallPlotView) &&
                      statusText.Text?.Contains("Try next: Waterfall proof", StringComparison.Ordinal) == true,
                "the newer waterfall selection should become active before the pending cache load completes.")
                .ConfigureAwait(true);

            cacheSourceCompletion.SetResult(inMemorySource);
            await Task.Yield();
            await Task.Yield();

            chartView.IsVisible.Should().BeFalse();
            waterfallPlotView.IsVisible.Should().BeTrue();
            statusText.Text.Should().Contain("Try next: Waterfall proof");
        });
    }

    [Fact]
    public Task DemoWindow_CacheLoadFailure_KeepsCachePathSelectedAndProjectsFailureDetail()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Analytics proof"));
            var previousSource = GetActiveSurfaceSource(chartView);
            previousSource.Should().NotBeNull();

            var cacheSourceTaskField = typeof(MainWindow).GetField("_cacheSourceTask", BindingFlags.Instance | BindingFlags.NonPublic);
            cacheSourceTaskField.Should().NotBeNull();
            cacheSourceTaskField!.SetValue(
                window,
                Task.FromException<ISurfaceTileSource>(new InvalidOperationException("cache manifest sidecar missing")));

            var cacheItem = GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming");
            SelectItem(sourceSelector, cacheItem);

            await WaitForConditionAsync(
                () => supportSummaryText.Text?.Contains("CacheLoadFailure: InvalidOperationException: cache manifest sidecar missing", StringComparison.Ordinal) == true,
                "cache-backed load failure should keep the exact failure available in the support summary.")
                .ConfigureAwait(true);

            sourceSelector.SelectedItem.Should().BeSameAs(cacheItem);
            GetActiveSurfaceSource(chartView).Should().BeSameAs(previousSource);
            statusText.Text.Should().Contain("Explore next: Cache-backed streaming");
            statusText.Text.Should().Contain("Cache-backed streaming failed to load: cache manifest sidecar missing");
            supportSummaryText.Text.Should().Contain("CacheLoadFailure: InvalidOperationException: cache manifest sidecar missing");
        });
    }

    [Fact]
    public Task DemoWindow_SupportSummaryProjectsSupportReadyChartTruth()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var copySupportSummaryButton = window.FindControl<Button>("CopySupportSummaryButton")
                ?? throw new InvalidOperationException("CopySupportSummaryButton is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");
            var supportSummaryStatusText = window.FindControl<TextBlock>("SupportSummaryStatusText")
                ?? throw new InvalidOperationException("SupportSummaryStatusText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(supportSummaryText.Text),
                "the demo should project a support-ready chart summary into visible text.")
                .ConfigureAwait(true);

            ClickButton(copySupportSummaryButton);

            await WaitForConditionAsync(
                () => supportSummaryStatusText.Text?.Contains("Copied support summary to the clipboard.", StringComparison.Ordinal) == true ||
                      supportSummaryStatusText.Text?.Contains("Clipboard is unavailable. The support summary remains visible below.", StringComparison.Ordinal) == true,
                "the demo should give users a copy workflow for the support summary.")
                .ConfigureAwait(true);

            supportSummaryText.Text.Should().Contain("Plot path:");
            supportSummaryText.Text.Should().Contain("ChartControl: VideraChartView");
            supportSummaryText.Text.Should().Contain("EnvironmentRuntime:");
            supportSummaryText.Text.Should().Contain("AssemblyIdentity:");
            supportSummaryText.Text.Should().Contain("BackendDisplayEnvironment:");
            supportSummaryText.Text.Should().Contain("CacheLoadFailure: none");
            supportSummaryText.Text.Should().Contain("ViewState:");
            supportSummaryText.Text.Should().Contain("InteractionQuality:");
            supportSummaryText.Text.Should().Contain("RenderingStatus:");
            supportSummaryText.Text.Should().Contain("ActiveBackend:");
            supportSummaryText.Text.Should().Contain("IsFallback:");
            supportSummaryText.Text.Should().Contain("FallbackReason:");
            supportSummaryText.Text.Should().Contain("UsesNativeSurface:");
            supportSummaryText.Text.Should().Contain("ResidentTileCount:");
            supportSummaryText.Text.Should().Contain("OverlayOptions:");
            supportSummaryText.Text.Should().Contain("Cache asset:");
            supportSummaryText.Text.Should().Contain("Dataset:");
            supportSummaryStatusText.Text.Should().MatchRegex("Copied support summary to the clipboard\\.|Clipboard is unavailable\\. The support summary remains visible below\\.");
        });
    }

    [Fact]
    public Task DemoWindow_CopySupportSummaryCapturesCurrentPlotPathAndViewState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<VideraChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var copySupportSummaryButton = window.FindControl<Button>("CopySupportSummaryButton")
                ?? throw new InvalidOperationException("CopySupportSummaryButton is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming"));

            await WaitForConditionAsync(
                () => HasActiveSurfaceSource(chartView) &&
                      supportSummaryText.Text?.Contains("Explore next: Cache-backed streaming", StringComparison.Ordinal) == true,
                "switching data paths should project the new Plot path into the visible support summary.")
                .ConfigureAwait(true);

            chartView.ZoomTo(new SurfaceDataWindow(8d, 6d, 32d, 24d));
            ClickButton(copySupportSummaryButton);

            await WaitForConditionAsync(
                () => supportSummaryText.Text?.Contains("Explore next: Cache-backed streaming", StringComparison.Ordinal) == true &&
                      supportSummaryText.Text?.Contains("Width 32", StringComparison.Ordinal) == true,
                "copying the support summary should refresh it from the current source and view state before export.")
                .ConfigureAwait(true);

            supportSummaryText.Text.Should().Contain("Explore next: Cache-backed streaming");
            supportSummaryText.Text.Should().Contain("Width 32");
        });
    }

    private static ComboBoxItem GetComboBoxItemByContent(ComboBox selector, string content)
    {
        var item = selector.Items
            .Cast<object?>()
            .OfType<ComboBoxItem>()
            .SingleOrDefault(candidate => string.Equals(candidate.Content as string, content, StringComparison.Ordinal));

        item.Should().NotBeNull($"Expected a ComboBoxItem with content '{content}'.");
        return item!;
    }

    private static void SelectItem(ComboBox selector, ComboBoxItem item)
    {
        var items = selector.Items
            .Cast<object?>()
            .OfType<ComboBoxItem>()
            .ToArray();
        var itemIndex = Array.IndexOf(items, item);

        itemIndex.Should().BeGreaterThanOrEqualTo(0);
        selector.SelectedIndex = itemIndex;
    }

    private static void ClickButton(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static bool HasActiveSurfaceSource(VideraChartView view)
    {
        return view.Plot.Series.Any(static series =>
            series.Kind is Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall &&
            series.SurfaceSource is not null);
    }

    private static ISurfaceTileSource GetActiveSurfaceSource(VideraChartView view)
    {
        var source = view.Plot.Series
            .LastOrDefault(static series => series.Kind is Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall)
            ?.SurfaceSource;

        return source ?? throw new InvalidOperationException("The active plot does not contain a surface or waterfall source.");
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, string because, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(true);
        }

        condition().Should().BeTrue(because);
    }
}
