using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

internal class MetalBuffer : IBuffer
{
    private IntPtr _buffer;
    private bool _disposed;

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

            var contents = ObjCRuntime.SendMessage(_buffer, ObjCRuntime.SEL("contents"));
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

            var contents = ObjCRuntime.SendMessage(_buffer, ObjCRuntime.SEL("contents"));

            fixed (T* dataPtr = data)
            {
                Buffer.MemoryCopy(dataPtr, contents.ToPointer(), SizeInBytes, dataSize);
            }
        }
    }

    public void SetData<T>(T data, uint offset) where T : unmanaged
    {
        unsafe
        {
            var dataSize = (uint)Marshal.SizeOf<T>();
            if (offset + dataSize > SizeInBytes)
                throw new InvalidOperationException($"Data size ({dataSize}) with offset exceeds buffer size ({SizeInBytes})");

            var contents = ObjCRuntime.SendMessage(_buffer, ObjCRuntime.SEL("contents"));
            var target = IntPtr.Add(contents, checked((int)offset));
            Marshal.StructureToPtr(data, target, false);
        }
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
        unsafe
        {
            var dataSize = (uint)(data.Length * Marshal.SizeOf<T>());
            if (offset + dataSize > SizeInBytes)
                throw new InvalidOperationException($"Data size ({dataSize}) with offset exceeds buffer size ({SizeInBytes})");

            var contents = ObjCRuntime.SendMessage(_buffer, ObjCRuntime.SEL("contents"));
            var target = IntPtr.Add(contents, checked((int)offset));
            fixed (T* dataPtr = data)
            {
                Buffer.MemoryCopy(dataPtr, target.ToPointer(), SizeInBytes - offset, dataSize);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_buffer != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_buffer, ObjCRuntime.SEL("release"));
            _buffer = IntPtr.Zero;
        }
    }
}
