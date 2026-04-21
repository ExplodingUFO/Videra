using System.Numerics;

namespace Videra.Core.Geometry;

public sealed class MeshTextureCoordinateSet
{
    private readonly Vector2[] _coordinates;

    public MeshTextureCoordinateSet(int setIndex, Vector2[] coordinates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(setIndex);
        ArgumentNullException.ThrowIfNull(coordinates);

        SetIndex = setIndex;
        _coordinates = (Vector2[])coordinates.Clone();
    }

    public int SetIndex { get; }

    public ReadOnlyMemory<Vector2> Coordinates => _coordinates;
}
