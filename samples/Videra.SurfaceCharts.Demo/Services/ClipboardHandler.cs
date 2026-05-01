using Avalonia.Controls;
using Avalonia.Interactivity;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Handles clipboard-related commands: copy support summary, copy recipe snippet,
/// capture PNG snapshot, and copy workspace evidence.
/// </summary>
internal sealed class ClipboardHandler
{
    private readonly Func<TopLevel?> _getTopLevel;
    private readonly TextBlock _supportSummaryText;
    private readonly TextBlock _supportSummaryStatusText;
    private readonly TextBlock _cookbookRecipeSnippetText;
    private readonly TextBlock _cookbookRecipeStatusText;
    private readonly TextBlock _statusText;
    private readonly TextBlock _workspaceStatusText;
    private readonly Func<VideraChartView> _getActiveChartView;
    private readonly Func<SurfaceChartWorkspaceService?> _getWorkspaceService;
    private readonly Action _updateSupportSummaryText;

    private PlotSnapshotResult? _lastSnapshotResult;

    internal ClipboardHandler(
        Func<TopLevel?> getTopLevel,
        TextBlock supportSummaryText,
        TextBlock supportSummaryStatusText,
        TextBlock cookbookRecipeSnippetText,
        TextBlock cookbookRecipeStatusText,
        TextBlock statusText,
        TextBlock workspaceStatusText,
        Func<VideraChartView> getActiveChartView,
        Func<SurfaceChartWorkspaceService?> getWorkspaceService,
        Action updateSupportSummaryText)
    {
        _getTopLevel = getTopLevel;
        _supportSummaryText = supportSummaryText;
        _supportSummaryStatusText = supportSummaryStatusText;
        _cookbookRecipeSnippetText = cookbookRecipeSnippetText;
        _cookbookRecipeStatusText = cookbookRecipeStatusText;
        _statusText = statusText;
        _workspaceStatusText = workspaceStatusText;
        _getActiveChartView = getActiveChartView;
        _getWorkspaceService = getWorkspaceService;
        _updateSupportSummaryText = updateSupportSummaryText;
    }

    internal PlotSnapshotResult? LastSnapshotResult => _lastSnapshotResult;

    internal async void OnCopySupportSummaryClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        _updateSupportSummaryText();

        var topLevel = _getTopLevel();
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(_supportSummaryText.Text ?? string.Empty).ConfigureAwait(true);
            _supportSummaryStatusText.Text = "Copied support summary to the clipboard.";
            return;
        }

        _supportSummaryStatusText.Text = "Clipboard is unavailable. The support summary remains visible below.";
    }

    internal async void OnCopyRecipeSnippetClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var topLevel = _getTopLevel();
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(_cookbookRecipeSnippetText.Text ?? string.Empty).ConfigureAwait(true);
            _cookbookRecipeStatusText.Text = "Copied cookbook recipe snippet to the clipboard.";
            return;
        }

        _cookbookRecipeStatusText.Text = "Clipboard is unavailable. The cookbook recipe snippet remains visible below.";
    }

    internal async void OnCaptureSnapshotClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        try
        {
            var chartView = _getActiveChartView();
            var outputPath = Path.Combine(Path.GetTempPath(), $"videra-surfacecharts-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
            var result = await chartView.Plot.SavePngAsync(outputPath, width: 1920, height: 1080).ConfigureAwait(true);
            _lastSnapshotResult = result;

            if (result.Succeeded)
            {
                _statusText.Text = $"PNG saved with Plot.SavePngAsync: {result.Path}";
                _supportSummaryStatusText.Text = $"Snapshot state: PNG captured at {result.Path}. The support summary now includes SnapshotStatus and SnapshotPath.";
            }
            else
            {
                _statusText.Text = $"Snapshot failed: {result.Failure?.Message}";
                _supportSummaryStatusText.Text = $"Snapshot state: capture failed. SnapshotStatus remains failed in the support summary.";
            }

            _updateSupportSummaryText();
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Snapshot failed: {ex.Message}";
            _supportSummaryStatusText.Text = $"Snapshot state: capture failed before a snapshot result was created. {ex.Message}";
            _updateSupportSummaryText();
        }
    }

    internal async void OnCopyWorkspaceEvidenceClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_getWorkspaceService() is not { } workspaceService)
        {
            _workspaceStatusText.Text = "No workspace is active.";
            return;
        }

        var evidence = workspaceService.GetWorkspaceEvidence();
        var topLevel = _getTopLevel();
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(evidence).ConfigureAwait(true);
            _workspaceStatusText.Text = "Copied workspace evidence to clipboard.";
            return;
        }

        _workspaceStatusText.Text = "Clipboard is unavailable. Workspace evidence remains in memory.";
    }
}
