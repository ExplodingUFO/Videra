---
phase: 387
title: "Axis Rules, Linked Views, and Live View Management"
status: passed
bead: Videra-v256.5
verified_at: "2026-04-30"
---

# Phase 387 Verification

status: passed

## Commands

Initial required `--no-restore` test commands failed because this fresh worktree had no `project.assets.json`. I restored the two focused test projects once, then reran the required commands unchanged.

- `dotnet restore tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj` — passed
- `dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj` — passed
- `dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter DataLogger3D --no-restore` — passed, 5/5
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "Axis|PlotApi" --no-restore` — passed, 47/47
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1` — passed
- `git diff --check` — passed

## Notes

The test output includes pre-existing analyzer warnings in unrelated files. No Phase 387 verification command failed after restore.
