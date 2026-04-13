using Avalonia.Controls;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Views;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsDemoViewportBehaviorTests
{
    [Fact]
    public Task DemoWindow_ViewportSelectionAndSourceSwitch_UseMetadataDrivenViewportPresets()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var window = new MainWindow();
            var chartView = window.FindControl<SurfaceChartView>("ChartView")
                ?? throw new InvalidOperationException("ChartView is missing.");
            var sourceSelector = window.FindControl<ComboBox>("SourceSelector")
                ?? throw new InvalidOperationException("SourceSelector is missing.");
            var viewportSelector = window.FindControl<ComboBox>("ViewportSelector")
                ?? throw new InvalidOperationException("ViewportSelector is missing.");
            var statusText = window.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("StatusText is missing.");

            chartView.Source.Should().NotBeNull();
            chartView.Viewport.Should().Be(CreateOverviewViewport(chartView.Source!.Metadata));

            SelectItem(viewportSelector, GetComboBoxItemByTag(viewportSelector, "detail"));

            await WaitForConditionAsync(
                () => chartView.Viewport == CreateZoomedDetailViewport(chartView.Source!.Metadata),
                "selecting the detail viewport should reapply the metadata-driven detail preset.")
                .ConfigureAwait(true);

            chartView.Viewport.Should().Be(CreateZoomedDetailViewport(chartView.Source!.Metadata));
            statusText.Text.Should().Contain("Zoomed detail");

            SelectItem(sourceSelector, GetComboBoxItemByContent(sourceSelector, "Cache-backed example"));

            await WaitForConditionAsync(
                () => chartView.Source is not null &&
                      chartView.Viewport == CreateZoomedDetailViewport(chartView.Source.Metadata) &&
                      statusText.Text?.Contains("Cache-backed example", StringComparison.Ordinal) == true,
                "switching sources should preserve the selected detail viewport using the active metadata.")
                .ConfigureAwait(true);

            chartView.Viewport.Should().Be(CreateZoomedDetailViewport(chartView.Source!.Metadata));
            statusText.Text.Should().Contain("Cache-backed example");

            SelectItem(viewportSelector, GetComboBoxItemByTag(viewportSelector, "overview"));

            await WaitForConditionAsync(
                () => chartView.Viewport == CreateOverviewViewport(chartView.Source!.Metadata),
                "selecting the overview viewport should restore the full-metadata viewport.")
                .ConfigureAwait(true);

            chartView.Viewport.Should().Be(CreateOverviewViewport(chartView.Source!.Metadata));
            statusText.Text.Should().Contain("Overview");
        });
    }

    [Fact]
    public Task DemoWindow_ViewportSelectionThrowsForUnknownViewportTag()
    {
        return AvaloniaHeadlessTestSession.RunAsync(() =>
        {
            var window = new MainWindow();
            var viewportSelector = window.FindControl<ComboBox>("ViewportSelector")
                ?? throw new InvalidOperationException("ViewportSelector is missing.");
            var detailItem = GetComboBoxItemByTag(viewportSelector, "detail");

            detailItem.Tag = "unexpected";

            Action act = () => SelectItem(viewportSelector, detailItem);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Viewport selector tag 'unexpected' is not supported. Expected 'overview' or 'detail'.");

            return Task.CompletedTask;
        });
    }

    [Fact]
    public Task DemoWindow_ViewportSelectionThrowsForMissingViewportTag()
    {
        return AvaloniaHeadlessTestSession.RunAsync(() =>
        {
            var window = new MainWindow();
            var viewportSelector = window.FindControl<ComboBox>("ViewportSelector")
                ?? throw new InvalidOperationException("ViewportSelector is missing.");
            var detailItem = GetComboBoxItemByTag(viewportSelector, "detail");

            detailItem.Tag = null;

            Action act = () => SelectItem(viewportSelector, detailItem);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Viewport selector items must define a non-empty string tag.");

            return Task.CompletedTask;
        });
    }

    private static ComboBoxItem GetComboBoxItemByTag(ComboBox selector, string tag)
    {
        var item = selector.Items
            .Cast<object?>()
            .OfType<ComboBoxItem>()
            .SingleOrDefault(candidate => string.Equals(candidate.Tag as string, tag, StringComparison.Ordinal));

        item.Should().NotBeNull($"Expected a ComboBoxItem tagged '{tag}'.");
        return item!;
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

    private static SurfaceViewport CreateOverviewViewport(SurfaceMetadata metadata)
    {
        return new SurfaceViewport(0d, 0d, metadata.Width, metadata.Height);
    }

    private static SurfaceViewport CreateZoomedDetailViewport(SurfaceMetadata metadata)
    {
        var detailWidth = Math.Max(1d, metadata.Width / 4d);
        var detailHeight = Math.Max(1d, metadata.Height / 4d);
        var startX = (metadata.Width - detailWidth) / 2d;
        var startY = (metadata.Height - detailHeight) / 2d;

        return new SurfaceViewport(startX, startY, detailWidth, detailHeight).ClampTo(metadata);
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
