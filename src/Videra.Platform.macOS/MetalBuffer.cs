using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalBuffer : IBuffer
{
    private IntPtr _buffer;

    public uint SizeInBytes { get; }
    
    internal IntPtr NativeBuffer => _buffer;

    public MetalBuffer(IntPtr buffer, uint sizeInBytes)
    {
        _buffer = buffer;
        SizeInBytes = sizeInBytes;
    }

    public void Update<T>(T data) where T : unmanaged
    {
        unsafe
        {
            var dataSize = (uint)Marshal.SizeOf<T>();
            if (dataSize > SizeInBytes)
                throw new InvalidOperationException($"Data size ({dataSize}) exceeds buffer size ({SizeInBytes})");

            var contents = GetBufferContents(_buffer, SEL("contents"));
            Marshal.StructureToPtr(data, contents, false);
        }
    }

    public void UpdateArray<T>(T[] data) where T : unmanaged
    {
        unsafe
        {
            var dataSize = (uint)(data.Length * Marshal.SizeOf<T>());
            if (dataSize > SizeInBytes)
                throw new InvalidOperationException($"Data size ({dataSize}) exceeds buffer size ({SizeInBytes})");

            var contents = GetBufferContents(_buffer, SEL("contents"));
            
            fixed (T* dataPtr = data)
            {
                Buffer.MemoryCopy(dataPtr, contents.ToPointer(), SizeInBytes, dataSize);
            }
        }
    }

    public void SetData<T>(T data, uint offset) where T : unmanaged
    {
        Update(data);
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
        UpdateArray(data);
    }

    public void Dispose()
    {
        if (_buffer != IntPtr.Zero)
        {
            SendMessage(_buffer, SEL("release"));
            _buffer = IntPtr.Zero;
        }
    }

    #region Metal Interop

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr GetBufferContents(IntPtr buffer, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessage(IntPtr receiver, IntPtr selector);

    // 辅助方法：将 selector 字符串转换为 SEL (IntPtr)
    private static IntPtr SEL(string name) => sel_registerName(name);

    #endregion
}
