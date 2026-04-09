using Avalonia.Input;

namespace Videra.Avalonia.Controls;

internal enum NativePointerKind
{
    Move,
    LeftDown,
    LeftUp,
    RightDown,
    RightUp,
    Wheel
}

internal readonly struct NativePointerEvent
{
    public NativePointerEvent(NativePointerKind kind, int x, int y, int wheelDelta, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        Kind = kind;
        X = x;
        Y = y;
        WheelDelta = wheelDelta;
        Modifiers = modifiers;
    }

    public NativePointerKind Kind { get; }
    public int X { get; }
    public int Y { get; }
    public int WheelDelta { get; }
    public RawInputModifiers Modifiers { get; }
}
