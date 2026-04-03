namespace Videra.Platform.Windows;

internal sealed class D3D11BackendTestHooks
{
    public Func<int, int, int>? ResizeBuffersOverride { get; init; }
}
