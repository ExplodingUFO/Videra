# Phase 217 Verification

completed: 2026-04-26
commit: c3bd924

## Commands

| Command | Result |
|---|---|
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BenchmarkThresholdRepositoryTests"` before the fix | FAILED as expected; test expected 2 Viewer thresholds while the committed file has 5 |
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BenchmarkThresholdRepositoryTests"` after the fix | PASS; 1 passed |
| `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~BenchmarkContractRepositoryTests|FullyQualifiedName~BenchmarkThresholdRepositoryTests|FullyQualifiedName~AlphaConsumerIntegrationTests"` | PASS; 10 passed |
| `git diff --check` | PASS; no whitespace errors, only existing line-ending warnings |
| `dotnet build Videra.slnx -c Release --no-restore` | NOT USED; missing benchmark `project.assets.json` showed restore was required |
| `dotnet build Videra.slnx -c Release` | PASS; 0 errors, 2 pre-existing CS0067 warnings in SurfaceCharts integration tests |

## Residuals

- A broader repository release-readiness test currently fails because transparency wording is split between `per-object carried alpha` and `per-primitive carried alpha` across docs/tests. This is outside Phase 217's benchmark-gate scope and is carried forward to the release-readiness guardrail phase.
