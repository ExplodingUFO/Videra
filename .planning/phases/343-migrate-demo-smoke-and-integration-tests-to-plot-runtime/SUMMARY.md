# Phase 343 Summary: Migrate Demo Smoke and Integration Tests to Plot Runtime

## Bead

- `Videra-i5z`
- Status: closed

## Changes

- Renamed SurfaceCharts demo and consumer-smoke support-summary evidence fields from `Source path/details` to `Plot path/details`.
- Renamed first-chart UI/test proof wording to `First-chart Plot path`.
- Kept active chart loading through `VideraChartView.Plot.Add.*` only.
- Updated demo tests to assert Plot-owned support vocabulary.

## Verification

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests"`
  - Passed: 14/14
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug -m:1 --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~VideraChartViewLifecycleTests|FullyQualifiedName~SurfaceChartTileSchedulingTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceAxisOverlayTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~VideraChartViewWaterfallIntegrationTests"`
  - Passed: 84/84
- Source scan confirmed no active SurfaceCharts demo/smoke/test support evidence still uses `Source path`, `Source details`, direct `VideraChartView.Source`, `SourceProperty`, or direct `.Source =` chart loading.

## Handoff

Phase 344 should add repository guardrails and final docs alignment so the deleted public Source path cannot return.
