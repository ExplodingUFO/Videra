using Videra.Core.Geometry;

namespace Videra.Core.Scene;

public sealed class MaterialInstance
{
    public MaterialInstance(
        MaterialInstanceId id,
        string name,
        RgbaFloat baseColorFactor,
        MaterialTextureBinding? baseColorTexture = null,
        MaterialMetallicRoughness? metallicRoughness = null,
        MaterialAlphaSettings? alpha = null,
        MaterialEmissive? emissive = null,
        MaterialNormalTextureBinding? normalTexture = null,
        MaterialOcclusionTextureBinding? occlusionTexture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        BaseColorFactor = baseColorFactor;
        BaseColorTexture = baseColorTexture;
        MetallicRoughness = metallicRoughness ?? MaterialMetallicRoughness.Default;
        Alpha = alpha ?? MaterialAlphaSettings.Opaque;
        Emissive = emissive ?? MaterialEmissive.Default;
        NormalTexture = normalTexture;
        OcclusionTexture = occlusionTexture;
    }

    public MaterialInstanceId Id { get; }

    public string Name { get; }

    public RgbaFloat BaseColorFactor { get; }

    public MaterialTextureBinding? BaseColorTexture { get; }

    public MaterialMetallicRoughness MetallicRoughness { get; }

    public MaterialAlphaSettings Alpha { get; }

    public MaterialEmissive Emissive { get; }

    public MaterialNormalTextureBinding? NormalTexture { get; }

    public MaterialOcclusionTextureBinding? OcclusionTexture { get; }
}
