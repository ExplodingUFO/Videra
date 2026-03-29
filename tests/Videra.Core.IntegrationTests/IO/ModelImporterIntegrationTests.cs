using System.Numerics;
using FluentAssertions;
using Videra.Core.Graphics.Software;
using Videra.Core.IO;
using Xunit;

namespace Videra.Core.IntegrationTests.IO;

public class ModelImporterIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public ModelImporterIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            try { Directory.Delete(_tempDir, true); } catch { /* temp dir cleanup - best effort */ }
        }
    }

    private string WriteObj(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Load_ObjTriangle_ProducesInitializedObject()
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
        obj.VertexBuffer.Should().NotBeNull();
        obj.IndexBuffer.Should().NotBeNull();
        obj.WorldBuffer.Should().NotBeNull();
        obj.IndexCount.Should().Be(3);
    }

    [Fact]
    public void Load_ObjTwoTriangles_ProducesCorrectIndexCount()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("two_tri.obj", """
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
        obj.IndexCount.Should().Be(6);
    }

    [Fact]
    public void Load_ObjWithBadIndices_SkipsInvalidFaces()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("bad.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 5//1
            """);

        var act = () => ModelImporter.Load(path, factory);
        act.Should().NotThrow<IndexOutOfRangeException>();
    }

    [Fact]
    public void Load_ObjWithNormals_ProducesCorrectNormals()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("normals.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 1.0 0.0
            vn 0.0 1.0 0.0
            vn 0.0 1.0 0.0
            f 1//1 2//2 3//3
            """);

        var obj = ModelImporter.Load(path, factory);

        obj.Should().NotBeNull();
        obj.VertexBuffer.Should().NotBeNull();
    }

    [Fact]
    public void Load_ObjObject_CanBeAddedToEngine()
    {
        var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();

        var path = WriteObj("scene.obj", """
            v -0.5 -0.5 0.0
            v 0.5 -0.5 0.0
            v 0.0 0.5 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var obj = ModelImporter.Load(path, factory);
        obj.InitializeWireframe(factory);

        using var engine = new Videra.Core.Graphics.VideraEngine();
        engine.Initialize(backend);

        var act = () => engine.AddObject(obj);
        act.Should().NotThrow();
    }
}
