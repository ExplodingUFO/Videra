# Phase 404: Interaction Cookbook Final Verification - Plan

bead: Videra-v259.4

## Goal

Close v2.59 by verifying the integrated Phase 402/403 result, synchronizing
Beads and generated roadmap state, cleaning this session's worktrees/branches,
and recording a handoff.

## Tasks

1. Claim `Videra-v259.4`.
2. Run focused integration checks for Plot/interaction API and cookbook/demo
   docs.
3. Run snapshot scope guardrail and generated public roadmap verification.
4. Close Phase 404 and the v2.59 epic in Beads.
5. Export `.beads/issues.jsonl` and regenerate `docs/ROADMAP.generated.md`.
6. Update `.planning/ROADMAP.md` and `.planning/STATE.md`.
7. Archive v2.59 phase artifacts under `.planning/milestones/v2.59-phases`.
8. Remove this session's temporary v2.59 worktrees and branches.
9. Push Dolt Beads state and Git commits.

## Validation Commands

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests"
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests|InteractionSampleConfigurationTests|DemoInteractionContractTests" --no-restore
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
bd export -o .beads\issues.jsonl
pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore
git diff --check
git status --short --branch
```
