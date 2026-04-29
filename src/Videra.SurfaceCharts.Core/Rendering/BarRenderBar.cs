using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one render-ready bar with 3D position, dimensions, and color.
/// </summary>
/// <param name="Position">The chart-space center position of the bar base.</param>
/// <param name="Size">The full width, height, and depth of the bar.</param>
/// <param name="Color">The ARGB bar color.</param>
public readonly record struct BarRenderBar(Vector3 Position, Vector3 Size, uint Color);
