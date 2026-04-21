---
phase: 64-happy-path-viewer-api-and-minimal-sample-simplification
plan: 02
subsystem: onboarding-docs
tags: [docs, alpha, onboarding]
provides:
  - canonical happy-path README flow
  - explicit separation between default usage and extensibility
key-files:
  modified:
    - README.md
    - src/Videra.Avalonia/README.md
requirements-completed: [API-04, API-05]
completed: 2026-04-17
---

# Phase 64 Plan 02 Summary

## Accomplishments

- Reworked the root README so the shortest public integration path now routes through the minimal sample.
- Reworked `src/Videra.Avalonia/README.md` so the primary example uses only `Options`, scene load, camera helpers, and diagnostics.
- Moved `Engine` and other advanced extensibility seams behind explicit opt-in guidance.

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~MinimalSampleConfigurationTests"`
