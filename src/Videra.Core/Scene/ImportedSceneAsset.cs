using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class ImportedSceneAsset
{
    private readonly SceneNode[] _nodes;
    private readonly MeshPrimitive[] _primitives;
    private readonly SceneNode[] _rootNodes;

    public ImportedSceneAsset(
        string filePath,
        string name,
        IEnumerable<SceneNode> nodes,
        IEnumerable<MeshPrimitive> primitives)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(primitives);

        FilePath = filePath;
        Name = name;
        _nodes = nodes.ToArray();
        _primitives = primitives.ToArray();
        _rootNodes = _nodes.Where(static node => node.ParentId is null).ToArray();
        Payload = ImportedSceneAssetPayloadBuilder.Build(_nodes, _primitives);
        Metrics = SceneAssetMetrics.FromPayload(Payload);
    }

    public string FilePath { get; }

    public string Name { get; }

    public IReadOnlyList<SceneNode> Nodes => _nodes;

    public IReadOnlyList<MeshPrimitive> Primitives => _primitives;

    public IReadOnlyList<SceneNode> RootNodes => _rootNodes;

    public SceneAssetMetrics Metrics { get; }

    internal MeshPayload Payload { get; }
}
