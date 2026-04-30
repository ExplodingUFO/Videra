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
        ContourChartData? contourData,
        LineChartData? lineData,
        RibbonChartData? ribbonData,
        VectorFieldChartData? vectorFieldData,
        HeatmapSliceData? heatmapSliceData,
        BoxPlotData? boxPlotData,
        HistogramData? histogramData = null,
        FunctionPlotData? functionPlotData = null,
        PieChartData? pieData = null)
    {
        Kind = kind;
        _label = NormalizeLabel(name);
        SurfaceSource = surfaceSource;
        ScatterData = scatterData;
        BarData = barData;
        ContourData = contourData;
        LineData = lineData;
        RibbonData = ribbonData;
        VectorFieldData = vectorFieldData;
        HeatmapSliceData = heatmapSliceData;
        BoxPlotData = boxPlotData;
        HistogramData = histogramData;
        FunctionPlotData = functionPlotData;
        PieData = pieData;
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

    /// <summary>
    /// Gets the line dataset for line series.
    /// </summary>
    public LineChartData? LineData { get; private set; }

    /// <summary>
    /// Gets the ribbon dataset for ribbon series.
    /// </summary>
    public RibbonChartData? RibbonData { get; private set; }

    /// <summary>
    /// Gets the vector field dataset for vector field series.
    /// </summary>
    public VectorFieldChartData? VectorFieldData { get; private set; }

    /// <summary>
    /// Gets the heatmap slice dataset for heatmap slice series.
    /// </summary>
    public HeatmapSliceData? HeatmapSliceData { get; private set; }

    /// <summary>
    /// Gets the box plot dataset for box plot series.
    /// </summary>
    public BoxPlotData? BoxPlotData { get; private set; }

    /// <summary>
    /// Gets the histogram dataset for histogram series.
    /// </summary>
    public HistogramData? HistogramData { get; private set; }

    /// <summary>
    /// Gets the function plot dataset for function plot series.
    /// </summary>
    public FunctionPlotData? FunctionPlotData { get; private set; }

    /// <summary>
    /// Gets the pie chart dataset for pie series.
    /// </summary>
    public PieChartData? PieData { get; private set; }

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

    private protected void ReplaceLineData(LineChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.Line)
        {
            throw new InvalidOperationException("Only line series can update line data.");
        }

        if (ReferenceEquals(LineData, data))
        {
            return;
        }

        LineData = data;
        NotifyChanged();
    }

    private protected void ReplaceRibbonData(RibbonChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.Ribbon)
        {
            throw new InvalidOperationException("Only ribbon series can update ribbon data.");
        }

        if (ReferenceEquals(RibbonData, data))
        {
            return;
        }

        RibbonData = data;
        NotifyChanged();
    }

    private protected void ReplaceVectorFieldData(VectorFieldChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.VectorField)
        {
            throw new InvalidOperationException("Only vector field series can update vector field data.");
        }

        if (ReferenceEquals(VectorFieldData, data))
        {
            return;
        }

        VectorFieldData = data;
        NotifyChanged();
    }

    private protected void ReplaceHeatmapSliceData(HeatmapSliceData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.HeatmapSlice)
        {
            throw new InvalidOperationException("Only heatmap slice series can update heatmap slice data.");
        }

        if (ReferenceEquals(HeatmapSliceData, data))
        {
            return;
        }

        HeatmapSliceData = data;
        NotifyChanged();
    }

    private protected void ReplaceBoxPlotData(BoxPlotData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (Kind != Plot3DSeriesKind.BoxPlot)
        {
            throw new InvalidOperationException("Only box plot series can update box plot data.");
        }

        if (ReferenceEquals(BoxPlotData, data))
        {
            return;
        }

        BoxPlotData = data;
        NotifyChanged();
    }

    private static string? NormalizeLabel(string? label)
    {
        return string.IsNullOrWhiteSpace(label) ? null : label;
    }
}
