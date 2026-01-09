using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareBuffer : IBuffer
{
    private readonly byte[] _data;

    public uint SizeInBytes { get; }

    public SoftwareBuffer(uint sizeInBytes)
    {
        SizeInBytes = sizeInBytes;
        _data = new byte[sizeInBytes];
    }

    internal Span<byte> Bytes => _data;

    internal ReadOnlySpan<T> AsSpan<T>() where T : unmanaged
    {
        return MemoryMarshal.Cast<byte, T>(_data);
    }

    internal T Read<T>(uint offset) where T : unmanaged
    {
        var size = (uint)Marshal.SizeOf<T>();
        if (offset + size > SizeInBytes)
            throw new ArgumentOutOfRangeException(nameof(offset), "Read exceeds buffer size.");

        return MemoryMarshal.Read<T>(_data.AsSpan((int)offset, (int)size));
    }

    public void Update<T>(T data) where T : unmanaged
    {
        SetData(data, 0);
    }

    public void UpdateArray<T>(T[] data) where T : unmanaged
    {
        SetData(data, 0);
    }

    public void SetData<T>(T data, uint offset) where T : unmanaged
    {
        var size = (uint)Marshal.SizeOf<T>();
        if (offset + size > SizeInBytes)
            throw new ArgumentOutOfRangeException(nameof(offset), "Write exceeds buffer size.");

        var span = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref data, 1));
        span.CopyTo(_data.AsSpan((int)offset, (int)size));
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
        if (data.Length == 0)
            return;

        var size = (uint)(Marshal.SizeOf<T>() * data.Length);
        if (offset + size > SizeInBytes)
            throw new ArgumentOutOfRangeException(nameof(offset), "Write exceeds buffer size.");

        var span = MemoryMarshal.AsBytes(data.AsSpan());
        span.CopyTo(_data.AsSpan((int)offset, (int)size));
    }

    public void Dispose()
    {
    }
}
