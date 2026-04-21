using System.Buffers.Binary;
using Videra.Core.Scene;

namespace Videra.Import.Gltf;

internal static class GltfTextureAssetReader
{
    public static Texture2D CreateOrGetTexture(
        SharpGLTF.Schema2.Texture sourceTexture,
        Dictionary<SharpGLTF.Schema2.Texture, Texture2D> textures)
    {
        if (textures.TryGetValue(sourceTexture, out var existing))
        {
            return existing;
        }

        var sourceImage = sourceTexture.PrimaryImage ?? sourceTexture.FallbackImage
            ?? throw new InvalidDataException("glTF texture is missing an image payload.");
        var imageBytes = sourceImage.Content.Content.ToArray();
        var (width, height) = ReadDimensions(imageBytes);
        var created = new Texture2D(
            Texture2DId.New(),
            sourceImage.Name ?? sourceTexture.Name ?? $"Texture {sourceTexture.LogicalIndex}",
            width,
            height,
            Texture2DPixelFormat.Rgba8Srgb,
            imageBytes,
            isSrgb: true);

        textures.Add(sourceTexture, created);
        return created;
    }

    public static Sampler CreateOrGetSampler(
        SharpGLTF.Schema2.TextureSampler? sourceSampler,
        Dictionary<SharpGLTF.Schema2.TextureSampler, Sampler> samplers,
        ref Sampler? defaultSampler)
    {
        if (sourceSampler is null)
        {
            defaultSampler ??= new Sampler(
                SamplerId.New(),
                "DefaultSampler",
                TextureFilter.Linear,
                TextureFilter.Linear,
                TextureWrapMode.Repeat,
                TextureWrapMode.Repeat);
            return defaultSampler;
        }

        if (samplers.TryGetValue(sourceSampler, out var existing))
        {
            return existing;
        }

        var created = new Sampler(
            SamplerId.New(),
            sourceSampler.Name ?? $"Sampler {sourceSampler.LogicalIndex}",
            MapMinFilter(sourceSampler.MinFilter),
            MapMagFilter(sourceSampler.MagFilter),
            MapWrapMode(sourceSampler.WrapS),
            MapWrapMode(sourceSampler.WrapT));

        samplers.Add(sourceSampler, created);
        return created;
    }

    private static TextureFilter MapMinFilter(SharpGLTF.Schema2.TextureMipMapFilter filter)
    {
        return filter switch
        {
            SharpGLTF.Schema2.TextureMipMapFilter.NEAREST
            or SharpGLTF.Schema2.TextureMipMapFilter.NEAREST_MIPMAP_NEAREST
            or SharpGLTF.Schema2.TextureMipMapFilter.NEAREST_MIPMAP_LINEAR => TextureFilter.Nearest,
            _ => TextureFilter.Linear
        };
    }

    private static TextureFilter MapMagFilter(SharpGLTF.Schema2.TextureInterpolationFilter filter)
    {
        return filter switch
        {
            SharpGLTF.Schema2.TextureInterpolationFilter.NEAREST => TextureFilter.Nearest,
            _ => TextureFilter.Linear
        };
    }

    private static TextureWrapMode MapWrapMode(SharpGLTF.Schema2.TextureWrapMode wrapMode)
    {
        return wrapMode switch
        {
            SharpGLTF.Schema2.TextureWrapMode.CLAMP_TO_EDGE => TextureWrapMode.ClampToEdge,
            SharpGLTF.Schema2.TextureWrapMode.MIRRORED_REPEAT => TextureWrapMode.MirroredRepeat,
            _ => TextureWrapMode.Repeat
        };
    }

    private static (int Width, int Height) ReadDimensions(ReadOnlySpan<byte> imageBytes)
    {
        if (TryReadPngDimensions(imageBytes, out var pngWidth, out var pngHeight))
        {
            return (pngWidth, pngHeight);
        }

        if (TryReadJpegDimensions(imageBytes, out var jpegWidth, out var jpegHeight))
        {
            return (jpegWidth, jpegHeight);
        }

        throw new InvalidDataException("Only PNG and JPEG texture payloads are currently supported.");
    }

    private static bool TryReadPngDimensions(ReadOnlySpan<byte> imageBytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        ReadOnlySpan<byte> pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (imageBytes.Length < 24 || !imageBytes[..8].SequenceEqual(pngSignature))
        {
            return false;
        }

        width = BinaryPrimitives.ReadInt32BigEndian(imageBytes.Slice(16, 4));
        height = BinaryPrimitives.ReadInt32BigEndian(imageBytes.Slice(20, 4));
        return width > 0 && height > 0;
    }

    private static bool TryReadJpegDimensions(ReadOnlySpan<byte> imageBytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (imageBytes.Length < 4 || imageBytes[0] != 0xFF || imageBytes[1] != 0xD8)
        {
            return false;
        }

        var offset = 2;
        while (offset + 8 < imageBytes.Length)
        {
            if (imageBytes[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            while (offset < imageBytes.Length && imageBytes[offset] == 0xFF)
            {
                offset++;
            }

            if (offset + 1 >= imageBytes.Length)
            {
                return false;
            }

            var marker = imageBytes[offset++];
            if (marker is 0xD9 or 0xDA)
            {
                return false;
            }

            if (offset + 1 >= imageBytes.Length)
            {
                return false;
            }

            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(imageBytes.Slice(offset, 2));
            if (segmentLength < 2 || offset + segmentLength > imageBytes.Length)
            {
                return false;
            }

            offset += 2;
            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                if (segmentLength < 7)
                {
                    return false;
                }

                height = BinaryPrimitives.ReadUInt16BigEndian(imageBytes.Slice(offset + 1, 2));
                width = BinaryPrimitives.ReadUInt16BigEndian(imageBytes.Slice(offset + 3, 2));
                return width > 0 && height > 0;
            }

            offset += segmentLength - 2;
        }

        return false;
    }
}
