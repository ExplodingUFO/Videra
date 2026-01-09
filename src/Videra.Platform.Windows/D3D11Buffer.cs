using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal unsafe class D3D11Buffer : IBuffer
{
    private ComPtr<ID3D11Buffer> _buffer;
    private readonly ComPtr<ID3D11DeviceContext> _context;
    private readonly D3D11 _d3d11;

    public uint SizeInBytes { get; }

    internal ID3D11Buffer* NativeBuffer => _buffer.Handle;

    public D3D11Buffer(ComPtr<ID3D11Buffer> buffer, uint sizeInBytes, ComPtr<ID3D11DeviceContext> context, D3D11 d3d11)
    {
        _buffer = buffer;
        SizeInBytes = sizeInBytes;
        _context = context;
        _d3d11 = d3d11;
    }

    public void Update<T>(T data) where T : unmanaged
    {
        var dataSize = (uint)Marshal.SizeOf<T>();
        if (dataSize > SizeInBytes)
            throw new InvalidOperationException($"Data size ({dataSize}) exceeds buffer size ({SizeInBytes})");

        MappedSubresource mapped;
        var result = _context.Handle->Map((ID3D11Resource*)_buffer.Handle, 0, Map.WriteDiscard, 0, &mapped);
        if (result != 0)
            throw new Exception($"Failed to map buffer. HRESULT: 0x{result:X8}");

        Marshal.StructureToPtr(data, (IntPtr)mapped.PData, false);

        _context.Handle->Unmap((ID3D11Resource*)_buffer.Handle, 0);
    }

    public void UpdateArray<T>(T[] data) where T : unmanaged
    {
        var dataSize = (uint)(data.Length * Marshal.SizeOf<T>());
        if (dataSize > SizeInBytes)
            throw new InvalidOperationException($"Data size ({dataSize}) exceeds buffer size ({SizeInBytes})");

        MappedSubresource mapped;
        var result = _context.Handle->Map((ID3D11Resource*)_buffer.Handle, 0, Map.WriteDiscard, 0, &mapped);
        if (result != 0)
            throw new Exception($"Failed to map buffer. HRESULT: 0x{result:X8}");

        fixed (T* dataPtr = data)
        {
            Buffer.MemoryCopy(dataPtr, mapped.PData, SizeInBytes, dataSize);
        }

        _context.Handle->Unmap((ID3D11Resource*)_buffer.Handle, 0);
    }

    public void SetData<T>(T data, uint offset) where T : unmanaged
    {
        var dataSize = (uint)Marshal.SizeOf<T>();
        if (offset + dataSize > SizeInBytes)
            throw new InvalidOperationException($"Data with offset ({offset + dataSize}) exceeds buffer size ({SizeInBytes})");

        MappedSubresource mapped;
        var mapType = offset + dataSize < SizeInBytes ? Map.WriteNoOverwrite : Map.WriteDiscard;
        var result = _context.Handle->Map((ID3D11Resource*)_buffer.Handle, 0, mapType, 0, &mapped);
        if (result != 0)
            throw new Exception($"Failed to map buffer. HRESULT: 0x{result:X8}");

        Marshal.StructureToPtr(data, (IntPtr)((byte*)mapped.PData + offset), false);

        _context.Handle->Unmap((ID3D11Resource*)_buffer.Handle, 0);
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
        var dataSize = (uint)(data.Length * Marshal.SizeOf<T>());
        if (offset + dataSize > SizeInBytes)
            throw new InvalidOperationException($"Data with offset ({offset + dataSize}) exceeds buffer size ({SizeInBytes})");

        MappedSubresource mapped;
        var mapType = offset + dataSize < SizeInBytes ? Map.WriteNoOverwrite : Map.WriteDiscard;
        var result = _context.Handle->Map((ID3D11Resource*)_buffer.Handle, 0, mapType, 0, &mapped);
        if (result != 0)
            throw new Exception($"Failed to map buffer. HRESULT: 0x{result:X8}");

        fixed (T* dataPtr = data)
        {
            Buffer.MemoryCopy(dataPtr, (byte*)mapped.PData + offset, SizeInBytes - offset, dataSize);
        }

        _context.Handle->Unmap((ID3D11Resource*)_buffer.Handle, 0);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}
