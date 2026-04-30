using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow
{
    private static ISurfaceTileSource CreateInMemorySource()
    {
        var matrix = CreateSampleMatrix();
        return new SurfacePyramidBuilder(32, 32).Build(matrix);
    }

    private static ISurfaceTileSource CreateAnalyticsProofSource()
    {
        var matrix = CreateAnalyticsProofMatrix();
        return new InMemorySurfaceTileSource(
            matrix,
            matrix.Metadata,
            [new SurfacePyramidLevel(0, 0, matrix)],
            maxTileWidth: 32,
            maxTileHeight: 32,
            detailLevelX: 0,
            detailLevelY: 0,
            reductionKernel: new ManagedSurfaceTileReductionKernel());
    }

    private static ISurfaceTileSource CreateWaterfallSource()
    {
        var matrix = CreateWaterfallMatrix();
        return new InMemorySurfaceTileSource(
            matrix,
            matrix.Metadata,
            [new SurfacePyramidLevel(0, 0, matrix)],
            maxTileWidth: 32,
            maxTileHeight: 32,
            detailLevelX: 0,
            detailLevelY: 0,
            reductionKernel: new ManagedSurfaceTileReductionKernel());
    }

    private static BarChartData CreateSampleBarData()
    {
        return new BarChartData(
        [
            new BarSeries([12.0, 19.0, 3.0, 5.0, 8.0], 0xFF38BDF8u, "Series A"),
            new BarSeries([7.0, 11.0, 15.0, 8.0, 13.0], 0xFFF97316u, "Series B"),
            new BarSeries([5.0, 9.0, 12.0, 18.0, 6.0], 0xFF2DD4BFu, "Series C"),
        ]);
    }

    private static ContourChartData CreateSampleContourField()
    {
        const int size = 32;
        var values = new float[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = (x - (size - 1) / 2.0) / ((size - 1) / 2.0);
                var dy = (y - (size - 1) / 2.0) / ((size - 1) / 2.0);
                values[y * size + x] = (float)Math.Sqrt(dx * dx + dy * dy);
            }
        }

        var range = new SurfaceValueRange(values.Min(), values.Max());
        var field = new SurfaceScalarField(size, size, values, range);
        return new ContourChartData(field);
    }

    private static SurfaceMatrix CreateAnalyticsProofMatrix()
    {
        const int width = 19;
        const int height = 13;
        var values = new float[width * height];
        var colorValues = new float[width * height];
        var horizontalCoordinates = new double[]
        {
            0.0d,
            0.14d,
            0.33d,
            0.54d,
            0.88d,
            1.19d,
            1.55d,
            1.96d,
            2.41d,
            2.96d,
            3.34d,
            3.88d,
            4.31d,
            4.91d,
            5.45d,
            6.05d,
            6.74d,
            7.68d,
            8.52d,
        };

        var verticalCoordinates = new double[]
        {
            -1.1d,
            -0.68d,
            -0.22d,
            0.09d,
            0.35d,
            0.66d,
            1.02d,
            1.41d,
            1.85d,
            2.31d,
            2.92d,
            3.48d,
            4.14d,
        };

        var valueIndex = 0;
        for (var y = 0; y < height; y++)
        {
            var axisY = verticalCoordinates[y];
            for (var x = 0; x < width; x++)
            {
                var axisX = horizontalCoordinates[x];
                var heightValue = Math.Tanh(
                    Math.Sin((axisX * 0.72d) + (axisY * 0.63d)) +
                    (0.48d * Math.Cos((axisX * 0.51d) - (axisY * 0.78d))));
                var colorValue = Math.Tanh(
                    (0.72d * Math.Cos((axisX * 0.38d) + (axisY * 0.27d))) +
                    (0.33d * Math.Sin((axisX * 0.14d) + (axisY * 1.08d))));

                values[valueIndex] = (float)heightValue;
                colorValues[valueIndex] = (float)colorValue;
                valueIndex++;
            }
        }

        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates),
            new SurfaceAxisDescriptor("Distance", "km", horizontalCoordinates[0], horizontalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Depth", "m", verticalCoordinates[0], verticalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-1d, 1d));

        var heightField = new SurfaceScalarField(width, height, values, metadata.ValueRange);
        var colorField = new SurfaceScalarField(width, height, colorValues, metadata.ValueRange);

        return new SurfaceMatrix(
            metadata,
            heightField,
            colorField);
    }

    private static ScatterChartData CreateScatterSource(ScatterStreamingScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var xMaximum = (scenario.InitialPointCount + scenario.UpdatePointCount) * 0.01d;
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Horizontal", "u", 0d, xMaximum),
            new SurfaceAxisDescriptor("Depth", "u", -20d, 20d),
            new SurfaceValueRange(-20d, 20d));

        var series = new[]
        {
            new ScatterSeries(
                new[]
                {
                    new ScatterPoint(1.2d, 2.1d, 1.0d, 0xFF5EEAD4u),
                    new ScatterPoint(2.7d, 3.8d, 2.2d),
                    new ScatterPoint(4.4d, 4.6d, 3.1d),
                    new ScatterPoint(5.8d, 5.0d, 4.0d),
                },
                0xFF38BDF8u,
                "Signal"),
            new ScatterSeries(
                new[]
                {
                    new ScatterPoint(7.1d, 7.8d, 5.6d, 0xFFF59E0Bu),
                    new ScatterPoint(8.4d, 8.6d, 6.8d),
                    new ScatterPoint(9.6d, 9.2d, 8.1d),
                    new ScatterPoint(10.8d, 11.0d, 9.3d),
                },
                0xFFF97316u,
                "Cluster"),
        };

        return new ScatterChartData(metadata, series, [ScatterStreamingScenarios.CreateSeries(scenario)]);
    }

    private static SurfaceMatrix CreateSampleMatrix()
    {
        const int width = 64;
        const int height = 48;
        var values = new float[width * height];
        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        var index = 0;

        for (var y = 0; y < height; y++)
        {
            var normalizedY = (double)y / (height - 1);
            var centeredY = (normalizedY * 2d) - 1d;

            for (var x = 0; x < width; x++)
            {
                var normalizedX = (double)x / (width - 1);
                var centeredX = (normalizedX * 2d) - 1d;

                var ripple = Math.Sin(centeredX * Math.PI * 2.75d) * Math.Cos(centeredY * Math.PI * 2.25d);
                var hill = Math.Exp(-2.8d * ((centeredX * centeredX) + (centeredY * centeredY)));
                var slope = centeredY * 0.35d;
                var value = (ripple * 0.6d) + (hill * 1.45d) - slope;

                values[index++] = (float)value;
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }
        }

        var metadata = new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0d, 180d),
            new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 48d),
            new SurfaceValueRange(minimum, maximum));

        return new SurfaceMatrix(metadata, values);
    }

    private static double[] CreateSampleLineXs()
    {
        const int count = 30;
        var xs = new double[count];
        for (var i = 0; i < count; i++)
        {
            xs[i] = i * 0.5;
        }
        return xs;
    }

    private static double[] CreateSampleLineYs()
    {
        const int count = 30;
        var ys = new double[count];
        for (var i = 0; i < count; i++)
        {
            ys[i] = Math.Sin(i * 0.3) * 2.0 + 1.0;
        }
        return ys;
    }

    private static double[] CreateSampleLineZs()
    {
        const int count = 30;
        var zs = new double[count];
        for (var i = 0; i < count; i++)
        {
            zs[i] = i * 0.4;
        }
        return zs;
    }

    private static double[] CreateSampleVectorFieldXs()
    {
        const int gridSize = 6;
        var xs = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                xs[z * gridSize + x] = x;
            }
        }
        return xs;
    }

    private static double[] CreateSampleVectorFieldYs()
    {
        const int gridSize = 6;
        var ys = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                ys[z * gridSize + x] = 0;
            }
        }
        return ys;
    }

    private static double[] CreateSampleVectorFieldZs()
    {
        const int gridSize = 6;
        var zs = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                zs[z * gridSize + x] = z;
            }
        }
        return zs;
    }

    private static double[] CreateSampleVectorFieldDxs()
    {
        const int gridSize = 6;
        var dxs = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                var cx = x - (gridSize - 1) / 2.0;
                var cz = z - (gridSize - 1) / 2.0;
                dxs[z * gridSize + x] = -cz * 0.3;
            }
        }
        return dxs;
    }

    private static double[] CreateSampleVectorFieldDys()
    {
        const int gridSize = 6;
        var dys = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                dys[z * gridSize + x] = 0.5;
            }
        }
        return dys;
    }

    private static double[] CreateSampleVectorFieldDzs()
    {
        const int gridSize = 6;
        var dzs = new double[gridSize * gridSize];
        for (var z = 0; z < gridSize; z++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                var cx = x - (gridSize - 1) / 2.0;
                var cz = z - (gridSize - 1) / 2.0;
                dzs[z * gridSize + x] = cx * 0.3;
            }
        }
        return dzs;
    }

    private static double[,] CreateSampleHeatmapValues()
    {
        const int size = 24;
        var values = new double[size, size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var nx = (x - size / 2.0) / (size / 2.0);
                var ny = (y - size / 2.0) / (size / 2.0);
                values[x, y] = Math.Sin(nx * 3.0) * Math.Cos(ny * 2.5) *
                               Math.Exp(-(nx * nx + ny * ny) * 0.8);
            }
        }
        return values;
    }

    private static BoxPlotData CreateSampleBoxPlotData()
    {
        return new BoxPlotData([
            new BoxPlotCategory("Algorithm A", min: 12, q1: 18, median: 24, q3: 31, max: 42, outliers: [8, 48]),
            new BoxPlotCategory("Algorithm B", min: 15, q1: 22, median: 28, q3: 35, max: 45),
            new BoxPlotCategory("Algorithm C", min: 8, q1: 14, median: 19, q3: 26, max: 38, outliers: [3, 42]),
            new BoxPlotCategory("Algorithm D", min: 20, q1: 27, median: 33, q3: 40, max: 52, outliers: [15, 58]),
        ]);
    }

    private static SurfaceMatrix CreateWaterfallMatrix()
    {
        const int width = 72;
        const int stripCount = 12;
        const int rowsPerStrip = 3;
        const double signalRowOffset = 0.15d;
        const double trailingBaselineOffset = 0.3d;
        var height = stripCount * rowsPerStrip;
        var values = new float[width * height];
        var horizontalCoordinates = new double[width];
        var verticalCoordinates = new double[height];
        var maximum = 0d;

        for (var x = 0; x < width; x++)
        {
            horizontalCoordinates[x] = (180d * x) / (width - 1d);
        }

        for (var strip = 0; strip < stripCount; strip++)
        {
            var baselineTop = strip * rowsPerStrip;
            var signalRow = baselineTop + 1;
            var baselineBottom = baselineTop + 2;
            verticalCoordinates[baselineTop] = strip;
            verticalCoordinates[signalRow] = strip + signalRowOffset;
            verticalCoordinates[baselineBottom] = strip + trailingBaselineOffset;
            var stripWeight = 1d + (strip * 0.045d);
            var hotspot = 0.12d + (strip * 0.055d);

            for (var x = 0; x < width; x++)
            {
                var normalizedX = (double)x / (width - 1);
                var wave = Math.Sin((normalizedX * Math.PI * (2.4d + (strip * 0.08d))) + (strip * 0.35d));
                var harmonic = Math.Cos((normalizedX * Math.PI * 5.6d) - (strip * 0.18d)) * 0.18d;
                var spike = Math.Exp(-44d * Math.Pow(normalizedX - hotspot, 2d));
                var envelope = 0.92d - (normalizedX * 0.25d);
                var value = Math.Max(0d, ((wave * 0.48d) + harmonic + (spike * 1.15d)) * stripWeight * envelope);

                values[(baselineTop * width) + x] = 0f;
                values[(signalRow * width) + x] = (float)value;
                values[(baselineBottom * width) + x] = 0f;
                maximum = Math.Max(maximum, value);
            }
        }

        var geometry = new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates);
        var metadata = new SurfaceMetadata(
            geometry,
            new SurfaceAxisDescriptor("Time", "s", horizontalCoordinates[0], horizontalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Sweep", unit: null, minimum: verticalCoordinates[0], maximum: verticalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(0d, maximum));

        return new SurfaceMatrix(metadata, values);
    }
}
