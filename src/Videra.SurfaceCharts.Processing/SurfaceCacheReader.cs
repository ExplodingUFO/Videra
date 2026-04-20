using System.Collections.ObjectModel;
using System.Text.Json;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Reads a validated surface cache manifest into memory for lazy tile lookup.
/// </summary>
public sealed class SurfaceCacheReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string payloadPath;
    private readonly IReadOnlyDictionary<SurfaceTileKey, SurfaceCacheEntry> entriesByKey;

    private SurfaceCacheReader(
        SurfaceCacheHeader header,
        SurfaceMetadata metadata,
        string payloadPath,
        IReadOnlyDictionary<SurfaceTileKey, SurfaceCacheEntry> entriesByKey)
    {
        Header = header;
        Metadata = metadata;
        this.payloadPath = payloadPath;
        this.entriesByKey = entriesByKey;
    }

    /// <summary>
    /// Gets the validated cache header.
    /// </summary>
    public SurfaceCacheHeader Header { get; }

    /// <summary>
    /// Gets the dataset metadata stored in the cache.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Reads and validates a surface cache manifest.
    /// </summary>
    /// <param name="cachePath">The cache manifest file path (.json).</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the read.</param>
    /// <returns>The parsed surface cache reader.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cachePath"/> is blank.</exception>
    /// <exception cref="InvalidDataException">Thrown when the cache content is invalid.</exception>
    public static async Task<SurfaceCacheReader> ReadAsync(
        string cachePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cachePath);

        try
        {
            CacheDocumentDto? document;
            var fileSystem = SurfaceCacheFileSystem.Current;
            #pragma warning disable CA2007
            await using (var stream = fileSystem.OpenRead(cachePath))
            {
                document = await JsonSerializer.DeserializeAsync<CacheDocumentDto>(
                    stream,
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
            }
            #pragma warning restore CA2007

            if (document is null)
            {
                throw new InvalidDataException("Surface cache file did not contain a valid document.");
            }

            var header = document.Header?.ToModel()
                ?? throw new InvalidDataException("Surface cache header is missing.");
            header.Validate();

            var metadata = document.Metadata?.ToModel()
                ?? throw new InvalidDataException("Surface cache metadata is missing.");
            var tileDtos = document.Tiles
                ?? throw new InvalidDataException("Surface cache tiles are missing.");

            if (header.TileCount != tileDtos.Count)
            {
                throw new InvalidDataException("Surface cache header tile count does not match the tile payload.");
            }

            var entriesByKey = new Dictionary<SurfaceTileKey, SurfaceCacheEntry>(tileDtos.Count);
            foreach (var tileDto in tileDtos)
            {
                var entry = CreateValidatedEntry(tileDto);

                if (!entriesByKey.TryAdd(entry.Key, entry))
                {
                    throw new InvalidDataException($"Surface cache contains a duplicate tile '{entry.Key}'.");
                }
            }

            var payloadPath = cachePath + ".bin";

            return new SurfaceCacheReader(
                header,
                metadata,
                payloadPath,
                new ReadOnlyDictionary<SurfaceTileKey, SurfaceCacheEntry>(entriesByKey));
        }
        catch (JsonException exception)
        {
            throw CreateInvalidPayloadException(
                cachePath,
                "Surface cache content is malformed JSON.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw CreateInvalidPayloadException(
                cachePath,
                "Surface cache content is semantically invalid.",
                exception);
        }
    }

    /// <summary>
    /// Loads a tile from the cache payload.
    /// </summary>
    /// <param name="tileKey">The requested tile key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded tile, or <c>null</c> if not found.</returns>
    /// <exception cref="IOException">Thrown when the payload file is missing or inaccessible.</exception>
    public async ValueTask<SurfaceTile?> LoadTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        if (!entriesByKey.ContainsKey(tileKey))
        {
            return null;
        }

        var session = CreatePayloadSession();
        await using (session.ConfigureAwait(false))
        {
            return await session.LoadTileAsync(tileKey, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Loads multiple tiles from the cache payload while preserving input order.
    /// </summary>
    /// <param name="tileKeys">The requested tile keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded tiles, preserving input order.</returns>
    public async ValueTask<IReadOnlyList<SurfaceTile?>> LoadTilesAsync(
        IReadOnlyList<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tileKeys);
        if (tileKeys.Count == 0)
        {
            return Array.Empty<SurfaceTile?>();
        }

        var hasCachedTile = tileKeys.Any(entriesByKey.ContainsKey);

        if (!hasCachedTile)
        {
            return new SurfaceTile?[tileKeys.Count];
        }

        var session = CreatePayloadSession();
        await using (session.ConfigureAwait(false))
        {
            return await session.LoadTilesAsync(tileKeys, cancellationToken).ConfigureAwait(false);
        }
    }

    internal bool ContainsTile(SurfaceTileKey tileKey)
    {
        return entriesByKey.ContainsKey(tileKey);
    }

    internal SurfaceCachePayloadSession CreatePayloadSession()
    {
        return new SurfaceCachePayloadSession(payloadPath, entriesByKey);
    }

    private sealed record CacheDocumentDto(
        CacheHeaderDto? Header,
        MetadataDto? Metadata,
        List<TileDto?>? Tiles);

    private sealed record CacheHeaderDto(
        string? Magic,
        int Version,
        int TileCount)
    {
        public SurfaceCacheHeader ToModel()
        {
            return new SurfaceCacheHeader(
                Magic ?? throw new InvalidDataException("Surface cache header magic is missing."),
                Version,
                TileCount);
        }
    }

    private sealed record MetadataDto(
        int Width,
        int Height,
        AxisDto? HorizontalAxis,
        AxisDto? VerticalAxis,
        ValueRangeDto? ValueRange)
    {
        public SurfaceMetadata ToModel()
        {
            return new SurfaceMetadata(
                Width,
                Height,
                HorizontalAxis?.ToModel() ?? throw new InvalidDataException("Surface cache horizontal axis is missing."),
                VerticalAxis?.ToModel() ?? throw new InvalidDataException("Surface cache vertical axis is missing."),
                ValueRange?.ToModel() ?? throw new InvalidDataException("Surface cache value range is missing."));
        }
    }

    private sealed record AxisDto(
        string? Label,
        string? Unit,
        double Minimum,
        double Maximum)
    {
        public SurfaceAxisDescriptor ToModel()
        {
            return new SurfaceAxisDescriptor(
                Label ?? throw new InvalidDataException("Surface cache axis label is missing."),
                Unit,
                Minimum,
                Maximum);
        }
    }

    private sealed record ValueRangeDto(
        double Minimum,
        double Maximum)
    {
        public SurfaceValueRange ToModel()
        {
            return new SurfaceValueRange(Minimum, Maximum);
        }
    }

    private sealed record TileDto(
        TileKeyDto? Key,
        int Width,
        int Height,
        TileBoundsDto? Bounds,
        ValueRangeDto? ValueRange,
        StatisticsDto? Statistics,
        long PayloadOffset,
        int PayloadLength)
    {
        public SurfaceCacheEntry ToModel()
        {
            return new SurfaceCacheEntry(
                Key?.ToModel() ?? throw new InvalidDataException("Surface cache tile key is missing."),
                Width,
                Height,
                Bounds?.ToModel() ?? throw new InvalidDataException("Surface cache tile bounds are missing."),
                ValueRange?.ToModel() ?? throw new InvalidDataException("Surface cache tile value range is missing."),
                Statistics?.ToModel(),
                PayloadOffset,
                PayloadLength);
        }
    }

    private sealed record StatisticsDto(
        ValueRangeDto? Range,
        double Average,
        int SampleCount,
        bool IsExact)
    {
        public SurfaceTileStatistics ToModel()
        {
            return new SurfaceTileStatistics(
                Range?.ToModel() ?? throw new InvalidDataException("Surface cache tile statistics range is missing."),
                Average,
                SampleCount,
                IsExact);
        }
    }

    private sealed record TileKeyDto(
        int LevelX,
        int LevelY,
        int TileX,
        int TileY)
    {
        public SurfaceTileKey ToModel()
        {
            return new SurfaceTileKey(LevelX, LevelY, TileX, TileY);
        }
    }

    private sealed record TileBoundsDto(
        int StartX,
        int StartY,
        int Width,
        int Height)
    {
        public SurfaceTileBounds ToModel()
        {
            return new SurfaceTileBounds(StartX, StartY, Width, Height);
        }
    }

    internal sealed class SurfaceCacheEntry
    {
        public SurfaceCacheEntry(
            SurfaceTileKey key,
            int width,
            int height,
            SurfaceTileBounds bounds,
            SurfaceValueRange valueRange,
            SurfaceTileStatistics? statistics,
            long payloadOffset,
            int payloadLength)
        {
            Key = key;
            Width = width;
            Height = height;
            Bounds = bounds;
            ValueRange = valueRange;
            Statistics = statistics;
            PayloadOffset = payloadOffset;
            PayloadLength = payloadLength;
        }

        public SurfaceTileKey Key { get; }
        public int Width { get; }
        public int Height { get; }
        public SurfaceTileBounds Bounds { get; }
        public SurfaceValueRange ValueRange { get; }
        public SurfaceTileStatistics? Statistics { get; }
        public long PayloadOffset { get; }
        public int PayloadLength { get; }
    }

    private static SurfaceCacheEntry CreateValidatedEntry(TileDto? tileDto)
    {
        var entry = tileDto?.ToModel()
            ?? throw new InvalidDataException("Surface cache contains a null tile entry.");

        ValidatePayloadLayout(entry);
        return entry;
    }

    private static void ValidatePayloadLayout(SurfaceCacheEntry entry)
    {
        if (entry.PayloadOffset < 0)
        {
            throw new InvalidDataException(
                $"Surface cache tile '{entry.Key}' payload offset '{entry.PayloadOffset}' cannot be negative.");
        }

        var expectedPayloadLength = GetExpectedPayloadLength(entry);
        if (entry.PayloadLength != expectedPayloadLength)
        {
            throw new InvalidDataException(
                $"Surface cache tile '{entry.Key}' payload length '{entry.PayloadLength}' does not match expected length '{expectedPayloadLength}'.");
        }

        if (entry.PayloadOffset > long.MaxValue - entry.PayloadLength)
        {
            throw new InvalidDataException(
                $"Surface cache tile '{entry.Key}' payload range exceeds the supported file size.");
        }
    }

    private static int GetExpectedPayloadLength(SurfaceCacheEntry entry)
    {
        var expectedPayloadLength = (long)entry.Width * entry.Height * sizeof(float);
        if (expectedPayloadLength < 0 || expectedPayloadLength > int.MaxValue)
        {
            throw new InvalidDataException(
                $"Surface cache tile '{entry.Key}' dimensions '{entry.Width}x{entry.Height}' are not supported.");
        }

        return (int)expectedPayloadLength;
    }

    internal static void EnsurePayloadRangeIsReadable(Stream stream, SurfaceCacheEntry entry)
    {
        if (entry.PayloadOffset > stream.Length || stream.Length - entry.PayloadOffset < entry.PayloadLength)
        {
            throw CreatePayloadRangeException(entry, stream.Length);
        }
    }

    internal static InvalidDataException CreatePayloadRangeException(SurfaceCacheEntry entry, long payloadFileLength)
    {
        return new InvalidDataException(
            $"Surface cache tile '{entry.Key}' payload range starting at '{entry.PayloadOffset}' with length '{entry.PayloadLength}' exceeds payload file length '{payloadFileLength}'.");
    }

    internal static JsonSerializerOptions GetSerializerOptions()
    {
        return SerializerOptions;
    }

    private static InvalidDataException CreateInvalidPayloadException(
        string cachePath,
        string reason,
        Exception innerException)
    {
        return new InvalidDataException(
            $"Surface cache '{cachePath}' is invalid. {reason}",
            innerException);
    }
}
