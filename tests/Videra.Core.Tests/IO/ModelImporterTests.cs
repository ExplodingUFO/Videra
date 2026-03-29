using FluentAssertions;
using Moq;
using Videra.Core.Exceptions;
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
    public void Load_CalledWithNullPath_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();

        var act = () => ModelImporter.Load(null!, mockFactory.Object);

        act.Should().Throw<InvalidModelInputException>()
            .Which.Operation.Should().Be("LoadModel");
    }

    [Fact]
    public void Load_CalledWithEmptyPath_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();

        var act = () => ModelImporter.Load(string.Empty, mockFactory.Object);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Operation.Should().Be("LoadModel");
        exception.Context.Should().ContainKey("FilePath");
    }

    [Fact]
    public void Load_CalledWithWhitespacePath_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();

        var act = () => ModelImporter.Load("   ", mockFactory.Object);

        act.Should().Throw<InvalidModelInputException>();
    }

    [Fact]
    public void Load_CalledWithDirectoryPath_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();
        var act = () => ModelImporter.Load(AppContext.BaseDirectory, mockFactory.Object);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Context.Should().ContainKey("NormalizedPath");
    }

    [Fact]
    public void Load_CalledWithNonExistentFile_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();

        var act = () => ModelImporter.Load("nonexistent_file.gltf", mockFactory.Object);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Context.Should().ContainKey("NormalizedPath");
    }

    [Fact]
    public void Load_CalledWithUnsupportedExtension_ThrowsInvalidModelInputException()
    {
        var mockFactory = CreateMockFactory();

        var act = () => ModelImporter.Load("model.xyz", mockFactory.Object);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Message.Should().Contain("supported");
        exception.Context.Should().ContainKey("Extension");
    }

    [Fact]
    public void SupportedFormats_Contains_ExpectedExtensions()
    {
        ModelImporter.SupportedFormats.Should().Contain("*.gltf");
        ModelImporter.SupportedFormats.Should().Contain("*.glb");
        ModelImporter.SupportedFormats.Should().Contain("*.obj");
    }

    [Fact]
    public void SupportedFormats_HasThreeFormats()
    {
        ModelImporter.SupportedFormats.Should().HaveCount(3);
    }
}
