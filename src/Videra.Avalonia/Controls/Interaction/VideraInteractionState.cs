using Avalonia;
using Avalonia.Input;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed class VideraInteractionState
{
    public bool IsLeftButtonDown { get; set; }

    public bool IsRightButtonDown { get; set; }

    public bool HasExceededClickThreshold { get; set; }

    public bool IsSelectionBoxActive { get; set; }

    public Point PointerDownPosition { get; set; }

    public Point LastPosition { get; set; }

    public RawInputModifiers PointerDownModifiers { get; set; }

    public bool HasActiveGesture => IsLeftButtonDown || IsRightButtonDown;

    public void Reset()
    {
        IsLeftButtonDown = false;
        IsRightButtonDown = false;
        HasExceededClickThreshold = false;
        IsSelectionBoxActive = false;
        PointerDownPosition = default;
        LastPosition = default;
        PointerDownModifiers = RawInputModifiers.None;
    }
}
