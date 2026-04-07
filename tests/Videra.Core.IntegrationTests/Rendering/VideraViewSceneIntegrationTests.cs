using System.Collections;
using System.Reflection;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewSceneIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public VideraViewSceneIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraScene_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task LoadModelAsync_ValidPath_ReturnsLoadedObjectAndAddsItToScene()
    {
        var path = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var view = new VideraView();
        try
        {
            var result = await view.LoadModelAsync(path);

            result.Succeeded.Should().BeTrue();
            result.LoadedObject.Should().NotBeNull();
            result.Failure.Should().BeNull();
            GetSceneObjectCount(view).Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public async Task LoadModelsAsync_MixedPaths_ReturnsLoadedObjectsAndFailures()
    {
        var validPath = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        var missingPath = Path.Combine(_tempDir, "missing.obj");

        var view = new VideraView();
        try
        {
            var result = await view.LoadModelsAsync(new[] { validPath, missingPath });

            result.LoadedObjects.Should().HaveCount(1);
            result.Failures.Should().HaveCount(1);
            result.Failures[0].Path.Should().Be(missingPath);
            GetSceneObjectCount(view).Should().Be(1);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ReplaceScene_ReplacesEngineSceneObjects()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();
            var first = DemoMeshFactory.CreateTestCube(factory, size: 1f);
            var second = DemoMeshFactory.CreateTestCube(factory, size: 2f);

            view.AddObject(first);
            GetSceneObjectCount(view).Should().Be(1);

            view.ReplaceScene(new[] { second });

            GetSceneObjectCount(view).Should().Be(1);
            GetSceneObjects(view).Should().ContainSingle().Which.Should().BeSameAs(second);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ClearScene_RemovesAllSceneObjects()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();

            view.AddObject(DemoMeshFactory.CreateTestCube(factory, size: 1f));
            view.AddObject(DemoMeshFactory.CreateTestCube(factory, size: 2f));
            GetSceneObjectCount(view).Should().Be(2);

            view.ClearScene();

            GetSceneObjectCount(view).Should().Be(0);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void FrameAll_EmptyScene_ReturnsFalse()
    {
        var view = new VideraView();
        try
        {
            view.FrameAll().Should().BeFalse();
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void FrameAll_WithObjects_ReturnsTrueAndUpdatesCameraTarget()
    {
        var view = new VideraView();
        try
        {
            var factory = new SoftwareResourceFactory();
            var cube = DemoMeshFactory.CreateTestCube(factory, size: 4f);
            cube.Position = new System.Numerics.Vector3(5f, 0f, 0f);
            view.AddObject(cube);

            var framed = view.FrameAll();

            framed.Should().BeTrue();
            view.Engine.Camera.Target.X.Should().BeApproximately(5f, 0.001f);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Fact]
    public void ResetCamera_RestoresDefaultCameraState()
    {
        var view = new VideraView();
        try
        {
            view.Engine.Camera.Rotate(20f, 10f);
            view.Engine.Camera.Zoom(3f);

            view.ResetCamera();

            view.Engine.Camera.Yaw.Should().BeApproximately(0.5f, 0.0001f);
            view.Engine.Camera.Pitch.Should().BeApproximately(0.5f, 0.0001f);
            view.Engine.Camera.Target.Should().Be(System.Numerics.Vector3.Zero);
        }
        finally
        {
            view.Engine.Dispose();
        }
    }

    [Theory]
    [InlineData(ViewPreset.Top)]
    [InlineData(ViewPreset.Front)]
    [InlineData(ViewPreset.Isometric)]
    public void SetViewPreset_UpdatesCameraOrientation(ViewPreset preset)
    {
        var view = new VideraView();
        try
        {
            var yawBefore = view.Engine.Camera.Yaw;
            var pitchBefore = view.Engine.Camera.Pitch;

            view.SetViewPreset(preset);

            if (preset == ViewPreset.Isometric)
            {
                view.Engine.Camera.Yaw.Should().NotBe(yawBefore);
                view.Engine.Camera.Pitch.Should().NotBe(pitchBefore);
            }
            else
            {
                view.Engine.Camera.Pitch.Should().NotBe(pitchBefore);
            }
        }
        finally
        {
            view.Engine.Dispose();
        }
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
            // Best-effort temp cleanup for test artifacts.
        }
    }

    private string WriteObj(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static int GetSceneObjectCount(VideraView view)
    {
        return GetSceneObjects(view).Count;
    }

    private static IList<Object3D> GetSceneObjects(VideraView view)
    {
        var field = typeof(VideraEngine).GetField("_sceneObjects", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        var value = field!.GetValue(view.Engine);
        value.Should().BeAssignableTo<IList<Object3D>>();
        return (IList<Object3D>)value!;
    }
}
