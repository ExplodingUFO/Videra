namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Defines how surface data should be presented in display space before rendering.
/// </summary>
public enum SurfaceDisplaySpace
{
    /// <summary>
    /// Raw mode preserves the original dataset proportions.
    /// </summary>
    Raw = 0,

    /// <summary>
    /// Presentation mode opts into a presentation-oriented display contract.
    /// </summary>
    Presentation = 1,
}
