using Avalonia.Controls;

namespace Videra.Avalonia.Controls;

internal interface IVideraNativeHost
{
    event Action<IntPtr>? HandleCreated;
    event Action? HandleDestroyed;
    event Action<NativePointerEvent>? NativePointer;
    bool IsHitTestVisible { get; set; }
}
