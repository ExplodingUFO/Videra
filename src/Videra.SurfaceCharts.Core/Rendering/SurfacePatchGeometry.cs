using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the reusable patch topology for a surface tile shape.
/// </summary>
public sealed class SurfacePatchGeometry
{
    private readonly ReadOnlyCollection<uint> indicesView;

    internal SurfacePatchGeometry(int sampleWidth, int sampleHeight, uint[] indices)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleHeight);
        ArgumentNullException.ThrowIfNull(indices);

        SampleWidth = sampleWidth;
        SampleHeight = sampleHeight;
        VertexCount = checked(sampleWidth * sampleHeight);
        indicesView = Array.AsReadOnly((uint[])indices.Clone());
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
    public IReadOnlyList<uint> Indices => indicesView;
}
