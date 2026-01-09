using System;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Optional interface for software backends that render into a CPU framebuffer.
/// </summary>
public interface ISoftwareBackend
{
    int Width { get; }
    int Height { get; }

    /// <summary>
    /// Copies the current frame into the destination buffer with the given stride.
    /// </summary>
    void CopyFrameTo(IntPtr destination, int destinationStride);
}
