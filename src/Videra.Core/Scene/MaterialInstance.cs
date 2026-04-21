using Videra.Core.Geometry;

namespace Videra.Core.Scene;

public sealed class MaterialInstance
{
    public MaterialInstance(
        MaterialInstanceId id,
        string name,
        RgbaFloat baseColorFactor,
        Texture2DId? baseColorTextureId = null,
        SamplerId? baseColorSamplerId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        BaseColorFactor = baseColorFactor;
        BaseColorTextureId = baseColorTextureId;
        BaseColorSamplerId = baseColorSamplerId;
    }

    public MaterialInstanceId Id { get; }

    public string Name { get; }

    public RgbaFloat BaseColorFactor { get; }

    public Texture2DId? BaseColorTextureId { get; }

    public SamplerId? BaseColorSamplerId { get; }
}
