# Phase 390: Release Readiness Inventory and Beads Coordination - Plan

bead: Videra-v257.1

## Goal

Document current release-readiness surfaces, release-candidate boundaries, Beads dependencies, validation expectations, and handoff notes for v2.57 without changing implementation code.

## Tasks

1. Claim `Videra-v257.1` and verify it is the only unblocked phase bead.
2. Inventory package, release, CI, smoke, demo, docs, support, public API, and guardrail surfaces.
3. Record release-candidate boundaries and deferred publish/tag scope.
4. Record Beads dependency and parallelization model.
5. Write Phase 390 context, summary, and verification artifacts.
6. Close the Phase 390 bead, export Beads, regenerate the public roadmap, update `.planning` progress, commit, push Dolt and Git.

## Ownership Boundaries

- Phase 390 owns planning artifacts and Beads state only.
- Phase 390 does not edit `src/`, `tests/`, `scripts/`, `docs/`, `samples/`, or `smoke/` product surfaces except generated roadmap export.
- API contract, package metadata, consumer smoke, release script, and docs fixes are deferred to their owning phase beads.

## Validation

- `bd show Videra-v257* --json`
- `bd ready --json`
- `git status --short --branch`
- `scripts/Export-BeadsRoadmap.ps1`

## Handoff

- Next bead: `Videra-v257.2` / Phase 391.
- Phase 391 should use an isolated worktree if implementation changes are needed.
- Phase 393 and Phase 394 can run in parallel only after Phase 392 closes and merges.
