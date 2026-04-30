# Phase 414 Plan: Native Cleanup and Old-Code Inventory

## Tasks

1. Claim `Videra-2wb`.
2. Run parallel read-only scans:
   - Code/API SurfaceCharts scan.
   - Docs/demo/support scan.
   - Tests/scripts/CI guardrail scan.
3. Classify findings into:
   - true cleanup candidates
   - intentional negative guardrails
   - product truth outside this cleanup scope
   - historical plan/docs only
   - test fixtures
   - CI/guardrail gaps
4. Create downstream child beads with explicit dependencies and ownership.
5. Update planning artifacts, Beads export, generated roadmap, and phase status.
6. Verify Beads dependency shape, generated roadmap, scope guardrails, and
   formatting.

## Downstream Bead Split

### Phase 415: Code/API cleanup

- `Videra-sva`: remove chart-local render fallback/downshift.
- `Videra-1wq`: remove compatibility camera-frame backfill and fallback naming.
- `Videra-avu`: clean stale compatibility vocabulary in tests.

### Phase 416: Demo/cookbook simplification

- `Videra-dtc`: extract cookbook recipe catalog.
- `Videra-b6e`: extract support summary/snapshot helpers.

### Phase 417: Guardrails/CI truth

- `Videra-5j5`: harden no-compat scope script.
- `Videra-raj`: pin CI runtime/smoke/release truth.

## Dependency Shape

- 415/416 child beads depend on `Videra-2wb`.
- 417 child beads depend on Phase 415 and Phase 416 parent beads.
- Phase 418 depends on Phase 417.

## Validation

- `bd ready --json`
- `bd dep cycles --json`
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore`
- `git diff --check`
