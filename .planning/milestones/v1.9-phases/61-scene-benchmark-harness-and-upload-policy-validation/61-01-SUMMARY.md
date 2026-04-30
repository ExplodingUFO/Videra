---
phase: 61-scene-benchmark-harness-and-upload-policy-validation
plan: 01
subsystem: benchmarks
tags: [scene, benchmark, evidence]
provides:
  - dedicated viewer scene benchmark project
  - solution-integrated scene performance harness
key-files:
  modified:
    - Videra.slnx
    - src/Videra.Core/Videra.Core.csproj
    - src/Videra.Avalonia/Videra.Avalonia.csproj
  added:
    - benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj
    - benchmarks/Videra.Viewer.Benchmarks/Program.cs
    - benchmarks/Videra.Viewer.Benchmarks/ScenePipelineBenchmarks.cs
requirements-completed: [OBS-03]
completed: 2026-04-17
---

# Phase 61 Plan 01 Summary

## Accomplishments

- Added a new `Videra.Viewer.Benchmarks` project dedicated to viewer-scene performance evidence.
- Wired the benchmark project into `Videra.slnx` and exposed the required internal seams through `InternalsVisibleTo`.
- Covered import, batch import orchestration, residency apply, upload drain, and rehydrate-after-backend-ready as explicit benchmark entrypoints.

## Verification

- `dotnet build benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj -c Release`
