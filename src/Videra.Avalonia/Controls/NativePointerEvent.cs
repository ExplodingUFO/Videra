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
    public NativePointerEvent(NativePointerKind kind, int x, int y, int wheelDelta)
    {
        Kind = kind;
        X = x;
        Y = y;
        WheelDelta = wheelDelta;
    }

    public NativePointerKind Kind { get; }
    public int X { get; }
    public int Y { get; }
    public int WheelDelta { get; }
}
