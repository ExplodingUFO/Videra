using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class SoftwareBackendIntegrationTests
{
    [Fact]
    public void SoftwareBackend_FullRenderCycle_InitializesAndDisposesWithoutGpu()
    {
        using var backend = new SoftwareBackend();

        backend.IsInitialized.Should().BeFalse();

        backend.Initialize(IntPtr.Zero, 100, 100);

        backend.IsInitialized.Should().BeTrue();
        backend.Width.Should().Be(100);
        backend.Height.Should().Be(100);

        backend.SetClearColor(new System.Numerics.Vector4(0.25f, 0.5f, 0.75f, 1f));
        backend.BeginFrame();

        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();

        factory.Should().BeOfType<SoftwareResourceFactory>();
        executor.Should().BeOfType<SoftwareCommandExecutor>();

        var vertices = new[]
        {
            new VertexPositionNormalColor(new System.Numerics.Vector3(-0.5f, -0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new System.Numerics.Vector3(0f, 0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new System.Numerics.Vector3(0.5f, -0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };

        using var vertexBuffer = factory.CreateVertexBuffer(vertices);
        using var indexBuffer = factory.CreateIndexBuffer(indices);
        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(vertexBuffer);
        executor.SetIndexBuffer(indexBuffer);
        executor.Clear(0f, 0f, 0f, 1f);
        executor.DrawIndexed(indexCount: 3u);

        backend.EndFrame();

        backend.Resize(64, 48);

        backend.Width.Should().Be(64);
        backend.Height.Should().Be(48);
    }
}
