using System.Collections.ObjectModel;
using System.Text.Json;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Reads a validated surface cache file into memory for tile lookup.
/// </summary>
public sealed class SurfaceCacheReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IReadOnlyDictionary<SurfaceTileKey, SurfaceTile> tilesByKey;

    private SurfaceCacheReader(
        SurfaceCacheHeader header,
        SurfaceMetadata metadata,
        IReadOnlyDictionary<SurfaceTileKey, SurfaceTile> tilesByKey)
    {
        Header = header;
        Metadata = metadata;
        this.tilesByKey = tilesByKey;
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
    /// Reads and validates a surface cache file.
    /// </summary>
    /// <param name="cachePath">The cache file path.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the read.</param>
    /// <returns>The parsed surface cache.</returns>
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
            #pragma warning disable CA2007
            await using (var stream = File.OpenRead(cachePath))
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

            var tilesByKey = new Dictionary<SurfaceTileKey, SurfaceTile>(tileDtos.Count);
            foreach (var tileDto in tileDtos)
            {
                var tile = tileDto?.ToModel()
                    ?? throw new InvalidDataException("Surface cache contains a null tile entry.");

                if (!tilesByKey.TryAdd(tile.Key, tile))
                {
                    throw new InvalidDataException($"Surface cache contains a duplicate tile '{tile.Key}'.");
                }
            }

            return new SurfaceCacheReader(
                header,
                metadata,
                new ReadOnlyDictionary<SurfaceTileKey, SurfaceTile>(tilesByKey));
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
    /// Tries to get a tile from the loaded cache.
    /// </summary>
    /// <param name="tileKey">The requested tile key.</param>
    /// <param name="tile">When this method returns, contains the tile when found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> when the tile exists; otherwise <c>false</c>.</returns>
    public bool TryGetTile(SurfaceTileKey tileKey, out SurfaceTile? tile)
    {
        return tilesByKey.TryGetValue(tileKey, out tile);
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
        float[]? Values)
    {
        public SurfaceTile ToModel()
        {
            return new SurfaceTile(
                Key?.ToModel() ?? throw new InvalidDataException("Surface cache tile key is missing."),
                Width,
                Height,
                Bounds?.ToModel() ?? throw new InvalidDataException("Surface cache tile bounds are missing."),
                Values ?? throw new InvalidDataException("Surface cache tile values are missing."),
                ValueRange?.ToModel() ?? throw new InvalidDataException("Surface cache tile value range is missing."));
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
