using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Xunit;

#pragma warning disable CA2007

namespace Videra.SurfaceCharts.Processing.Tests;

public class SurfaceCacheTileSourceTests
{
    [Fact]
    public async Task GetTileAsync_ReadsOverviewAndDetailTilesFromSameCache()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var detailKey = new SurfaceTileKey(2, 1, 3, 1);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, new[] { overviewKey, detailKey });

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            ISurfaceTileSource cacheSource = new SurfaceCacheTileSource(reader);

            var overviewTile = await cacheSource.GetRequiredTileAsync(overviewKey);
            var detailTile = await cacheSource.GetRequiredTileAsync(detailKey);

            overviewTile.Key.LevelX.Should().Be(0);
            overviewTile.Key.LevelY.Should().Be(0);
            overviewTile.Values.ToArray().Should().Equal(6f, 10f, 22f, 26f);

            detailTile.Key.LevelX.Should().Be(2);
            detailTile.Key.LevelY.Should().Be(1);
            detailTile.Values.ToArray().Should().Equal(22.5f, 23.5f, 30.5f, 31.5f);
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task ReadAsync_RejectsInvalidCacheHeader()
    {
        var cachePath = CreateCachePath();
        var invalidCache = """
            {
              "header": {
                "magic": "INVALID",
                "version": 1,
                "tileCount": 0
              },
              "metadata": {
                "width": 4,
                "height": 4,
                "horizontalAxis": {
                  "label": "Time",
                  "unit": "s",
                  "minimum": 0,
                  "maximum": 1
                },
                "verticalAxis": {
                  "label": "Frequency",
                  "unit": "Hz",
                  "minimum": 0,
                  "maximum": 1
                },
                "valueRange": {
                  "minimum": 0,
                  "maximum": 1
                }
              },
              "tiles": []
            }
            """;

        try
        {
            await File.WriteAllTextAsync(cachePath, invalidCache);

            var act = async () => _ = await SurfaceCacheReader.ReadAsync(cachePath);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*header*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    private static ISurfaceTileSource CreateSource()
    {
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        return builder.Build(CreateMatrix(8, 4));
    }

    private static SurfaceMatrix CreateMatrix(int width, int height)
    {
        var values = new float[width * height];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = index + 0.5f;
        }

        return new SurfaceMatrix(CreateMetadata(width, height), values);
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(-3.5, 31.5));
    }

    private static string CreateCachePath()
    {
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Videra.SurfaceCharts.Processing.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directoryPath);
        return Path.Combine(directoryPath, "surface-cache.json");
    }

    private static void DeleteCachePath(string cachePath)
    {
        var directoryPath = Path.GetDirectoryName(cachePath);
        if (directoryPath is not null && Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}

#pragma warning restore CA2007
