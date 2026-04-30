# Phase 168 Context: Release Candidate Final Validation Scope Lock

## Milestone

`v2.9 Release Candidate Final Validation`

## Goal

Lock `v2.9` to final release-candidate validation before implementation starts.

## Boundary

This milestone validates the release-candidate process itself:

- candidate version/tag simulation
- release evidence packet and review index
- release dry-run artifact reviewability
- package validation evidence traceability
- benchmark/native/consumer-smoke evidence traceability
- abort criteria and manual cutover boundary

## Decisions

- The milestone must not publish packages or create repository release tags.
- The milestone must not add runtime, renderer, material, glTF, backend, platform, chart, or UI-adapter breadth.
- The milestone must not introduce compatibility shims, migration adapters, fallback release paths, or transitional contracts.
- Version/tag validation should remain credential-free and deterministic.
- Release evidence should connect existing workflows and artifacts instead of inventing a parallel release path.
- Cutover to an actual release remains human-controlled and outside `v2.9`.

## Existing Evidence

- `v2.8` shipped public API contract guardrails in `eng/public-api-contract.json`.
- `.github/workflows/release-dry-run.yml` already packs the public package set without publishing.
- `scripts/Invoke-ReleaseDryRun.ps1` already produces release dry-run summaries.
- `scripts/Validate-Packages.ps1` already validates package metadata, symbols, assets, dependencies, and package-size budgets.
- Existing repository tests already guard release-candidate docs and workflow truth.

## Deferred

- Actual public publishing and tag creation remain deferred until a human release cutover.
- Advanced runtime/renderer/material/backend/chart/UI breadth remains deferred.
- Second UI adapter product work remains deferred.
- Any fallback or compatibility release path remains out of scope.
