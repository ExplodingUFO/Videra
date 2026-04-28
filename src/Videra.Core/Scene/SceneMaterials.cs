using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

public static class SceneMaterials
{
    public static MaterialInstance Matte(string name, RgbaFloat color)
    {
        return new MaterialInstance(
            MaterialInstanceId.New(),
            name,
            color,
            metallicRoughness: new MaterialMetallicRoughness(0f, 1f));
    }

    public static MaterialInstance Metal(string name, RgbaFloat color, float roughness = 0.35f)
    {
        return new MaterialInstance(
            MaterialInstanceId.New(),
            name,
            color,
            metallicRoughness: new MaterialMetallicRoughness(1f, roughness));
    }

    public static MaterialInstance Emissive(string name, RgbaFloat color, float intensity = 1f)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(intensity);

        return new MaterialInstance(
            MaterialInstanceId.New(),
            name,
            color,
            metallicRoughness: new MaterialMetallicRoughness(0f, 1f),
            emissive: new MaterialEmissive(new Vector3(color.R, color.G, color.B) * intensity));
    }

    public static MaterialInstance AlphaMask(string name, RgbaFloat color, float cutoff = 0.5f, bool doubleSided = false)
    {
        return new MaterialInstance(
            MaterialInstanceId.New(),
            name,
            color,
            metallicRoughness: new MaterialMetallicRoughness(0f, 1f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Mask, cutoff, doubleSided));
    }

    public static MaterialInstance Transparent(string name, RgbaFloat color, bool doubleSided = false)
    {
        return new MaterialInstance(
            MaterialInstanceId.New(),
            name,
            color,
            metallicRoughness: new MaterialMetallicRoughness(0f, 1f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, doubleSided));
    }
}
