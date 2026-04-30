---
status: passed
phase: 166
milestone: v2.8
verified_at: 2026-04-23
---

# Phase 166 Verification

## Goal

Validate tag/version/package asset expectations without publishing to public feeds.

## Evidence

- `.github/workflows/release-dry-run.yml` runs the dry-run script with read-only repository permissions and uploads `release-dry-run-evidence`.
- `scripts/Invoke-ReleaseDryRun.ps1` reads `eng/public-api-contract.json`, packs all public packages with an explicit dry-run version, runs `Validate-Packages.ps1`, and writes JSON/text evidence summaries.
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs` verifies:
  - the dry-run script is contract-driven
  - package validation is reused
  - summary artifacts are written
  - the dry-run workflow does not push packages or create releases
  - existing public/preview publish workflows still validate packages before publishing

## Commands

- `pwsh -File ./scripts/Invoke-ReleaseDryRun.ps1 -ExpectedVersion 0.0.0-phase166 -OutputRoot ./artifacts/phase166-release-dry-run`
  - Result: passed
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~PackageSizeBudgetRepositoryTests"`
  - Result: passed `27/27`
- `git diff --cached --check`
  - Result: passed before commit

## Scope Check

- No public feed publish behavior, package push token usage, GitHub release creation, runtime feature, rendering feature, chart feature, backend feature, compatibility shim, transition adapter, or fallback path was added.
- The dry-run path is evidence-only and keeps the existing public/preview publishing workflows unchanged except for repository tests that verify their validation boundaries.
