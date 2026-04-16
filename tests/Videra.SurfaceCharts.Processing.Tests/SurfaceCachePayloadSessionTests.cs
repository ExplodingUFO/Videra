using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Processing.Tests;

public class SurfaceCachePayloadSessionTests
{
    [Fact]
    public async Task LoadTilesAsync_ReadsOverviewAndDetailTilesFromOneSession()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var detailKey = new SurfaceTileKey(2, 1, 3, 1);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, new[] { overviewKey, detailKey });

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            await using var session = reader.CreatePayloadSession();

            var tiles = await session.LoadTilesAsync(new[] { overviewKey, detailKey });

            tiles.Should().HaveCount(2);
            tiles[0]!.Key.Should().Be(overviewKey);
            tiles[0]!.Values.ToArray().Should().Equal(6f, 10f, 22f, 26f);
            tiles[1]!.Key.Should().Be(detailKey);
            tiles[1]!.Values.ToArray().Should().Equal(22.5f, 23.5f, 30.5f, 31.5f);
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task LoadTilesAsync_UsesOnePayloadOpenForABatch()
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
            fileSystem.PayloadOpenReadCount.Should().Be(0);

            await using var session = reader.CreatePayloadSession();
            var tiles = await session.LoadTilesAsync(
                new[]
                {
                    new SurfaceTileKey(0, 0, 0, 0),
                    new SurfaceTileKey(2, 1, 3, 1)
                });

            tiles.Should().NotContainNulls();
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

    private sealed class CountingSurfaceCacheFileSystem : ISurfaceCacheFileSystem
    {
        private readonly ISurfaceCacheFileSystem inner;

        public CountingSurfaceCacheFileSystem(ISurfaceCacheFileSystem inner)
        {
            this.inner = inner;
        }

        public int PayloadOpenReadCount { get; private set; }

        public Stream CreateFile(string path)
        {
            return inner.CreateFile(path);
        }

        public Stream OpenRead(string path)
        {
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
