namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

/// <summary>
/// Chart-local switches for the built-in interaction and command surface.
/// </summary>
public sealed class SurfaceChartInteractionProfile
{
    private static readonly SurfaceChartCommand[] EmptyCommands = [];

    /// <summary>
    /// Gets the default profile that preserves all built-in interactions.
    /// </summary>
    public static SurfaceChartInteractionProfile Default { get; } = new();

    /// <summary>
    /// Gets a profile with all built-in interactions disabled.
    /// </summary>
    public static SurfaceChartInteractionProfile Disabled { get; } = new()
    {
        IsOrbitEnabled = false,
        IsPanEnabled = false,
        IsDollyEnabled = false,
        IsResetCameraEnabled = false,
        IsFitToDataEnabled = false,
        IsKeyboardShortcutsEnabled = false,
        IsFocusSelectionEnabled = false,
        IsProbePinningEnabled = false,
        IsToolbarEnabled = false,
        FocusOnPointerPressed = false,
    };

    /// <summary>
    /// Gets a value indicating whether left-button orbit gestures are enabled.
    /// </summary>
    public bool IsOrbitEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether right-button and command pan actions are enabled.
    /// </summary>
    public bool IsPanEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether mouse-wheel and command zoom/dolly actions are enabled.
    /// </summary>
    public bool IsDollyEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether reset-camera commands are enabled.
    /// </summary>
    public bool IsResetCameraEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether fit-to-data commands are enabled.
    /// </summary>
    public bool IsFitToDataEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether built-in keyboard shortcuts are enabled.
    /// </summary>
    public bool IsKeyboardShortcutsEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether Ctrl+left focus selection is enabled.
    /// </summary>
    public bool IsFocusSelectionEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether Shift+left probe pinning is enabled.
    /// </summary>
    public bool IsProbePinningEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether built-in toolbar commands are enabled.
    /// </summary>
    public bool IsToolbarEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether pointer presses request keyboard focus for the chart.
    /// </summary>
    public bool FocusOnPointerPressed { get; init; } = true;

    /// <summary>
    /// Gets the built-in chart commands enabled by this profile.
    /// </summary>
    public IReadOnlyList<SurfaceChartCommand> EnabledCommands
    {
        get
        {
            if (!IsDollyEnabled && !IsPanEnabled && !IsResetCameraEnabled && !IsFitToDataEnabled)
            {
                return EmptyCommands;
            }

            var commands = new List<SurfaceChartCommand>(8);

            if (IsDollyEnabled)
            {
                commands.Add(SurfaceChartCommand.ZoomIn);
                commands.Add(SurfaceChartCommand.ZoomOut);
            }

            if (IsPanEnabled)
            {
                commands.Add(SurfaceChartCommand.PanLeft);
                commands.Add(SurfaceChartCommand.PanRight);
                commands.Add(SurfaceChartCommand.PanUp);
                commands.Add(SurfaceChartCommand.PanDown);
            }

            if (IsResetCameraEnabled)
            {
                commands.Add(SurfaceChartCommand.ResetCamera);
            }

            if (IsFitToDataEnabled)
            {
                commands.Add(SurfaceChartCommand.FitToData);
            }

            return commands;
        }
    }
}
