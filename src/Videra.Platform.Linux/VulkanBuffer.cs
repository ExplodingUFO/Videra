using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal unsafe class VulkanBuffer : IBuffer
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly DeviceMemory _memory;
    private bool _disposed;
    public Silk.NET.Vulkan.Buffer NativeBuffer { get; }
    public uint SizeInBytes { get; }

    public VulkanBuffer(Vk vk, Device device, Silk.NET.Vulkan.Buffer buffer, DeviceMemory memory, uint sizeInBytes)
    {
        _vk = vk;
        _device = device;
        _memory = memory;
        NativeBuffer = buffer;
        SizeInBytes = sizeInBytes;
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
            throw new InvalidOperationException("Data exceeds buffer size.");

        void* mapped = null;
        _vk.MapMemory(_device, _memory, offset, size, 0, &mapped);
        Marshal.StructureToPtr(data, (IntPtr)mapped, false);
        _vk.UnmapMemory(_device, _memory);
    }

    public void SetData<T>(T[] data, uint offset) where T : unmanaged
    {
        var size = (uint)(data.Length * Marshal.SizeOf<T>());
        if (offset + size > SizeInBytes)
            throw new InvalidOperationException("Data exceeds buffer size.");

        void* mapped = null;
        _vk.MapMemory(_device, _memory, offset, size, 0, &mapped);
        fixed (T* dataPtr = data)
        {
            System.Buffer.MemoryCopy(dataPtr, mapped, SizeInBytes - offset, size);
        }
        _vk.UnmapMemory(_device, _memory);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _vk.DestroyBuffer(_device, NativeBuffer, null);
        _vk.FreeMemory(_device, _memory, null);
    }
}
