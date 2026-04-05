using System.Numerics;
using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal sealed partial class MetalCommandExecutor : ICommandExecutor
{
    private const int LoadActionClear = 2;
    private const int StoreActionDontCare = 0;
    private const int StoreActionStore = 1;

    private readonly IntPtr _commandQueue;
    private readonly ILogger _logger;
    private IntPtr _commandBuffer;
    private IntPtr _renderEncoder;
    private IntPtr _currentDrawable;

    public MetalCommandExecutor(IntPtr commandQueue, ILogger<MetalCommandExecutor>? logger = null)
    {
        _commandQueue = commandQueue;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<MetalCommandExecutor>();
    }

    public void BeginFrame(IntPtr metalLayer, Vector4 clearColor, IntPtr depthStencilState, IntPtr depthTexture)
    {
        _currentIndexBuffer = IntPtr.Zero;

        // Create Command Buffer
        _commandBuffer = ObjCRuntime.SendMessage(_commandQueue, ObjCRuntime.SEL("commandBuffer"));
        if (_commandBuffer == IntPtr.Zero)
        {
            Log.CommandBufferCreateFailed(_logger);
            return;
        }

        // Get Drawable
        _currentDrawable = ObjCRuntime.SendMessage(metalLayer, ObjCRuntime.SEL("nextDrawable"));
        if (_currentDrawable == IntPtr.Zero)
        {
            // Normal case - window may be minimized or not visible
            _commandBuffer = IntPtr.Zero;
            return;
        }

        // Get Texture
        var texture = ObjCRuntime.SendMessage(_currentDrawable, ObjCRuntime.SEL("texture"));
        if (texture == IntPtr.Zero)
        {
            Log.DrawableTextureMissing(_logger);
            return;
        }

        // Create Render Pass Descriptor
        var renderPassDesc = ObjCRuntime.AllocInit("MTLRenderPassDescriptor");
        var colorAttachments = ObjCRuntime.SendMessage(renderPassDesc, ObjCRuntime.SEL("colorAttachments"));
        var colorAttachment = ObjCRuntime.GetObjectAtIndex(colorAttachments, 0);

        // Configure Color Attachment
        ObjCRuntime.SendMessagePtr(colorAttachment, ObjCRuntime.SEL("setTexture:"), texture);
        ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setLoadAction:"), LoadActionClear);
        ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setStoreAction:"), StoreActionStore);
        SetClearColor(colorAttachment, clearColor);

        if (depthTexture != IntPtr.Zero)
        {
            var depthAttachment = ObjCRuntime.SendMessage(renderPassDesc, ObjCRuntime.SEL("depthAttachment"));
            ObjCRuntime.SendMessagePtrVoid(depthAttachment, ObjCRuntime.SEL("setTexture:"), depthTexture);
            ObjCRuntime.SendMessageInt(depthAttachment, ObjCRuntime.SEL("setLoadAction:"), LoadActionClear);
            ObjCRuntime.SendMessageInt(depthAttachment, ObjCRuntime.SEL("setStoreAction:"), StoreActionDontCare);
            ObjCRuntime.SendMessageDoubleArg(depthAttachment, ObjCRuntime.SEL("setClearDepth:"), 1.0);
        }

        // Create Render Command Encoder
        _renderEncoder = ObjCRuntime.SendMessagePtr(_commandBuffer, ObjCRuntime.SEL("renderCommandEncoderWithDescriptor:"), renderPassDesc);

        if (_renderEncoder == IntPtr.Zero)
        {
            Log.RenderEncoderCreateFailed(_logger);
        }

        // Set render states
        if (_renderEncoder != IntPtr.Zero)
        {
            // Disable backface culling so all faces are visible
            ObjCRuntime.SendMessageInt(_renderEncoder, ObjCRuntime.SEL("setCullMode:"), 0); // MTLCullModeNone = 0

            // Set fill mode
            ObjCRuntime.SendMessageInt(_renderEncoder, ObjCRuntime.SEL("setTriangleFillMode:"), 0); // MTLTriangleFillModeFill = 0

            // Set front face to counter-clockwise
            ObjCRuntime.SendMessageInt(_renderEncoder, ObjCRuntime.SEL("setFrontFacingWinding:"), 1); // MTLWindingCounterClockwise = 1
        }

        if (depthStencilState != IntPtr.Zero)
        {
            ObjCRuntime.SendMessagePtrVoid(_renderEncoder, ObjCRuntime.SEL("setDepthStencilState:"), depthStencilState);
        }

        // Release descriptor
        ObjCRuntime.SendMessageVoid(renderPassDesc, ObjCRuntime.SEL("release"));
    }

    public void EndFrame()
    {
        if (_renderEncoder != IntPtr.Zero)
        {
            ObjCRuntime.SendMessage(_renderEncoder, ObjCRuntime.SEL("endEncoding"));
            _renderEncoder = IntPtr.Zero;
        }

        if (_commandBuffer != IntPtr.Zero)
        {
            if (_currentDrawable != IntPtr.Zero)
            {
                ObjCRuntime.SendMessagePtr(_commandBuffer, ObjCRuntime.SEL("presentDrawable:"), _currentDrawable);
            }
            ObjCRuntime.SendMessage(_commandBuffer, ObjCRuntime.SEL("commit"));
            ObjCRuntime.SendMessage(_commandBuffer, ObjCRuntime.SEL("waitUntilCompleted"));

            _currentIndexBuffer = IntPtr.Zero;
            _currentDrawable = IntPtr.Zero;
            _commandBuffer = IntPtr.Zero;
        }
    }

    public void SetPipeline(IPipeline pipeline)
    {
        if (pipeline is MetalPipeline metalPipeline && _renderEncoder != IntPtr.Zero)
        {
            var pipelineState = metalPipeline.NativePipelineState;
            if (pipelineState != IntPtr.Zero)
            {
                ObjCRuntime.SendMessagePtr(_renderEncoder, ObjCRuntime.SEL("setRenderPipelineState:"), pipelineState);
            }
        }
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
        if (buffer is not MetalBuffer metalBuffer)
            throw new ArgumentException("Buffer must be a MetalBuffer");

        if (_renderEncoder != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVertexBuffer(_renderEncoder, ObjCRuntime.SEL("setVertexBuffer:offset:atIndex:"), metalBuffer.NativeBuffer, 0, index);
        }
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        // Metal needs the index buffer at drawIndexed time, store it here
        if (buffer is MetalBuffer metalBuffer)
        {
            _currentIndexBuffer = metalBuffer.NativeBuffer;
        }
    }

    private IntPtr _currentIndexBuffer = IntPtr.Zero;

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
        // Placeholder: Would set resource bindings here
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        // Default to triangle mode
        DrawIndexed(3, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        if (_renderEncoder == IntPtr.Zero)
        {
            Log.DrawIndexedSkippedNoEncoder(_logger);
            return;
        }

        if (_currentIndexBuffer == IntPtr.Zero)
        {
            Log.DrawIndexedSkippedNoIndexBuffer(_logger);
            return;
        }

        ObjCRuntime.SendMessageDrawIndexedPrimitives(
            _renderEncoder,
            ObjCRuntime.SEL("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:"),
            primitiveType,
            indexCount,
            1, // indexType: uint32
            _currentIndexBuffer,
            firstIndex * 4 // indexBufferOffset (4 bytes per uint32)
        );
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        if (_renderEncoder == IntPtr.Zero)
            return;

        ObjCRuntime.SendMessageDrawPrimitives(
            _renderEncoder,
            ObjCRuntime.SEL("drawPrimitives:vertexStart:vertexCount:"),
            3, // primitiveType: triangle
            firstVertex,
            vertexCount
        );
    }

    public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
    {
        var viewport = new MTLViewport
        {
            originX = x,
            originY = y,
            width = width,
            height = height,
            znear = minDepth,
            zfar = maxDepth
        };

        ObjCRuntime.SendMessageViewport(_renderEncoder, ObjCRuntime.SEL("setViewport:"), viewport);
    }

    public void Clear(float r, float g, float b, float a)
    {
        // Metal clear is handled in BeginFrame through RenderPassDescriptor clearColor
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        var scissor = new MTLScissorRect
        {
            x = (nuint)x,
            y = (nuint)y,
            width = (nuint)width,
            height = (nuint)height
        };
        ObjCRuntime.SendMessageScissorRect(_renderEncoder, ObjCRuntime.SEL("setScissorRect:"), scissor);
    }

    public void SetDepthState(bool testEnabled, bool writeEnabled)
    {
        // Metal depth state is managed through MTLDepthStencilState in BeginFrame
        // For now, this is a no-op as depth state changes require creating new state objects
    }

    public void ResetDepthState()
    {
        // No-op for Metal - depth state is managed through MTLDepthStencilState
    }

    #region Objective-C Interop

    private static void SetClearColor(IntPtr colorAttachment, Vector4 color)
    {
        var mtlColor = new MTLClearColor
        {
            red = color.X,
            green = color.Y,
            blue = color.Z,
            alpha = color.W
        };
        ObjCRuntime.SetClearColor(colorAttachment, mtlColor);
    }

    #endregion

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to create command buffer")]
        public static partial void CommandBufferCreateFailed(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to get texture from drawable")]
        public static partial void DrawableTextureMissing(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create render encoder")]
        public static partial void RenderEncoderCreateFailed(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "DrawIndexed skipped: render encoder is null")]
        public static partial void DrawIndexedSkippedNoEncoder(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "DrawIndexed skipped: index buffer is null")]
        public static partial void DrawIndexedSkippedNoIndexBuffer(ILogger logger);
    }
}
