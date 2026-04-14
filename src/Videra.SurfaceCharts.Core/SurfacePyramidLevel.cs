namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one matrix level inside a surface pyramid.
/// </summary>
public sealed class SurfacePyramidLevel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfacePyramidLevel"/> class.
    /// </summary>
    /// <param name="levelX">The horizontal pyramid level.</param>
    /// <param name="levelY">The vertical pyramid level.</param>
    /// <param name="matrix">The matrix for the level.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a level index is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> is <c>null</c>.</exception>
    public SurfacePyramidLevel(int levelX, int levelY, SurfaceMatrix matrix)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(levelX);
        ArgumentOutOfRangeException.ThrowIfNegative(levelY);
        ArgumentNullException.ThrowIfNull(matrix);

        LevelX = levelX;
        LevelY = levelY;
        Matrix = matrix;
    }

    /// <summary>
    /// Gets the horizontal pyramid level.
    /// </summary>
    public int LevelX { get; }

    /// <summary>
    /// Gets the vertical pyramid level.
    /// </summary>
    public int LevelY { get; }

    /// <summary>
    /// Gets the matrix stored at this level.
    /// </summary>
    public SurfaceMatrix Matrix { get; }
}
