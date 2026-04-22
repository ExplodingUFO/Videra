namespace Videra.Core.Graphics;

/// <summary>
/// Stable vertex-buffer binding slots shared by the engine and backend implementations.
/// </summary>
public static class RenderBindingSlots
{
    public const uint Vertex = 0;
    public const uint Camera = 1;
    public const uint World = 2;
    public const uint Style = 3;
    public const uint SurfaceColorMap = 4;
    public const uint SurfaceTileScalars = 5;
    public const uint AlphaMask = 6;
}

