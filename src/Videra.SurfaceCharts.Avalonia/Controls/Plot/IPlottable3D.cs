namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Minimal host-facing contract shared by Plot-authored 3D plottables.
/// </summary>
public interface IPlottable3D
{
    /// <summary>
    /// Gets or sets the host-facing plottable label.
    /// </summary>
    string? Label { get; set; }

    /// <summary>
    /// Gets or sets whether the plottable participates in rendering.
    /// </summary>
    bool IsVisible { get; set; }
}
