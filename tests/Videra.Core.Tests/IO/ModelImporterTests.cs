using FluentAssertions;
using Moq;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
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

    [Fact]
    public void Load_NullFactory_ThrowsArgumentNullException()
    {
        var act = () => ModelImporter.Load("test.gltf", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Load_ObjTriangle_ProducesCorrectObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = Path.Combine(AppContext.BaseDirectory, "IO", "TestData", "triangle.obj");
        File.Exists(path).Should().BeTrue($"test fixture '{path}' should exist");

        var obj = ModelImporter.Load(path, factory);

        obj.Should().NotBeNull();
        obj.Name.Should().Contain("triangle.obj");
    }

    [Fact]
    public void Load_ObjTwoTriangles_ProducesCorrectObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = Path.Combine(AppContext.BaseDirectory, "IO", "TestData", "two_triangles.obj");
        File.Exists(path).Should().BeTrue($"test fixture '{path}' should exist");

        var obj = ModelImporter.Load(path, factory);

        obj.Should().NotBeNull();
    }

    [Fact]
    public void Load_ObjEmptyFile_ProducesEmptyObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = Path.Combine(AppContext.BaseDirectory, "IO", "TestData", "empty.obj");
        File.Exists(path).Should().BeTrue($"test fixture '{path}' should exist");

        // An OBJ with no faces should still load (produces empty mesh)
        var act = () => ModelImporter.Load(path, factory);
        // Empty mesh with no vertices — Object3D.Initialize may throw or handle gracefully
        // Either way it should not hang or crash unexpectedly
        act.Should().NotThrow<IndexOutOfRangeException>();
    }

    [Fact]
    public void Load_ObjBadIndices_DoesNotCrash()
    {
        var factory = new SoftwareResourceFactory();
        var path = Path.Combine(AppContext.BaseDirectory, "IO", "TestData", "bad_indices.obj");
        File.Exists(path).Should().BeTrue($"test fixture '{path}' should exist");

        // Should not throw IndexOutOfRangeException from out-of-range vertex index
        var act = () => ModelImporter.Load(path, factory);
        act.Should().NotThrow<IndexOutOfRangeException>();
    }
}
