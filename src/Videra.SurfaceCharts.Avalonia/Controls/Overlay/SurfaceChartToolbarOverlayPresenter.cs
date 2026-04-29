using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Presents the zoom/pan toolbar overlay with clickable action buttons.
/// </summary>
internal static class SurfaceChartToolbarOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush ButtonForeground = new SolidColorBrush(Color.FromArgb(230, 240, 240, 240));
    private static readonly IBrush ButtonBackground = new SolidColorBrush(Color.FromArgb(160, 30, 30, 30));
    private static readonly IBrush ButtonHoverBackground = new SolidColorBrush(Color.FromArgb(200, 60, 60, 60));
    private static readonly IBrush ButtonBorderBrush = new SolidColorBrush(Color.FromArgb(120, 120, 120, 120));
    private static readonly IBrush TooltipBackground = new SolidColorBrush(Color.FromArgb(220, 20, 20, 20));
    private static readonly IBrush TooltipForeground = Brushes.White;

    private const double ButtonSize = 28d;
    private const double ButtonSpacing = 4d;
    private const double ButtonCornerRadius = 4d;
    private const double Padding = 10d;
    private const double TooltipOffset = 8d;

    /// <summary>
    /// Creates the toolbar overlay state.
    /// </summary>
    public static SurfaceChartToolbarOverlayState CreateState(
        Size viewSize,
        Point? pointerScreenPosition,
        bool canInteract)
    {
        if (!canInteract || viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return SurfaceChartToolbarOverlayState.Empty;
        }

        var buttons = CreateButtons(viewSize, pointerScreenPosition);

        return new SurfaceChartToolbarOverlayState(
            isVisible: true,
            buttons: buttons);
    }

    /// <summary>
    /// Renders the toolbar overlay.
    /// </summary>
    public static void Render(
        DrawingContext context,
        SurfaceChartToolbarOverlayState state,
        Point? pointerScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(state);

        if (!state.IsVisible || state.Buttons.Count == 0)
        {
            return;
        }

        foreach (var button in state.Buttons)
        {
            var isHovered = pointerScreenPosition is Point pointer &&
                            button.ScreenBounds.Contains(pointer);
            var background = isHovered ? ButtonHoverBackground : ButtonBackground;
            var borderPen = new Pen(ButtonBorderBrush, 1d);

            // Draw button background
            context.DrawRectangle(
                background,
                borderPen,
                button.ScreenBounds,
                ButtonCornerRadius,
                ButtonCornerRadius);

            // Draw button icon centered
            var iconText = CreateText(button.Icon, ButtonForeground);
            var iconX = button.ScreenBounds.X + (button.ScreenBounds.Width - iconText.Width) / 2;
            var iconY = button.ScreenBounds.Y + (button.ScreenBounds.Height - iconText.Height) / 2;
            context.DrawText(iconText, new Point(iconX, iconY));

            // Draw tooltip on hover
            if (isHovered)
            {
                DrawTooltip(context, button);
            }
        }
    }

    /// <summary>
    /// Determines which button (if any) was clicked at the given position.
    /// </summary>
    public static SurfaceChartToolbarAction? HitTest(
        SurfaceChartToolbarOverlayState state,
        Point position)
    {
        if (!state.IsVisible)
        {
            return null;
        }

        foreach (var button in state.Buttons)
        {
            if (button.ScreenBounds.Contains(position))
            {
                return button.Action;
            }
        }

        return null;
    }

    private static List<SurfaceChartToolbarButton> CreateButtons(Size viewSize, Point? pointerScreenPosition)
    {
        var buttons = new List<SurfaceChartToolbarButton>(4);

        // Position toolbar in bottom-right corner, vertically stacked
        var toolbarX = viewSize.Width - ButtonSize - Padding;
        var startY = viewSize.Height - Padding - ((ButtonSize + ButtonSpacing) * 4) + ButtonSpacing;

        var actions = new[]
        {
            ("+", "Zoom In", SurfaceChartToolbarAction.ZoomIn),
            ("\u2212", "Zoom Out", SurfaceChartToolbarAction.ZoomOut),
            ("\u2302", "Reset Camera", SurfaceChartToolbarAction.ResetCamera),
            ("\u229E", "Fit to Data", SurfaceChartToolbarAction.FitToData),
        };

        for (var i = 0; i < actions.Length; i++)
        {
            var (icon, tooltip, action) = actions[i];
            var buttonY = startY + (i * (ButtonSize + ButtonSpacing));
            var bounds = new Rect(toolbarX, buttonY, ButtonSize, ButtonSize);

            buttons.Add(new SurfaceChartToolbarButton(
                icon: icon,
                tooltip: tooltip,
                action: action,
                screenBounds: bounds));
        }

        return buttons;
    }

    private static void DrawTooltip(DrawingContext context, SurfaceChartToolbarButton button)
    {
        var tooltipText = CreateText(button.Tooltip, TooltipForeground);
        var tooltipX = button.ScreenBounds.X - tooltipText.Width - TooltipOffset;
        var tooltipY = button.ScreenBounds.Y + (button.ScreenBounds.Height - tooltipText.Height) / 2;

        // Ensure tooltip stays within view
        if (tooltipX < Padding)
        {
            tooltipX = button.ScreenBounds.Right + TooltipOffset;
        }

        var tooltipRect = new Rect(
            tooltipX - 4d,
            tooltipY - 2d,
            tooltipText.Width + 8d,
            tooltipText.Height + 4d);

        context.DrawRectangle(TooltipBackground, null, tooltipRect, 3d, 3d);
        context.DrawText(tooltipText, new Point(tooltipX, tooltipY));
    }

    private static FormattedText CreateText(string text, IBrush foreground)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            14d,
            foreground);
    }
}
