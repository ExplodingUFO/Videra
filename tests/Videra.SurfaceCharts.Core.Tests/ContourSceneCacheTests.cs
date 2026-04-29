using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class ContourSceneCacheTests
{
    [Fact]
    public void GetOrCompute_SameRevision_ReturnsCachedInstance()
    {
        var cache = new ContourSceneCache();
        var data = CreateContourData();

        var scene1 = cache.GetOrCompute(data, revision: 1);
        var scene2 = cache.GetOrCompute(data, revision: 1);

        scene1.Should().BeSameAs(scene2);
    }

    [Fact]
    public void GetOrCompute_DifferentRevision_ReturnsNewInstance()
    {
        var cache = new ContourSceneCache();
        var data = CreateContourData();

        var scene1 = cache.GetOrCompute(data, revision: 1);
        var scene2 = cache.GetOrCompute(data, revision: 2);

        scene1.Should().NotBeSameAs(scene2);
    }

    [Fact]
    public void Invalidate_ClearsCache()
    {
        var cache = new ContourSceneCache();
        var data = CreateContourData();

        var scene1 = cache.GetOrCompute(data, revision: 1);
        cache.Invalidate();
        var scene2 = cache.GetOrCompute(data, revision: 1);

        scene1.Should().NotBeSameAs(scene2);
    }

    [Fact]
    public void GetOrCompute_WithNullData_ThrowsArgumentNullException()
    {
        var cache = new ContourSceneCache();

        var action = () => cache.GetOrCompute(null!, revision: 1);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetOrCompute_ReturnsSceneWithCorrectMetadata()
    {
        var cache = new ContourSceneCache();
        var data = CreateContourData();

        var scene = cache.GetOrCompute(data, revision: 1);

        scene.Metadata.Should().NotBeNull();
        scene.Metadata.Width.Should().Be(5);
        scene.Metadata.Height.Should().Be(5);
    }

    private static ContourChartData CreateContourData()
    {
        var width = 5;
        var height = 5;
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = x - 2f;
                var dy = y - 2f;
                values[y * width + x] = MathF.Sqrt(dx * dx + dy * dy);
            }
        }

        var field = new SurfaceScalarField(width, height, values, new SurfaceValueRange(0f, 2.83f));
        return new ContourChartData(field, levelCount: 3);
    }
}
