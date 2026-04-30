---
phase: 408
title: "v2.60 Final Verification Plan"
bead: Videra-448
status: complete
created_at: 2026-04-30
---

# Phase 408 Plan

## Goal

Close v2.60 with synchronized verification, Beads export, generated roadmap,
phase archive, branch/worktree cleanup, Git push, and Dolt Beads push.

## Scope

- Compose the Phase 406 cookbook QA gate and Phase 407 interaction handoff gate.
- Re-run snapshot scope guardrails.
- Synchronize Beads state and `docs/ROADMAP.generated.md`.
- Archive v2.60 phase artifacts.
- Clean only worktrees/branches created for this v2.60 closeout.

## Non-Goals

- No product/runtime changes.
- No additional cookbook or interaction feature work.
- No compatibility layer, hidden fallback/downshift, backend expansion,
  PDF/vector export, generic plotting engine, or generic workbench scope.

## Verification Commands

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsCookbook" --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests" --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
bd export -o .beads\issues.jsonl
pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore
git diff --check
git status --short --branch
```
