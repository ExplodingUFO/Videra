using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalResourceFactory : IResourceFactory
{
    private readonly IntPtr _device;
    private readonly ILogger _logger;

    public MetalResourceFactory(IntPtr device, ILogger<MetalResourceFactory>? logger = null)
    {
        _device = device;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<MetalResourceFactory>();
    }

    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        var sizeInBytes = (uint)(vertices.Length * Marshal.SizeOf<VertexPositionNormalColor>());

        unsafe
        {
            fixed (VertexPositionNormalColor* dataPtr = vertices)
            {
                var buffer = ObjCRuntime.CreateMetalBuffer(_device, (IntPtr)dataPtr, sizeInBytes);
                return new MetalBuffer(buffer, sizeInBytes);
            }
        }
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        var sizeInBytes = (uint)(indices.Length * sizeof(uint));

        unsafe
        {
            fixed (uint* dataPtr = indices)
            {
                var buffer = ObjCRuntime.CreateMetalBuffer(_device, (IntPtr)dataPtr, sizeInBytes);
                return new MetalBuffer(buffer, sizeInBytes);
            }
        }
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0); // MTLResourceStorageModeShared = 0
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        _logger.LogInformation("Creating pipeline with vertexSize={VertexSize}, hasNormals={HasNormals}, hasColors={HasColors}", vertexSize, hasNormals, hasColors);

        try
        {
            // Create library from source
            var shaderSource = GetMetalShaderSource();
            IntPtr libraryError = IntPtr.Zero;
            var library = ObjCRuntime.CreateLibraryFromSource(_device, shaderSource, ref libraryError);

            if (library == IntPtr.Zero)
            {
                _logger.LogError("Failed to create library from source");
                return new MetalPipeline(IntPtr.Zero);
            }

            // Get vertex and fragment shader functions
            var vertexFunction = ObjCRuntime.GetFunction(library, "vertex_main");
            var fragmentFunction = ObjCRuntime.GetFunction(library, "fragment_main");

            if (vertexFunction == IntPtr.Zero || fragmentFunction == IntPtr.Zero)
            {
                _logger.LogError("Failed to get shader functions");
                ObjCRuntime.SendMessageVoid(library, ObjCRuntime.SEL("release"));
                return new MetalPipeline(IntPtr.Zero);
            }

            // Create render pipeline descriptor
            var pipelineDescriptor = ObjCRuntime.AllocInit("MTLRenderPipelineDescriptor");

            // Set shader functions
            ObjCRuntime.SendMessagePtr(pipelineDescriptor, ObjCRuntime.SEL("setVertexFunction:"), vertexFunction);
            ObjCRuntime.SendMessagePtr(pipelineDescriptor, ObjCRuntime.SEL("setFragmentFunction:"), fragmentFunction);

            // Set color attachment format
            var colorAttachments = ObjCRuntime.SendMessage(pipelineDescriptor, ObjCRuntime.SEL("colorAttachments"));
            var colorAttachment = ObjCRuntime.GetObjectAtIndex(colorAttachments, 0);
            ObjCRuntime.SendMessageInt(pipelineDescriptor, ObjCRuntime.SEL("setDepthAttachmentPixelFormat:"), MetalBackend.MetalPixelFormatDepth32Float);

            // Create vertex descriptor
            var vertexDescriptor = CreateVertexDescriptor(vertexSize, hasNormals, hasColors);
            ObjCRuntime.SendMessagePtr(pipelineDescriptor, ObjCRuntime.SEL("setVertexDescriptor:"), vertexDescriptor);

            // Create pipeline state
            IntPtr error = IntPtr.Zero;
            var pipelineState = ObjCRuntime.CreatePipelineState(_device, pipelineDescriptor, ref error);

            // Cleanup
            ObjCRuntime.SendMessageVoid(vertexDescriptor, ObjCRuntime.SEL("release"));
            ObjCRuntime.SendMessageVoid(pipelineDescriptor, ObjCRuntime.SEL("release"));
            ObjCRuntime.SendMessageVoid(fragmentFunction, ObjCRuntime.SEL("release"));
            ObjCRuntime.SendMessageVoid(vertexFunction, ObjCRuntime.SEL("release"));
            ObjCRuntime.SendMessageVoid(library, ObjCRuntime.SEL("release"));

            if (pipelineState == IntPtr.Zero)
            {
                string errorMsg = "Failed to create pipeline state.";
                if (error != IntPtr.Zero)
                {
                    var errorDesc = ObjCRuntime.SendMessage(error, ObjCRuntime.SEL("localizedDescription"));
                    _logger.LogError("Failed to create pipeline state. Error: {Error}", errorDesc);
                    errorMsg = $"Failed to create pipeline state. Error: {errorDesc}";
                }

                throw new PipelineCreationException(errorMsg, "CreatePipeline");
            }

            _logger.LogInformation("Pipeline created successfully");
            return new MetalPipeline(pipelineState);
        }
        catch (PipelineCreationException)
        {
            throw; // Don't double-wrap
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating pipeline: {Error}", ex.Message);
            throw new PipelineCreationException($"Exception creating pipeline: {ex.Message}", "CreatePipeline", ex);
        }
    }

    private string GetMetalShaderSource()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

struct VertexIn {
    float3 position [[attribute(0)]];
    float3 normal   [[attribute(1)]];
    float4 color    [[attribute(2)]];
};

struct VertexOut {
    float4 position [[position]];
    float3 normal;
    float4 color;
};

struct CameraUniforms {
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

struct WorldUniforms {
    float4x4 worldMatrix;
};

vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant CameraUniforms& camera [[buffer(1)]],
    constant WorldUniforms& world [[buffer(2)]],
    uint vid [[vertex_id]])
{
    VertexOut out;

    // Normal MVP path for all geometry
    float4 worldPos = world.worldMatrix * float4(in.position, 1.0);
    float4 viewPos = camera.viewMatrix * worldPos;
    out.position = camera.projectionMatrix * viewPos;

    // Debug Color Generation based on ID if color is black/missing
    if (in.color.a == 0.0) {
        if (vid % 3 == 0) out.color = float4(1, 0, 0, 1);
        else if (vid % 3 == 1) out.color = float4(0, 1, 0, 1);
        else out.color = float4(0, 0, 1, 1);
    } else {
        out.color = in.color;
    }

    out.normal = in.normal;
    return out;
}

fragment float4 fragment_main(VertexOut in [[stage_in]])
{
    // DIRECT COLOR OUTPUT - No lighting, no complex math
    return in.color;
}
";
    }

    private IntPtr CreateVertexDescriptor(uint vertexSize, bool hasNormals, bool hasColors)
    {
        var vertexDescriptor = ObjCRuntime.AllocInit("MTLVertexDescriptor");
        var attributes = ObjCRuntime.SendMessage(vertexDescriptor, ObjCRuntime.SEL("attributes"));
        var layouts = ObjCRuntime.SendMessage(vertexDescriptor, ObjCRuntime.SEL("layouts"));

        int offset = 0;
        int attributeIndex = 0;

        var attr0 = ObjCRuntime.GetObjectAtIndex(attributes, attributeIndex++);
        ObjCRuntime.SendMessageInt(attr0, ObjCRuntime.SEL("setFormat:"), 30);
        ObjCRuntime.SendMessageInt(attr0, ObjCRuntime.SEL("setOffset:"), offset);
        ObjCRuntime.SendMessageInt(attr0, ObjCRuntime.SEL("setBufferIndex:"), 0);
        offset += 12;

        if (hasNormals)
        {
            var attr1 = ObjCRuntime.GetObjectAtIndex(attributes, attributeIndex++);
            ObjCRuntime.SendMessageInt(attr1, ObjCRuntime.SEL("setFormat:"), 30);
            ObjCRuntime.SendMessageInt(attr1, ObjCRuntime.SEL("setOffset:"), offset);
            ObjCRuntime.SendMessageInt(attr1, ObjCRuntime.SEL("setBufferIndex:"), 0);
            offset += 12;
        }

        if (hasColors)
        {
            var attr2 = ObjCRuntime.GetObjectAtIndex(attributes, attributeIndex++);
            ObjCRuntime.SendMessageInt(attr2, ObjCRuntime.SEL("setFormat:"), 31);
            ObjCRuntime.SendMessageInt(attr2, ObjCRuntime.SEL("setOffset:"), offset);
            ObjCRuntime.SendMessageInt(attr2, ObjCRuntime.SEL("setBufferIndex:"), 0);
            offset += 16;
        }

        var layout0 = ObjCRuntime.GetObjectAtIndex(layouts, 0);
        ObjCRuntime.SendMessageInt(layout0, ObjCRuntime.SEL("setStride:"), (int)vertexSize);
        ObjCRuntime.SendMessageInt(layout0, ObjCRuntime.SEL("setStepFunction:"), 1);
        ObjCRuntime.SendMessageInt(layout0, ObjCRuntime.SEL("setStepRate:"), 1);

        return vertexDescriptor;
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        throw new UnsupportedOperationException(
            "Pipeline creation will be implemented in a future Metal backend release.",
            "CreatePipeline",
            "macOS");
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new UnsupportedOperationException(
            "Shader creation will be implemented in a future Metal backend release.",
            "CreateShader",
            "macOS");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new UnsupportedOperationException(
            "ResourceSet creation will be implemented in a future Metal backend release.",
            "CreateResourceSet",
            "macOS");
    }

    #region Metal Interop

    #endregion
}
