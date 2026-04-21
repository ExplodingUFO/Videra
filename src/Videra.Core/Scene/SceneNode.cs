using System.Numerics;

namespace Videra.Core.Scene;

public sealed record SceneNode
{
    private readonly MeshPrimitiveId[] _primitiveIds;

    public SceneNode(
        SceneNodeId id,
        string name,
        Matrix4x4 localTransform,
        SceneNodeId? parentId,
        IEnumerable<MeshPrimitiveId> primitiveIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(primitiveIds);

        Id = id;
        Name = name;
        LocalTransform = localTransform;
        ParentId = parentId;
        _primitiveIds = primitiveIds.ToArray();
    }

    public SceneNodeId Id { get; }

    public string Name { get; }

    public Matrix4x4 LocalTransform { get; }

    public SceneNodeId? ParentId { get; }

    public IReadOnlyList<MeshPrimitiveId> PrimitiveIds => _primitiveIds;
}
