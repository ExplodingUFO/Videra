# Phase 224 Context: Release Dry-Run Contract Closure

## Goal

Close the read-only release dry-run contract so the package candidate path produces structured evidence and fails closed on drift.

## Assumptions

- Dry-run remains local/read-only and must not publish packages, mutate feeds, push tags, or change remotes.
- `eng/public-api-contract.json` remains the canonical package set.
- Existing package validation, size-budget validation, version simulation, and evidence-index scripts own their respective logic.

## Relevant Files

- `scripts/Invoke-ReleaseDryRun.ps1`
- `scripts/New-ReleaseCandidateEvidenceIndex.ps1`
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`

## Success Criteria

1. Dry-run packs the contract package set, validates metadata and size budgets, simulates version/tag truth, and writes an evidence index.
2. Dry-run summary artifacts expose structured status and artifact paths.
3. Missing or stale contract, package validation, version simulation, or evidence-index inputs fail the dry run.
4. Dry-run remains non-publishing.
