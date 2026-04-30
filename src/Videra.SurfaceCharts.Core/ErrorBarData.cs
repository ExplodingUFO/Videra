using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents error bar data for a set of points.
/// </summary>
public sealed class ErrorBarData
{
    private readonly ReadOnlyCollection<ErrorBarEntry> _entriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorBarData"/> class.
    /// </summary>
    /// <param name="entries">The error bar entries. Count must match the associated scatter point count.</param>
    /// <param name="color">The ARGB color for the error bars.</param>
    /// <param name="capSize">The cap size in pixels.</param>
    /// <param name="lineWidth">The line width in pixels.</param>
    public ErrorBarData(
        IReadOnlyList<ErrorBarEntry> entries,
        uint color = 0xFFFFFFFFu,
        double capSize = 6d,
        double lineWidth = 1.5d)
    {
        ArgumentNullException.ThrowIfNull(entries);

        _entriesView = Array.AsReadOnly(entries.ToArray());
        Color = color;
        CapSize = capSize;
        LineWidth = lineWidth;
    }

    /// <summary>
    /// Gets the immutable error bar entries.
    /// </summary>
    public IReadOnlyList<ErrorBarEntry> Entries => _entriesView;

    /// <summary>
    /// Gets the number of entries.
    /// </summary>
    public int Count => _entriesView.Count;

    /// <summary>
    /// Gets the ARGB color for error bars.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the cap size in pixels.
    /// </summary>
    public double CapSize { get; }

    /// <summary>
    /// Gets the line width in pixels.
    /// </summary>
    public double LineWidth { get; }
}

/// <summary>
/// Represents the error values for a single point.
/// </summary>
public readonly record struct ErrorBarEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorBarEntry"/> struct with symmetric errors.
    /// </summary>
    public ErrorBarEntry(double xError, double yError)
        : this(xError, xError, yError, yError)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorBarEntry"/> struct with asymmetric errors.
    /// </summary>
    public ErrorBarEntry(double xErrorLow, double xErrorHigh, double yErrorLow, double yErrorHigh)
    {
        XErrorLow = xErrorLow;
        XErrorHigh = xErrorHigh;
        YErrorLow = yErrorLow;
        YErrorHigh = yErrorHigh;
    }

    /// <summary>
    /// Gets the negative X error magnitude.
    /// </summary>
    public double XErrorLow { get; }

    /// <summary>
    /// Gets the positive X error magnitude.
    /// </summary>
    public double XErrorHigh { get; }

    /// <summary>
    /// Gets the negative Y error magnitude.
    /// </summary>
    public double YErrorLow { get; }

    /// <summary>
    /// Gets the positive Y error magnitude.
    /// </summary>
    public double YErrorHigh { get; }
}
