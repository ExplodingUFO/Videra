---
phase: 386
title: "Selection, Probe, and Draggable Overlay Recipes Verification"
status: passed
bead: Videra-v256.4
verified_at: "2026-04-30T02:01:52+08:00"
---

# Phase 386 Verification

## Commands

```powershell
dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "Probe|PinnedProbe|Interaction" --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
```

## Results

- Restore: passed. Required because this worktree did not yet have
  `tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\obj\project.assets.json`.
- Focused tests: passed, 67 passed / 0 failed / 0 skipped.
- Snapshot scope guard: passed.
- Diff whitespace check: passed.

## Notes

The test command emitted existing analyzer warnings in unrelated files, including
`BarRenderer.cs`, `Plot3DAddApi.cs`, and overlay presenter files. No Phase 386
warnings blocked the focused test result.
