using System.Collections.ObjectModel;
using System.Globalization;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes deterministic dataset evidence for the current Plot-authored series.
/// </summary>
public sealed class Plot3DDatasetEvidence
{
    private Plot3DDatasetEvidence(
        int plotRevision,
        int activeSeriesIndex,
        string precisionProfile,
        IReadOnlyList<Plot3DSeriesDatasetEvidence> series)
    {
        PlotRevision = plotRevision;
        ActiveSeriesIndex = activeSeriesIndex;
        PrecisionProfile = precisionProfile;
        Series = new ReadOnlyCollection<Plot3DSeriesDatasetEvidence>(series.ToArray());
    }

    /// <summary>
    /// Gets the evidence kind.
    /// </summary>
    public string EvidenceKind { get; } = "Plot3DDatasetEvidence";

    /// <summary>
    /// Gets the Plot revision that produced this evidence snapshot.
    /// </summary>
    public int PlotRevision { get; }

    /// <summary>
    /// Gets the active series index, or <c>-1</c> when the Plot has no series.
    /// </summary>
    public int ActiveSeriesIndex { get; }

    /// <summary>
    /// Gets the deterministic numeric precision profile used by Plot labels.
    /// </summary>
    public string PrecisionProfile { get; }

    /// <summary>
    /// Gets one evidence entry per current Plot series in draw order.
    /// </summary>
    public IReadOnlyList<Plot3DSeriesDatasetEvidence> Series { get; }

    internal static Plot3DDatasetEvidence Create(
        int plotRevision,
        IReadOnlyList<Plot3DSeries> series,
        SurfaceChartOverlayOptions overlayOptions,
        IReadOnlyList<Plot3DSeries> activeComposedSeries)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(overlayOptions);
        ArgumentNullException.ThrowIfNull(activeComposedSeries);

        var activeSeriesIndex = FindActiveSeriesIndex(series);
        var activeSeries = activeComposedSeries.ToHashSet();
        var seriesEvidence = new Plot3DSeriesDatasetEvidence[series.Count];
        for (var index = 0; index < series.Count; index++)
        {
            seriesEvidence[index] = Plot3DSeriesDatasetEvidence.Create(
                index,
                activeSeries.Contains(series[index]),
                series[index]);
        }

        return new Plot3DDatasetEvidence(
            plotRevision,
            activeSeriesIndex,
            SurfaceChartOverlayEvidenceFormatter.DescribePrecisionProfile(overlayOptions),
            seriesEvidence);
    }

    private static int FindActiveSeriesIndex(IReadOnlyList<Plot3DSeries> series)
    {
        for (var index = series.Count - 1; index >= 0; index--)
        {
            if (series[index].IsVisible)
            {
                return index;
            }
        }

        return -1;
    }
}

/// <summary>
/// Describes deterministic dataset evidence for one Plot-authored series.
/// </summary>
public sealed class Plot3DSeriesDatasetEvidence
{
    private Plot3DSeriesDatasetEvidence(
        int index,
        bool isActive,
        string identity,
        string? name,
        Plot3DSeriesKind kind,
        int width,
        int height,
        long sampleCount,
        int seriesCount,
        int pointCount,
        int columnarSeriesCount,
        int columnarPointCount,
        int pickablePointCount,
        int streamingAppendBatchCount,
        int streamingReplaceBatchCount,
        long streamingDroppedPointCount,
        int lastStreamingDroppedPointCount,
        int configuredFifoCapacity,
        SurfaceAxisDatasetEvidence? horizontalAxis,
        SurfaceAxisDatasetEvidence? verticalAxis,
        SurfaceAxisDatasetEvidence? depthAxis,
        SurfaceValueRangeDatasetEvidence? valueRange,
        string samplingProfile,
        IReadOnlyList<string> categoryLabels,
        IReadOnlyList<ScatterColumnarSeriesDatasetEvidence> columnarSeries)
    {
        Index = index;
        IsActive = isActive;
        Identity = identity;
        Name = name;
        Kind = kind;
        Width = width;
        Height = height;
        SampleCount = sampleCount;
        SeriesCount = seriesCount;
        PointCount = pointCount;
        ColumnarSeriesCount = columnarSeriesCount;
        ColumnarPointCount = columnarPointCount;
        PickablePointCount = pickablePointCount;
        StreamingAppendBatchCount = streamingAppendBatchCount;
        StreamingReplaceBatchCount = streamingReplaceBatchCount;
        StreamingDroppedPointCount = streamingDroppedPointCount;
        LastStreamingDroppedPointCount = lastStreamingDroppedPointCount;
        ConfiguredFifoCapacity = configuredFifoCapacity;
        HorizontalAxis = horizontalAxis;
        VerticalAxis = verticalAxis;
        DepthAxis = depthAxis;
        ValueRange = valueRange;
        SamplingProfile = samplingProfile;
        CategoryLabels = new ReadOnlyCollection<string>(categoryLabels.ToArray());
        ColumnarSeries = new ReadOnlyCollection<ScatterColumnarSeriesDatasetEvidence>(columnarSeries.ToArray());
    }

    /// <summary>
    /// Gets the draw-order index in the current Plot series list.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets a value indicating whether this series is the Plot active series.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Gets the deterministic Plot-local series identity.
    /// </summary>
    public string Identity { get; }

    /// <summary>
    /// Gets the optional Plot-authored series name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the chart family for this dataset.
    /// </summary>
    public Plot3DSeriesKind Kind { get; }

    /// <summary>
    /// Gets the surface or waterfall sample width, or zero for scatter datasets.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the surface or waterfall sample height, or zero for scatter datasets.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the surface or waterfall sample count, or zero for scatter datasets.
    /// </summary>
    public long SampleCount { get; }

    /// <summary>
    /// Gets the logical series count represented by this dataset.
    /// </summary>
    public int SeriesCount { get; }

    /// <summary>
    /// Gets the total scatter point count, or zero for surface and waterfall datasets.
    /// </summary>
    public int PointCount { get; }

    /// <summary>
    /// Gets the number of columnar scatter series.
    /// </summary>
    public int ColumnarSeriesCount { get; }

    /// <summary>
    /// Gets the retained point count across columnar scatter series.
    /// </summary>
    public int ColumnarPointCount { get; }

    /// <summary>
    /// Gets the retained point count that participates in scatter picking.
    /// </summary>
    public int PickablePointCount { get; }

    /// <summary>
    /// Gets the total append batches applied to columnar scatter series.
    /// </summary>
    public int StreamingAppendBatchCount { get; }

    /// <summary>
    /// Gets the total replacement batches applied to columnar scatter series.
    /// </summary>
    public int StreamingReplaceBatchCount { get; }

    /// <summary>
    /// Gets the total points dropped by FIFO trimming across columnar scatter series.
    /// </summary>
    public long StreamingDroppedPointCount { get; }

    /// <summary>
    /// Gets the points dropped by the most recent columnar scatter update.
    /// </summary>
    public int LastStreamingDroppedPointCount { get; }

    /// <summary>
    /// Gets the sum of configured FIFO capacities across bounded columnar scatter series.
    /// </summary>
    public int ConfiguredFifoCapacity { get; }

    /// <summary>
    /// Gets horizontal-axis metadata when the dataset declares one.
    /// </summary>
    public SurfaceAxisDatasetEvidence? HorizontalAxis { get; }

    /// <summary>
    /// Gets surface or waterfall vertical-axis metadata when available.
    /// </summary>
    public SurfaceAxisDatasetEvidence? VerticalAxis { get; }

    /// <summary>
    /// Gets scatter depth-axis metadata when available.
    /// </summary>
    public SurfaceAxisDatasetEvidence? DepthAxis { get; }

    /// <summary>
    /// Gets the dataset value range.
    /// </summary>
    public SurfaceValueRangeDatasetEvidence? ValueRange { get; }

    /// <summary>
    /// Gets a deterministic sampling description for reproduction notes.
    /// </summary>
    public string SamplingProfile { get; }

    /// <summary>
    /// Gets bar category labels, or an empty collection when the dataset does not declare labels.
    /// </summary>
    public IReadOnlyList<string> CategoryLabels { get; }

    /// <summary>
    /// Gets high-volume scatter series evidence.
    /// </summary>
    public IReadOnlyList<ScatterColumnarSeriesDatasetEvidence> ColumnarSeries { get; }

    internal static Plot3DSeriesDatasetEvidence Create(int index, bool isActive, Plot3DSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        return series.Kind switch
        {
            Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall => CreateSurfaceEvidence(index, isActive, series),
            Plot3DSeriesKind.Scatter => CreateScatterEvidence(index, isActive, series),
            Plot3DSeriesKind.Bar => CreateBarEvidence(index, isActive, series),
            Plot3DSeriesKind.Contour => CreateContourEvidence(index, isActive, series),
            _ => throw new ArgumentOutOfRangeException(nameof(series), $"Unsupported Plot series kind: {series.Kind}"),
        };
    }

    private static Plot3DSeriesDatasetEvidence CreateSurfaceEvidence(int index, bool isActive, Plot3DSeries series)
    {
        var metadata = series.SurfaceSource?.Metadata
            ?? throw new InvalidOperationException("Surface and waterfall series require surface metadata.");

        return new Plot3DSeriesDatasetEvidence(
            index,
            isActive,
            CreateIdentity(index, series),
            series.Name,
            series.Kind,
            metadata.Width,
            metadata.Height,
            metadata.SampleCount,
            seriesCount: 1,
            pointCount: 0,
            columnarSeriesCount: 0,
            columnarPointCount: 0,
            pickablePointCount: 0,
            streamingAppendBatchCount: 0,
            streamingReplaceBatchCount: 0,
            streamingDroppedPointCount: 0,
            lastStreamingDroppedPointCount: 0,
            configuredFifoCapacity: 0,
            SurfaceAxisDatasetEvidence.Create(metadata.HorizontalAxis),
            SurfaceAxisDatasetEvidence.Create(metadata.VerticalAxis),
            depthAxis: null,
            SurfaceValueRangeDatasetEvidence.Create(metadata.ValueRange),
            CreateSurfaceSamplingProfile(metadata),
            categoryLabels: [],
            []);
    }

    private static Plot3DSeriesDatasetEvidence CreateScatterEvidence(int index, bool isActive, Plot3DSeries series)
    {
        var data = series.ScatterData
            ?? throw new InvalidOperationException("Scatter series require scatter data.");

        return new Plot3DSeriesDatasetEvidence(
            index,
            isActive,
            CreateIdentity(index, series),
            series.Name,
            series.Kind,
            width: 0,
            height: 0,
            sampleCount: 0,
            data.SeriesCount,
            data.PointCount,
            data.ColumnarSeriesCount,
            data.ColumnarPointCount,
            data.PickablePointCount,
            data.StreamingAppendBatchCount,
            data.StreamingReplaceBatchCount,
            data.StreamingDroppedPointCount,
            data.LastStreamingDroppedPointCount,
            data.ConfiguredFifoCapacity,
            SurfaceAxisDatasetEvidence.Create(data.Metadata.HorizontalAxis),
            verticalAxis: null,
            SurfaceAxisDatasetEvidence.Create(data.Metadata.DepthAxis),
            SurfaceValueRangeDatasetEvidence.Create(data.Metadata.ValueRange),
            "ScatterPoints",
            categoryLabels: [],
            CreateColumnarSeriesEvidence(data.ColumnarSeries));
    }

    private static Plot3DSeriesDatasetEvidence CreateBarEvidence(int index, bool isActive, Plot3DSeries series)
    {
        var data = series.BarData
            ?? throw new InvalidOperationException("Bar series require bar data.");

        return new Plot3DSeriesDatasetEvidence(
            index,
            isActive,
            CreateIdentity(index, series),
            series.Name,
            series.Kind,
            width: 0,
            height: 0,
            sampleCount: 0,
            seriesCount: data.SeriesCount,
            pointCount: data.CategoryCount,
            columnarSeriesCount: 0,
            columnarPointCount: 0,
            pickablePointCount: 0,
            streamingAppendBatchCount: 0,
            streamingReplaceBatchCount: 0,
            streamingDroppedPointCount: 0,
            lastStreamingDroppedPointCount: 0,
            configuredFifoCapacity: 0,
            horizontalAxis: null,
            verticalAxis: null,
            depthAxis: null,
            valueRange: null,
            samplingProfile: CreateBarSamplingProfile(data),
            data.CategoryLabels,
            []);
    }

    private static Plot3DSeriesDatasetEvidence CreateContourEvidence(int index, bool isActive, Plot3DSeries series)
    {
        var data = series.ContourData
            ?? throw new InvalidOperationException("Contour series require contour data.");

        return new Plot3DSeriesDatasetEvidence(
            index,
            isActive,
            CreateIdentity(index, series),
            series.Name,
            series.Kind,
            data.Field.Width,
            data.Field.Height,
            data.Field.Width * data.Field.Height,
            seriesCount: 1,
            pointCount: 0,
            columnarSeriesCount: 0,
            columnarPointCount: 0,
            pickablePointCount: 0,
            streamingAppendBatchCount: 0,
            streamingReplaceBatchCount: 0,
            streamingDroppedPointCount: 0,
            lastStreamingDroppedPointCount: 0,
            configuredFifoCapacity: 0,
            horizontalAxis: SurfaceAxisDatasetEvidence.Create(new SurfaceAxisDescriptor("X", null, 0d, data.Field.Width - 1)),
            verticalAxis: null,
            depthAxis: SurfaceAxisDatasetEvidence.Create(new SurfaceAxisDescriptor("Y", null, 0d, data.Field.Height - 1)),
            valueRange: SurfaceValueRangeDatasetEvidence.Create(data.Field.Range),
            samplingProfile: $"ContourPlot:Width={data.Field.Width};Height={data.Field.Height};Levels={data.LevelCount}",
            categoryLabels: [],
            []);
    }

    private static IReadOnlyList<ScatterColumnarSeriesDatasetEvidence> CreateColumnarSeriesEvidence(
        IReadOnlyList<ScatterColumnarSeries> columnarSeries)
    {
        var evidence = new ScatterColumnarSeriesDatasetEvidence[columnarSeries.Count];
        for (var index = 0; index < columnarSeries.Count; index++)
        {
            evidence[index] = ScatterColumnarSeriesDatasetEvidence.Create(index, columnarSeries[index]);
        }

        return evidence;
    }

    private static string CreateIdentity(int index, Plot3DSeries series)
    {
        return Plot3D.CreateSeriesDatasetIdentity(index, series);
    }

    private static string CreateSurfaceSamplingProfile(SurfaceMetadata metadata)
    {
        if (metadata.Geometry is SurfaceExplicitGrid)
        {
            return string.Create(CultureInfo.InvariantCulture, $"ExplicitCoordinates:Width={metadata.Width};Height={metadata.Height}");
        }

        var horizontalSpacing = metadata.Width > 1 ? metadata.HorizontalAxis.Span / (metadata.Width - 1d) : 0d;
        var verticalSpacing = metadata.Height > 1 ? metadata.VerticalAxis.Span / (metadata.Height - 1d) : 0d;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"RegularGrid:XSpacing={horizontalSpacing:G17};YSpacing={verticalSpacing:G17}");
    }

    private static string CreateBarSamplingProfile(BarChartData data)
    {
        var profile = $"BarChart:Categories={data.CategoryCount};Series={data.SeriesCount};Layout={data.Layout}";
        return data.CategoryLabels.Count > 0
            ? $"{profile};CategoryLabels={data.CategoryLabels.Count}"
            : profile;
    }
}

/// <summary>
/// Describes deterministic axis metadata for dataset evidence.
/// </summary>
public sealed class SurfaceAxisDatasetEvidence
{
    private SurfaceAxisDatasetEvidence(string label, string? unit, double minimum, double maximum, SurfaceAxisScaleKind scaleKind)
    {
        Label = label;
        Unit = unit;
        Minimum = minimum;
        Maximum = maximum;
        ScaleKind = scaleKind;
    }

    /// <summary>
    /// Gets the axis label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the optional axis unit.
    /// </summary>
    public string? Unit { get; }

    /// <summary>
    /// Gets the inclusive axis minimum.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive axis maximum.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets the axis scale semantics.
    /// </summary>
    public SurfaceAxisScaleKind ScaleKind { get; }

    internal static SurfaceAxisDatasetEvidence Create(SurfaceAxisDescriptor axis)
    {
        ArgumentNullException.ThrowIfNull(axis);
        return new SurfaceAxisDatasetEvidence(axis.Label, axis.Unit, axis.Minimum, axis.Maximum, axis.ScaleKind);
    }
}

/// <summary>
/// Describes deterministic value-range metadata for dataset evidence.
/// </summary>
public sealed class SurfaceValueRangeDatasetEvidence
{
    private SurfaceValueRangeDatasetEvidence(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the inclusive value minimum.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive value maximum.
    /// </summary>
    public double Maximum { get; }

    internal static SurfaceValueRangeDatasetEvidence Create(SurfaceValueRange valueRange)
    {
        return new SurfaceValueRangeDatasetEvidence(valueRange.Minimum, valueRange.Maximum);
    }
}

/// <summary>
/// Describes deterministic high-volume scatter series metadata for dataset evidence.
/// </summary>
public sealed class ScatterColumnarSeriesDatasetEvidence
{
    private ScatterColumnarSeriesDatasetEvidence(
        int index,
        string? label,
        int pointCount,
        bool isSortedX,
        bool containsNaN,
        bool pickable,
        int? fifoCapacity,
        int appendBatchCount,
        int replaceBatchCount,
        long totalAppendedPointCount,
        long totalDroppedPointCount,
        int lastDroppedPointCount)
    {
        Index = index;
        Label = label;
        PointCount = pointCount;
        IsSortedX = isSortedX;
        ContainsNaN = containsNaN;
        Pickable = pickable;
        FifoCapacity = fifoCapacity;
        AppendBatchCount = appendBatchCount;
        ReplaceBatchCount = replaceBatchCount;
        TotalAppendedPointCount = totalAppendedPointCount;
        TotalDroppedPointCount = totalDroppedPointCount;
        LastDroppedPointCount = lastDroppedPointCount;
    }

    /// <summary>
    /// Gets the columnar series index inside the scatter dataset.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the optional columnar series label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the retained point count.
    /// </summary>
    public int PointCount { get; }

    /// <summary>
    /// Gets whether appended X values are known to be sorted.
    /// </summary>
    public bool IsSortedX { get; }

    /// <summary>
    /// Gets whether coordinate columns may contain NaN gaps.
    /// </summary>
    public bool ContainsNaN { get; }

    /// <summary>
    /// Gets whether retained points participate in picking.
    /// </summary>
    public bool Pickable { get; }

    /// <summary>
    /// Gets the optional FIFO retained-point capacity.
    /// </summary>
    public int? FifoCapacity { get; }

    /// <summary>
    /// Gets the number of append batches applied to this series.
    /// </summary>
    public int AppendBatchCount { get; }

    /// <summary>
    /// Gets the number of replacement batches applied to this series.
    /// </summary>
    public int ReplaceBatchCount { get; }

    /// <summary>
    /// Gets the total number of points accepted through append batches.
    /// </summary>
    public long TotalAppendedPointCount { get; }

    /// <summary>
    /// Gets the total number of points dropped by FIFO trimming.
    /// </summary>
    public long TotalDroppedPointCount { get; }

    /// <summary>
    /// Gets the number of points dropped by the most recent update.
    /// </summary>
    public int LastDroppedPointCount { get; }

    internal static ScatterColumnarSeriesDatasetEvidence Create(int index, ScatterColumnarSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        return new ScatterColumnarSeriesDatasetEvidence(
            index,
            series.Label,
            series.Count,
            series.IsSortedX,
            series.ContainsNaN,
            series.Pickable,
            series.FifoCapacity,
            series.AppendBatchCount,
            series.ReplaceBatchCount,
            series.TotalAppendedPointCount,
            series.TotalDroppedPointCount,
            series.LastDroppedPointCount);
    }
}
