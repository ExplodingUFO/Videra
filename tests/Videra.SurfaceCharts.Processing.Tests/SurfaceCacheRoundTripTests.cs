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
        var tileKeys = new[]
        {
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileKey(1, 0, 1, 0),
            new SurfaceTileKey(2, 1, 3, 1)
        };

        try
        {
            await SurfaceCacheWriter.WriteAsync(cachePath, source, tileKeys);

            var reader = await SurfaceCacheReader.ReadAsync(cachePath);

            foreach (var tileKey in tileKeys)
            {
                var expected = await source.GetRequiredTileAsync(tileKey);
                var found = reader.TryGetTile(tileKey, out var actual);

                found.Should().BeTrue();
                actual.Should().NotBeNull();
                AssertTile(actual!, expected);
            }
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

    private static void AssertTile(SurfaceTile actual, SurfaceTile expected)
    {
        actual.Key.Should().Be(expected.Key);
        actual.Width.Should().Be(expected.Width);
        actual.Height.Should().Be(expected.Height);
        actual.Bounds.Should().Be(expected.Bounds);
        actual.ValueRange.Should().Be(expected.ValueRange);
        actual.Values.ToArray().Should().Equal(expected.Values.ToArray());
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

    private sealed class NonFiniteTileSource : ISurfaceTileSource
    {
        public SurfaceMetadata Metadata => CreateMetadata(2, 2);

        public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
        {
            var tile = new SurfaceTile(
                tileKey,
                2,
                2,
                new SurfaceTileBounds(0, 0, 2, 2),
                new[] { float.NaN, 1f, 2f, 3f },
                new SurfaceValueRange(0.0, 3.0));

            return ValueTask.FromResult<SurfaceTile?>(tile);
        }
    }
}

#pragma warning restore CA2007
