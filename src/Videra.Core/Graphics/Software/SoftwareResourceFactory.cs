using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareResourceFactory : IResourceFactory
{
    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        var sizeInBytes = (uint)(vertices.Length * VertexPositionNormalColor.SizeInBytes);
        var buffer = new SoftwareBuffer(sizeInBytes);
        buffer.SetData(vertices, 0);
        return buffer;
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        return new SoftwareBuffer(sizeInBytes);
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        var sizeInBytes = (uint)(indices.Length * sizeof(uint));
        var buffer = new SoftwareBuffer(sizeInBytes);
        buffer.SetData(indices, 0);
        return buffer;
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        return new SoftwareBuffer(sizeInBytes);
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        return new SoftwareBuffer(sizeInBytes);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        return new SoftwarePipeline(description.Topology, description.AlphaBlendEnabled);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        return new SoftwarePipeline(PrimitiveTopology.TriangleList, alphaBlendEnabled: false);
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        return new SoftwareShader();
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        return new SoftwareResourceSet(description);
    }
}
