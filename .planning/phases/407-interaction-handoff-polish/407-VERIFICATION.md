---
phase: 407
bead: Videra-xq1
title: "Phase 407 Interaction Handoff Polish"
status: validated
---

# Phase 407 Verification

## Required Commands

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests" --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short --branch
```

## Results

| Command | Result |
| --- | --- |
| `dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj` | Passed; required because the fresh worktree was missing `project.assets.json`. |
| `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests" --no-restore` | Passed: 25 passed, 0 failed, 0 skipped. |
| `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1` | Passed: all scope checks passed. |
| `git diff --check` | Passed; Git reported LF-to-CRLF normalization warnings for touched tracked files only. |
| `git status --short --branch` | Checked before commit. |
| Mainline merge rerun: `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartInteractionRecipeTests\|FullyQualifiedName~VideraChartViewKeyboardToolbarTests" --no-restore` | Passed after merge: 25 tests, 0 failed. |
| Mainline merge rerun: `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1` | Passed after merge: all scope checks passed. |

## Blockers

None.
