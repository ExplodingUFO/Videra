# Contour Recipe

This recipe shows the bounded Videra-native contour path used by
`Videra.SurfaceCharts.Demo`. It uses `VideraChartView` with `Plot.Add.Contour`
and a `ContourChartData` value built from a `SurfaceScalarField`.

## Setup

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
```

The demo proof generates a small radial scalar field in row-major order. The
range is computed from the actual values and passed into `SurfaceScalarField`
so contour extraction has explicit scalar bounds.

```csharp
var chart = new VideraChartView();

const int size = 32;
var values = new float[size * size];

for (var y = 0; y < size; y++)
{
    for (var x = 0; x < size; x++)
    {
        var dx = (x - (size - 1) / 2.0) / ((size - 1) / 2.0);
        var dy = (y - (size - 1) / 2.0) / ((size - 1) / 2.0);
        values[y * size + x] = (float)Math.Sqrt(dx * dx + dy * dy);
    }
}

var range = new SurfaceValueRange(values.Min(), values.Max());
var field = new SurfaceScalarField(size, size, values, range);
var contour = new ContourChartData(field, explicitLevels: [0.25f, 0.5f, 0.75f]);

chart.Plot.Clear();
chart.Plot.Add.Contour(contour, "Radial contours");
chart.FitToData();
```

When a host already has the scalar field and only needs explicit iso-values, use
the direct overload:

```csharp
chart.Plot.Add.Contour(field, explicitLevels: [0.25f, 0.5f, 0.75f], name: "Radial contours");
```

## Demo Proof

In the sample app, select `Try next: Contour plot proof`. The visible proof is
authored through `VideraChartView.Plot.Add.Contour`; it does not swap to a
different chart control when the scalar field changes.

The support summary for this proof includes:

- `EvidenceKind: SurfaceChartsContourDatasetProof`
- `Plot path: Try next: Contour plot proof`
- `ContourRenderingStatus: HasSource`, `IsReady`, `Levels`, `Lines`, and
  `Segments`
- `ContourChartData.HasExplicitLevels` and the explicit `ContourChartData.ExplicitLevels`
  sequence when the host supplies fixed iso-values
- `DatasetEvidenceKind`, `DatasetSeriesCount`, `DatasetActiveSeriesIndex`, and
  `DatasetActiveSeriesMetadata`
- `OutputCapabilityDiagnostics` from the Plot output evidence path

These values are support evidence only. They should not be rewritten into fake
throughput, latency, or frame-rate claims.

## Boundaries

Use `Plot.Add.Contour` with `ContourChartData` for contour loading. Do not add
legacy chart controls, a direct public `Source` API, hidden scalar-field
fallback, backend expansion, OpenGL/WebGL promises, or a generic plotting engine
to make this recipe work.
