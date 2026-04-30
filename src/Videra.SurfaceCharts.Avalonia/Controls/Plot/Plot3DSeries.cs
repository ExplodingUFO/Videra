using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes one series attached to a <see cref="Plot3D"/>.
/// </summary>
public class Plot3DSeries : IPlottable3D
{
    private Action? _changed;
    private bool _isVisible = true;
    private string? _label;

    internal Plot3DSeries(
        Plot3DSeriesKind kind,
        string? name,
        ISurfaceTileSource? surfaceSource,
        ScatterChartData? scatterData,
        BarChartData? barData,
        ContourChartData? contourData)
    {
        Kind = kind;
        _label = NormalizeLabel(name);
        SurfaceSource = surfaceSource;
        ScatterData = scatterData;
        BarData = barData;
        ContourData = contourData;
    }

    /// <summary>
    /// Gets the chart family for this series.
    /// </summary>
    public Plot3DSeriesKind Kind { get; }

    /// <summary>
    /// Gets the optional host-facing series name.
    /// </summary>
    public string? Name => Label;

    /// <inheritdoc />
    public string? Label
    {
        get => _label;
        set
        {
            var normalized = NormalizeLabel(value);
            if (string.Equals(_label, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _label = normalized;
            NotifyChanged();
        }
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
            {
                return;
            }

            _isVisible = value;
            NotifyChanged();
        }
    }

    /// <summary>
    /// Gets the tiled surface source for surface and waterfall series.
    /// </summary>
    public ISurfaceTileSource? SurfaceSource { get; }

    /// <summary>
    /// Gets the scatter dataset for scatter series.
    /// </summary>
    public ScatterChartData? ScatterData { get; }

    /// <summary>
    /// Gets the bar dataset for bar chart series.
    /// </summary>
    public BarChartData? BarData { get; private set; }

    /// <summary>
    /// Gets the contour dataset for contour series.
    /// </summary>
    public ContourChartData? ContourData { get; }

    internal void Attach(Action changed)
    {
        _changed = changed ?? throw new ArgumentNullException(nameof(changed));
    }

    private protected void NotifyChanged()
    {
        _changed?.Invoke();
    }

    private protected void ReplaceBarData(BarChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.Bar)
        {
            throw new InvalidOperationException("Only bar series can update bar data.");
        }

        if (ReferenceEquals(BarData, data))
        {
            return;
        }

        BarData = data;
        NotifyChanged();
    }

    private static string? NormalizeLabel(string? label)
    {
        return string.IsNullOrWhiteSpace(label) ? null : label;
    }
}
