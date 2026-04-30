using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable contour-chart dataset with a 2D scalar field and optional mask.
/// </summary>
public sealed class ContourChartData
{
    private static readonly IReadOnlyList<float> NoExplicitLevels = Array.Empty<float>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContourChartData"/> class.
    /// </summary>
    /// <param name="field">The 2D scalar field to extract contours from.</param>
    /// <param name="mask">Optional mask where <see langword="true"/> means the sample is present.</param>
    /// <param name="levelCount">The number of contour levels to extract. Defaults to 10.</param>
    public ContourChartData(
        SurfaceScalarField field,
        SurfaceMask? mask = null,
        int levelCount = 10)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(levelCount);

        Field = field;
        Mask = mask;
        LevelCount = levelCount;
        ExplicitLevels = NoExplicitLevels;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContourChartData"/> class with explicit contour levels.
    /// </summary>
    /// <param name="field">The 2D scalar field to extract contours from.</param>
    /// <param name="explicitLevels">The finite contour levels to extract, in deterministic extraction order.</param>
    /// <param name="mask">Optional mask where <see langword="true"/> means the sample is present.</param>
    public ContourChartData(
        SurfaceScalarField field,
        IReadOnlyList<float> explicitLevels,
        SurfaceMask? mask = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(explicitLevels);

        if (explicitLevels.Count == 0)
        {
            throw new ArgumentException("Explicit contour levels must include at least one level.", nameof(explicitLevels));
        }

        var levels = new float[explicitLevels.Count];
        for (var index = 0; index < explicitLevels.Count; index++)
        {
            var level = explicitLevels[index];
            if (!float.IsFinite(level))
            {
                throw new ArgumentOutOfRangeException(nameof(explicitLevels), "Explicit contour levels must be finite.");
            }

            levels[index] = level;
        }

        Field = field;
        Mask = mask;
        LevelCount = levels.Length;
        ExplicitLevels = new ReadOnlyCollection<float>(levels);
    }

    /// <summary>
    /// Gets the 2D scalar field.
    /// </summary>
    public SurfaceScalarField Field { get; }

    /// <summary>
    /// Gets the optional mask where <see langword="true"/> means the sample is present.
    /// </summary>
    public SurfaceMask? Mask { get; }

    /// <summary>
    /// Gets the number of contour levels to extract.
    /// </summary>
    public int LevelCount { get; }

    /// <summary>
    /// Gets explicit contour levels in extraction order, or an empty list when levels are generated from <see cref="LevelCount"/>.
    /// </summary>
    public IReadOnlyList<float> ExplicitLevels { get; }

    /// <summary>
    /// Gets a value indicating whether this dataset uses explicit contour levels.
    /// </summary>
    public bool HasExplicitLevels => ExplicitLevels.Count > 0;
}
