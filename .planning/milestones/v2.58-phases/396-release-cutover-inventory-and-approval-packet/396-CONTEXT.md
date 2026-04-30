# Phase 396: Release Cutover Inventory and Approval Packet - Context

**Gathered:** 2026-04-30
**Status:** Ready for execution
**Mode:** Autonomous smart discuss with read-only subagent fan-out
**Bead:** `Videra-v258.1`

<domain>

## Phase Boundary

Phase 396 starts v2.58 by converting completed v2.57 release-readiness evidence into an approval packet, abort/hold criteria, and dependency-aware Beads/worktree handoff. It is an inventory and coordination phase only.

It must not publish packages, create tags, create GitHub releases, mutate package feeds, change public APIs, restore old chart controls, add compatibility layers, add fallback/downshift behavior, or widen the demo/workbench scope.

</domain>

<decisions>

## Implementation Decisions

- Treat Beads as the task spine. Phase bead `Videra-v258.1` owns the phase; child beads `Videra-v258.1a`, `Videra-v258.1b`, and `Videra-v258.1c` split evidence, package/script, and Beads/docs inventory.
- Use isolated worktrees and branches for the parallel read-only child beads:
  - `.worktrees/v258-phase396-evidence` on `v2.58-phase396-evidence`
  - `.worktrees/v258-phase396-package` on `v2.58-phase396-package`
  - `.worktrees/v258-phase396-beadsdocs` on `v2.58-phase396-beadsdocs`
- Keep Phase 396 artifacts under `.planning/phases/396-release-cutover-inventory-and-approval-packet/`.
- Record weak evidence as risk and downstream planning input rather than editing generated v2.57 artifacts in place.

</decisions>

<code_context>

## Existing Evidence Surfaces

- v2.57 archive:
  - `.planning/milestones/v2.57-ROADMAP.md`
  - `.planning/milestones/v2.57-REQUIREMENTS.md`
  - `.planning/milestones/v2.57-phases/`
- v2.57 final phase:
  - `.planning/phases/395-integration-guardrails-and-milestone-evidence/395-VERIFICATION.md`
- Final release-readiness evidence:
  - `artifacts/phase395-release-readiness-final/release-readiness-validation-summary.json`
  - `artifacts/phase395-release-readiness-final/release-readiness-validation-summary.txt`
  - `artifacts/phase395-release-readiness-final/release-dry-run/release-dry-run-summary.json`
  - `artifacts/phase395-release-readiness-final/release-dry-run/release-candidate-evidence-index.json`
  - `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/consumer-smoke-result.json`
  - `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/surfacecharts-support-summary.txt`
  - `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/diagnostics-snapshot.txt`
- Package/release surfaces:
  - `Directory.Build.props`
  - `eng/public-api-contract.json`
  - `eng/package-size-budgets.json`
  - `eng/release-candidate-evidence.json`
  - `scripts/Invoke-ReleaseReadinessValidation.ps1`
  - `scripts/Invoke-ReleaseDryRun.ps1`
  - `scripts/Invoke-PublicReleasePreflight.ps1`
  - `scripts/Invoke-FinalReleaseSimulation.ps1`
  - `scripts/Test-SnapshotExportScope.ps1`
  - `.github/workflows/release-dry-run.yml`
  - `.github/workflows/publish-public.yml`
  - `.github/workflows/publish-existing-public-release.yml`

</code_context>

<specifics>

## Specific Findings

- Current version truth is `0.1.0-alpha.7` in `Directory.Build.props`.
- v2.57 final readiness command passed with package build, SurfaceCharts full consumer smoke, focused tests, and snapshot scope guardrails.
- Public publish, tag, and GitHub release actions were intentionally skipped and remain gated.
- `artifacts/phase395-release-readiness-final/surfacecharts-consumer-smoke/consumer-smoke-result.json` lists optional `inspection-snapshot.png` and `inspection-bundle` paths that were not present in the inspected artifact folder. This is a weak evidence note, not a Phase 396 blocker.
- Optional Doctor and Performance Lab visual evidence are recorded as optional/non-blocking in the release-candidate evidence index.
- Beads dependency graph has the first true v2.58 parallel point after Phase 397: Phase 398 and Phase 399 can run independently.

</specifics>

<deferred>

## Deferred Ideas

- Phase 397 should confirm package/version contracts and decide whether the optional inspection artifact path mismatch needs a producer fix, docs clarification, or explicit non-blocking note.
- Phase 398 should make pass/fail/skipped/manual-gate states explicit in dry-run output.
- Phase 399 should keep release notes/support docs clear that optional visual evidence does not block publish decisions by itself.

</deferred>
