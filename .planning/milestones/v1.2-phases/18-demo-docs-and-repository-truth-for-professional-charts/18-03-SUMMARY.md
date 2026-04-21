---
phase: 18-demo-docs-and-repository-truth-for-professional-charts
plan: 03
subsystem: surface-charts-repository-truth
tags: [surface-charts, repository-guards, docs, localization, tests]
requires:
  - phase: 18-demo-docs-and-repository-truth-for-professional-charts
    provides: aligned demo and docs truth from plans 18-01 and 18-02
provides:
  - repository-level exact phrase guards for chart onboarding, probe workflow, and renderer-status truth
  - stale-phrase blocking for guarded English and Chinese entrypoints
  - final requirement closure for DEMO-01
affects: []
tech-stack:
  patterns: [exact sentence guards, headless sample tests, localization parity checks]
key-files:
  modified:
    - tests/Videra.Core.Tests/Repository/SurfaceChartsDocumentationTerms.cs
    - tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs
    - tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs
    - tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs
requirements-completed: [DEMO-01]
completed: 2026-04-14
---

# Phase 18 Plan 03 Summary

## Accomplishments
- Extended `SurfaceChartsDocumentationTerms` with exact phrases for onboarding, renderer-status truth, probe workflow, Chinese parity, and stale wording that must stay gone.
- Expanded repository guards so root/demo/Chinese chart entrypoints must continue to describe the same shipped boundary and limitation truth.
- Kept the sample-behavior tests aligned with the repo wording so the demo UX and documentation story freeze the same outward-facing contract.

## Verification
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests"`

## Notes
- Guards stay deterministic and sentence-focused; no whole-file snapshots were introduced.
- The guarded entrypoints now explicitly block the old "axes/legend incomplete" wording from reappearing.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
