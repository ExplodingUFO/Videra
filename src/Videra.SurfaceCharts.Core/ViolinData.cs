using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable violin plot dataset.
/// </summary>
public sealed class ViolinData
{
    private readonly ReadOnlyCollection<ViolinGroup> _groupsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViolinData"/> class.
    /// </summary>
    /// <param name="groups">The violin groups. Must not be empty.</param>
    public ViolinData(IReadOnlyList<ViolinGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        if (groups.Count == 0)
        {
            throw new ArgumentException("Violin data must contain at least one group.", nameof(groups));
        }

        _groupsView = Array.AsReadOnly(groups.ToArray());
    }

    /// <summary>
    /// Gets the immutable violin groups.
    /// </summary>
    public IReadOnlyList<ViolinGroup> Groups => _groupsView;

    /// <summary>
    /// Gets the number of groups.
    /// </summary>
    public int GroupCount => _groupsView.Count;
}

/// <summary>
/// Represents a single violin group with raw values.
/// </summary>
public sealed class ViolinGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViolinGroup"/> class.
    /// </summary>
    /// <param name="values">The raw values for KDE estimation.</param>
    /// <param name="color">The ARGB color.</param>
    /// <param name="label">The optional label.</param>
    /// <param name="bandwidth">The KDE bandwidth. If null, Silverman's rule is used.</param>
    public ViolinGroup(
        IReadOnlyList<double> values,
        uint color = 0xFF4DA3FFu,
        string? label = null,
        double? bandwidth = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count < 2)
        {
            throw new ArgumentException("Violin group requires at least 2 values.", nameof(values));
        }

        Values = values;
        Color = color;
        Label = label;
        Bandwidth = bandwidth;
    }

    /// <summary>
    /// Gets the raw values.
    /// </summary>
    public IReadOnlyList<double> Values { get; }

    /// <summary>
    /// Gets the ARGB color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the optional label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the KDE bandwidth, or null for automatic selection.
    /// </summary>
    public double? Bandwidth { get; }
}
