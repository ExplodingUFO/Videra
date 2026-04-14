namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Selects a surface-chart pyramid level and tile range from a viewport request.
/// </summary>
public sealed class SurfaceLodPolicy
{
    // Keep a small explicit neighborhood around the visible range so nearby tiles stay resident.
    private const int NeighborhoodTileMargin = 1;
    private readonly int _maxLevel;

    /// <summary>
    /// Gets the default overview-first policy.
    /// </summary>
    public static SurfaceLodPolicy Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceLodPolicy"/> class.
    /// </summary>
    /// <param name="maxLevel">The maximum selectable pyramid level.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLevel"/> is negative.</exception>
    public SurfaceLodPolicy(int maxLevel = 30)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLevel);
        _maxLevel = maxLevel;
    }

    /// <summary>
    /// Gets the selected level and tile range for the specified viewport request.
    /// </summary>
    /// <param name="request">The viewport request to evaluate.</param>
    /// <returns>The resulting LOD selection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is invalid.</exception>
    public SurfaceLodSelection Select(in SurfaceViewportRequest request)
    {
        var clampedViewport = request.ClampedViewport;
        var levelX = GetTargetLevel(request.HorizontalZoomDensity);
        var levelY = GetTargetLevel(request.VerticalZoomDensity);

        var tileCountX = 1 << levelX;
        var tileCountY = 1 << levelY;

        var visibleTileXStart = GetTileIndexStart(clampedViewport.StartX, request.Metadata.Width, tileCountX);
        var visibleTileXEnd = GetTileIndexEnd(clampedViewport.EndXExclusive, request.Metadata.Width, tileCountX);
        var visibleTileYStart = GetTileIndexStart(clampedViewport.StartY, request.Metadata.Height, tileCountY);
        var visibleTileYEnd = GetTileIndexEnd(clampedViewport.EndYExclusive, request.Metadata.Height, tileCountY);

        var tileXStart = ExpandStart(visibleTileXStart, NeighborhoodTileMargin);
        var tileXEnd = ExpandEnd(visibleTileXEnd, NeighborhoodTileMargin, tileCountX);
        var tileYStart = ExpandStart(visibleTileYStart, NeighborhoodTileMargin);
        var tileYEnd = ExpandEnd(visibleTileYEnd, NeighborhoodTileMargin, tileCountY);

        return new SurfaceLodSelection(request, levelX, levelY, tileXStart, tileXEnd, tileYStart, tileYEnd);
    }

    /// <summary>
    /// Maps zoom density to a target pyramid level.
    /// </summary>
    /// <param name="zoomDensity">The zoom density in samples per output pixel.</param>
    /// <returns>The target pyramid level.</returns>
    public int GetTargetLevel(double zoomDensity)
    {
        if (!double.IsFinite(zoomDensity))
        {
            throw new ArgumentOutOfRangeException(nameof(zoomDensity), "Zoom density must be finite.");
        }

        if (zoomDensity <= 1.0)
        {
            return 0;
        }

        var targetLevel = (int)Math.Floor(Math.Log2(zoomDensity));
        return Math.Clamp(targetLevel, 0, _maxLevel);
    }

    private static int GetTileIndexStart(double viewportStart, int datasetSpan, int tileCount)
    {
        var normalizedStart = viewportStart / datasetSpan;
        var tileIndex = (int)Math.Floor(normalizedStart * tileCount);
        return Math.Clamp(tileIndex, 0, tileCount - 1);
    }

    private static int GetTileIndexEnd(double viewportEndExclusive, int datasetSpan, int tileCount)
    {
        var normalizedEndExclusive = viewportEndExclusive / datasetSpan;
        var tileIndex = (int)Math.Ceiling(normalizedEndExclusive * tileCount) - 1;
        return Math.Clamp(tileIndex, 0, tileCount - 1);
    }

    private static int ExpandStart(int visibleStart, int margin)
    {
        return Math.Max(0, visibleStart - margin);
    }

    private static int ExpandEnd(int visibleEnd, int margin, int tileCount)
    {
        return Math.Min(tileCount - 1, visibleEnd + margin);
    }
}
