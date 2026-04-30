using System.Globalization;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal static class Plot3DSeriesComposition
{
    public static ISurfaceTileSource? CreateSurfaceSource(IReadOnlyList<Plot3DSeries> series)
    {
        var sources = series
            .Select(static item => item.SurfaceSource)
            .OfType<ISurfaceTileSource>()
            .ToArray();

        return sources.Length switch
        {
            0 => null,
            1 => sources[0],
            _ => new ComposedSurfaceTileSource(sources),
        };
    }

    public static ScatterChartData? CreateScatterData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.ScatterData)
            .OfType<ScatterChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var metadata = CreateScatterMetadata(datasets);
        var pointSeries = datasets.SelectMany(static data => data.Series).ToArray();
        var columnarSeries = datasets.SelectMany(static data => data.ColumnarSeries).ToArray();
        return new ScatterChartData(metadata, pointSeries, columnarSeries);
    }

    public static BarChartData? CreateBarData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.BarData)
            .OfType<BarChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var first = datasets[0];
        var compatible = datasets.All(data =>
            data.Layout == first.Layout &&
            data.CategoryCount == first.CategoryCount);
        if (!compatible)
        {
            return datasets[^1];
        }

        return new BarChartData(datasets.SelectMany(static data => data.Series).ToArray(), first.Layout);
    }

    public static LineChartData? CreateLineData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.LineData)
            .OfType<LineChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var allSeries = datasets.SelectMany(static data => data.Series).ToArray();
        var metadata = CreateScatterMetadata(datasets.Select(static d => d.Metadata).ToArray());
        return new LineChartData(allSeries, metadata);
    }

    public static RibbonChartData? CreateRibbonData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.RibbonData)
            .OfType<RibbonChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var allSeries = datasets.SelectMany(static data => data.Series).ToArray();
        var metadata = CreateScatterMetadata(datasets.Select(static d => d.Metadata).ToArray());
        return new RibbonChartData(allSeries, metadata);
    }

    public static VectorFieldChartData? CreateVectorFieldData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.VectorFieldData)
            .OfType<VectorFieldChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var allPoints = datasets.SelectMany(static data => data.Points).ToArray();
        var first = datasets[0];
        return new VectorFieldChartData(allPoints, first.HorizontalAxis, first.DepthAxis, first.MagnitudeRange);
    }

    public static HeatmapSliceData? CreateHeatmapSliceData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.HeatmapSliceData)
            .OfType<HeatmapSliceData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        return datasets[^1];
    }

    public static BoxPlotData? CreateBoxPlotData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.BoxPlotData)
            .OfType<BoxPlotData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        var allCategories = datasets.SelectMany(static data => data.Categories).ToArray();
        return new BoxPlotData(allCategories);
    }

    public static HistogramData? CreateHistogramData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.HistogramData)
            .OfType<HistogramData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        if (datasets.Length == 1)
        {
            return datasets[0];
        }

        // Merge all values from multiple histogram datasets
        var allValues = datasets.SelectMany(static data => data.Values).ToArray();
        var first = datasets[0];
        return new HistogramData(allValues, first.BinCount, first.Mode);
    }

    public static FunctionPlotData? CreateFunctionPlotData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.FunctionPlotData)
            .OfType<FunctionPlotData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        // Function plots are independent — return the first one
        return datasets[0];
    }

    public static PieChartData? CreatePieData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.PieData)
            .OfType<PieChartData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        // Pie charts are independent — return the first one
        return datasets[0];
    }

    public static OHLCData? CreateOHLCData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.OHLCData)
            .OfType<OHLCData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        // OHLC charts are independent — return the first one
        return datasets[0];
    }

    public static ViolinData? CreateViolinData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.ViolinData)
            .OfType<ViolinData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        // Violin plots are independent — return the first one
        return datasets[0];
    }

    public static PolygonData? CreatePolygonData(IReadOnlyList<Plot3DSeries> series)
    {
        var datasets = series
            .Select(static item => item.PolygonData)
            .OfType<PolygonData>()
            .ToArray();

        if (datasets.Length == 0)
        {
            return null;
        }

        // Polygon charts are independent — return the first one
        return datasets[0];
    }

    private static ScatterChartMetadata CreateScatterMetadata(IReadOnlyList<ScatterChartData> datasets)
    {
        var first = datasets[0].Metadata;
        return new ScatterChartMetadata(
            CreateAxisUnion(first.HorizontalAxis, datasets.Select(static data => data.Metadata.HorizontalAxis)),
            CreateAxisUnion(first.DepthAxis, datasets.Select(static data => data.Metadata.DepthAxis)),
            CreateValueRangeUnion(datasets.Select(static data => data.Metadata.ValueRange)));
    }

    private static ScatterChartMetadata CreateScatterMetadata(IReadOnlyList<ScatterChartMetadata> metadatas)
    {
        var first = metadatas[0];
        return new ScatterChartMetadata(
            CreateAxisUnion(first.HorizontalAxis, metadatas.Select(static m => m.HorizontalAxis)),
            CreateAxisUnion(first.DepthAxis, metadatas.Select(static m => m.DepthAxis)),
            CreateValueRangeUnion(metadatas.Select(static m => m.ValueRange)));
    }

    private static SurfaceAxisDescriptor CreateAxisUnion(
        SurfaceAxisDescriptor first,
        IEnumerable<SurfaceAxisDescriptor> axes)
    {
        var minimum = double.MaxValue;
        var maximum = double.MinValue;
        foreach (var axis in axes)
        {
            minimum = Math.Min(minimum, axis.Minimum);
            maximum = Math.Max(maximum, axis.Maximum);
        }

        return new SurfaceAxisDescriptor(first.Label, first.Unit, minimum, maximum, first.ScaleKind);
    }

    private static SurfaceValueRange CreateValueRangeUnion(IEnumerable<SurfaceValueRange> ranges)
    {
        var minimum = double.MaxValue;
        var maximum = double.MinValue;
        foreach (var range in ranges)
        {
            minimum = Math.Min(minimum, range.Minimum);
            maximum = Math.Max(maximum, range.Maximum);
        }

        return new SurfaceValueRange(minimum, maximum);
    }

    private sealed class ComposedSurfaceTileSource : ISurfaceTileSource
    {
        private readonly ISurfaceTileSource[] _sources;

        public ComposedSurfaceTileSource(IReadOnlyList<ISurfaceTileSource> sources)
        {
            ArgumentNullException.ThrowIfNull(sources);
            if (sources.Count == 0)
            {
                throw new ArgumentException("At least one source is required.", nameof(sources));
            }

            var compatibleSources = sources
                .Where(source => IsCompatible(sources[0].Metadata, source.Metadata))
                .ToArray();
            _sources = compatibleSources.Length == 0 ? [sources[^1]] : compatibleSources;
            Metadata = CreateSurfaceMetadata(_sources);
        }

        public SurfaceMetadata Metadata { get; }

        public async ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            var tiles = new List<SurfaceTile>(_sources.Length);
            foreach (var source in _sources)
            {
                var tile = await source.GetTileAsync(tileKey, cancellationToken).ConfigureAwait(false);
                if (tile is not null)
                {
                    tiles.Add(tile);
                }
            }

            return tiles.Count switch
            {
                0 => null,
                1 => tiles[0],
                _ => ComposeTile(tileKey, tiles),
            };
        }

        private static SurfaceMetadata CreateSurfaceMetadata(IReadOnlyList<ISurfaceTileSource> sources)
        {
            var first = sources[0].Metadata;
            return new SurfaceMetadata(
                first.Geometry,
                first.HorizontalAxis,
                first.VerticalAxis,
                CreateValueRangeUnion(sources.Select(static source => source.Metadata.ValueRange)));
        }

        private static SurfaceTile ComposeTile(SurfaceTileKey key, IReadOnlyList<SurfaceTile> tiles)
        {
            var first = tiles[0];
            var values = new float[first.Values.Length];
            var sourceCount = 0;

            foreach (var tile in tiles)
            {
                if (tile.Width != first.Width || tile.Height != first.Height || tile.Bounds != first.Bounds)
                {
                    continue;
                }

                var span = tile.Values.Span;
                for (var index = 0; index < values.Length; index++)
                {
                    values[index] += span[index];
                }

                sourceCount++;
            }

            if (sourceCount == 0)
            {
                return first;
            }

            for (var index = 0; index < values.Length; index++)
            {
                values[index] /= sourceCount;
            }

            var range = CreateValueRange(values);
            return new SurfaceTile(
                key,
                first.Width,
                first.Height,
                first.Bounds,
                values,
                range);
        }

        private static SurfaceValueRange CreateValueRange(ReadOnlySpan<float> values)
        {
            var minimum = double.MaxValue;
            var maximum = double.MinValue;
            var hasFiniteValue = false;
            foreach (var value in values)
            {
                if (!float.IsFinite(value))
                {
                    continue;
                }

                hasFiniteValue = true;
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }

            return hasFiniteValue
                ? new SurfaceValueRange(minimum, maximum)
                : new SurfaceValueRange(0d, 0d);
        }

        private static bool IsCompatible(SurfaceMetadata first, SurfaceMetadata candidate)
        {
            return first.Width == candidate.Width
                && first.Height == candidate.Height
                && string.Equals(DescribeAxis(first.HorizontalAxis), DescribeAxis(candidate.HorizontalAxis), StringComparison.Ordinal)
                && string.Equals(DescribeAxis(first.VerticalAxis), DescribeAxis(candidate.VerticalAxis), StringComparison.Ordinal);
        }

        private static string DescribeAxis(SurfaceAxisDescriptor axis)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{axis.Label}|{axis.Unit}|{axis.Minimum:G17}|{axis.Maximum:G17}|{axis.ScaleKind}");
        }
    }
}
