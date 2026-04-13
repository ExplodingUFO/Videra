using System.Runtime.InteropServices;
using System.Text.Json;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Writes explicit surface cache files from a tile source.
/// </summary>
public static class SurfaceCacheWriter
{
    /// <summary>
    /// Writes the requested tiles and metadata to a cache file.
    /// </summary>
    /// <param name="cachePath">The destination cache file path.</param>
    /// <param name="source">The tile source to serialize.</param>
    /// <param name="tileKeys">The tile keys to persist.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the write.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cachePath"/> is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="tileKeys"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tileKeys"/> contains duplicates.</exception>
    public static async Task WriteAsync(
        string cachePath,
        ISurfaceTileSource source,
        IEnumerable<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cachePath);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tileKeys);

        var uniqueKeys = new HashSet<SurfaceTileKey>();
        var tileDtos = new List<TileDto>();

        var payloadPath = cachePath + ".bin";
        var tempCachePath = CreateTemporaryPath(cachePath);
        var tempPayloadPath = CreateTemporaryPath(payloadPath);
        var manifestBackupPath = CreateBackupPath(cachePath);
        var payloadBackupPath = CreateBackupPath(payloadPath);

        try
        {
            EnsureDestinationDirectoryExists(cachePath);

            #pragma warning disable CA2007
            await using (var payloadStream = File.Create(tempPayloadPath))
            {
                foreach (var tileKey in tileKeys)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!uniqueKeys.Add(tileKey))
                    {
                        throw new ArgumentException($"Duplicate tile key '{tileKey}' was provided.", nameof(tileKeys));
                    }

                    var tile = await source.GetRequiredTileAsync(tileKey, cancellationToken).ConfigureAwait(false);
                    
                    var offset = payloadStream.Position;
                    var byteCount = tile.Values.Length * sizeof(float);
                    var buffer = new byte[byteCount];
                    MemoryMarshal.Cast<float, byte>(tile.Values.Span).CopyTo(buffer);
                    
                    await payloadStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                    var length = payloadStream.Position - offset;

                    tileDtos.Add(TileDto.FromModel(tile, offset, (int)length));
                }
                await payloadStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            #pragma warning restore CA2007

            var metadata = source.GetRequiredMetadata();
            var document = new CacheDocumentDto(
                new CacheHeaderDto(
                    SurfaceCacheHeader.ExpectedMagic,
                    SurfaceCacheHeader.CurrentVersion,
                    tileDtos.Count),
                MetadataDto.FromModel(metadata),
                tileDtos);

            var directoryPath = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            #pragma warning disable CA2007
            await using (var stream = File.Create(tempCachePath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    document,
                    SurfaceCacheReader.GetSerializerOptions(),
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            #pragma warning restore CA2007

            ReplaceFile(tempPayloadPath, payloadPath, payloadBackupPath);

            try
            {
                ReplaceFile(tempCachePath, cachePath, manifestBackupPath);
            }
            catch
            {
                RestoreFile(payloadPath, payloadBackupPath);
                throw;
            }

            DeleteFile(manifestBackupPath);
            DeleteFile(payloadBackupPath);
        }
        catch
        {
            DeleteFile(tempCachePath);
            DeleteFile(tempPayloadPath);
            DeleteFile(manifestBackupPath);
            DeleteFile(payloadBackupPath);
            throw;
        }
    }

    private sealed record CacheDocumentDto(
        CacheHeaderDto? Header,
        MetadataDto? Metadata,
        List<TileDto>? Tiles);

    private sealed record CacheHeaderDto(
        string? Magic,
        int Version,
        int TileCount);

    private sealed record MetadataDto(
        int Width,
        int Height,
        AxisDto? HorizontalAxis,
        AxisDto? VerticalAxis,
        ValueRangeDto? ValueRange)
    {
        public static MetadataDto FromModel(SurfaceMetadata metadata)
        {
            return new MetadataDto(
                metadata.Width,
                metadata.Height,
                AxisDto.FromModel(metadata.HorizontalAxis),
                AxisDto.FromModel(metadata.VerticalAxis),
                ValueRangeDto.FromModel(metadata.ValueRange));
        }
    }

    private sealed record AxisDto(
        string? Label,
        string? Unit,
        double Minimum,
        double Maximum)
    {
        public static AxisDto FromModel(SurfaceAxisDescriptor axis)
        {
            return new AxisDto(axis.Label, axis.Unit, axis.Minimum, axis.Maximum);
        }
    }

    private sealed record ValueRangeDto(
        double Minimum,
        double Maximum)
    {
        public static ValueRangeDto FromModel(SurfaceValueRange valueRange)
        {
            return new ValueRangeDto(valueRange.Minimum, valueRange.Maximum);
        }
    }

    private sealed record TileDto(
        TileKeyDto? Key,
        int Width,
        int Height,
        TileBoundsDto? Bounds,
        ValueRangeDto? ValueRange,
        long PayloadOffset,
        int PayloadLength)
    {
        public static TileDto FromModel(SurfaceTile tile, long offset, int length)
        {
            return new TileDto(
                TileKeyDto.FromModel(tile.Key),
                tile.Width,
                tile.Height,
                TileBoundsDto.FromModel(tile.Bounds),
                ValueRangeDto.FromModel(tile.ValueRange),
                offset,
                length);
        }
    }

    private sealed record TileKeyDto(
        int LevelX,
        int LevelY,
        int TileX,
        int TileY)
    {
        public static TileKeyDto FromModel(SurfaceTileKey tileKey)
        {
            return new TileKeyDto(tileKey.LevelX, tileKey.LevelY, tileKey.TileX, tileKey.TileY);
        }
    }

    private sealed record TileBoundsDto(
        int StartX,
        int StartY,
        int Width,
        int Height)
    {
        public static TileBoundsDto FromModel(SurfaceTileBounds bounds)
        {
            return new TileBoundsDto(bounds.StartX, bounds.StartY, bounds.Width, bounds.Height);
        }
    }

    private static string CreateTemporaryPath(string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        var tempFileName = $"{fileName}.{Guid.NewGuid():N}.tmp";

        return string.IsNullOrWhiteSpace(directoryPath)
            ? tempFileName
            : Path.Combine(directoryPath, tempFileName);
    }

    private static string CreateBackupPath(string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        var backupFileName = $"{fileName}.{Guid.NewGuid():N}.bak";

        return string.IsNullOrWhiteSpace(directoryPath)
            ? backupFileName
            : Path.Combine(directoryPath, backupFileName);
    }

    private static void EnsureDestinationDirectoryExists(string destinationPath)
    {
        var directoryPath = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static void ReplaceFile(string temporaryPath, string destinationPath, string backupPath)
    {
        if (File.Exists(destinationPath))
        {
            DeleteFile(backupPath);
            File.Replace(temporaryPath, destinationPath, destinationBackupFileName: backupPath, ignoreMetadataErrors: true);
            return;
        }

        File.Move(temporaryPath, destinationPath);
    }

    private static void RestoreFile(string destinationPath, string backupPath)
    {
        if (File.Exists(backupPath))
        {
            if (File.Exists(destinationPath))
            {
                File.Replace(backupPath, destinationPath, destinationBackupFileName: null, ignoreMetadataErrors: true);
                return;
            }

            File.Move(backupPath, destinationPath);
            return;
        }

        DeleteFile(destinationPath);
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
