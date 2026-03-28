using FluentAssertions;
using Moq;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics.Abstractions;

public class ResourceFactoryMockTests
{
    [Fact]
    public void CreateVertexBuffer_CalledWithVertices_ReturnsMockBuffer()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();
        var vertices = new VertexPositionNormalColor[]
        {
            new(System.Numerics.Vector3.Zero, System.Numerics.Vector3.UnitY, RgbaFloat.White)
        };

        mockFactory.Setup(f => f.CreateVertexBuffer(vertices)).Returns(mockBuffer.Object);

        // Act
        var result = mockFactory.Object.CreateVertexBuffer(vertices);

        // Assert
        result.Should().BeSameAs(mockBuffer.Object);
        mockFactory.Verify(f => f.CreateVertexBuffer(vertices), Times.Once);
    }

    [Fact]
    public void CreateVertexBuffer_CalledWithSize_ReturnsMockBuffer()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();

        mockFactory.Setup(f => f.CreateVertexBuffer(1024u)).Returns(mockBuffer.Object);

        // Act
        var result = mockFactory.Object.CreateVertexBuffer(1024u);

        // Assert
        result.Should().BeSameAs(mockBuffer.Object);
        mockFactory.Verify(f => f.CreateVertexBuffer(1024u), Times.Once);
    }

    [Fact]
    public void CreateIndexBuffer_CalledWithIndices_ReturnsMockBuffer()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();
        var indices = new uint[] { 0, 1, 2 };

        mockFactory.Setup(f => f.CreateIndexBuffer(indices)).Returns(mockBuffer.Object);

        // Act
        var result = mockFactory.Object.CreateIndexBuffer(indices);

        // Assert
        result.Should().BeSameAs(mockBuffer.Object);
        mockFactory.Verify(f => f.CreateIndexBuffer(indices), Times.Once);
    }

    [Fact]
    public void CreateIndexBuffer_CalledWithSize_ReturnsMockBuffer()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();

        mockFactory.Setup(f => f.CreateIndexBuffer(512u)).Returns(mockBuffer.Object);

        // Act
        var result = mockFactory.Object.CreateIndexBuffer(512u);

        // Assert
        result.Should().BeSameAs(mockBuffer.Object);
        mockFactory.Verify(f => f.CreateIndexBuffer(512u), Times.Once);
    }

    [Fact]
    public void CreateUniformBuffer_CalledWithSize_ReturnsMockBuffer()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();

        mockFactory.Setup(f => f.CreateUniformBuffer(64u)).Returns(mockBuffer.Object);

        // Act
        var result = mockFactory.Object.CreateUniformBuffer(64u);

        // Assert
        result.Should().BeSameAs(mockBuffer.Object);
        mockFactory.Verify(f => f.CreateUniformBuffer(64u), Times.Once);
    }

    [Fact]
    public void CreatePipeline_CalledWithDescription_ReturnsMockPipeline()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockPipeline = new Mock<IPipeline>();
        var description = new PipelineDescription();

        mockFactory.Setup(f => f.CreatePipeline(description)).Returns(mockPipeline.Object);

        // Act
        var result = mockFactory.Object.CreatePipeline(description);

        // Assert
        result.Should().BeSameAs(mockPipeline.Object);
        mockFactory.Verify(f => f.CreatePipeline(description), Times.Once);
    }

    [Fact]
    public void CreatePipeline_CalledWithSimplifiedParameters_ReturnsMockPipeline()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockPipeline = new Mock<IPipeline>();

        mockFactory.Setup(f => f.CreatePipeline(40u, true, true)).Returns(mockPipeline.Object);

        // Act
        var result = mockFactory.Object.CreatePipeline(40u, true, true);

        // Assert
        result.Should().BeSameAs(mockPipeline.Object);
        mockFactory.Verify(f => f.CreatePipeline(40u, true, true), Times.Once);
    }

    [Fact]
    public void CreateShader_CalledWithParameters_ReturnsMockShader()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockShader = new Mock<IShader>();
        var bytecode = new byte[] { 0x01, 0x02, 0x03 };

        mockFactory.Setup(f => f.CreateShader(ShaderStage.Vertex, bytecode, "main"))
            .Returns(mockShader.Object);

        // Act
        var result = mockFactory.Object.CreateShader(ShaderStage.Vertex, bytecode, "main");

        // Assert
        result.Should().BeSameAs(mockShader.Object);
        mockFactory.Verify(f => f.CreateShader(ShaderStage.Vertex, bytecode, "main"), Times.Once);
    }

    [Fact]
    public void CreateResourceSet_CalledWithDescription_ReturnsMockResourceSet()
    {
        // Arrange
        var mockFactory = new Mock<IResourceFactory>();
        var mockResourceSet = new Mock<IResourceSet>();
        var mockLayout = new Mock<IResourceLayout>();
        var mockBuffer = new Mock<IBuffer>();
        var description = new ResourceSetDescription
        {
            Layout = mockLayout.Object,
            Buffers = new[] { mockBuffer.Object }
        };

        mockFactory.Setup(f => f.CreateResourceSet(description)).Returns(mockResourceSet.Object);

        // Act
        var result = mockFactory.Object.CreateResourceSet(description);

        // Assert
        result.Should().BeSameAs(mockResourceSet.Object);
        mockFactory.Verify(f => f.CreateResourceSet(description), Times.Once);
    }
}
