using System.Runtime.InteropServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalResourceFactory : IResourceFactory
{
    private readonly IntPtr _device;

    public MetalResourceFactory(IntPtr device)
    {
        _device = device;
    }

    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        var sizeInBytes = (uint)(vertices.Length * Marshal.SizeOf<VertexPositionNormalColor>());
        
        unsafe
        {
            fixed (VertexPositionNormalColor* dataPtr = vertices)
            {
                // 创建 Metal Buffer
                var buffer = CreateMetalBuffer(_device, (IntPtr)dataPtr, sizeInBytes);
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
                var buffer = CreateMetalBuffer(_device, (IntPtr)dataPtr, sizeInBytes);
                return new MetalBuffer(buffer, sizeInBytes);
            }
        }
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        // Metal uniform buffers are created with MTLResourceStorageModeShared
        var buffer = CreateMetalBufferEmpty(_device, sizeInBytes, 0); // MTLResourceStorageModeShared = 0
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        var buffer = CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        var buffer = CreateMetalBufferEmpty(_device, sizeInBytes, 0);
        return new MetalBuffer(buffer, sizeInBytes);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        Console.WriteLine($"[Metal] CreatePipeline called with vertexSize={vertexSize}, hasNormals={hasNormals}, hasColors={hasColors}");
        
        try
        {
            // 从源代码创建库
            var shaderSource = GetMetalShaderSource();
            var library = CreateLibraryFromSource(_device, shaderSource);
            
            if (library == IntPtr.Zero)
            {
                Console.WriteLine("[Metal] Failed to create library from source");
                return new MetalPipeline(IntPtr.Zero);
            }
            
            // 获取顶点和片段着色器函数
            var vertexFunction = GetFunction(library, "vertex_main");
            var fragmentFunction = GetFunction(library, "fragment_main");
            
            if (vertexFunction == IntPtr.Zero || fragmentFunction == IntPtr.Zero)
            {
                Console.WriteLine("[Metal] Failed to get shader functions");
                SendMessage(library, SEL("release"));
                return new MetalPipeline(IntPtr.Zero);
            }
            
            // 创建渲染管线描述符
            var pipelineDescriptor = AllocInit("MTLRenderPipelineDescriptor");
            
            // 设置着色器函数
            SendMessageWithPtr(pipelineDescriptor, SEL("setVertexFunction:"), vertexFunction);
            SendMessageWithPtr(pipelineDescriptor, SEL("setFragmentFunction:"), fragmentFunction);
            
            // 设置颜色附件格式
            var colorAttachments = SendMessage(pipelineDescriptor, SEL("colorAttachments"));
            var colorAttachment = GetObjectAtIndex(colorAttachments, 0);
            SendMessageWithInt(colorAttachment, SEL("setPixelFormat:"), 80); // MTLPixelFormatBGRA8Unorm
            
            // 【重要】不设置深度附件格式，因为我们没有深度纹理
            // 这样深度测试不会生效，但至少能显示
            // SendMessageWithInt(pipelineDescriptor, SEL("setDepthAttachmentPixelFormat:"), 252); // MTLPixelFormatDepth32Float
            
            // 创建顶点描述符
            var vertexDescriptor = CreateVertexDescriptor(vertexSize, hasNormals, hasColors);
            SendMessageWithPtr(pipelineDescriptor, SEL("setVertexDescriptor:"), vertexDescriptor);
            
            // 创建管线状态
            IntPtr error = IntPtr.Zero;
            var pipelineState = CreatePipelineState(_device, pipelineDescriptor, ref error);
            
            // 清理
            SendMessage(vertexDescriptor, SEL("release"));
            SendMessage(pipelineDescriptor, SEL("release"));
            SendMessage(fragmentFunction, SEL("release"));
            SendMessage(vertexFunction, SEL("release"));
            SendMessage(library, SEL("release"));
            
            if (pipelineState == IntPtr.Zero)
            {
                Console.WriteLine("[Metal] Failed to create pipeline state");
                if (error != IntPtr.Zero)
                {
                    var errorDesc = SendMessage(error, SEL("localizedDescription"));
                    Console.WriteLine($"[Metal] Error: {errorDesc}");
                }
                return new MetalPipeline(IntPtr.Zero);
            }
            
            Console.WriteLine("[Metal] Pipeline created successfully");
            return new MetalPipeline(pipelineState);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Metal] Exception creating pipeline: {ex.Message}");
            return new MetalPipeline(IntPtr.Zero);
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
    
    private IntPtr CreateLibraryFromSource(IntPtr device, string source)
    {
        Console.WriteLine("[Metal] Compiling shader from source...");
        var sourceStr = CreateNSString(source);
        IntPtr error = IntPtr.Zero;
        
        var library = objc_msgSend_newLibrary(device, SEL("newLibraryWithSource:options:error:"), sourceStr, IntPtr.Zero, ref error);
        
        SendMessage(sourceStr, SEL("release"));
        
        if (error != IntPtr.Zero)
        {
            var errorDesc = SendMessage(error, SEL("localizedDescription"));
            var errorCStr = SendMessage(errorDesc, SEL("UTF8String"));
            Console.WriteLine($"[Metal] Library creation error: {Marshal.PtrToStringAnsi(errorCStr)}");
            return IntPtr.Zero;
        }
        
        if (library != IntPtr.Zero)
        {
            Console.WriteLine("[Metal] Shader library compiled successfully");
        }
        
        return library;
    }
    
    private IntPtr CreateVertexDescriptor(uint vertexSize, bool hasNormals, bool hasColors)
    {
        var vertexDescriptor = AllocInit("MTLVertexDescriptor");
        var attributes = SendMessage(vertexDescriptor, SEL("attributes"));
        var layouts = SendMessage(vertexDescriptor, SEL("layouts"));
        
        int offset = 0;
        int attributeIndex = 0;
        
        // Position (attribute 0) - float3
        var attr0 = GetObjectAtIndex(attributes, attributeIndex++);
        SendMessageWithInt(attr0, SEL("setFormat:"), 30); // MTLVertexFormatFloat3
        SendMessageWithInt(attr0, SEL("setOffset:"), offset);
        SendMessageWithInt(attr0, SEL("setBufferIndex:"), 0);
        offset += 12; // 3 * 4 bytes
        
        if (hasNormals)
        {
            // Normal (attribute 1) - float3
            var attr1 = GetObjectAtIndex(attributes, attributeIndex++);
            SendMessageWithInt(attr1, SEL("setFormat:"), 30); // MTLVertexFormatFloat3
            SendMessageWithInt(attr1, SEL("setOffset:"), offset);
            SendMessageWithInt(attr1, SEL("setBufferIndex:"), 0);
            offset += 12;
        }
        
        if (hasColors)
        {
            // Color (attribute 2) - float4
            var attr2 = GetObjectAtIndex(attributes, attributeIndex++);
            SendMessageWithInt(attr2, SEL("setFormat:"), 31); // MTLVertexFormatFloat4
            SendMessageWithInt(attr2, SEL("setOffset:"), offset);
            SendMessageWithInt(attr2, SEL("setBufferIndex:"), 0);
            offset += 16;
        }
        
        // 设置布局
        var layout0 = GetObjectAtIndex(layouts, 0);
        SendMessageWithInt(layout0, SEL("setStride:"), (int)vertexSize);
        SendMessageWithInt(layout0, SEL("setStepFunction:"), 1); // MTLVertexStepFunctionPerVertex
        SendMessageWithInt(layout0, SEL("setStepRate:"), 1);
        
        return vertexDescriptor;
    }
    
    private IntPtr GetFunction(IntPtr library, string functionName)
    {
        var nameStr = CreateNSString(functionName);
        var function = SendMessageWithPtr(library, SEL("newFunctionWithName:"), nameStr);
        SendMessage(nameStr, SEL("release"));
        return function;
    }
    
    private IntPtr CreateNSString(string str)
    {
        var nsStringClass = objc_getClass("NSString");
        var alloc = SendMessage(nsStringClass, SEL("alloc"));
        
        unsafe
        {
            fixed (char* strPtr = str)
            {
                return InitWithCharacters(alloc, SEL("initWithCharacters:length:"), (IntPtr)strPtr, str.Length);
            }
        }
    }
    
    private IntPtr GetObjectAtIndex(IntPtr array, int index)
    {
        return objc_msgSend_index(array, SEL("objectAtIndexedSubscript:"), index);
    }
    
    private IntPtr CreatePipelineState(IntPtr device, IntPtr descriptor, ref IntPtr error)
    {
        return objc_msgSend_pipelineState(device, SEL("newRenderPipelineStateWithDescriptor:error:"), descriptor, ref error);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        throw new NotImplementedException("Pipeline creation will be implemented in full backend");
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new NotImplementedException("Shader creation will be implemented in full backend");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new NotImplementedException("ResourceSet creation will be implemented in full backend");
    }

    #region Metal Interop

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageWithInt(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtrAndSize(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtrSizeOptions(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length, nuint options);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithSizeAndOptions(IntPtr receiver, IntPtr selector, nuint length, nuint options);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_index(IntPtr receiver, IntPtr selector, int index);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_pipelineState(IntPtr receiver, IntPtr selector, IntPtr descriptor, ref IntPtr error);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_newLibrary(IntPtr receiver, IntPtr selector, IntPtr source, IntPtr options, ref IntPtr error);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr InitWithCharacters(IntPtr receiver, IntPtr selector, IntPtr characters, int length);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    // 辅助方法：将 selector 字符串转换为 SEL (IntPtr)
    private static IntPtr SEL(string name) => sel_registerName(name);
    
    private static IntPtr AllocInit(string className)
    {
        var cls = objc_getClass(className);
        var alloc = SendMessage(cls, SEL("alloc"));
        return SendMessage(alloc, SEL("init"));
    }

    private static IntPtr CreateMetalBuffer(IntPtr device, IntPtr data, uint length)
    {
        var selector = SEL("newBufferWithBytes:length:options:");
        return SendMessageWithPtrSizeOptions(device, selector, data, length, 0); // MTLResourceStorageModeShared = 0
    }

    private static IntPtr CreateMetalBufferEmpty(IntPtr device, uint length, uint options)
    {
        var selector = SEL("newBufferWithLength:options:");
        return SendMessageWithSizeAndOptions(device, selector, length, options);
    }

    #endregion
}
