# Phase 332 Plan

## Tasks

1. Document `VideraInteractionEvidenceFormatter` as report-only viewer interaction evidence.
2. Document `SurfaceChartProbeEvidenceFormatter` as chart-local probe evidence.
3. Update alpha-feedback support routing for Workbench interaction evidence captures.
4. Add repository guardrail tests for those documentation boundaries.
5. Regenerate Beads export and public roadmap after closing the phase and epic.

## Verification

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter FullyQualifiedName~WorkbenchSampleConfigurationTests`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug --no-restore -m:1 --filter FullyQualifiedName~BeadsPublicRoadmapTests`
