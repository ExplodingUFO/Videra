# Phase 380 Handoff

Bead: Videra-v255.5 remains open and claimed.

Implemented runtime/evidence support for deterministic visible same-type composition without adding chart families, public Source APIs, renderer backends, or export formats.

Changed behavior:
- Surface and waterfall active outputs now use a Plot-owned composed tile source when multiple compatible visible same-kind series are active. Tile values are averaged deterministically in draw order over compatible sources; incompatible surface metadata falls back to the compatible subset selected from the first visible source.
- Scatter active output aggregates visible same-kind ScatterChartData into one existing ScatterChartData contract with unioned metadata ranges.
- Bar active output aggregates compatible visible BarChartData series with the existing BarChartData contract; incompatible layout/category data falls back deterministically to the top visible dataset.
- Contour status/rendering iterates visible same-kind contour datasets and reports aggregate line/segment counts.
- Dataset/output/snapshot evidence now identify composed active series through deterministic PlotSeries identities.
- Legend entries now honor Plot3DSeries.IsVisible.

Verification run:
- dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "VideraChartViewPlotApiTests|SurfaceLegendOverlayTests|SurfaceChartProbeEvidenceTests|PlotSnapshot" -> passed, 41 tests.
- dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --no-restore -> passed, 308 tests. First no-restore attempt failed before restore because project.assets.json was absent in the fresh worktree; reran with restore, then no-restore passed.
- pwsh -File scripts/Test-SnapshotExportScope.ps1 -> passed.

Residual risk:
- Surface/waterfall composition is intentionally bounded to compatible metadata and uses averaged tile values because the existing renderer still consumes one heightfield source. This avoids backend expansion but is not a generalized layered surface renderer.