using System.Numerics;
using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalCommandExecutor : ICommandExecutor
{
    private readonly IntPtr _commandQueue;
    private IntPtr _commandBuffer;
    private IntPtr _renderEncoder;
    private IntPtr _currentDrawable;

    public MetalCommandExecutor(IntPtr commandQueue)
    {
        _commandQueue = commandQueue;
    }

    public void BeginFrame(IntPtr metalLayer, Vector4 clearColor, IntPtr depthStencilState)
    {
        // 创建 Command Buffer
        _commandBuffer = SendMessage(_commandQueue, SEL("commandBuffer"));
        if (_commandBuffer == IntPtr.Zero)
        {
            Console.WriteLine("[Metal] Failed to create command buffer");
            return;
        }
        
        // 获取 Drawable
        _currentDrawable = SendMessage(metalLayer, SEL("nextDrawable"));
        if (_currentDrawable == IntPtr.Zero)
        {
            // 这是正常情况，可能是窗口最小化或不可见
            _commandBuffer = IntPtr.Zero;
            return;
        }

        // 获取 Texture
        var texture = SendMessage(_currentDrawable, SEL("texture"));
        if (texture == IntPtr.Zero)
        {
            Console.WriteLine("[Metal] Failed to get texture from drawable");
            return;
        }
        
        // 创建 Render Pass Descriptor
        var renderPassDesc = AllocInit("MTLRenderPassDescriptor");
        var colorAttachments = SendMessage(renderPassDesc, SEL("colorAttachments"));
        var colorAttachment = GetObjectAtIndex(colorAttachments, SEL("objectAtIndexedSubscript:"), 0);
        
        // 配置 Color Attachment
        SendMessageWithPtr(colorAttachment, SEL("setTexture:"), texture);
        SendMessageWithInt(colorAttachment, SEL("setLoadAction:"), 2); // MTLLoadActionClear
        SendMessageWithInt(colorAttachment, SEL("setStoreAction:"), 1); // MTLStoreActionStore
        SetClearColor(colorAttachment, clearColor);
        
        // 创建 Render Command Encoder
        _renderEncoder = SendMessageWithPtr(_commandBuffer, SEL("renderCommandEncoderWithDescriptor:"), renderPassDesc);
        
        // 设置深度模板状态
        if (depthStencilState != IntPtr.Zero)
            SendMessageWithPtr(_renderEncoder, SEL("setDepthStencilState:"), depthStencilState);
        
        // 释放 descriptor
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

    public void SetVertexBuffer(IBuffer buffer)
    {
        if (buffer is not MetalBuffer metalBuffer)
            throw new ArgumentException("Buffer must be a MetalBuffer");

        if (_renderEncoder != IntPtr.Zero)
        {
            SetVertexBufferAtIndex(_renderEncoder, SEL("setVertexBuffer:offset:atIndex:"), metalBuffer.NativeBuffer, 0, 0);
        }
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        // Metal 在 drawIndexed 时需要索引缓冲区，这里先存储
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
        if (_renderEncoder == IntPtr.Zero || _currentIndexBuffer == IntPtr.Zero)
            return;
            
        // Metal draw indexed primitives
        // MTLPrimitiveTypeTriangle = 3
        // MTLIndexTypeUInt32 = 1
        DrawIndexedPrimitives(
            _renderEncoder,
            SEL("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:"),
            3, // primitiveType: triangle
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
        // Metal 的清屏操作在 BeginFrame 中通过 RenderPassDescriptor 的 clearColor 完成
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

    // 辅助方法：将 selector 字符串转换为 SEL (IntPtr)
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
