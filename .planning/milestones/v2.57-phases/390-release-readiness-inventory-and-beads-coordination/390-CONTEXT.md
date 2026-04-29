# Phase 390: Release Readiness Inventory and Beads Coordination - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 390 documents the current SurfaceCharts release-readiness surfaces and coordination model for v2.57. It does not change product code, package metadata, public APIs, docs, validation scripts, or release behavior. Follow-up implementation belongs to Phase 391 and later.

</domain>

<decisions>
## Implementation Decisions

### Release Boundary
- v2.57 validates local release-candidate readiness only.
- Public NuGet publishing, public release tags, public release notes, and package release cutover remain out of scope.
- Local package consumption must use package/public APIs only.
- No old chart controls, direct public `Source`, compatibility wrappers, hidden fallback/downshift behavior, backend expansion, or god-code demo/editor scope.

### Coordination
- Beads are the task spine: `Videra-v257` plus `Videra-v257.1` through `Videra-v257.6`.
- Phase 390 is the only unblocked root phase.
- Phase 391 blocks Phase 392.
- Phase 392 unlocks Phase 393 and Phase 394, which are the first implementation phases suitable for parallel isolated worktrees after Phase 392 merges.
- Phase 395 waits for Phase 393 and Phase 394.

### the agent's Discretion
- Inventory details may be recorded in phase artifacts rather than introducing new persistent docs during Phase 390.
- Follow-up gaps are passed to the owning phase rather than fixed during the inventory phase.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- SurfaceCharts package projects: `src/Videra.SurfaceCharts.Core`, `src/Videra.SurfaceCharts.Rendering`, `src/Videra.SurfaceCharts.Processing`, `src/Videra.SurfaceCharts.Avalonia`.
- Shared package metadata: `Directory.Build.props`.
- Package validation: `scripts/Validate-Packages.ps1`, `eng/package-size-budgets.json`, `eng/public-api-contract.json`.
- Package consumer proof: `scripts/Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts`, `smoke/Videra.SurfaceCharts.ConsumerSmoke`.
- Snapshot/export guardrail: `scripts/Test-SnapshotExportScope.ps1`.
- Demo/cookbook surface: `samples/Videra.SurfaceCharts.Demo`.
- Doctor/support surfaces: `scripts/Invoke-VideraDoctor.ps1`, `docs/videra-doctor.md`, `docs/troubleshooting.md`, `docs/support-matrix.md`.

### Established Patterns
- Release validation is script-driven and emits deterministic artifacts under `artifacts/`.
- Package validation checks README/icon/license/repository/tags/descriptions, symbols, dependency boundaries, and size budgets.
- Consumer smoke packs public packages into a local feed, restores against that feed, and validates support artifacts.
- Repository guardrails are split between PowerShell scripts and focused repository tests.

### Integration Points
- Phase 391 should own public API catalog and stale API guardrail metadata.
- Phase 392 should own clean package-consumer validation using package/public APIs only.
- Phase 393 should compose the release-readiness script and CI/manual boundary.
- Phase 394 should own release-candidate docs and support handoff.

</code_context>

<specifics>
## Specific Ideas

- `eng/public-api-contract.json` is stale for SurfaceCharts Avalonia and should be reviewed in Phase 391.
- `SurfaceChartsOutputEvidenceGuardrails_ShouldStayChartLocalAndStructured` appears stale around image export support and should be reviewed in Phase 391.
- Demo README/support docs lag the actual UI around Bar/Contour entries and support artifact wording; Phase 394 should align them.
- Current local `artifacts/consumer-smoke` does not contain fresh SurfaceCharts smoke result/support files; Phase 392 should regenerate deterministic evidence.

</specifics>

<deferred>
## Deferred Ideas

- Public NuGet publish and tag creation.
- Full API diff tooling against a prior published package.
- Visual regression screenshots for cookbook recipes.

</deferred>
