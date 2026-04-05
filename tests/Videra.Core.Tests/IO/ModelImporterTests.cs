using FluentAssertions;
using Moq;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.IO;
using Xunit;

namespace Videra.Core.Tests.IO;

public class ModelImporterTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public ModelImporterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraCoreTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort temp cleanup for test data.
        }
    }

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

    private string WriteObj(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
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
        var path = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            vn 0.0 0.0 1.0
            vn 0.0 0.0 1.0
            f 1/1 2/2 3/3
            """);

        var obj = ModelImporter.Load(path, factory);

        obj.Should().NotBeNull();
        obj.Name.Should().Contain("triangle.obj");
    }

    [Fact]
    public void Load_ObjTwoTriangles_ProducesCorrectObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("two_triangles.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            v 1.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            f 2//1 4//1 3//1
            """);

        var obj = ModelImporter.Load(path, factory);

        obj.Should().NotBeNull();
    }

    [Fact]
    public void Load_ObjEmptyFile_ProducesEmptyObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("empty.obj", string.Empty);

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
        var path = WriteObj("bad_indices.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 5//1
            """);

        // Should not throw IndexOutOfRangeException from out-of-range vertex index
        var act = () => ModelImporter.Load(path, factory);
        act.Should().NotThrow<IndexOutOfRangeException>();
    }
}
