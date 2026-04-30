using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one render-ready histogram bin.
/// </summary>
public readonly record struct HistogramRenderBin(Vector3 Position, Vector3 Size, uint Color);
