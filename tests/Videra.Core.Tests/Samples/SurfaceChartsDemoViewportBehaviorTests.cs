using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
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
            statusText.Text.Should().Contain("Built-in navigation");
            interactionQualityText.Text.Should().Contain("Refine");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Cache-backed example"));

            await WaitForConditionAsync(
                () => chartView.Source is not null &&
                      statusText.Text?.Contains("Cache-backed example", StringComparison.Ordinal) == true,
                "switching sources should keep the built-in interaction workflow active on the new source.")
                .ConfigureAwait(true);

            chartView.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, chartView.Source!.Metadata.Width, chartView.Source.Metadata.Height));
            statusText.Text.Should().Contain("Cache-backed example");
            statusText.Text.Should().Contain("lazy");
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

            statusText.Text.Should().Contain("Built-in navigation");
            statusText.Text.Should().Contain("Ctrl + Left drag");
            interactionQualityText.Text.Should().Contain("Interactive");
            interactionQualityText.Text.Should().Contain("Refine");
            viewStateText.Text.Should().Contain("ViewState");
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
