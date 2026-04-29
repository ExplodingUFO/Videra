---
phase: 388
title: "Cookbook Demo Gallery and Docs"
status: passed
bead: Videra-v256.6
---

# Phase 388 Verification

status: passed

## Commands

- `dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj`
  - Passed. Required because this new worktree did not have
    `project.assets.json` yet.
- `dotnet restore samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj`
  - Passed. Required before no-restore demo build in this worktree.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter SurfaceChartsDemoConfigurationTests --no-restore`
  - Passed: 1, Failed: 0, Skipped: 0.
- `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore`
  - Passed with existing analyzer warnings.
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
  - Passed.
- `git diff --check`
  - Passed.

## Notes

An initial parallel run of the focused test and demo build failed because both
processes attempted to write the same demo PDB. The demo build was rerun
successfully, and the focused test was rerun by itself successfully.
