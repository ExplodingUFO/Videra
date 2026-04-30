# Phase 430: CI Performance and Release-Readiness Truth - Context

**Gathered:** 2026-04-30
**Status:** Blocked (depends on Phase 429)
**Mode:** Autonomous (smart discuss)
**Bead:** `Videra-7tqx.6`

## Phase Boundary

Phase 430 keeps the larger workflow surface honest in CI and release-readiness.
It adds focused tests and CI filters for workspace, linked interaction, streaming,
cookbook, package templates, and generated roadmap checks. Release-readiness
validation includes new package-consumer evidence without counting skipped or
unavailable checks as success. Guardrails continue to reject compatibility claims,
old chart paths, hidden fallback/downshift, broad workbench scope, backend
expansion, and fake evidence.

## User Constraints (from Phase 425)

- Beads are the task, state, and handoff spine.
- Split tasks small and identify dependencies before implementation.
- Use isolated worktrees and branches for parallel beads when write scopes do
  not block each other.
- Every worker must have a responsibility boundary, write scope, validation
  command, and handoff notes.
- Avoid god code.
- Do not add compatibility layers, downshift behavior, fallback behavior, old
  chart controls, or fake validation evidence.
- Keep implementation simple and direct.

## Decisions

### D-01: CI Filter Extension

Extend existing CI filters to cover v2.64 surfaces:
- **Workspace filter**: tests for workspace registration, link groups, evidence
- **Linked interaction filter**: tests for propagator lifecycle, evidence
- **Streaming filter**: existing + workspace-aware streaming evidence
- **Cookbook filter**: recipe coverage and snippet correctness
- **Package template filter**: template structure and support wording

### D-02: Release-Readiness Composition

`Invoke-ReleaseReadinessValidation.ps1` gains:
- Workspace evidence validation
- Linked interaction evidence validation
- Package template evidence validation
- Explicit reporting of skipped/unavailable checks (not silent pass)

### D-03: Guardrail Extensions

`Test-SnapshotExportScope.ps1` and related tests add:
- No generic workbench shell claims
- No compatibility adapter claims
- No hidden propagation state claims

### D-04: Generated Roadmap Sync

`Export-BeadsRoadmap.ps1` and `BeadsPublicRoadmapTests` verify:
- Generated roadmap reflects v2.64 phase completion
- Phase status matches Beads state
- No manual edits to generated roadmap

## Canonical References

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425D-CI-RELEASE-GUARDRAIL-INVENTORY.md` — full inventory
- `.github/workflows/ci.yml` — CI workflow
- `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs` — CI truth tests
- `scripts/Invoke-ReleaseReadinessValidation.ps1` — release readiness
- `scripts/Invoke-ConsumerSmoke.ps1` — consumer smoke
- `scripts/Test-SnapshotExportScope.ps1` — scope guardrails
- `scripts/Export-BeadsRoadmap.ps1` — generated roadmap
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` — roadmap tests

## Existing Code Insights

### Reusable Assets
- `SurfaceChartsCiTruthTests` — CI truth test pattern
- `Invoke-ReleaseReadinessValidation.ps1` — release readiness composition
- `Invoke-ConsumerSmoke.ps1` — consumer smoke validation
- `Test-SnapshotExportScope.ps1` — scope guardrail pattern
- `Export-BeadsRoadmap.ps1` + `BeadsPublicRoadmapTests` — roadmap truth

### Established Patterns
- CI truth tests validate YAML and reject fake green patterns
- Release readiness composes multiple validation scripts
- Consumer smoke requires real support summary and rendering status
- Scope guardrails reject old controls and hidden fallback
- Generated roadmap is deterministically generated from Beads

### Integration Points
- `.github/workflows/ci.yml` — add focused filters
- `tests/Videra.Core.Tests/Repository/` — new CI truth tests
- `scripts/Invoke-ReleaseReadinessValidation.ps1` — extend validation
- `scripts/Test-SnapshotExportScope.ps1` — extend guardrails
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` — update expectations

## Deferred Ideas

- Performance benchmark threshold promotion — needs CI history
- Automated package template smoke in CI — complex integration
- Visual evidence capture in CI — needs GPU/host environment

---

*Phase: 430-ci-performance-and-release-readiness-truth*
*Context gathered: 2026-04-30*
