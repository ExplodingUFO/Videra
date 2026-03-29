using FluentAssertions;
using Videra.Core.Graphics.Wireframe;
using Xunit;

namespace Videra.Core.Tests.Graphics.Wireframe;

public sealed class EdgeExtractorTests
{
    [Fact]
    public void ExtractUniqueEdges_SingleTriangle_ReturnsThreeEdges()
    {
        var indices = new uint[] { 0, 1, 2 };

        var edges = EdgeExtractor.ExtractUniqueEdges(indices);

        // 3 unique edges from one triangle: (0,1), (1,2), (0,2)
        edges.Should().HaveCount(6);
    }

    [Fact]
    public void ExtractUniqueEdges_TwoTrianglesSharingEdge_ReturnsFiveUniqueEdges()
    {
        // Quad made of two triangles sharing edge (1,2)
        var indices = new uint[] { 0, 1, 2, 2, 1, 3 };

        var edges = EdgeExtractor.ExtractUniqueEdges(indices);

        // 5 unique edges: (0,1), (1,2), (0,2), (1,3), (2,3)
        edges.Should().HaveCount(10);
    }

    [Fact]
    public void ExtractUniqueEdges_NullInput_ReturnsEmpty()
    {
        var edges = EdgeExtractor.ExtractUniqueEdges(null!);

        edges.Should().BeEmpty();
    }

    [Fact]
    public void ExtractUniqueEdges_EmptyInput_ReturnsEmpty()
    {
        var edges = EdgeExtractor.ExtractUniqueEdges(Array.Empty<uint>());

        edges.Should().BeEmpty();
    }

    [Fact]
    public void ExtractUniqueEdges_LessThanThreeIndices_ReturnsEmpty()
    {
        var edges = EdgeExtractor.ExtractUniqueEdges(new uint[] { 0, 1 });

        edges.Should().BeEmpty();
    }

    [Fact]
    public void ExtractUniqueEdges_IncompleteTriangle_Truncates()
    {
        // 4 indices = 1 triangle + 1 leftover index (ignored)
        var edges = EdgeExtractor.ExtractUniqueEdges(new uint[] { 0, 1, 2, 3 });

        edges.Should().HaveCount(6);
    }

    [Fact]
    public void Edge_NormalizesOrder_SmallerIndexFirst()
    {
        var edge1 = new EdgeExtractor.Edge(5, 2);
        var edge2 = new EdgeExtractor.Edge(2, 5);

        edge1.V0.Should().Be(2);
        edge1.V1.Should().Be(5);
        edge1.Should().Be(edge2);
    }

    [Fact]
    public void Edge_DifferentEdges_AreNotEqual()
    {
        var edge1 = new EdgeExtractor.Edge(0, 1);
        var edge2 = new EdgeExtractor.Edge(1, 2);

        edge1.Should().NotBe(edge2);
    }

    [Fact]
    public void EstimateEdgeCount_ReturnsExpectedValue()
    {
        EdgeExtractor.EstimateEdgeCount(10).Should().Be(15);
        EdgeExtractor.EstimateEdgeCount(0).Should().Be(0);
    }

    [Fact]
    public void ExtractUniqueEdges_SharedEdgeQuad_Deduplicates()
    {
        // Two triangles forming a quad, sharing edge (1,2)
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var edges = EdgeExtractor.ExtractUniqueEdges(indices);

        // 4 unique edges: (0,1), (1,2), (0,2), (0,3), (2,3) = 5 unique edges
        edges.Should().HaveCount(10);
    }

    [Fact]
    public void ExtractUniqueEdges_IdenticalTriangles_DeduplicatesAllEdges()
    {
        // Same triangle repeated 3 times
        var indices = new uint[] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };

        var edges = EdgeExtractor.ExtractUniqueEdges(indices);

        // Only 3 unique edges
        edges.Should().HaveCount(6);
    }
}
