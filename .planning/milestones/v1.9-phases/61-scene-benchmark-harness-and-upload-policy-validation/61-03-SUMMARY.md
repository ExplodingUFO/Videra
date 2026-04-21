---
phase: 61-scene-benchmark-harness-and-upload-policy-validation
plan: 03
subsystem: repository-guards
tags: [scene, benchmark, repository]
provides:
  - repository guard for viewer benchmark harness
  - locked benchmark scope vocabulary
key-files:
  modified:
    - tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs
requirements-completed: [OBS-03, UPLD-05, UPLD-06]
completed: 2026-04-17
---

# Phase 61 Plan 03 Summary

## Accomplishments

- Added repository architecture tests that require the viewer benchmark project to remain present and solution-wired.
- Locked the benchmark scope to the intended scene pipeline surfaces.
- Made the benchmark harness part of repository truth rather than an optional local experiment.

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Repository_ShouldIncludeViewerBenchmarkProjectForScenePipelineEvidence"`
