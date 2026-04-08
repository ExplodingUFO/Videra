using Videra.Avalonia.Controls;
using Videra.Core.Graphics;

namespace Videra.Avalonia.Rendering;

internal sealed record RenderSessionInputs
{
    public GraphicsBackendPreference RequestedBackend { get; init; } = GraphicsBackendPreference.Auto;

    public VideraBackendOptions? BackendOptions { get; init; }

    public IntPtr Handle { get; init; }

    public bool HandleBound { get; init; }

    public uint Width { get; init; }

    public uint Height { get; init; }

    public float RenderScale { get; init; } = 1f;
}
