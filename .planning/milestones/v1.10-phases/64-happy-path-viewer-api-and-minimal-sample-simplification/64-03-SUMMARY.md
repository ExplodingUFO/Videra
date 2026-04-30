---
phase: 64-happy-path-viewer-api-and-minimal-sample-simplification
plan: 03
subsystem: repository-guards
tags: [tests, repository, onboarding]
provides:
  - repository guard for happy-path sample truth
  - repository guard for default docs vocabulary
key-files:
  added:
    - tests/Videra.Core.Tests/Samples/MinimalSampleConfigurationTests.cs
  modified:
    - tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
requirements-completed: [API-04, API-05, API-06]
completed: 2026-04-17
---

# Phase 64 Plan 03 Summary

## Accomplishments

- Added repository tests that require `Videra.MinimalSample` to exist, build, and stay on the narrow public API path.
- Locked root README and Avalonia README references to the minimal sample.
- Prevented default onboarding from silently drifting back toward internal extensibility-first examples.

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~MinimalSampleConfigurationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"`
