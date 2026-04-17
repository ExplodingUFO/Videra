namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Represents one begun frame on a render surface.
/// Disposing the context ends and presents the frame.
/// </summary>
internal interface IFrameContext : IDisposable
{
}
