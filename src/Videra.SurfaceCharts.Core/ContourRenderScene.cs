namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a render-ready contour-chart snapshot with extracted contour lines.
/// </summary>
public sealed class ContourRenderScene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContourRenderScene"/> class.
    /// </summary>
    /// <param name="metadata">The dataset metadata for the render snapshot.</param>
    /// <param name="lines">The extracted contour lines.</param>
    public ContourRenderScene(SurfaceMetadata metadata, IReadOnlyList<ContourLine> lines)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(lines);

        Metadata = metadata;
        Lines = lines;
    }

    /// <summary>
    /// Gets the dataset metadata for the render snapshot.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the extracted contour lines.
    /// </summary>
    public IReadOnlyList<ContourLine> Lines { get; }
}
