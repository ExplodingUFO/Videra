using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class ImportedSceneAsset
{
    private readonly SceneNode[] _nodes;
    private readonly MeshPrimitive[] _primitives;
    private readonly MaterialInstance[] _materials;
    private readonly Texture2D[] _textures;
    private readonly Sampler[] _samplers;
    private readonly SceneNode[] _rootNodes;

    public ImportedSceneAsset(
        string filePath,
        string name,
        IEnumerable<SceneNode> nodes,
        IEnumerable<MeshPrimitive> primitives,
        IEnumerable<MaterialInstance>? materials = null,
        IEnumerable<Texture2D>? textures = null,
        IEnumerable<Sampler>? samplers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(primitives);

        FilePath = filePath;
        Name = name;
        _nodes = nodes.ToArray();
        _primitives = primitives.ToArray();
        _materials = materials?.ToArray() ?? Array.Empty<MaterialInstance>();
        _textures = textures?.ToArray() ?? Array.Empty<Texture2D>();
        _samplers = samplers?.ToArray() ?? Array.Empty<Sampler>();
        _rootNodes = _nodes.Where(static node => node.ParentId is null).ToArray();
        Payload = ImportedSceneAssetPayloadBuilder.Build(_nodes, _primitives, _materials);
        Metrics = SceneAssetMetrics.FromPayload(Payload);
    }

    public string FilePath { get; }

    public string Name { get; }

    public IReadOnlyList<SceneNode> Nodes => _nodes;

    public IReadOnlyList<MeshPrimitive> Primitives => _primitives;

    public IReadOnlyList<MaterialInstance> Materials => _materials;

    public IReadOnlyList<Texture2D> Textures => _textures;

    public IReadOnlyList<Sampler> Samplers => _samplers;

    public IReadOnlyList<SceneNode> RootNodes => _rootNodes;

    public SceneAssetMetrics Metrics { get; }

    internal MeshPayload Payload { get; }
}
