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
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var interactionQualityText = window.FindControl<TextBlock>("InteractionQualityText")
                ?? throw new InvalidOperationException("InteractionQualityText is missing.");

            chartView.Source.Should().NotBeNull();
            window.FindControl<ComboBox>("ViewportSelector").Should().BeNull();
            chartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, chartView.Source!.Metadata.Width, chartView.Source.Metadata.Height));
            statusText.Text.Should().Contain("Start here: In-memory first chart");
            statusText.Text.Should().Contain("baseline");
            interactionQualityText.Text.Should().Contain("Refine");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming"));

            await WaitForConditionAsync(
                () => chartView.Source is not null &&
                      statusText.Text?.Contains("Explore next: Cache-backed streaming", StringComparison.Ordinal) == true,
                "switching sources should keep the built-in interaction workflow active on the new source.")
                .ConfigureAwait(true);

            chartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, chartView.Source!.Metadata.Width, chartView.Source.Metadata.Height));
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
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var datasetText = window.FindControl<TextBlock>("DatasetText")
                ?? throw new InvalidOperationException("DatasetText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Try next: Analytics proof"));

            await WaitForConditionAsync(
                () => chartView.Source is not null &&
                      statusText.Text?.Contains("Try next: Analytics proof", StringComparison.Ordinal) == true &&
                      chartView.Source!.Metadata.HorizontalAxis.ScaleKind == SurfaceAxisScaleKind.ExplicitCoordinates,
                "the analytics proof should switch to an explicit-coordinate source and update the onboarding status.")
                .ConfigureAwait(true);

            chartView.Source.Should().BeOfType<InMemorySurfaceTileSource>();
            var surfaceSource = (InMemorySurfaceTileSource)chartView.Source!;
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

            supportSummaryText.Text.Should().Contain("Source path:");
            supportSummaryText.Text.Should().Contain("Source details:");
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
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
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
                () => chartView.ViewState.Camera == SurfaceCameraPose.CreateDefault(chartView.Source!.Metadata, chartView.ViewState.DataWindow),
                "Reset camera should restore the default pose for the active data window.")
                .ConfigureAwait(true);

            ClickButton(fitToDataButton);

            await WaitForConditionAsync(
                () => chartView.ViewState.DataWindow == new SurfaceDataWindow(0d, 0d, chartView.Source!.Metadata.Width, chartView.Source.Metadata.Height),
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
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var overlayOptionsText = window.FindControl<TextBlock>("OverlayOptionsText")
                ?? throw new InvalidOperationException("OverlayOptionsText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(overlayOptionsText.Text),
                "the demo should project overlay customization truth into visible onboarding text.")
                .ConfigureAwait(true);

            chartView.OverlayOptions.ShowMinorTicks.Should().BeTrue();
            chartView.OverlayOptions.MinorTickDivisions.Should().Be(4);
            chartView.OverlayOptions.GridPlane.Should().Be(SurfaceChartGridPlane.XZ);
            chartView.OverlayOptions.AxisSideMode.Should().Be(SurfaceChartAxisSideMode.Auto);
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
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var renderingPathText = window.FindControl<TextBlock>("RenderingPathText")
                ?? throw new InvalidOperationException("RenderingPathText is missing.");

            await WaitForConditionAsync(
                () => !string.IsNullOrWhiteSpace(renderingPathText.Text),
                "the demo should project rendering status into visible onboarding text.")
                .ConfigureAwait(true);

            renderingPathText.Text.Should().Contain("Active backend");
            renderingPathText.Text.Should().Contain(chartView.RenderingStatus.ActiveBackend.ToString());
            renderingPathText.Text.Should().Contain("Resident tiles");
        });
    }

    [Fact]
    public Task DemoWindow_WaterfallProof_SwitchesToWaterfallChartPath_AndPreservesViewStateButtons()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var surfaceChartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallChartView = window.FindControl<WaterfallChartView>("WaterfallChartView")
                ?? throw new InvalidOperationException("WaterfallChartView is missing.");
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
                () => waterfallChartView.IsVisible &&
                      waterfallChartView.Source is not null &&
                      statusText.Text?.Contains("Try next: Waterfall proof", StringComparison.Ordinal) == true,
                "switching sources should activate the thin Waterfall proof path.")
                .ConfigureAwait(true);

            surfaceChartView.IsVisible.Should().BeFalse();
            waterfallChartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, waterfallChartView.Source!.Metadata.Width, waterfallChartView.Source.Metadata.Height));

            var requestedWindow = new SurfaceDataWindow(-12d, -6d, 120d, 60d);
            var expectedWindow = requestedWindow.ClampTo(waterfallChartView.Source!.Metadata);
            waterfallChartView.ZoomTo(requestedWindow);
            waterfallChartView.ViewState.DataWindow.Should().Be(expectedWindow);

            var camera = new SurfaceCameraPose(new System.Numerics.Vector3(2f, 3f, 4f), 180d, 22d, 16d, 40d);
            waterfallChartView.ViewState = new SurfaceViewState(
                expectedWindow,
                camera);

            ClickButton(resetCameraButton);

            await WaitForConditionAsync(
                () => waterfallChartView.ViewState.Camera == SurfaceCameraPose.CreateDefault(waterfallChartView.Source!.Metadata, waterfallChartView.ViewState.DataWindow),
                "reset camera should keep working on the Waterfall proof path.")
                .ConfigureAwait(true);

            ClickButton(fitToDataButton);

            await WaitForConditionAsync(
                () => waterfallChartView.ViewState.DataWindow == new SurfaceDataWindow(0d, 0d, waterfallChartView.Source!.Metadata.Width, waterfallChartView.Source.Metadata.Height),
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
            var surfaceChartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallChartView = window.FindControl<WaterfallChartView>("WaterfallChartView")
                ?? throw new InvalidOperationException("WaterfallChartView is missing.");
            var scatterChartView = window.FindControl<ScatterChartView>("ScatterChartView")
                ?? throw new InvalidOperationException("ScatterChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var fitToDataButton = window.FindControl<Button>("FitToDataButton")
                ?? throw new InvalidOperationException("FitToDataButton is missing.");
            var resetCameraButton = window.FindControl<Button>("ResetCameraButton")
                ?? throw new InvalidOperationException("ResetCameraButton is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");
            var renderingPathText = window.FindControl<TextBlock>("RenderingPathText")
                ?? throw new InvalidOperationException("RenderingPathText is missing.");
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
                () => scatterChartView.IsVisible &&
                      scatterChartView.Source is not null &&
                      statusText.Text?.Contains("Try next: Scatter proof", StringComparison.Ordinal) == true,
                "switching sources should activate the direct scatter proof path.")
                .ConfigureAwait(true);

            surfaceChartView.IsVisible.Should().BeFalse();
            waterfallChartView.IsVisible.Should().BeFalse();
            scatterChartView.IsVisible.Should().BeTrue();

            builtInInteractionText.Text.Should().Contain("Left drag orbit");
            builtInInteractionText.Text.Should().Contain("Wheel dolly");
            builtInInteractionText.Text.Should().Contain("does not expose right-drag pan");

            interactionQualityText.Text.Should().Contain("does not expose");
            overlayOptionsText.Text.Should().Contain("does not expose");
            statusText.Text.Should().Contain("Scatter proof navigation");
            statusText.Text.Should().Contain("Current scene:");
            renderingPathText.Text.Should().Contain("Backend kind");
            renderingPathText.Text.Should().Contain("Series:");
            renderingPathText.Text.Should().Contain("Points:");

            var source = scatterChartView.Source!;
            var bounds = GetScatterBounds(source);
            var customCamera = new SurfaceCameraPose(
                new System.Numerics.Vector3(99f, 88f, 77f),
                73d,
                -17d,
                42d,
                60d);

            SetCamera(scatterChartView, customCamera);

            ClickButton(fitToDataButton);

            var fitCamera = GetCamera(scatterChartView);
            fitCamera.Target.Should().Be(bounds.Center);
            fitCamera.YawDegrees.Should().Be(customCamera.YawDegrees);
            fitCamera.PitchDegrees.Should().Be(customCamera.PitchDegrees);
            fitCamera.Distance.Should().BeApproximately(GetFitDistance(bounds.Size, customCamera.FieldOfViewDegrees), 0.0001d);

            ClickButton(resetCameraButton);

            var resetCamera = GetCamera(scatterChartView);
            resetCamera.Target.Should().Be(bounds.Center);
            resetCamera.YawDegrees.Should().Be(SurfaceCameraPose.DefaultYawDegrees);
            resetCamera.PitchDegrees.Should().Be(SurfaceCameraPose.DefaultPitchDegrees);
            resetCamera.Distance.Should().BeApproximately(GetFitDistance(bounds.Size, SurfaceCameraPose.DefaultFieldOfViewDegrees), 0.0001d);

            supportSummaryText.Text.Should().Contain("ScatterChartView");
            supportSummaryText.Text.Should().Contain("RenderingStatus: BackendKind");
            supportSummaryText.Text.Should().Contain("SeriesCount");
            supportSummaryText.Text.Should().Contain("PointCount");
            supportSummaryText.Text.Should().Contain("InteractionQuality: not exposed");
            supportSummaryText.Text.Should().Contain("OverlayOptions: not exposed");
            supportSummaryText.Text.Should().NotContain("ViewState:");
        });
    }

    [Fact]
    public Task DemoWindow_CompletedCacheLoad_DoesNotOverrideNewerWaterfallSelection()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var surfaceChartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var waterfallChartView = window.FindControl<WaterfallChartView>("WaterfallChartView")
                ?? throw new InvalidOperationException("WaterfallChartView is missing.");
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
                () => waterfallChartView.IsVisible &&
                      waterfallChartView.Source is not null &&
                      statusText.Text?.Contains("Try next: Waterfall proof", StringComparison.Ordinal) == true,
                "the newer waterfall selection should become active before the pending cache load completes.")
                .ConfigureAwait(true);

            cacheSourceCompletion.SetResult(inMemorySource);
            await Task.Yield();
            await Task.Yield();

            waterfallChartView.IsVisible.Should().BeTrue();
            surfaceChartView.IsVisible.Should().BeFalse();
            statusText.Text.Should().Contain("Try next: Waterfall proof");
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

            supportSummaryText.Text.Should().Contain("Source path:");
            supportSummaryText.Text.Should().Contain("ViewState:");
            supportSummaryText.Text.Should().Contain("InteractionQuality:");
            supportSummaryText.Text.Should().Contain("RenderingStatus:");
            supportSummaryText.Text.Should().Contain("OverlayOptions:");
            supportSummaryText.Text.Should().Contain("Cache asset:");
            supportSummaryText.Text.Should().Contain("Dataset:");
            supportSummaryStatusText.Text.Should().MatchRegex("Copied support summary to the clipboard\\.|Clipboard is unavailable\\. The support summary remains visible below\\.");
        });
    }

    [Fact]
    public Task DemoWindow_CopySupportSummaryCapturesCurrentSourceAndViewState()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var copySupportSummaryButton = window.FindControl<Button>("CopySupportSummaryButton")
                ?? throw new InvalidOperationException("CopySupportSummaryButton is missing.");
            var supportSummaryText = window.FindControl<TextBlock>("SupportSummaryText")
                ?? throw new InvalidOperationException("SupportSummaryText is missing.");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Explore next: Cache-backed streaming"));

            await WaitForConditionAsync(
                () => chartView.Source is not null &&
                      supportSummaryText.Text?.Contains("Explore next: Cache-backed streaming", StringComparison.Ordinal) == true,
                "switching sources should project the new source path into the visible support summary.")
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

        itemIndex.Should().BeGreaterOrEqualTo(0);
        selector.SelectedIndex = itemIndex;
    }

    private static void ClickButton(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static SurfacePlotBounds GetScatterBounds(ScatterChartData source)
    {
        var hasPoint = false;
        double minX = 0d;
        double maxX = 0d;
        double minY = 0d;
        double maxY = 0d;
        double minZ = 0d;
        double maxZ = 0d;

        foreach (var series in source.Series)
        {
            foreach (var point in series.Points)
            {
                if (!hasPoint)
                {
                    minX = maxX = point.Horizontal;
                    minY = maxY = point.Value;
                    minZ = maxZ = point.Depth;
                    hasPoint = true;
                    continue;
                }

                minX = Math.Min(minX, point.Horizontal);
                maxX = Math.Max(maxX, point.Horizontal);
                minY = Math.Min(minY, point.Value);
                maxY = Math.Max(maxY, point.Value);
                minZ = Math.Min(minZ, point.Depth);
                maxZ = Math.Max(maxZ, point.Depth);
            }
        }

        if (!hasPoint)
        {
            throw new InvalidOperationException("Scatter source should contain at least one point.");
        }

        return new SurfacePlotBounds(
            new System.Numerics.Vector3((float)minX, (float)minY, (float)minZ),
            new System.Numerics.Vector3((float)maxX, (float)maxY, (float)maxZ));
    }

    private static SurfaceCameraPose GetCamera(ScatterChartView view)
    {
        var field = typeof(ScatterChartView).GetField(
            "_camera",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        return (SurfaceCameraPose)field!.GetValue(view)!;
    }

    private static void SetCamera(ScatterChartView view, SurfaceCameraPose camera)
    {
        var field = typeof(ScatterChartView).GetField(
            "_camera",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        field!.SetValue(view, camera);
    }

    private static double GetFitDistance(System.Numerics.Vector3 size, double fieldOfViewDegrees)
    {
        var diagonal = Math.Sqrt((size.X * size.X) + (size.Y * size.Y) + (size.Z * size.Z));
        var halfFieldOfViewRadians = (fieldOfViewDegrees * (Math.PI / 180d)) * 0.5d;
        return Math.Max((Math.Max(diagonal, 1d) * 0.5d) / Math.Tan(halfFieldOfViewRadians), 1d);
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
