using System.Numerics;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Internal render-surface abstraction bound to one device and one presentation target.
/// </summary>
internal interface IRenderSurface : IDisposable
{
    bool IsInitialized { get; }

    bool UsesSoftwarePresentationCopy { get; }

    void Initialize(IntPtr windowHandle, int width, int height);

    void Resize(int width, int height);

    IFrameContext BeginFrame(Vector4 clearColor);
}
