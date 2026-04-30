# Phase 220 Verification

completed: 2026-04-26
commit: 7f19ad9

## Commands

| Command | Result |
|---|---|
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BenchmarkContractRepositoryTests"` after adding expected diagnostics names but before contract implementation | FAILED as expected; contract did not yet declare the new diagnostics benchmarks |
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BenchmarkContractRepositoryTests|FullyQualifiedName~BenchmarkThresholdRepositoryTests"` | PASS; 2 passed |
| `dotnet build benchmarks\Videra.SurfaceCharts.Benchmarks\Videra.SurfaceCharts.Benchmarks.csproj -c Release` | PASS; 0 warnings, 0 errors |
| `dotnet build benchmarks\Videra.Viewer.Benchmarks\Videra.Viewer.Benchmarks.csproj -c Release` | FAILED due user-level NuGet source `local-artifacts` pointing at missing `.worktrees/phase206/artifacts/packages`; no repo file references that path |
| `dotnet build benchmarks\Videra.Viewer.Benchmarks\Videra.Viewer.Benchmarks.csproj -c Release --ignore-failed-sources` | PASS; 0 warnings, 0 errors |
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~BenchmarkContractRepositoryTests|FullyQualifiedName~BenchmarkThresholdRepositoryTests|FullyQualifiedName~AlphaConsumerIntegrationTests"` | PASS; 10 passed |
| `dotnet test tests\Videra.SurfaceCharts.Benchmarks.Tests\Videra.SurfaceCharts.Benchmarks.Tests.csproj -c Release --no-build` | PASS; 2 passed |
| `dotnet build Videra.slnx -c Release --no-restore` | FAILED because several fresh worktree projects had no `project.assets.json`; not a code failure |
| `dotnet build Videra.slnx -c Release --ignore-failed-sources` | PASS; 0 errors, 2 pre-existing SurfaceCharts CS0067 warnings |
| `git diff --check` | PASS; no whitespace errors, only line-ending warnings |

## Residuals

- User-level NuGet source `local-artifacts` still points at a missing phase206 path; this phase did not mutate user configuration.
- The transparency wording drift noted in Phase 217 remains deferred to Phase 221.
