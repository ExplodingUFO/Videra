using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed record MeshPrimitive
{
    private readonly MeshPayload _payload;

    public MeshPrimitive(
        MeshPrimitiveId id,
        string name,
        MeshData meshData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(meshData);

        Id = id;
        Name = name;
        MeshData = meshData;
        _payload = MeshPayload.FromMesh(meshData, cloneArrays: false);
    }

    public MeshPrimitiveId Id { get; }

    public string Name { get; }

    public MeshData MeshData { get; }

    internal MeshPayload Payload => _payload;
}
