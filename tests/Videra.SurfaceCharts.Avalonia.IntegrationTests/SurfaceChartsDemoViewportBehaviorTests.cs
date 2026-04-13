using System.Reflection;
using Avalonia.Controls;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Views;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartsDemoViewportBehaviorTests
{
    [Fact]
    public Task DemoWindow_ViewportSelectionAndSourceSwitch_UseExpectedViewportPresets()
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

            AssertViewport(chartView.Viewport, 0d, 0d, 64d, 48d);

            var detailViewportItem = GetComboBoxItemByTag(viewportSelector, "detail");
            SelectItem(viewportSelector, detailViewportItem);
            InvokeSelectionChanged(window, "OnViewportSelectionChanged", viewportSelector);

            AssertViewport(chartView.Viewport, 24d, 18d, 16d, 12d);
            statusText.Text.Should().Contain("Zoomed detail");

            var cacheSourceItem = GetComboBoxItemByContent(sourceSelector, "Cache-backed example");
            SelectItem(sourceSelector, cacheSourceItem);
            InvokeSelectionChanged(window, "OnSourceSelectionChanged", sourceSelector);

            await WaitForConditionAsync(
                () => ViewportMatches(chartView.Viewport, 24d, 18d, 16d, 12d) &&
                      statusText.Text?.Contains("Cache-backed example", StringComparison.Ordinal) == true,
                "switching to the cache-backed sample should preserve the selected detail viewport.");

            AssertViewport(chartView.Viewport, 24d, 18d, 16d, 12d);
            statusText.Text.Should().Contain("Cache-backed example");
        });
    }

    private static ComboBoxItem GetComboBoxItemByTag(ComboBox selector, string tag)
    {
        var item = selector.Items
            .Cast<object?>()
            .OfType<ComboBoxItem>()
            .SingleOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        item.Should().NotBeNull($"Expected a ComboBoxItem tagged '{tag}'.");
        return item!;
    }

    private static ComboBoxItem GetComboBoxItemByContent(ComboBox selector, string content)
    {
        var item = selector.Items
            .Cast<object?>()
            .OfType<ComboBoxItem>()
            .SingleOrDefault(item => string.Equals(item.Content as string, content, StringComparison.Ordinal));

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
        selector.SelectedItem = item;
        selector.SelectedIndex = itemIndex;
    }

    private static void InvokeSelectionChanged(MainWindow window, string methodName, ComboBox selector)
    {
        var method = typeof(MainWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull($"Expected {nameof(MainWindow)} to expose '{methodName}' for deterministic demo selection handling.");
        method!.Invoke(window, [selector, null]);
    }

    private static void AssertViewport(SurfaceViewport viewport, double startX, double startY, double width, double height)
    {
        ViewportMatches(viewport, startX, startY, width, height).Should().BeTrue(
            $"Expected viewport ({startX}, {startY}, {width}, {height}) but found {FormatViewport(viewport)}.");
    }

    private static bool ViewportMatches(SurfaceViewport viewport, double startX, double startY, double width, double height)
    {
        return GetViewportComponent(viewport, "StartX") == startX &&
               GetViewportComponent(viewport, "StartY") == startY &&
               GetViewportComponent(viewport, "Width") == width &&
               GetViewportComponent(viewport, "Height") == height;
    }

    private static string FormatViewport(SurfaceViewport viewport)
    {
        return $"({GetViewportComponent(viewport, "StartX")}, {GetViewportComponent(viewport, "StartY")}, {GetViewportComponent(viewport, "Width")}, {GetViewportComponent(viewport, "Height")})";
    }

    private static double GetViewportComponent(SurfaceViewport viewport, string propertyName)
    {
        var property = typeof(SurfaceViewport).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull($"Expected SurfaceViewport to expose '{propertyName}'.");
        return (double)property!.GetValue(viewport)!;
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
