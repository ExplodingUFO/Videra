using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Synchronizes only the data window between exactly two chart views,
/// preserving each chart's own camera pose and display space.
/// </summary>
internal sealed class VideraChartViewAxisOnlyLink : IDisposable
{
    private readonly VideraChartView _first;
    private readonly VideraChartView _second;
    private bool _disposed;
    private bool _isSynchronizing;

    internal VideraChartViewAxisOnlyLink(VideraChartView first, VideraChartView second)
    {
        _first = first;
        _second = second;
        _first.PropertyChanged += OnFirstPropertyChanged;
        _second.PropertyChanged += OnSecondPropertyChanged;
        CopyDataWindow(_first, _second);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _first.PropertyChanged -= OnFirstPropertyChanged;
        _second.PropertyChanged -= OnSecondPropertyChanged;
        _disposed = true;
    }

    private void OnFirstPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == VideraChartView.ViewStateProperty)
        {
            CopyDataWindow(_first, _second);
        }
    }

    private void OnSecondPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == VideraChartView.ViewStateProperty)
        {
            CopyDataWindow(_second, _first);
        }
    }

    private void CopyDataWindow(VideraChartView source, VideraChartView target)
    {
        if (_disposed || _isSynchronizing)
        {
            return;
        }

        var sourceState = source.ViewState;
        var targetState = target.ViewState;

        if (targetState.DataWindow == sourceState.DataWindow)
        {
            return;
        }

        _isSynchronizing = true;
        try
        {
            target.ViewState = new SurfaceViewState(sourceState.DataWindow, targetState.Camera, targetState.DisplaySpace);
        }
        finally
        {
            _isSynchronizing = false;
        }
    }
}
