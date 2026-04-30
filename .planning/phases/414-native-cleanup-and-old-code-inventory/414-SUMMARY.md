# Phase 414 Summary: Native Cleanup and Old-Code Inventory

## Outcome

Phase 414 completed the read-only inventory and produced a downstream execution
split. The scan found no active restoration of old public chart controls or
direct public `VideraChartView.Source`, but it did find real cleanup candidates
in chart-local fallback/downshift behavior, compatibility camera-frame backfill,
misleading fallback naming, stale compatibility test vocabulary, demo
code-behind size, and guardrail/CI coverage.

## True Cleanup Candidates

- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs`
  - `allowSoftwareFallback = true` and exception-driven software rendering create
    chart-local downshift behavior.
  - silent GPU backend creation failure can fall into software rendering without
    a clear failure boundary.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderInputs.cs`
  - compatibility camera-frame backfill preserves stale behavior and naming.
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs`
  and `VideraChartView.Overlay.cs`
  - `CreateFallbackColorMap` is really default color-map behavior and should not
    use fallback vocabulary.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeEvidenceCompatibilityTests.cs`
  - stale compatibility vocabulary in a test name/comment.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
  - 1600+ line demo code-behind owns recipe catalog, chart switching, cache
    loading, snapshot capture, support summary, diagnostics, and evidence
    formatting.

## Intentional Guardrails / No Action

- Negative mentions of old controls, direct `Source`, compatibility wrappers,
  PDF/vector export, and hidden fallback in release/support docs are guardrails.
- Internal `Source` terminology in chart runtime and tile source types is not the
  removed public direct `VideraChartView.Source` API.
- Explicit viewer/runtime and Linux XWayland fallback/compatibility truth is
  outside the SurfaceCharts old-code cleanup scope.
- Consumer-smoke failure fallback artifacts are failure evidence and still throw;
  they are not fake green status.

## Guardrail / CI Gaps

- `scripts/Test-SnapshotExportScope.ps1` under-matches some public class and
  multi-line direct `Source` shapes.
- `SurfaceChartsCiTruthTests` does not yet pin all runtime evidence and packaged
  SurfaceCharts smoke gates as non-optional.
- Release-readiness validation filters are stale relative to current cookbook
  and CI-truth checks.
- Publish release truth tests under-pin the actual SurfaceCharts-relevant needs
  block.

## Handoff

Phase 415 and Phase 416 can run in parallel after this phase closes. Phase 415
owns code/API cleanup. Phase 416 owns demo/cookbook simplification. Phase 417
then hardens scripts/CI/tests after both implementation slices land.
