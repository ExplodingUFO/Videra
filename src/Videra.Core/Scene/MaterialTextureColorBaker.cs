using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

internal static class MaterialTextureColorBaker
{
    public static RgbaFloat ResolveVertexColor(
        VertexPositionNormalColor vertex,
        int vertexIndex,
        MeshData meshData,
        MaterialInstance? material,
        IReadOnlyDictionary<Texture2DId, Texture2D> textureById,
        IReadOnlyDictionary<SamplerId, Sampler> samplerById)
    {
        ArgumentNullException.ThrowIfNull(meshData);
        ArgumentNullException.ThrowIfNull(textureById);
        ArgumentNullException.ThrowIfNull(samplerById);

        if (material is null)
        {
            return vertex.Color;
        }

        var color = material.BaseColorTexture is { } baseColorBinding
            ? Multiply(
                vertex.Color,
                material.BaseColorFactor,
                ResolveTextureSample(baseColorBinding, material.Name, vertexIndex, meshData, textureById, samplerById))
            : vertex.Color;

        if (material.OcclusionTexture is { } occlusionBinding)
        {
            var occlusionSample = ResolveTextureSample(
                occlusionBinding.Texture,
                material.Name,
                vertexIndex,
                meshData,
                textureById,
                samplerById);
            color = ApplyOcclusion(color, occlusionSample.R, occlusionBinding.Strength);
        }

        return ApplyEmissive(color, material.Emissive, material.Name, vertexIndex, meshData, textureById, samplerById);
    }

    internal static RgbaFloat ResolveTextureSample(
        MaterialTextureBinding binding,
        string materialName,
        int vertexIndex,
        MeshData meshData,
        IReadOnlyDictionary<Texture2DId, Texture2D> textureById,
        IReadOnlyDictionary<SamplerId, Sampler> samplerById)
    {
        if (!textureById.TryGetValue(binding.TextureId, out var texture))
        {
            throw new InvalidOperationException($"Scene material '{materialName}' references unknown texture '{binding.TextureId.Value}'.");
        }

        if (!samplerById.TryGetValue(binding.SamplerId, out var sampler))
        {
            throw new InvalidOperationException($"Scene material '{materialName}' references unknown sampler '{binding.SamplerId.Value}'.");
        }

        if (!TryGetCoordinates(meshData.TextureCoordinateSets, binding.CoordinateSet, vertexIndex, out var coordinates))
        {
            throw new InvalidOperationException(
                $"Scene material '{materialName}' requires texture coordinate set {binding.CoordinateSet} but the mesh does not provide it.");
        }

        return Sample(texture, sampler, binding.Transform, binding.ColorSpace, coordinates);
    }

    private static bool TryGetCoordinates(
        IReadOnlyList<MeshTextureCoordinateSet> sets,
        int setIndex,
        int vertexIndex,
        out Vector2 coordinates)
    {
        for (var i = 0; i < sets.Count; i++)
        {
            var set = sets[i];
            if (set.SetIndex != setIndex)
            {
                continue;
            }

            var values = set.Coordinates.Span;
            if ((uint)vertexIndex >= (uint)values.Length)
            {
                break;
            }

            coordinates = values[vertexIndex];
            return true;
        }

        coordinates = default;
        return false;
    }

    private static RgbaFloat Sample(
        Texture2D texture,
        Sampler sampler,
        MaterialTextureTransform transform,
        TextureColorSpace colorSpace,
        Vector2 coordinates)
    {
        var transformed = ApplyTransform(coordinates, transform);
        var useNearest = sampler.MinFilter == TextureFilter.Nearest || sampler.MagFilter == TextureFilter.Nearest;

        return useNearest
            ? SampleNearest(texture, transformed, sampler, colorSpace)
            : SampleLinear(texture, transformed, sampler, colorSpace);
    }

    private static Vector2 ApplyTransform(Vector2 coordinates, MaterialTextureTransform transform)
    {
        var scaled = coordinates * transform.Scale;
        if (MathF.Abs(transform.Rotation) < float.Epsilon)
        {
            return scaled + transform.Offset;
        }

        var sin = MathF.Sin(transform.Rotation);
        var cos = MathF.Cos(transform.Rotation);
        var rotated = new Vector2(
            (scaled.X * cos) - (scaled.Y * sin),
            (scaled.X * sin) + (scaled.Y * cos));
        return rotated + transform.Offset;
    }

    private static RgbaFloat SampleNearest(
        Texture2D texture,
        Vector2 coordinates,
        Sampler sampler,
        TextureColorSpace colorSpace)
    {
        var x = (int)MathF.Round(coordinates.X * texture.Width - 0.5f);
        var y = (int)MathF.Round(coordinates.Y * texture.Height - 0.5f);
        return ReadPixel(texture, x, y, sampler.WrapU, sampler.WrapV, colorSpace);
    }

    private static RgbaFloat SampleLinear(
        Texture2D texture,
        Vector2 coordinates,
        Sampler sampler,
        TextureColorSpace colorSpace)
    {
        var x = coordinates.X * texture.Width - 0.5f;
        var y = coordinates.Y * texture.Height - 0.5f;

        var x0 = (int)MathF.Floor(x);
        var y0 = (int)MathF.Floor(y);
        var x1 = x0 + 1;
        var y1 = y0 + 1;
        var tx = x - x0;
        var ty = y - y0;

        var c00 = ReadPixel(texture, x0, y0, sampler.WrapU, sampler.WrapV, colorSpace).ToVector4();
        var c10 = ReadPixel(texture, x1, y0, sampler.WrapU, sampler.WrapV, colorSpace).ToVector4();
        var c01 = ReadPixel(texture, x0, y1, sampler.WrapU, sampler.WrapV, colorSpace).ToVector4();
        var c11 = ReadPixel(texture, x1, y1, sampler.WrapU, sampler.WrapV, colorSpace).ToVector4();

        var top = Vector4.Lerp(c00, c10, tx);
        var bottom = Vector4.Lerp(c01, c11, tx);
        var blended = Vector4.Lerp(top, bottom, ty);
        return new RgbaFloat(blended.X, blended.Y, blended.Z, blended.W);
    }

    private static RgbaFloat ReadPixel(
        Texture2D texture,
        int x,
        int y,
        TextureWrapMode wrapU,
        TextureWrapMode wrapV,
        TextureColorSpace colorSpace)
    {
        x = WrapIndex(x, texture.Width, wrapU);
        y = WrapIndex(y, texture.Height, wrapV);

        var pixels = texture.PixelBytes.Span;
        var offset = ((y * texture.Width) + x) * 4;
        var r = pixels[offset] / 255f;
        var g = pixels[offset + 1] / 255f;
        var b = pixels[offset + 2] / 255f;
        var a = pixels[offset + 3] / 255f;

        if (colorSpace == TextureColorSpace.Srgb)
        {
            r = SrgbToLinear(r);
            g = SrgbToLinear(g);
            b = SrgbToLinear(b);
        }

        return new RgbaFloat(r, g, b, a);
    }

    private static int WrapIndex(int index, int size, TextureWrapMode wrapMode)
    {
        return wrapMode switch
        {
            TextureWrapMode.ClampToEdge => Math.Clamp(index, 0, size - 1),
            TextureWrapMode.MirroredRepeat => WrapMirrored(index, size),
            _ => WrapRepeat(index, size)
        };
    }

    private static int WrapRepeat(int index, int size)
    {
        var wrapped = index % size;
        return wrapped < 0 ? wrapped + size : wrapped;
    }

    private static int WrapMirrored(int index, int size)
    {
        var period = size * 2;
        var wrapped = index % period;
        if (wrapped < 0)
        {
            wrapped += period;
        }

        return wrapped < size
            ? wrapped
            : period - wrapped - 1;
    }

    private static float SrgbToLinear(float channel)
    {
        return channel <= 0.04045f
            ? channel / 12.92f
            : MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);
    }

    private static RgbaFloat Multiply(RgbaFloat left, RgbaFloat middle, RgbaFloat right)
    {
        return new RgbaFloat(
            left.R * middle.R * right.R,
            left.G * middle.G * right.G,
            left.B * middle.B * right.B,
            left.A * middle.A * right.A);
    }

    private static RgbaFloat ApplyOcclusion(RgbaFloat color, float occlusion, float strength)
    {
        var factor = 1f + ((occlusion - 1f) * strength);
        return new RgbaFloat(
            color.R * factor,
            color.G * factor,
            color.B * factor,
            color.A);
    }

    private static RgbaFloat ApplyEmissive(
        RgbaFloat color,
        MaterialEmissive emissive,
        string materialName,
        int vertexIndex,
        MeshData meshData,
        IReadOnlyDictionary<Texture2DId, Texture2D> textureById,
        IReadOnlyDictionary<SamplerId, Sampler> samplerById)
    {
        var emissiveSample = emissive.Texture is { } emissiveBinding
            ? ResolveTextureSample(emissiveBinding, materialName, vertexIndex, meshData, textureById, samplerById)
            : RgbaFloat.White;

        return new RgbaFloat(
            color.R + (emissive.Factor.X * emissiveSample.R),
            color.G + (emissive.Factor.Y * emissiveSample.G),
            color.B + (emissive.Factor.Z * emissiveSample.B),
            color.A);
    }
}
