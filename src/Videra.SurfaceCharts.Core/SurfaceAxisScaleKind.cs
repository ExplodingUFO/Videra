namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes how one surface axis maps sample-space coordinates into axis-space coordinates.
/// </summary>
public enum SurfaceAxisScaleKind
{
    /// <summary>
    /// Sample-space coordinates are linearly mapped across the axis minimum/maximum range.
    /// </summary>
    Linear = 0,

    /// <summary>
    /// Sample-space coordinates are logarithmically mapped across the axis minimum/maximum range.
    /// </summary>
    Log = 1,

    /// <summary>
    /// Sample-space coordinates are linearly mapped across a time-based numeric axis range.
    /// </summary>
    DateTime = 2,

    /// <summary>
    /// Sample-space coordinates are resolved from explicit per-sample coordinate arrays.
    /// </summary>
    ExplicitCoordinates = 3
}
