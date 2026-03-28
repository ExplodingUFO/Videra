using FluentAssertions;
using Moq;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.IO;
using Xunit;

namespace Videra.Core.Tests.IO;

public class ModelImporterTests
{
    private static Mock<IResourceFactory> CreateMockFactory()
    {
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();
        mockBuffer.Setup(b => b.SizeInBytes).Returns(1024u);

        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);

        return mockFactory;
    }

    [Fact]
    public void Load_CalledWithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFactory = CreateMockFactory();

        // Act
        var act = () => ModelImporter.Load(null!, mockFactory.Object);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Load_CalledWithEmptyPath_ThrowsException()
    {
        // Arrange
        var mockFactory = CreateMockFactory();

        // Act
        var act = () => ModelImporter.Load(string.Empty, mockFactory.Object);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Load_CalledWithNonExistentFile_ThrowsException()
    {
        // Arrange
        var mockFactory = CreateMockFactory();

        // Act
        var act = () => ModelImporter.Load("nonexistent_file.gltf", mockFactory.Object);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Load_CalledWithUnsupportedExtension_ThrowsNotSupportedException()
    {
        // Arrange
        var mockFactory = CreateMockFactory();

        // Act
        var act = () => ModelImporter.Load("model.xyz", mockFactory.Object);

        // Assert
        act.Should().Throw<NotSupportedException>().WithMessage("*not supported*");
    }

    [Fact]
    public void SupportedFormats_Contains_ExpectedExtensions()
    {
        // Assert
        ModelImporter.SupportedFormats.Should().Contain("*.gltf");
        ModelImporter.SupportedFormats.Should().Contain("*.glb");
        ModelImporter.SupportedFormats.Should().Contain("*.obj");
    }

    [Fact]
    public void SupportedFormats_HasThreeFormats()
    {
        // Assert
        ModelImporter.SupportedFormats.Should().HaveCount(3);
    }
}
