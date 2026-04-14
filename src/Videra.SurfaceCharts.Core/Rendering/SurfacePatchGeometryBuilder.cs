namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds and caches reusable patch topology for regular surface tiles.
/// </summary>
public sealed class SurfacePatchGeometryBuilder
{
    private readonly Dictionary<(int Width, int Height), SurfacePatchGeometry> cache = new();

    /// <summary>
    /// Builds the shared patch geometry for a tile shape.
    /// </summary>
    /// <param name="sampleWidth">The tile width in samples.</param>
    /// <param name="sampleHeight">The tile height in samples.</param>
    /// <returns>The shared geometry for the requested shape.</returns>
    public SurfacePatchGeometry Build(int sampleWidth, int sampleHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleHeight);

        var key = (sampleWidth, sampleHeight);
        if (cache.TryGetValue(key, out var geometry))
        {
            return geometry;
        }

        geometry = new SurfacePatchGeometry(sampleWidth, sampleHeight, BuildIndices(sampleWidth, sampleHeight));
        cache.Add(key, geometry);
        return geometry;
    }

    private static uint[] BuildIndices(int sampleWidth, int sampleHeight)
    {
        if (sampleWidth < 2 || sampleHeight < 2)
        {
            return Array.Empty<uint>();
        }

        var quadCount = checked((sampleWidth - 1) * (sampleHeight - 1));
        var indices = new uint[checked(quadCount * 6)];
        var writeIndex = 0;

        for (var row = 0; row < sampleHeight - 1; row++)
        {
            var topRowStart = checked(row * sampleWidth);
            var bottomRowStart = checked((row + 1) * sampleWidth);

            for (var column = 0; column < sampleWidth - 1; column++)
            {
                var topLeft = checked((uint)(topRowStart + column));
                var topRight = checked((uint)(topRowStart + column + 1));
                var bottomLeft = checked((uint)(bottomRowStart + column));
                var bottomRight = checked((uint)(bottomRowStart + column + 1));

                indices[writeIndex++] = topLeft;
                indices[writeIndex++] = bottomLeft;
                indices[writeIndex++] = topRight;
                indices[writeIndex++] = topRight;
                indices[writeIndex++] = bottomLeft;
                indices[writeIndex++] = bottomRight;
            }
        }

        return indices;
    }
}
