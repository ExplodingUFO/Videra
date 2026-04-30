# Phase 410C Summary: Scatter and Live Data Cookbook Hardening

## Scope

Bead: `Videra-4qk`

Phase 410C hardened the scatter/live-data cookbook recipe around the shipped
Videra SurfaceCharts path:

- `VideraChartView.Plot.Add.Scatter(...)`
- `DataLogger3D`
- `ScatterColumnarData`
- `ScatterColumnarSeries`
- `ScatterChartData`
- FIFO retention counters and latest-window live evidence
- high-volume `Pickable=false` guidance
- evidence-only support-summary truth

## Delivered

- Added `samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md`
  as a Videra-native recipe for static scatter and live columnar scatter.
- Documented FIFO retention versus `UseLatestWindow(...)` visibility evidence
  without implying another data copy or renderer fallback.
- Documented `CreateLiveViewEvidence()` fields and the dataset counters that
  support summary output should report from observed runtime state.
- Kept the recipe inside the existing chart family boundary: no wrapper chart
  type, no public `Source` assignment, no fallback/downshift behavior, no
  benchmark claims, and no broad demo workbench behavior.
- Added `SurfaceChartsCookbookScatterLiveRecipeTests` to pin required API and
  evidence tokens while blocking performance-promise and scope-expansion
  language.

## Verification

Planned verification for closeout:

- `git diff --check -- samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookScatterLiveRecipeTests.cs .planning/phases/410-detailed-3d-cookbook-demo-recipes/410C-SUMMARY.md`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter FullyQualifiedName~SurfaceChartsCookbookScatterLiveRecipeTests --no-restore`
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter FullyQualifiedName~ScatterDataLogger3DTests --no-restore`
