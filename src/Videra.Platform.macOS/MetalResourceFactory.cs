using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal sealed partial class MetalResourceFactory : IResourceFactory
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
                if (buffer == IntPtr.Zero)
                    throw new ResourceCreationException(
                        "Failed to create Metal vertex buffer.",
                        "CreateVertexBuffer");
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
                if (buffer == IntPtr.Zero)
                    throw new ResourceCreationException(
                        "Failed to create Metal index buffer.",
                        "CreateIndexBuffer");
                return new MetalBuffer(buffer, sizeInBytes);
            }
        }
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0); // MTLResourceStorageModeShared = 0
        if (buffer == IntPtr.Zero)
            throw new ResourceCreationException(
                "Failed to create Metal uniform buffer.",
                "CreateUniformBuffer");
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        if (buffer == IntPtr.Zero)
            throw new ResourceCreationException(
                "Failed to create Metal vertex buffer.",
                "CreateVertexBuffer");
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        var buffer = ObjCRuntime.CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        if (buffer == IntPtr.Zero)
            throw new ResourceCreationException(
                "Failed to create Metal index buffer.",
                "CreateIndexBuffer");
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        return CreatePipelineCore(vertexSize, hasNormals, hasColors, GetMetalShaderSource(), alphaBlendEnabled: false);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        if (IsSurfaceChartScalarPipeline(description))
        {
            return CreatePipelineCore(
                VertexPositionNormalColor.SizeInBytes,
                hasNormals: true,
                hasColors: true,
                GetSurfaceChartShaderSource(),
                alphaBlendEnabled: description.AlphaBlendEnabled);
        }

        return CreatePipelineCore(
            VertexPositionNormalColor.SizeInBytes,
            hasNormals: true,
            hasColors: true,
            GetMetalShaderSource(),
            alphaBlendEnabled: description.AlphaBlendEnabled);
    }

    private IPipeline CreatePipelineCore(uint vertexSize, bool hasNormals, bool hasColors, string shaderSource, bool alphaBlendEnabled)
    {
        Log.CreatingPipeline(_logger, vertexSize, hasNormals, hasColors);

        try
        {
            // Create library from source
            IntPtr libraryError = IntPtr.Zero;
            var library = ObjCRuntime.CreateLibraryFromSource(_device, shaderSource, ref libraryError);
            IntPtr vertexFunction = IntPtr.Zero;
            IntPtr fragmentFunction = IntPtr.Zero;
            IntPtr pipelineDescriptor = IntPtr.Zero;
            IntPtr vertexDescriptor = IntPtr.Zero;

            if (library == IntPtr.Zero)
            {
                throw new PipelineCreationException(
                    "Failed to create Metal shader library from source.",
                    "CreatePipeline");
            }

            try
            {
                // Get vertex and fragment shader functions
                vertexFunction = ObjCRuntime.GetFunction(library, "vertex_main");
                fragmentFunction = ObjCRuntime.GetFunction(library, "fragment_main");

                // Create render pipeline descriptor
                pipelineDescriptor = ObjCRuntime.AllocInit("MTLRenderPipelineDescriptor");

                // Set shader functions
                ObjCRuntime.SendMessagePtrVoid(pipelineDescriptor, ObjCRuntime.SEL("setVertexFunction:"), vertexFunction);
                ObjCRuntime.SendMessagePtrVoid(pipelineDescriptor, ObjCRuntime.SEL("setFragmentFunction:"), fragmentFunction);

                // Set color attachment format
                var colorAttachments = ObjCRuntime.RequireNonZeroHandle(
                    ObjCRuntime.SendMessage(pipelineDescriptor, ObjCRuntime.SEL("colorAttachments")),
                    "CreatePipeline",
                    "Failed to get Metal color attachments.");
                var colorAttachment = ObjCRuntime.GetObjectAtIndex(colorAttachments, 0);
                ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setPixelFormat:"), 80);
                ObjCRuntime.SendMessageInt(pipelineDescriptor, ObjCRuntime.SEL("setDepthAttachmentPixelFormat:"), MetalBackend.MetalPixelFormatDepth32Float);
                ObjCRuntime.SendMessageBool(colorAttachment, ObjCRuntime.SEL("setBlendingEnabled:"), alphaBlendEnabled);
                if (alphaBlendEnabled)
                {
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setSourceRGBBlendFactor:"), 4); // MTLBlendFactorSourceAlpha
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setDestinationRGBBlendFactor:"), 5); // MTLBlendFactorOneMinusSourceAlpha
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setRgbBlendOperation:"), 0); // MTLBlendOperationAdd
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setSourceAlphaBlendFactor:"), 1); // MTLBlendFactorOne
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setDestinationAlphaBlendFactor:"), 5); // MTLBlendFactorOneMinusSourceAlpha
                    ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setAlphaBlendOperation:"), 0); // MTLBlendOperationAdd
                }

                // Create vertex descriptor
                vertexDescriptor = CreateVertexDescriptor(vertexSize, hasNormals, hasColors);
                ObjCRuntime.SendMessagePtrVoid(pipelineDescriptor, ObjCRuntime.SEL("setVertexDescriptor:"), vertexDescriptor);

                // Create pipeline state
                IntPtr error = IntPtr.Zero;
                var pipelineState = ObjCRuntime.CreatePipelineState(_device, pipelineDescriptor, ref error);

                if (pipelineState == IntPtr.Zero)
                {
                    string errorMsg = "Failed to create pipeline state.";
                    if (error != IntPtr.Zero)
                    {
                        var errorDesc = ObjCRuntime.SendMessage(error, ObjCRuntime.SEL("localizedDescription"));
                        Log.PipelineStateCreateFailed(_logger, errorDesc.ToInt64());
                        errorMsg = $"Failed to create pipeline state. Error: {errorDesc}";
                    }

                    throw new PipelineCreationException(errorMsg, "CreatePipeline");
                }

                Log.PipelineCreated(_logger);
                return new MetalPipeline(pipelineState);
            }
            finally
            {
                ReleaseIfNeeded(vertexDescriptor);
                ReleaseIfNeeded(pipelineDescriptor);
                ReleaseIfNeeded(fragmentFunction);
                ReleaseIfNeeded(vertexFunction);
                ReleaseIfNeeded(library);
            }
        }
        catch (PipelineCreationException)
        {
            throw; // Don't double-wrap
        }
        catch (Exception ex)
        {
            Log.PipelineCreateException(_logger, ex.Message, ex);
            throw new PipelineCreationException($"Exception creating pipeline: {ex.Message}", "CreatePipeline", ex);
        }
    }

    private static bool IsSurfaceChartScalarPipeline(PipelineDescription description)
    {
        return description.ResourceLayouts is not null
            && description.ResourceLayouts.Any(static resourceLayout =>
                resourceLayout.Elements is not null
                && resourceLayout.Elements.Any(static element =>
                    element.Binding == RenderBindingSlots.SurfaceColorMap || element.Binding == RenderBindingSlots.SurfaceTileScalars));
    }

    private static string GetMetalShaderSource()
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
    float maskEnabled;
    float alphaCutoff;
};

struct CameraUniforms {
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

struct WorldUniforms {
    float4x4 worldMatrix;
};

struct AlphaMaskUniforms {
    float maskEnabled;
    float alphaCutoff;
    float2 padding;
};

vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant CameraUniforms& camera [[buffer(1)]],
    constant WorldUniforms& world [[buffer(2)]],
    constant AlphaMaskUniforms& alphaMask [[buffer(6)]],
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
    out.maskEnabled = alphaMask.maskEnabled;
    out.alphaCutoff = alphaMask.alphaCutoff;
    return out;
}

fragment float4 fragment_main(VertexOut in [[stage_in]])
{
    if (in.maskEnabled > 0.5 && in.color.a < in.alphaCutoff) {
        discard_fragment();
    }

    // DIRECT COLOR OUTPUT - No lighting, no complex math
    return in.maskEnabled > 0.5
        ? float4(in.color.rgb, 1.0)
        : in.color;
}
";
    }

    private static IntPtr CreateVertexDescriptor(uint vertexSize, bool hasNormals, bool hasColors)
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

    private static void ReleaseIfNeeded(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(handle, ObjCRuntime.SEL("release"));
        }
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

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Creating pipeline with vertexSize={VertexSize}, hasNormals={HasNormals}, hasColors={HasColors}")]
        public static partial void CreatingPipeline(ILogger logger, uint vertexSize, bool hasNormals, bool hasColors);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to create pipeline state. Error: {Error}")]
        public static partial void PipelineStateCreateFailed(ILogger logger, long error);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Pipeline created successfully")]
        public static partial void PipelineCreated(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Exception creating pipeline: {Error}")]
        public static partial void PipelineCreateException(ILogger logger, string error, Exception exception);
    }

    private static string GetSurfaceChartShaderSource()
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

struct SurfaceColorMapUniforms {
    float minimum;
    float maximum;
    float paletteCount;
    float segmentCount;
    float4 palette[256];
};

inline float load_tile_scalar(constant float4* tileScalars, uint vertexId)
{
    float4 group = tileScalars[vertexId / 4];
    uint componentIndex = vertexId % 4;
    if (componentIndex == 0) return group.x;
    if (componentIndex == 1) return group.y;
    if (componentIndex == 2) return group.z;
    return group.w;
}

inline float4 map_surface_color(float scalar, constant SurfaceColorMapUniforms& colorMap)
{
    if (colorMap.paletteCount <= 1.0 || colorMap.maximum <= colorMap.minimum)
    {
        return colorMap.palette[0];
    }

    if (scalar <= colorMap.minimum)
    {
        return colorMap.palette[0];
    }

    uint lastIndex = uint(colorMap.paletteCount - 1.0);
    if (scalar >= colorMap.maximum)
    {
        return colorMap.palette[lastIndex];
    }

    float normalized = clamp((scalar - colorMap.minimum) / (colorMap.maximum - colorMap.minimum), 0.0, 1.0);
    float scaled = normalized * colorMap.segmentCount;
    uint lowerIndex = uint(floor(scaled));
    uint upperIndex = min(lowerIndex + 1, lastIndex);
    float fraction = scaled - float(lowerIndex);
    return mix(colorMap.palette[lowerIndex], colorMap.palette[upperIndex], fraction);
}

vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant CameraUniforms& camera [[buffer(1)]],
    constant WorldUniforms& world [[buffer(2)]],
    constant SurfaceColorMapUniforms& colorMap [[buffer(4)]],
    constant float4* tileScalars [[buffer(5)]],
    uint vid [[vertex_id]])
{
    VertexOut out;
    float4 worldPos = world.worldMatrix * float4(in.position, 1.0);
    float4 viewPos = camera.viewMatrix * worldPos;
    out.position = camera.projectionMatrix * viewPos;
    out.normal = in.normal;
    out.color = map_surface_color(load_tile_scalar(tileScalars, vid), colorMap);
    return out;
}

fragment float4 fragment_main(VertexOut in [[stage_in]])
{
    float3 normal = normalize(in.normal);
    float3 lightDir = normalize(float3(-0.45, 0.75, -0.45));
    float ambient = 0.35;
    float diffuse = max(dot(normal, lightDir), 0.0) * 0.65;
    return float4(saturate(in.color.rgb * (ambient + diffuse)), in.color.a);
}
";
    }
}
