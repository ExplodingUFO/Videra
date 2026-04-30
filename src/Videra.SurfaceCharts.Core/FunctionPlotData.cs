namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable function-plot dataset that evaluates a mathematical
/// function over a specified domain.
/// </summary>
public sealed class FunctionPlotData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionPlotData"/> class.
    /// </summary>
    /// <param name="function">The function to evaluate: y = f(x).</param>
    /// <param name="xMin">The domain minimum.</param>
    /// <param name="xMax">The domain maximum.</param>
    /// <param name="sampleCount">The number of sample points. Must be at least 2.</param>
    public FunctionPlotData(Func<double, double> function, double xMin, double xMax, int sampleCount = 200)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentOutOfRangeException.ThrowIfLessThan(xMax, xMin);
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleCount, 2);

        Function = function;
        XMin = xMin;
        XMax = xMax;
        SampleCount = sampleCount;

        var xs = new double[sampleCount];
        var ys = new double[sampleCount];
        var step = (xMax - xMin) / (sampleCount - 1);

        for (var i = 0; i < sampleCount; i++)
        {
            xs[i] = xMin + (i * step);
            ys[i] = function(xs[i]);
        }

        Xs = Array.AsReadOnly(xs);
        Ys = Array.AsReadOnly(ys);
    }

    /// <summary>
    /// Gets the function to evaluate.
    /// </summary>
    public Func<double, double> Function { get; }

    /// <summary>
    /// Gets the domain minimum.
    /// </summary>
    public double XMin { get; }

    /// <summary>
    /// Gets the domain maximum.
    /// </summary>
    public double XMax { get; }

    /// <summary>
    /// Gets the number of sample points.
    /// </summary>
    public int SampleCount { get; }

    /// <summary>
    /// Gets the computed X coordinates.
    /// </summary>
    public IReadOnlyList<double> Xs { get; }

    /// <summary>
    /// Gets the computed Y coordinates.
    /// </summary>
    public IReadOnlyList<double> Ys { get; }
}
