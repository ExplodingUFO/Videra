using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.SurfaceCharts.Benchmarks;

// Benchmark-local fake backend isolates chart-local render-host contract cost.
// It intentionally does not model driver, swapchain, or native window overhead.
public sealed class BenchmarkGraphicsBackend : IGraphicsBackend
{
    private readonly BenchmarkResourceFactory _resourceFactory = new();
    private readonly BenchmarkCommandExecutor _commandExecutor = new();

    public bool IsInitialized { get; private set; }

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        IsInitialized = true;
    }

    public void Resize(int width, int height)
    {
    }

    public void BeginFrame()
    {
    }

    public void EndFrame()
    {
    }

    public void SetClearColor(Vector4 color)
    {
    }

    public IResourceFactory GetResourceFactory() => _resourceFactory;

    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

    public void Dispose()
    {
        IsInitialized = false;
    }
}

public sealed class BenchmarkCommandExecutor : ICommandExecutor, IBufferBindingCacheInvalidator
{
    public void SetPipeline(IPipeline pipeline)
    {
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
    }

    public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
    }

    public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
    {
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
    }

    public void Clear(float r, float g, float b, float a)
    {
    }

    public void SetDepthState(bool testEnabled, bool writeEnabled)
    {
    }

    public void ResetDepthState()
    {
    }

    public void ReleaseBuffer(IBuffer buffer)
    {
    }
}

public sealed class BenchmarkResourceFactory : IResourceFactory
{
    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        return new BenchmarkBuffer((uint)(vertices.Length * VertexPositionNormalColor.SizeInBytes));
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        return new BenchmarkBuffer(sizeInBytes);
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(indices);
        return new BenchmarkBuffer((uint)(indices.Length * sizeof(uint)));
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        return new BenchmarkBuffer(sizeInBytes);
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        return new BenchmarkBuffer(sizeInBytes);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        return new BenchmarkPipeline();
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        return new BenchmarkPipeline();
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        return new BenchmarkShader();
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        return new BenchmarkResourceSet();
    }
}

public sealed class BenchmarkBuffer(uint sizeInBytes) : IBuffer
{
    public uint SizeInBytes { get; } = sizeInBytes;

    public void Update<T>(T data) where T : unmanaged
    {
    }

    public void UpdateArray<T>(T[] data) where T : unmanaged
    {
    }

    public void SetData<T>(T data, uint offset) where T : unmanaged
    {
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
    }

    public void Dispose()
    {
    }
}

public sealed class BenchmarkPipeline : IPipeline
{
    public void Dispose()
    {
    }
}

public sealed class BenchmarkShader : IShader
{
    public void Dispose()
    {
    }
}

public sealed class BenchmarkResourceSet : IResourceSet
{
    public void Dispose()
    {
    }
}
