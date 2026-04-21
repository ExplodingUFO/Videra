using Videra.Core.Geometry;

namespace Videra.Core.Scene;

public sealed class MaterialInstance
{
    public MaterialInstance(
        MaterialInstanceId id,
        string name,
        RgbaFloat baseColorFactor,
        MaterialTextureBinding? baseColorTexture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        BaseColorFactor = baseColorFactor;
        BaseColorTexture = baseColorTexture;
    }

    public MaterialInstanceId Id { get; }

    public string Name { get; }

    public RgbaFloat BaseColorFactor { get; }

    public MaterialTextureBinding? BaseColorTexture { get; }
}
