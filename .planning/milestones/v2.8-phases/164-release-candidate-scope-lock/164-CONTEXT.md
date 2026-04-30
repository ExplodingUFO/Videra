# Phase 164 Context: Release Candidate Scope Lock

## Milestone

`v2.8 Release Candidate Contract Closure`

## Goal

Lock `v2.8` to release-candidate contract closure before code changes start.

## Boundary

This milestone is about confidence in the existing shipped public surface:

- public package line
- public API contract
- release dry run
- package assets
- generated release notes
- support boundary
- docs/repository truth

## Decisions

- The milestone should not add runtime, renderer, material, glTF, backend, platform, chart, or UI-adapter breadth.
- The milestone should not introduce compatibility shims, migration adapters, fallback layers, or transition APIs.
- The milestone should not publish to `nuget.org` or GitHub Packages and should not create release tags.
- Public API drift guardrails must be deterministic in repo/CI.
- Release evidence must be non-publishing and should validate the current package/release path instead of inventing a parallel release process.

## Existing Evidence

- `docs/releasing.md` already describes public and preview publishing flows.
- `.github/workflows/publish-public.yml` and `.github/workflows/publish-github-packages.yml` already pack the public package set.
- `scripts/Validate-Packages.ps1` already validates package metadata, package-size budgets, and package assets.
- PR gates already include CI, native validation, consumer smoke, package evidence, sample-contract evidence, quality-gate evidence, and benchmark gates.

## Deferred

- Advanced runtime/renderer breadth remains deferred.
- New chart families remain deferred.
- Extra public UI adapters remain deferred.
- Actual publication and release tagging remain outside this milestone.
