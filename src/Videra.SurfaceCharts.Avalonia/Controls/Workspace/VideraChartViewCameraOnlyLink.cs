using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Synchronizes only the camera pose between exactly two chart views,
/// preserving each chart's own data window and display space.
/// </summary>
internal sealed class VideraChartViewCameraOnlyLink : IDisposable
{
    private readonly VideraChartView _first;
    private readonly VideraChartView _second;
    private bool _disposed;
    private bool _isSynchronizing;

    internal VideraChartViewCameraOnlyLink(VideraChartView first, VideraChartView second)
    {
        _first = first;
        _second = second;
        _first.PropertyChanged += OnFirstPropertyChanged;
        _second.PropertyChanged += OnSecondPropertyChanged;
        CopyCamera(_first, _second);
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
            CopyCamera(_first, _second);
        }
    }

    private void OnSecondPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == VideraChartView.ViewStateProperty)
        {
            CopyCamera(_second, _first);
        }
    }

    private void CopyCamera(VideraChartView source, VideraChartView target)
    {
        if (_disposed || _isSynchronizing)
        {
            return;
        }

        var sourceState = source.ViewState;
        var targetState = target.ViewState;

        if (targetState.Camera == sourceState.Camera)
        {
            return;
        }

        _isSynchronizing = true;
        try
        {
            target.ViewState = new SurfaceViewState(targetState.DataWindow, sourceState.Camera, targetState.DisplaySpace);
        }
        finally
        {
            _isSynchronizing = false;
        }
    }
}
