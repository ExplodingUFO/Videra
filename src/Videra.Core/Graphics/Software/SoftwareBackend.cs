using System.Numerics;
using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareBackend : IGraphicsBackend, ISoftwareBackend
{
    private readonly SoftwareFrameBuffer _frameBuffer = new();
    private readonly SoftwareResourceFactory _resourceFactory = new();
    private readonly SoftwareCommandExecutor _executor;

    public SoftwareBackend()
    {
        _executor = new SoftwareCommandExecutor(_frameBuffer);
    }

    public bool IsInitialized { get; private set; }
    public int Width => _frameBuffer.Width;
    public int Height => _frameBuffer.Height;

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        Resize(width, height);
        IsInitialized = true;
    }

    public void Resize(int width, int height)
    {
        _frameBuffer.Resize(width, height);
        _executor.SetViewport(0, 0, width, height);
    }

    public void BeginFrame()
    {
    }

    public void EndFrame()
    {
    }

    public void SetClearColor(Vector4 color)
    {
        _frameBuffer.ClearColor = color;
    }

    public IResourceFactory GetResourceFactory() => _resourceFactory;

    public ICommandExecutor GetCommandExecutor() => _executor;

    public void CopyFrameTo(IntPtr destination, int destinationStride)
    {
        if (destination == IntPtr.Zero || _frameBuffer.Width <= 0 || _frameBuffer.Height <= 0)
            return;

        var src = _frameBuffer.ColorBufferArray;
        var rowBytes = _frameBuffer.Width * 4;

        for (int y = 0; y < _frameBuffer.Height; y++)
        {
            var srcOffset = y * rowBytes;
            var dst = IntPtr.Add(destination, y * destinationStride);
            Marshal.Copy(src, srcOffset, dst, rowBytes);
        }
    }

    public void Dispose()
    {
    }
}
