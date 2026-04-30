using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable pie or donut chart dataset.
/// </summary>
public sealed class PieChartData
{
    private readonly ReadOnlyCollection<PieSlice> _slicesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="PieChartData"/> class.
    /// </summary>
    /// <param name="slices">The pie slices. Must not be empty.</param>
    /// <param name="holeRatio">The donut hole ratio (0 = full pie, 0.5 = half-radius hole). Must be in [0, 0.9).</param>
    public PieChartData(IReadOnlyList<PieSlice> slices, double holeRatio = 0d)
    {
        ArgumentNullException.ThrowIfNull(slices);

        if (slices.Count == 0)
        {
            throw new ArgumentException("Pie chart data must contain at least one slice.", nameof(slices));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(holeRatio);
        if (holeRatio >= 0.9d)
        {
            throw new ArgumentOutOfRangeException(nameof(holeRatio), "Hole ratio must be less than 0.9.");
        }

        HoleRatio = holeRatio;
        _slicesView = Array.AsReadOnly(slices.ToArray());

        var total = 0d;
        foreach (var slice in slices)
        {
            total += slice.Value;
        }

        TotalValue = total;
    }

    /// <summary>
    /// Gets the donut hole ratio (0 = full pie, 0.5 = half-radius hole).
    /// </summary>
    public double HoleRatio { get; }

    /// <summary>
    /// Gets the immutable pie slices.
    /// </summary>
    public IReadOnlyList<PieSlice> Slices => _slicesView;

    /// <summary>
    /// Gets the number of slices.
    /// </summary>
    public int SliceCount => _slicesView.Count;

    /// <summary>
    /// Gets the sum of all slice values.
    /// </summary>
    public double TotalValue { get; }
}

/// <summary>
/// Represents a single slice in a pie chart.
/// </summary>
public sealed class PieSlice
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PieSlice"/> class.
    /// </summary>
    /// <param name="value">The slice value. Must be non-negative.</param>
    /// <param name="color">The ARGB color of the slice.</param>
    /// <param name="label">The optional label for the slice.</param>
    /// <param name="explodeOffset">The explode offset (0 = no explode, 0.2 = 20% of radius).</param>
    public PieSlice(double value, uint color = 0xFF4DA3FFu, string? label = null, double explodeOffset = 0d)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfNegative(explodeOffset);

        Value = value;
        Color = color;
        Label = label;
        ExplodeOffset = explodeOffset;
    }

    /// <summary>
    /// Gets the slice value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the ARGB color of the slice.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the optional label for the slice.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the explode offset (0 = no explode).
    /// </summary>
    public double ExplodeOffset { get; }
}
