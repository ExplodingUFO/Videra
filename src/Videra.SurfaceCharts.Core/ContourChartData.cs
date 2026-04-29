namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable contour-chart dataset with a 2D scalar field and optional mask.
/// </summary>
public sealed class ContourChartData
{
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
}
