namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Caches <see cref="ContourRenderScene"/> instances to avoid redundant contour extraction.
/// </summary>
public sealed class ContourSceneCache
{
    private ContourRenderScene? _cachedScene;
    private int _cachedRevision = -1;

    /// <summary>
    /// Gets or computes the contour render scene for the specified data and revision.
    /// </summary>
    /// <param name="data">The contour dataset.</param>
    /// <param name="revision">The current data revision.</param>
    /// <returns>The cached or newly computed contour render scene.</returns>
    public ContourRenderScene GetOrCompute(ContourChartData data, int revision)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (_cachedScene is not null && _cachedRevision == revision)
        {
            return _cachedScene;
        }

        var lines = ContourExtractor.ExtractAll(data);
        var metadata = CreateMetadata(data);
        _cachedScene = new ContourRenderScene(metadata, lines);
        _cachedRevision = revision;

        return _cachedScene;
    }

    /// <summary>
    /// Clears the cached scene.
    /// </summary>
    public void Invalidate()
    {
        _cachedScene = null;
        _cachedRevision = -1;
    }

    private static SurfaceMetadata CreateMetadata(ContourChartData data)
    {
        var field = data.Field;
        return new SurfaceMetadata(
            field.Width,
            field.Height,
            new SurfaceAxisDescriptor("X", null, 0d, field.Width - 1),
            new SurfaceAxisDescriptor("Y", null, 0d, field.Height - 1),
            field.Range);
    }
}
