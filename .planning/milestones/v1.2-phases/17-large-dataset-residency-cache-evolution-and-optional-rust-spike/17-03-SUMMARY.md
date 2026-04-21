---
phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike
plan: 03
subsystem: surface-charts-data
tags: [surface-charts, benchmarks, native-boundary, reduction-kernel, repository-guards]
requires:
  - phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike
    provides: persistent cache sessions, batch reads, and tile statistics from plan 17-02
provides:
  - runnable `BenchmarkDotNet` measurements for scheduler, cache, and pyramid hotspots
  - coarse managed reduction-kernel seam ready for future measurement-gated replacement
  - repository guards that keep native interop out of chart interaction and rendering layers
affects: [18-demo-docs-and-repository-truth-for-professional-charts]
tech-stack:
  added: [BenchmarkDotNet]
  patterns: [measurement-first optimization, coarse native seam, repository truth guards]
key-files:
  created:
    - benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj
    - benchmarks/Videra.SurfaceCharts.Benchmarks/Program.cs
    - benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsBenchmarks.cs
    - src/Videra.SurfaceCharts.Core/ISurfaceTileReductionKernel.cs
    - src/Videra.SurfaceCharts.Core/ManagedSurfaceTileReductionKernel.cs
    - tests/Videra.SurfaceCharts.Core.Tests/SurfaceReductionKernelParityTests.cs
  modified:
    - Videra.slnx
    - src/Videra.SurfaceCharts.Core/SurfacePyramidBuilder.cs
    - src/Videra.SurfaceCharts.Core/InMemorySurfaceTileSource.cs
    - src/Videra.SurfaceCharts.Processing/README.md
    - tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs
requirements-completed: [DATA-04]
completed: 2026-04-14
---

# Phase 17 Plan 03 Summary

## Accomplishments
- Added `benchmarks/Videra.SurfaceCharts.Benchmarks` with runnable hotspot coverage for viewport selection, cache batch reads, and pyramid/statistics generation.
- Extracted `ISurfaceTileReductionKernel` plus `ManagedSurfaceTileReductionKernel`, then routed both pyramid reduction and source-region statistics through that one coarse seam.
- Updated processing docs and repository guards so native work stays explicitly optional, measurement-gated, and out of chart interaction/rendering layers.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfacePyramidBuilderTests|FullyQualifiedName~InMemorySurfaceTileSourceTests|FullyQualifiedName~SurfaceReductionKernelParityTests"`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests"`
- `dotnet run --project benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj -c Release -- --list flat`

## Notes
- No Rust project, native packaging, or runtime callback graph was added in this plan; the only new seam is the coarse reduction boundary inside `Videra.SurfaceCharts.Core`.
- The native-boundary repository guard is intentionally scoped to `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction` and `src/Videra.SurfaceCharts.Rendering`, because the phase-16 chart-native-host shell already owns the existing interop code elsewhere in Avalonia.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
