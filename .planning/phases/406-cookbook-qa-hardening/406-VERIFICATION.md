---
phase: 406
title: "Cookbook QA Hardening Verification"
bead: Videra-1h1
status: verified
updated_at: 2026-04-30
---

# Phase 406 Verification

Validation commands for this phase:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsCookbook" --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short --branch
```

## Results

| Command | Result |
| --- | --- |
| `dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj` | Passed; required because the fresh worktree was missing `project.assets.json`. |
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests\|SurfaceChartsCookbook" --no-restore` | Passed: 3 tests, 0 failed. Existing analyzer warnings and Avalonia Accelerate telemetry notices were emitted from referenced projects. |
| `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1` | Passed: all scope checks passed. |
| `git diff --check` | Passed; Git reported LF-to-CRLF working-copy warnings for existing markdown files. |
| `git status --short --branch` | Clean except for the Phase 406 changes before commit. |
| Mainline merge rerun: `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests\|SurfaceChartsCookbook" --no-restore` | Passed after merge: 3 tests, 0 failed. |
| Mainline merge rerun: `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1` | Passed after merge: all scope checks passed. |

## Blockers

None.
