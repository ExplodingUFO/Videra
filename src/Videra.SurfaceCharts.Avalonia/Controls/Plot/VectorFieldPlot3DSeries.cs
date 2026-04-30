using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a vector field plottable.
/// </summary>
public sealed class VectorFieldPlot3DSeries : Plot3DSeries
{
    internal VectorFieldPlot3DSeries(string? name, VectorFieldChartData data)
        : base(Plot3DSeriesKind.VectorField, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null,
            vectorFieldData: data, heatmapSliceData: null, boxPlotData: null)
    {
    }

    /// <summary>
    /// Updates the arrow scale factor for the vector field.
    /// </summary>
    /// <param name="scale">The replacement scale factor.</param>
    public void SetScale(float scale)
    {
        var data = VectorFieldData ?? throw new InvalidOperationException("Vector field series requires vector field data.");
        var rescaled = data.Points
            .Select(p => new VectorFieldPoint(p.Position, p.Direction * scale, p.Magnitude))
            .ToArray();
        ReplaceVectorFieldData(new VectorFieldChartData(rescaled, data.HorizontalAxis, data.DepthAxis, data.MagnitudeRange));
    }
}
