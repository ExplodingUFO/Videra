# Phase 344 Summary: Plot Runtime Docs and Guardrails

## Bead

- `Videra-rwp`
- Status: closed

## Changes

- Updated root README, SurfaceCharts Avalonia README, demo README, and Chinese docs to describe `Plot.Add.*` as chart authoring and runtime data-loading entrypoints.
- Reworded scatter docs from "direct scatter" to `Plot.Add.Scatter`.
- Added repository guardrail coverage that rejects deleted public `VideraChartView.Source`, `SourceProperty`, `Source path/details`, and direct `.Source =` examples in active SurfaceCharts proof paths.
- Regenerated Beads export and public roadmap.

## Verification

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests"`
  - Passed: 31/31
- Source scan confirmed the guarded active docs/proof paths no longer contain the deleted direct Source API or support-summary wording.

## Handoff

v2.48 is complete. The next milestone can start from the Plot-owned chart runtime model without preserving any public direct `Source` compatibility path.
