using System.Numerics;
using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalCommandExecutor : ICommandExecutor
{
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

    public void BeginFrame(IntPtr metalLayer, Vector4 clearColor, IntPtr depthStencilState)
    {
        // Create Command Buffer
        _commandBuffer = ObjCRuntime.SendMessage(_commandQueue, ObjCRuntime.SEL("commandBuffer"));
        if (_commandBuffer == IntPtr.Zero)
        {
            _logger.LogError("Failed to create command buffer");
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
            _logger.LogError("Failed to get texture from drawable");
            return;
        }

        // Create Render Pass Descriptor
        var renderPassDesc = ObjCRuntime.AllocInit("MTLRenderPassDescriptor");
        var colorAttachments = ObjCRuntime.SendMessage(renderPassDesc, ObjCRuntime.SEL("colorAttachments"));
        var colorAttachment = ObjCRuntime.GetObjectAtIndex(colorAttachments, 0);

        // Configure Color Attachment
        ObjCRuntime.SendMessagePtr(colorAttachment, ObjCRuntime.SEL("setTexture:"), texture);
        ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setLoadAction:"), 2); // MTLLoadActionClear
        ObjCRuntime.SendMessageInt(colorAttachment, ObjCRuntime.SEL("setStoreAction:"), 1); // MTLStoreActionStore
        SetClearColor(colorAttachment, clearColor);

        // Create Render Command Encoder
        _renderEncoder = ObjCRuntime.SendMessagePtr(_commandBuffer, ObjCRuntime.SEL("renderCommandEncoderWithDescriptor:"), renderPassDesc);

        if (_renderEncoder == IntPtr.Zero)
        {
            _logger.LogError("Failed to create render encoder");
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

        // Set depth stencil state (disabled for now, no depth buffer configured)
        if (depthStencilState != IntPtr.Zero)
        {
            _logger.LogDebug("Depth stencil state available but not applied (no depth buffer configured)");
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
            _logger.LogDebug("DrawIndexed skipped: render encoder is null");
            return;
        }

        if (_currentIndexBuffer == IntPtr.Zero)
        {
            _logger.LogDebug("DrawIndexed skipped: index buffer is null");
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
}
