# Phase 170 Context: Release Evidence Packet and Review Index

## Milestone

`v2.9 Release Candidate Final Validation`

## Goal

Provide a single reviewable candidate evidence index that ties release dry-run output to the required CI checks, package validation artifacts, benchmark gates, native validations, consumer smoke, and support docs.

## Boundary

This phase is limited to read-only evidence indexing:

- define the required release-candidate evidence checklist
- generate a review index from the release dry-run path
- keep the index non-publishing and credential-free
- avoid new CI workflows or release paths

## Decisions

- Use a small JSON contract under `eng/` for required evidence names.
- Use a small PowerShell generator under `scripts/`.
- Generate JSON and text index files inside the existing release dry-run artifact root.
- Do not require unrelated CI artifacts to be present during local dry-run; the index lists expected evidence for reviewer signoff.
- Keep docs/tests as repository guardrails.

## Deferred

- Abort/cutover runbook truth is deferred to Phase 171.
- Actual public release creation remains out of scope.
