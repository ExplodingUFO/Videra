# Phase 169 Context: Candidate Version and Tag Simulation Guardrails

## Milestone

`v2.9 Release Candidate Final Validation`

## Goal

Add credential-free validation for candidate tag/version semantics without creating a repository tag or publishing packages.

## Boundary

This phase is limited to release-candidate validation:

- validate the proposed `v*` tag spelling as a simulated tag
- validate the derived package version against repository package metadata
- validate release dry-run summary truth against the public API contract package set
- keep all validation read-only and non-publishing

## Decisions

- Implement the guard as a small script under `scripts/`, not a new service or framework.
- Integrate the guard into `scripts/Invoke-ReleaseDryRun.ps1` so CI dry-run validation automatically exercises it.
- Treat `Directory.Build.props` version as the repository package metadata truth for the current candidate.
- Keep repository tests as direct guardrails over script/workflow truth.
- Do not create tags, push packages, or add credentials.

## Deferred

- Human cutover docs and abort criteria are deferred to Phase 171.
- Evidence packet/index is deferred to Phase 170.
