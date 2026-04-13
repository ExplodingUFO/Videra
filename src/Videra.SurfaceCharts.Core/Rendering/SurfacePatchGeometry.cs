namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the reusable patch topology for a surface tile shape.
/// </summary>
public sealed class SurfacePatchGeometry
{
    private readonly uint[] indices;

    internal SurfacePatchGeometry(int sampleWidth, int sampleHeight, uint[] indices)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleHeight);
        ArgumentNullException.ThrowIfNull(indices);

        SampleWidth = sampleWidth;
        SampleHeight = sampleHeight;
        VertexCount = checked(sampleWidth * sampleHeight);
        this.indices = indices;
    }

    /// <summary>
    /// Gets the patch width in samples.
    /// </summary>
    public int SampleWidth { get; }

    /// <summary>
    /// Gets the patch height in samples.
    /// </summary>
    public int SampleHeight { get; }

    /// <summary>
    /// Gets the vertex count for the patch.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// Gets the shared triangle index pattern for the patch.
    /// </summary>
    public IReadOnlyList<uint> Indices => indices;
}
