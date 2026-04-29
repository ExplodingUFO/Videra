# Phase 391: Public API and Package Metadata Review - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 391 makes the existing SurfaceCharts public API and package metadata reviewable. It may update release-readiness metadata and stale guardrail expectations, but it must not add new public APIs, restore removed chart controls, introduce compatibility wrappers, publish packages, or create release tags.

</domain>

<decisions>
## Implementation Decisions

### Public API Contract
- Refresh `eng/public-api-contract.json` against current top-level public types using the repository test's rules.
- Treat public API drift as explicit release-readiness evidence, not as a new API design phase.
- Do not hide public API growth behind compatibility layers or ignored test gaps.

### Output Capability Guardrail
- Keep PNG/image export support explicit because `Plot.CaptureSnapshotAsync` and `Plot.SavePngAsync` are shipped chart-local bitmap features.
- Keep PDF and vector export unsupported.
- Update stale tests to assert `plot-output.export.image.supported` rather than the pre-snapshot unsupported code.

### Package Metadata
- Confirm SurfaceCharts packages can be packed locally without public publish credentials.
- Use phase-local artifact output only; do not publish or tag.

### the agent's Discretion
- Use focused repository tests and snapshot scope guardrail rather than broad solution tests for this phase.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tests/Videra.Core.Tests/Repository/PublicApiContractRepositoryTests.cs` already enforces package IDs, source roots, uniqueness, ordinal order, and actual top-level public type matching.
- `scripts/Test-SnapshotExportScope.ps1` covers old controls, public `Source`, PDF/vector export implementation patterns, viewer export-service coupling, and hidden snapshot fallback patterns.
- `dotnet pack` can inspect package metadata locally without publish credentials.

### Established Patterns
- Release readiness evidence is captured through repository contracts and scripts.
- Package metadata is shared through `Directory.Build.props` plus project-level package IDs/descriptions/tags/README/icon entries.

### Integration Points
- `eng/public-api-contract.json`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/VideraDoctorRepositoryTests.cs`

</code_context>

<specifics>
## Specific Ideas

No new API design. Align the existing release-readiness contract to the current shipped SurfaceCharts public surface and PNG snapshot capability.

</specifics>

<deferred>
## Deferred Ideas

- Full API diff tooling against a previous public release.
- Any public package publish/tag decision.

</deferred>
