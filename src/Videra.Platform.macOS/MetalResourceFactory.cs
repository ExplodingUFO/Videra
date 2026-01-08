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
        return new MetalPipeline(); // Placeholder implementation
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

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtrAndSize(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithSizeAndOptions(IntPtr receiver, IntPtr selector, nuint length, nuint options);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    // 辅助方法：将 selector 字符串转换为 SEL (IntPtr)
    private static IntPtr SEL(string name) => sel_registerName(name);

    private static IntPtr CreateMetalBuffer(IntPtr device, IntPtr data, uint length)
    {
        var selector = SEL("newBufferWithBytes:length:options:");
        return SendMessageWithPtrAndSize(device, selector, data, length);
    }

    private static IntPtr CreateMetalBufferEmpty(IntPtr device, uint length, uint options)
    {
        var selector = SEL("newBufferWithLength:options:");
        return SendMessageWithSizeAndOptions(device, selector, length, options);
    }

    #endregion
}
