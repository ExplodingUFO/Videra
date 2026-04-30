using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Services;

public static class SurfaceDemoPathEvidence
{
    public const int DemoSurfaceTileWidth = 32;
    public const int DemoSurfaceTileHeight = 32;

    public static SurfaceMatrixPyramidEvidence CreateMatrixPyramidEvidence(SurfaceMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        var builder = new SurfacePyramidBuilder(DemoSurfaceTileWidth, DemoSurfaceTileHeight);
        var source = builder.Build(matrix);
        var inMemorySource = source as InMemorySurfaceTileSource
            ?? throw new InvalidOperationException("The demo surface pyramid path must build an in-memory tile source.");

        return new SurfaceMatrixPyramidEvidence(
            SourceMatrixTypeName: matrix.GetType().FullName ?? matrix.GetType().Name,
            PyramidBuilderTypeName: builder.GetType().FullName ?? builder.GetType().Name,
            MaxTileWidth: builder.MaxTileWidth,
            MaxTileHeight: builder.MaxTileHeight,
            TileSourceTypeName: inMemorySource.GetType().FullName ?? inMemorySource.GetType().Name,
            MetadataWidth: inMemorySource.Metadata.Width,
            MetadataHeight: inMemorySource.Metadata.Height,
            LevelCount: inMemorySource.Levels.Count);
    }

    public static async Task<SurfaceCachePathEvidence> CreateCachePathEvidenceAsync(
        string cacheManifestPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheManifestPath);

        var reader = await SurfaceCacheReader.ReadAsync(cacheManifestPath, cancellationToken).ConfigureAwait(false);
        var source = new SurfaceCacheTileSource(reader);
        await using (source.ConfigureAwait(false))
        {
            return new SurfaceCachePathEvidence(
                CacheReaderTypeName: reader.GetType().FullName ?? reader.GetType().Name,
                TileSourceTypeName: source.GetType().FullName ?? source.GetType().Name,
                MetadataWidth: source.Metadata.Width,
                MetadataHeight: source.Metadata.Height,
                HorizontalAxisLabel: source.Metadata.HorizontalAxis.Label,
                VerticalAxisLabel: source.Metadata.VerticalAxis.Label,
                ValueMinimum: source.Metadata.ValueRange.Minimum,
                ValueMaximum: source.Metadata.ValueRange.Maximum);
        }
    }
}

public sealed record SurfaceMatrixPyramidEvidence(
    string SourceMatrixTypeName,
    string PyramidBuilderTypeName,
    int MaxTileWidth,
    int MaxTileHeight,
    string TileSourceTypeName,
    int MetadataWidth,
    int MetadataHeight,
    int LevelCount);

public sealed record SurfaceCachePathEvidence(
    string CacheReaderTypeName,
    string TileSourceTypeName,
    int MetadataWidth,
    int MetadataHeight,
    string HorizontalAxisLabel,
    string VerticalAxisLabel,
    double ValueMinimum,
    double ValueMaximum);
