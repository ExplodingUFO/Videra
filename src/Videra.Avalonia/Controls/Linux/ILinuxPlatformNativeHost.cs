using Avalonia;
using Avalonia.Platform;

namespace Videra.Avalonia.Controls.Linux;

internal interface ILinuxPlatformNativeHost
{
    IntPtr Handle { get; }

    IPlatformHandle Create(IPlatformHandle parent, Size bounds, double renderScaling);

    void Resize(Size bounds, double renderScaling);

    void Destroy();
}
