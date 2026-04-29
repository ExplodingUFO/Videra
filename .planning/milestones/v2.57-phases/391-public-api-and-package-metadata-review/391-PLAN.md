# Phase 391: Public API and Package Metadata Review - Plan

bead: Videra-v257.2

## Goal

Make SurfaceCharts public API and package metadata reviewable for release readiness without changing runtime behavior or publishing packages.

## Tasks

1. Refresh `eng/public-api-contract.json` using the same top-level public type extraction semantics as `PublicApiContractRepositoryTests`.
2. Update stale guardrail expectations for PNG image export support while preserving PDF/vector unsupported assertions.
3. Pack the four SurfaceCharts package projects into a local phase artifact directory to prove package metadata can be inspected without publish credentials.
4. Run focused repository tests and snapshot scope guardrail.
5. Record verification and handoff notes.

## Ownership Boundaries

- Owns only public API contract metadata, stale guardrail test expectations, and phase planning artifacts.
- Does not change `src/` runtime behavior.
- Does not change package IDs, versions, dependencies, or publish workflows.
- Does not publish packages or create tags.

## Validation

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~PublicApiContractRepositoryTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~VideraDoctorRepositoryTests"`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1`
- `dotnet pack` for the four SurfaceCharts package projects into `artifacts/phase391-surfacecharts-packages`

## Handoff

Phase 392 can build on the refreshed API/package metadata and should prove clean package consumption through public package references only.
