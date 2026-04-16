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
            await using var cacheSource = new SurfaceCacheTileSource(reader);

            var overviewTile = await cacheSource.GetRequiredTileAsync(overviewKey);
            var detailTile = await cacheSource.GetRequiredTileAsync(detailKey);

            overviewTile.Key.LevelX.Should().Be(0);
            overviewTile.Key.LevelY.Should().Be(0);
            overviewTile.Bounds.Should().Be(new SurfaceTileBounds(0, 0, 8, 4));
            overviewTile.Values.ToArray().Should().Equal(6f, 10f, 22f, 26f);

            detailTile.Key.LevelX.Should().Be(2);
            detailTile.Key.LevelY.Should().Be(1);
            detailTile.Bounds.Should().Be(new SurfaceTileBounds(6, 2, 2, 2));
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
                "version": 2,
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

    [Fact]
    public async Task ReadAsync_RejectsMalformedJsonAsInvalidData()
    {
        var cachePath = CreateCachePath();

        try
        {
            await File.WriteAllTextAsync(cachePath, "{");

            var act = async () => _ = await SurfaceCacheReader.ReadAsync(cachePath);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*invalid*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task ReadAsync_RejectsSemanticallyInvalidTilePayloadAsInvalidData()
    {
        var cachePath = CreateCachePath();
        var invalidCache = """
            {
              "header": {
                "magic": "VIDERA_SURFACE_CACHE",
                "version": 2,
                "tileCount": 1
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
              "tiles": [
                {
                  "key": {
                    "levelX": -1,
                    "levelY": 0,
                    "tileX": 0,
                    "tileY": 0
                  },
                  "width": 2,
                  "height": 2,
                  "bounds": {
                    "startX": 0,
                    "startY": 0,
                    "width": 2,
                    "height": 2
                  },
                  "valueRange": {
                    "minimum": 0,
                    "maximum": 1
                  },
                  "payloadOffset": 0,
                  "payloadLength": 16
                }
              ]
            }
            """;

        try
        {
            await File.WriteAllTextAsync(cachePath, invalidCache);

            var act = async () => _ = await SurfaceCacheReader.ReadAsync(cachePath);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*semantically invalid*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task ReadAsync_RejectsPayloadLengthThatDoesNotMatchTileDimensions()
    {
        var cachePath = CreateCachePath();

        try
        {
            await File.WriteAllTextAsync(cachePath, CreateSingleTileCacheJson(payloadOffset: 0, payloadLength: 12));

            var act = async () => _ = await SurfaceCacheReader.ReadAsync(cachePath);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*payload*length*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task ReadAsync_RejectsNegativePayloadOffset()
    {
        var cachePath = CreateCachePath();

        try
        {
            await File.WriteAllTextAsync(cachePath, CreateSingleTileCacheJson(payloadOffset: -1, payloadLength: 16));

            var act = async () => _ = await SurfaceCacheReader.ReadAsync(cachePath);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*payload*offset*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task GetTileAsync_RejectsPayloadRangesThatExtendPastPayloadFile()
    {
        var cachePath = CreateCachePath();
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);

        try
        {
            await File.WriteAllTextAsync(cachePath, CreateSingleTileCacheJson(payloadOffset: 0, payloadLength: 16));
            await File.WriteAllBytesAsync(cachePath + ".bin", new byte[8]);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            await using var cacheSource = new SurfaceCacheTileSource(reader);

            var act = async () => await cacheSource.GetTileAsync(tileKey);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*payload*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task GetTileAsync_FailsWhenPayloadFileIsMissing()
    {
        var cachePath = CreateCachePath();
        var payloadPath = cachePath + ".bin";
        var source = CreateSource();
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, new[] { tileKey });

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            await using var cacheSource = new SurfaceCacheTileSource(reader);

            // Verify the payload file exists
            File.Exists(payloadPath).Should().BeTrue("Payload file should have been created during WriteAsync");

            // Delete the payload file to simulate a missing sidecar
            File.Delete(payloadPath);

            // Attempting to get the tile should now fail because it's loaded lazily
            var act = async () => await cacheSource.GetTileAsync(tileKey);

            await act.Should().ThrowAsync<IOException>()
                .WithMessage("*payload*");
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task GetTilesAsync_BatchesReadsThroughPersistentSession()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var fileSystem = new CountingSurfaceCacheFileSystem(SurfaceCacheFileSystem.Current);

        try
        {
            SurfaceCacheFileSystem.Current = fileSystem;
            await SurfaceCacheWriter.WriteAsync(
                cachePath,
                source,
                new[]
                {
                    new SurfaceTileKey(0, 0, 0, 0),
                    new SurfaceTileKey(2, 1, 3, 1)
                });

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            await using var cacheSource = new SurfaceCacheTileSource(reader);
            var batchSource = cacheSource.Should().BeAssignableTo<ISurfaceTileBatchSource>().Subject;
            fileSystem.PayloadOpenReadCount.Should().Be(0);

            var tiles = await batchSource.GetTilesAsync(
                new[]
                {
                    new SurfaceTileKey(0, 0, 0, 0),
                    new SurfaceTileKey(2, 1, 3, 1)
                });

            tiles.Should().HaveCount(2);
            tiles[0]!.Key.Should().Be(new SurfaceTileKey(0, 0, 0, 0));
            tiles[1]!.Key.Should().Be(new SurfaceTileKey(2, 1, 3, 1));
            fileSystem.PayloadOpenReadCount.Should().Be(1);

            _ = await cacheSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));
            fileSystem.PayloadOpenReadCount.Should().Be(1);
        }
        finally
        {
            SurfaceCacheFileSystem.ResetForTests();
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

    private static string CreateSingleTileCacheJson(long payloadOffset, int payloadLength)
    {
        return $$"""
            {
              "header": {
                "magic": "VIDERA_SURFACE_CACHE",
                "version": 2,
                "tileCount": 1
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
              "tiles": [
                {
                  "key": {
                    "levelX": 0,
                    "levelY": 0,
                    "tileX": 0,
                    "tileY": 0
                  },
                  "width": 2,
                  "height": 2,
                  "bounds": {
                    "startX": 0,
                    "startY": 0,
                    "width": 2,
                    "height": 2
                  },
                  "valueRange": {
                    "minimum": 0,
                    "maximum": 1
                  },
                  "payloadOffset": {{payloadOffset}},
                  "payloadLength": {{payloadLength}}
                }
              ]
            }
            """;
    }

    private sealed class CountingSurfaceCacheFileSystem : ISurfaceCacheFileSystem
    {
        private readonly ISurfaceCacheFileSystem inner;

        public CountingSurfaceCacheFileSystem(ISurfaceCacheFileSystem inner)
        {
            this.inner = inner;
        }

        public int OpenReadCount { get; private set; }

        public int PayloadOpenReadCount { get; private set; }

        public Stream CreateFile(string path)
        {
            return inner.CreateFile(path);
        }

        public Stream OpenRead(string path)
        {
            OpenReadCount++;
            if (path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                PayloadOpenReadCount++;
            }

            return inner.OpenRead(path);
        }

        public bool FileExists(string path)
        {
            return inner.FileExists(path);
        }

        public void CreateDirectory(string path)
        {
            inner.CreateDirectory(path);
        }

        public void ReplaceFile(string sourcePath, string destinationPath, string? backupPath)
        {
            inner.ReplaceFile(sourcePath, destinationPath, backupPath);
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            inner.MoveFile(sourcePath, destinationPath);
        }

        public void DeleteFile(string path)
        {
            inner.DeleteFile(path);
        }
    }
}

#pragma warning restore CA2007
