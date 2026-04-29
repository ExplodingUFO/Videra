# Phase 377 Handoff: Raw Add Overloads and Typed Plottables

## Scope Completed

- Added `IPlottable3D` with mutable `Label` and `IsVisible`.
- Added typed Plot handles:
  - `SurfacePlot3DSeries`
  - `WaterfallPlot3DSeries`
  - `ScatterPlot3DSeries`
- Kept `Plot3DSeries` as the shared base so existing advanced overload callers can still treat returned handles as `Plot3DSeries`.
- Added concise raw numeric overloads:
  - `Plot.Add.Surface(double[,] values, string? name = null)`
  - `Plot.Add.Waterfall(double[,] values, string? name = null)`
  - `Plot.Add.Scatter(double[] x, double[] y, double[] z, string? name = null, uint color = 0xFF2F80EDu)`
- Visibility now affects active-series selection: the active series is the last visible series in draw order.

## Verification

- PASS: `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "VideraChartViewPlotApiTests|RegressionGuardrailTests"`
- PASS: `pwsh -File scripts/Test-SnapshotExportScope.ps1`
- FAIL, unrelated to Phase 377 write set: `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj`
  - Failing test: `Videra.SurfaceCharts.Core.Tests.SurfaceMetadataTests.AxisCtor_RejectsLogScaleUntilDisplaySpaceSupportLands`
  - Failure: expected `ArgumentException`, but no exception was thrown.
  - No files under `src/Videra.SurfaceCharts.Core` or `tests/Videra.SurfaceCharts.Core.Tests` were changed in this phase.

## Integration Notes

- Phase 380 same-type multi-series composition should preserve the Phase 377 rule that hidden series do not become active.
- Phase 378 and Phase 379 branches should be able to merge this as a Plot facade/API change; no VideraView, native backend, old chart control, public Source API, PDF/vector export, compatibility-wrapper, or generic plotting-engine files were touched.
- Bead `Videra-v255.2` remains open by request.
