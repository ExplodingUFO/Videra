namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Reusable color-map palettes for common SurfaceChart presentations.
/// </summary>
public static class SurfaceColorMapPresets
{
    /// <summary>
    /// Creates the current chart default two-stop blue-gray palette.
    /// </summary>
    public static SurfaceColorMapPalette CreateDefault() => new(0xFF102030u, 0xFFE6EEF5u);

    /// <summary>
    /// Creates a compact engineering-style diverging palette.
    /// </summary>
    public static SurfaceColorMapPalette CreateCoolWarm() => new(0xFF102030u, 0xFF80C0FFu, 0xFFFFE080u);

    /// <summary>
    /// Creates a neutral grayscale palette.
    /// </summary>
    public static SurfaceColorMapPalette CreateGrayscale() => new(0xFF000000u, 0xFFFFFFFFu);
}
