using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class VideraChartView
{
    /// <summary>
    /// Links this chart view with one peer until the returned lifetime is disposed.
    /// </summary>
    /// <param name="peer">The chart view to synchronize with this view.</param>
    /// <returns>A deterministic lifetime that disconnects the two chart views.</returns>
    public IDisposable LinkViewWith(VideraChartView peer)
    {
        ArgumentNullException.ThrowIfNull(peer);
        if (ReferenceEquals(this, peer))
        {
            throw new ArgumentException("A chart view cannot link to itself.", nameof(peer));
        }

        return new VideraChartViewLink(this, peer);
    }
}

/// <summary>
/// Synchronizes the persisted view state of exactly two chart views.
/// </summary>
public sealed class VideraChartViewLink : IDisposable
{
    private readonly VideraChartView _first;
    private readonly VideraChartView _second;
    private bool _disposed;
    private bool _isSynchronizing;

    internal VideraChartViewLink(VideraChartView first, VideraChartView second)
    {
        _first = first;
        _second = second;
        _first.PropertyChanged += OnFirstPropertyChanged;
        _second.PropertyChanged += OnSecondPropertyChanged;
        CopyViewState(_first, _second);
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
            CopyViewState(_first, _second);
        }
    }

    private void OnSecondPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == VideraChartView.ViewStateProperty)
        {
            CopyViewState(_second, _first);
        }
    }

    private void CopyViewState(VideraChartView source, VideraChartView target)
    {
        if (_disposed || _isSynchronizing || target.ViewState == source.ViewState)
        {
            return;
        }

        _isSynchronizing = true;
        try
        {
            target.ViewState = source.ViewState;
        }
        finally
        {
            _isSynchronizing = false;
        }
    }
}
