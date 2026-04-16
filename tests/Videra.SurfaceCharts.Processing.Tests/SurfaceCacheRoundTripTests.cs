using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Xunit;

#pragma warning disable CA2007

namespace Videra.SurfaceCharts.Processing.Tests;

public class SurfaceCacheRoundTripTests
{
    [Fact]
    public async Task WriteAsync_PersistsMetadataAcrossRoundTrip()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var tileKeys = new[]
        {
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileKey(2, 1, 3, 1)
        };

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, tileKeys);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);

            reader.Metadata.Width.Should().Be(source.Metadata.Width);
            reader.Metadata.Height.Should().Be(source.Metadata.Height);
            reader.Metadata.HorizontalAxis.Label.Should().Be(source.Metadata.HorizontalAxis.Label);
            reader.Metadata.HorizontalAxis.Unit.Should().Be(source.Metadata.HorizontalAxis.Unit);
            reader.Metadata.HorizontalAxis.Minimum.Should().Be(source.Metadata.HorizontalAxis.Minimum);
            reader.Metadata.HorizontalAxis.Maximum.Should().Be(source.Metadata.HorizontalAxis.Maximum);
            reader.Metadata.VerticalAxis.Label.Should().Be(source.Metadata.VerticalAxis.Label);
            reader.Metadata.VerticalAxis.Unit.Should().Be(source.Metadata.VerticalAxis.Unit);
            reader.Metadata.VerticalAxis.Minimum.Should().Be(source.Metadata.VerticalAxis.Minimum);
            reader.Metadata.VerticalAxis.Maximum.Should().Be(source.Metadata.VerticalAxis.Maximum);
            reader.Metadata.ValueRange.Should().Be(source.Metadata.ValueRange);
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_PersistsTilesAcrossLevels()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var intermediateKey = new SurfaceTileKey(1, 0, 1, 0);
        var detailKey = new SurfaceTileKey(2, 1, 3, 1);
        var tileKeys = new[]
        {
            overviewKey,
            intermediateKey,
            detailKey
        };

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, tileKeys);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);

            foreach (var tileKey in tileKeys)
            {
                var expected = await source.GetRequiredTileAsync(tileKey);
                var actual = await reader.LoadTileAsync(tileKey);

                actual.Should().NotBeNull();
                AssertTile(actual!, expected);
            }

            (await reader.LoadTileAsync(overviewKey))!.Bounds.Should().Be(new SurfaceTileBounds(0, 0, 8, 4));
            (await reader.LoadTileAsync(intermediateKey))!.Bounds.Should().Be(new SurfaceTileBounds(4, 0, 4, 4));
            (await reader.LoadTileAsync(detailKey))!.Bounds.Should().Be(new SurfaceTileBounds(6, 2, 2, 2));
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_PersistsTileStatisticsAcrossRoundTrip()
    {
        var cachePath = CreateCachePath();
        var source = CreateSource();
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, new[] { tileKey });

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            var expectedTile = await source.GetRequiredTileAsync(tileKey);
            var actualTile = await reader.LoadTileAsync(tileKey);

            actualTile.Should().NotBeNull();
            actualTile!.Statistics.Should().Be(expectedTile.Statistics);
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_PreservesExistingCacheWhenSerializationFails()
    {
        var cachePath = CreateCachePath();
        var validSource = CreateSource();
        var validTileKeys = new[]
        {
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileKey(2, 1, 3, 1)
        };

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, validSource, validTileKeys);
            var originalContent = await File.ReadAllTextAsync(cachePath);

            var failingSource = new NonFiniteTileSource();
            var act = async () => await SurfaceCacheWriter.WriteAsync(
                cachePath,
                failingSource,
                new[] { new SurfaceTileKey(0, 0, 0, 0) });

            await act.Should().ThrowAsync<Exception>();

            var currentContent = await File.ReadAllTextAsync(cachePath);
            currentContent.Should().Be(originalContent);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            reader.Metadata.Width.Should().Be(validSource.Metadata.Width);
            reader.Metadata.Height.Should().Be(validSource.Metadata.Height);
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_CreatesDestinationDirectoryBeforeWritingPayloadSidecar()
    {
        var cachePath = CreateCachePath(createDirectory: false);
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);

        try
        {
            Directory.Exists(Path.GetDirectoryName(cachePath)!).Should().BeFalse();

            await SurfaceCacheWriter.WriteAsync(cachePath, CreateSource(width: 2, height: 2), new[] { tileKey });

            File.Exists(cachePath).Should().BeTrue();
            File.Exists(cachePath + ".bin").Should().BeTrue();
        }
        finally
        {
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_RestoresExistingCacheWhenPayloadReplaceFails()
    {
        var cachePath = CreateCachePath();
        var payloadPath = cachePath + ".bin";
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);
        var originalSource = CreateSource(width: 2, height: 2);
        var replacementSource = CreateSource(width: 4, height: 4);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, originalSource, new[] { tileKey });

            var originalManifest = await File.ReadAllTextAsync(cachePath);
            var expectedTile = await originalSource.GetRequiredTileAsync(tileKey);
            SurfaceCacheFileSystem.Current = new ThrowingSurfaceCacheFileSystem(
                SurfaceCacheFileSystem.Current,
                destinationPath => string.Equals(destinationPath, payloadPath, StringComparison.Ordinal),
                "Simulated payload replacement failure.");

            var act = async () => await SurfaceCacheWriter.WriteAsync(cachePath, replacementSource, new[] { tileKey });
            await act.Should().ThrowAsync<IOException>();

            var currentManifest = await File.ReadAllTextAsync(cachePath);
            currentManifest.Should().Be(originalManifest);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            var actualTile = await reader.LoadTileAsync(tileKey);

            reader.Metadata.Width.Should().Be(originalSource.Metadata.Width);
            reader.Metadata.Height.Should().Be(originalSource.Metadata.Height);
            actualTile.Should().NotBeNull();
            AssertTile(actualTile!, expectedTile);
        }
        finally
        {
            SurfaceCacheFileSystem.ResetForTests();
            DeleteCachePath(cachePath);
        }
    }

    [Fact]
    public async Task WriteAsync_RestoresExistingPayloadWhenManifestReplaceFails()
    {
        var cachePath = CreateCachePath();
        var payloadPath = cachePath + ".bin";
        var tileKey = new SurfaceTileKey(0, 0, 0, 0);
        var originalSource = CreateSource(width: 2, height: 2);
        var replacementSource = CreateSource(width: 4, height: 4);

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, originalSource, new[] { tileKey });

            var originalManifest = await File.ReadAllTextAsync(cachePath);
            var expectedTile = await originalSource.GetRequiredTileAsync(tileKey);
            SurfaceCacheFileSystem.Current = new ThrowingSurfaceCacheFileSystem(
                SurfaceCacheFileSystem.Current,
                destinationPath => string.Equals(destinationPath, cachePath, StringComparison.Ordinal),
                "Simulated manifest replacement failure.");

            var act = async () => await SurfaceCacheWriter.WriteAsync(cachePath, replacementSource, new[] { tileKey });
            await act.Should().ThrowAsync<IOException>();

            var currentManifest = await File.ReadAllTextAsync(cachePath);
            currentManifest.Should().Be(originalManifest);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);
            var actualTile = await reader.LoadTileAsync(tileKey);

            reader.Metadata.Width.Should().Be(originalSource.Metadata.Width);
            reader.Metadata.Height.Should().Be(originalSource.Metadata.Height);
            File.Exists(payloadPath).Should().BeTrue();
            actualTile.Should().NotBeNull();
            AssertTile(actualTile!, expectedTile);
        }
        finally
        {
            SurfaceCacheFileSystem.ResetForTests();
            DeleteCachePath(cachePath);
        }
    }

    private static void AssertTile(SurfaceTile actual, SurfaceTile expected)
    {
        actual.Key.Should().Be(expected.Key);
        actual.Width.Should().Be(expected.Width);
        actual.Height.Should().Be(expected.Height);
        actual.Bounds.Should().Be(expected.Bounds);
        actual.ValueRange.Should().Be(expected.ValueRange);
        actual.Statistics.Should().Be(expected.Statistics);
        actual.Values.ToArray().Should().Equal(expected.Values.ToArray());
    }

    private static ISurfaceTileSource CreateSource(int width = 8, int height = 4)
    {
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        return builder.Build(CreateMatrix(width, height));
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

    private static string CreateCachePath(bool createDirectory = true)
    {
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Videra.SurfaceCharts.Processing.Tests",
            Guid.NewGuid().ToString("N"));

        if (createDirectory)
        {
            Directory.CreateDirectory(directoryPath);
        }

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

    private sealed class NonFiniteTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(2, 2);

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            throw new Exception("Simulated serialization failure.");
        }
    }

    private sealed class ThrowingSurfaceCacheFileSystem : ISurfaceCacheFileSystem
    {
        private readonly ISurfaceCacheFileSystem inner;
        private readonly Func<string, bool> shouldThrowOnDestination;
        private readonly string errorMessage;

        public ThrowingSurfaceCacheFileSystem(
            ISurfaceCacheFileSystem inner,
            Func<string, bool> shouldThrowOnDestination,
            string errorMessage)
        {
            this.inner = inner;
            this.shouldThrowOnDestination = shouldThrowOnDestination;
            this.errorMessage = errorMessage;
        }

        public Stream CreateFile(string path)
        {
            return inner.CreateFile(path);
        }

        public bool FileExists(string path)
        {
            return inner.FileExists(path);
        }

        public Stream OpenRead(string path)
        {
            return inner.OpenRead(path);
        }

        public void CreateDirectory(string path)
        {
            inner.CreateDirectory(path);
        }

        public void ReplaceFile(string sourcePath, string destinationPath, string? backupPath)
        {
            if (shouldThrowOnDestination(destinationPath))
            {
                throw new IOException(errorMessage);
            }

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
