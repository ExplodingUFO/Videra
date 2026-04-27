using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one render-ready scatter point with chart-space position, color, and marker size.
/// </summary>
/// <param name="Position">The chart-space point position.</param>
/// <param name="Color">The resolved ARGB point color.</param>
/// <param name="Size">The resolved positive marker size.</param>
public readonly record struct ScatterRenderPoint(Vector3 Position, uint Color, float Size = 1f);
