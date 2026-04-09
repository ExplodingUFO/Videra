using Avalonia;
using Avalonia.Input;

namespace Videra.Avalonia.Controls.Interaction;

internal readonly record struct VideraPointerGestureSnapshot(
    Point Position,
    RawInputModifiers Modifiers,
    bool IsLeftButtonPressed,
    bool IsRightButtonPressed,
    MouseButton InitialPressMouseButton,
    float WheelDeltaY);
