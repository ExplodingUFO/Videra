using System.Reflection;
using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;
using Videra.SurfaceCharts.Demo.Views;
using Videra.SurfaceCharts.Processing;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsHighPerformancePathTests
{
    [Fact]
    public void SurfaceDemoPathEvidence_CreatesMatrixPyramidThroughThirtyTwoSampleInMemoryTiles()
    {
        var matrix = CreateMatrix(width: 64, height: 48);

        var evidence = SurfaceDemoPathEvidence.CreateMatrixPyramidEvidence(matrix);

        evidence.SourceMatrixTypeName.Should().Be(typeof(SurfaceMatrix).FullName);
        evidence.PyramidBuilderTypeName.Should().Be(typeof(SurfacePyramidBuilder).FullName);
        evidence.MaxTileWidth.Should().Be(32);
        evidence.MaxTileHeight.Should().Be(32);
        evidence.TileSourceTypeName.Should().Be(typeof(InMemorySurfaceTileSource).FullName);
        evidence.MetadataWidth.Should().Be(matrix.Metadata.Width);
        evidence.MetadataHeight.Should().Be(matrix.Metadata.Height);
        evidence.LevelCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public void DemoInMemorySurfaceFactory_UsesInMemorySurfaceTileSource()
    {
        var createSource = typeof(MainWindow).GetMethod(
            "CreateInMemorySource",
            BindingFlags.NonPublic | BindingFlags.Static);

        createSource.Should().NotBeNull();
        var source = createSource!.Invoke(null, null).Should().BeAssignableTo<ISurfaceTileSource>().Subject;

        var inMemorySource = source.Should().BeOfType<InMemorySurfaceTileSource>().Subject;
        inMemorySource.Metadata.Width.Should().Be(64);
        inMemorySource.Metadata.Height.Should().Be(48);
        inMemorySource.Levels.Should().Contain(level => level.LevelX == 1 && level.LevelY == 1);
    }

    [Fact]
    public async Task SurfaceDemoPathEvidence_ReadsCacheBackedTileSourceMetadata()
    {
        var cachePath = FindSampleCacheManifestPath();

        var evidence = await SurfaceDemoPathEvidence.CreateCachePathEvidenceAsync(cachePath);

        evidence.CacheReaderTypeName.Should().Be(typeof(SurfaceCacheReader).FullName);
        evidence.TileSourceTypeName.Should().Be(typeof(SurfaceCacheTileSource).FullName);
        evidence.MetadataWidth.Should().BeGreaterThan(0);
        evidence.MetadataHeight.Should().BeGreaterThan(0);
        evidence.HorizontalAxisLabel.Should().NotBeNullOrWhiteSpace();
        evidence.VerticalAxisLabel.Should().NotBeNullOrWhiteSpace();
        evidence.ValueMaximum.Should().BeGreaterThanOrEqualTo(evidence.ValueMinimum);
    }

    private static SurfaceMatrix CreateMatrix(int width, int height)
    {
        var values = new float[width * height];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = index;
        }

        return new SurfaceMatrix(
            new SurfaceMetadata(
                width,
                height,
                new SurfaceAxisDescriptor("X", null, 0d, width - 1d),
                new SurfaceAxisDescriptor("Y", null, 0d, height - 1d),
                new SurfaceValueRange(0d, values[^1])),
            values);
    }

    private static string FindSampleCacheManifestPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "samples",
                "Videra.SurfaceCharts.Demo",
                "Assets",
                "sample-surface-cache",
                "sample.surfacecache.json");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate the demo sample surface cache manifest.");
    }
}
