using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public class SurfacePatchGeometryBuilderTests
{
    [Fact]
    public void Build_GeneratesExpectedTopologyForRegularPatch()
    {
        var builder = new SurfacePatchGeometryBuilder();

        var geometry = builder.Build(3, 2);

        geometry.SampleWidth.Should().Be(3);
        geometry.SampleHeight.Should().Be(2);
        geometry.VertexCount.Should().Be(6);
        geometry.Indices.Should().Equal(
            0u, 3u, 1u,
            1u, 3u, 4u,
            1u, 4u, 2u,
            2u, 4u, 5u);
    }

    [Fact]
    public void Build_ReusesSharedIndexPatternForMatchingPatchShape()
    {
        var builder = new SurfacePatchGeometryBuilder();

        var first = builder.Build(4, 4);
        var second = builder.Build(4, 4);

        first.Should().BeSameAs(second);
        first.Indices.Should().BeSameAs(second.Indices);
    }

    [Fact]
    public void Build_DoesNotAllowMutatingExposedSharedIndices()
    {
        var builder = new SurfacePatchGeometryBuilder();

        var first = builder.Build(2, 2);
        var second = builder.Build(2, 2);
        var act = () => ((IList<uint>)first.Indices)[0] = 99u;

        act.Should().Throw<NotSupportedException>();
        first.Indices.Should().Equal(0u, 2u, 1u, 1u, 2u, 3u);
        second.Indices.Should().Equal(0u, 2u, 1u, 1u, 2u, 3u);
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(4, 1)]
    [InlineData(1, 1)]
    public void Build_ReturnsEmptyIndexBufferForDegeneratePatch(int sampleWidth, int sampleHeight)
    {
        var builder = new SurfacePatchGeometryBuilder();

        var geometry = builder.Build(sampleWidth, sampleHeight);

        geometry.VertexCount.Should().Be(sampleWidth * sampleHeight);
        geometry.Indices.Should().BeEmpty();
    }
}
