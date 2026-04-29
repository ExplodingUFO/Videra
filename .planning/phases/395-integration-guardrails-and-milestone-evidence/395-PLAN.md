# Phase 395 Plan: Integration Guardrails and Milestone Evidence

## Goal

Close v2.57 with repeatable evidence and synchronized project state.

## Tasks

1. Run final `scripts/Invoke-ReleaseReadinessValidation.ps1` without `-ConsumerSmokeBuildOnly` to prove package build, full SurfaceCharts packaged consumer smoke, focused tests, and snapshot guardrails.
2. Confirm docs/repository release-candidate tests remain green after Phase 393 and Phase 394 integration.
3. Mark VER requirements and Phase 395 complete.
4. Close Phase 395 and v2.57 Beads.
5. Export `.beads/issues.jsonl` and regenerate `docs/ROADMAP.generated.md`.
6. Archive v2.57 roadmap, requirements, and phase evidence under `.planning/milestones/`.
7. Push Git and Dolt refs, then verify clean worktree/branch state.

## Validation

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseReadinessValidation.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts\phase395-release-readiness-final`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests" --no-restore`
- `rg` checks for release-candidate support artifacts and forbidden ScottPlot compatibility claims

