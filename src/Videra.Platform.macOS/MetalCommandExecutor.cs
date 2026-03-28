using System.Numerics;
using System.Runtime.InteropServices;
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
        _commandBuffer = SendMessage(_commandQueue, SEL("commandBuffer"));
        if (_commandBuffer == IntPtr.Zero)
        {
            _logger.LogError("Failed to create command buffer");
            return;
        }

        // Get Drawable
        _currentDrawable = SendMessage(metalLayer, SEL("nextDrawable"));
        if (_currentDrawable == IntPtr.Zero)
        {
            // Normal case - window may be minimized or not visible
            _commandBuffer = IntPtr.Zero;
            return;
        }

        // Get Texture
        var texture = SendMessage(_currentDrawable, SEL("texture"));
        if (texture == IntPtr.Zero)
        {
            _logger.LogError("Failed to get texture from drawable");
            return;
        }

        // Create Render Pass Descriptor
        var renderPassDesc = AllocInit("MTLRenderPassDescriptor");
        var colorAttachments = SendMessage(renderPassDesc, SEL("colorAttachments"));
        var colorAttachment = GetObjectAtIndex(colorAttachments, SEL("objectAtIndexedSubscript:"), 0);

        // Configure Color Attachment
        SendMessageWithPtr(colorAttachment, SEL("setTexture:"), texture);
        SendMessageWithInt(colorAttachment, SEL("setLoadAction:"), 2); // MTLLoadActionClear
        SendMessageWithInt(colorAttachment, SEL("setStoreAction:"), 1); // MTLStoreActionStore
        SetClearColor(colorAttachment, clearColor);

        // Create Render Command Encoder
        _renderEncoder = SendMessageWithPtr(_commandBuffer, SEL("renderCommandEncoderWithDescriptor:"), renderPassDesc);

        if (_renderEncoder == IntPtr.Zero)
        {
            _logger.LogError("Failed to create render encoder");
        }

        // Set render states
        if (_renderEncoder != IntPtr.Zero)
        {
            // Disable backface culling so all faces are visible
            SendMessageWithInt(_renderEncoder, SEL("setCullMode:"), 0); // MTLCullModeNone = 0

            // Set fill mode
            SendMessageWithInt(_renderEncoder, SEL("setTriangleFillMode:"), 0); // MTLTriangleFillModeFill = 0

            // Set front face to counter-clockwise
            SendMessageWithInt(_renderEncoder, SEL("setFrontFacingWinding:"), 1); // MTLWindingCounterClockwise = 1
        }

        // Set depth stencil state (disabled for now, no depth buffer configured)
        if (depthStencilState != IntPtr.Zero)
        {
            _logger.LogDebug("Depth stencil state available but not applied (no depth buffer configured)");
        }

        // Release descriptor
        SendMessage(renderPassDesc, SEL("release"));
    }

    public void EndFrame()
    {
        if (_renderEncoder != IntPtr.Zero)
        {
            SendMessage(_renderEncoder, SEL("endEncoding"));
            _renderEncoder = IntPtr.Zero;
        }

        if (_commandBuffer != IntPtr.Zero)
        {
            if (_currentDrawable != IntPtr.Zero)
            {
                SendMessageWithPtr(_commandBuffer, SEL("presentDrawable:"), _currentDrawable);
            }
            SendMessage(_commandBuffer, SEL("commit"));
            SendMessage(_commandBuffer, SEL("waitUntilCompleted"));

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
                SendMessageWithPtr(_renderEncoder, SEL("setRenderPipelineState:"), pipelineState);
            }
        }
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
        if (buffer is not MetalBuffer metalBuffer)
            throw new ArgumentException("Buffer must be a MetalBuffer");

        if (_renderEncoder != IntPtr.Zero)
        {
            SetVertexBufferAtIndex(_renderEncoder, SEL("setVertexBuffer:offset:atIndex:"), metalBuffer.NativeBuffer, 0, index);
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

        DrawIndexedPrimitives(
            _renderEncoder,
            SEL("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:"),
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

        DrawPrimitivesCall(
            _renderEncoder,
            SEL("drawPrimitives:vertexStart:vertexCount:"),
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

        SetViewportStruct(_renderEncoder, SEL("setViewport:"), viewport);
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
        SetScissorRectStruct(_renderEncoder, SEL("setScissorRect:"), scissor);
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

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageWithInt(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr GetObjectAtIndex(IntPtr array, IntPtr selector, nuint index);

    // Helper: convert selector string to SEL (IntPtr)
    private static IntPtr SEL(string name) => sel_registerName(name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SetVertexBufferAtIndex(IntPtr encoder, IntPtr selector, IntPtr buffer, nuint offset, nuint index);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void DrawPrimitivesCall(IntPtr encoder, IntPtr selector, nuint primitiveType, nuint vertexStart, nuint vertexCount);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void DrawIndexedPrimitives(IntPtr encoder, IntPtr selector, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SetViewportStruct(IntPtr encoder, IntPtr selector, MTLViewport viewport);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SetScissorRectStruct(IntPtr encoder, IntPtr selector, MTLScissorRect scissor);

    private static IntPtr AllocInit(string className)
    {
        var cls = objc_getClass(className);
        var alloc = SendMessage(cls, SEL("alloc"));
        return SendMessage(alloc, SEL("init"));
    }

    private static void SetClearColor(IntPtr colorAttachment, Vector4 color)
    {
        var selector = SEL("setClearColor:");
        var mtlColor = new MTLClearColor
        {
            red = color.X,
            green = color.Y,
            blue = color.Z,
            alpha = color.W
        };
        objc_msgSend_clearColor(colorAttachment, selector, mtlColor);
    }

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_clearColor(IntPtr receiver, IntPtr selector, MTLClearColor color);

    [StructLayout(LayoutKind.Sequential)]
    private struct MTLClearColor
    {
        public double red;
        public double green;
        public double blue;
        public double alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MTLViewport
    {
        public double originX;
        public double originY;
        public double width;
        public double height;
        public double znear;
        public double zfar;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MTLScissorRect
    {
        public nuint x;
        public nuint y;
        public nuint width;
        public nuint height;
    }

    #endregion
}
