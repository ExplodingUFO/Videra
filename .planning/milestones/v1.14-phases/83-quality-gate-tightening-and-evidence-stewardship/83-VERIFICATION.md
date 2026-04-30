# Phase 83 Verification

**Phase Goal:** Convert today’s advisory quality signals into an actionable alpha-ready definition of green.

## Verification Commands

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~RuntimeFramePreludeTests|FullyQualifiedName~SceneDeltaPlannerTests|FullyQualifiedName~VideraSnapshotExportServiceTests"`  
  Result: passed, 4 tests.
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests|FullyQualifiedName~VideraViewExtensibilityIntegrationTests|FullyQualifiedName~VideraViewInteractionIntegrationTests|FullyQualifiedName~VideraViewInspectionIntegrationTests|FullyQualifiedName~VideraInspectionBundleIntegrationTests"`  
  Result: passed, 33 tests.
- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -OutputRoot artifacts/consumer-smoke-quality -BuildOnly -TreatWarningsAsErrors`  
  Result: passed; packed public packages restored the local-feed consumer smoke app and built it with warnings treated as errors.
- `dotnet build tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release -p:TreatWarningsAsErrors=true`  
  Result: passed with 0 warnings and 0 errors.
- `dotnet build tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release -p:TreatWarningsAsErrors=true`  
  Result: passed with 0 warnings and 0 errors.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~AlphaConsumerIntegrationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"`  
  Result: passed, 21 tests.

## Requirement Check

| Requirement | Status | Evidence |
|-------------|--------|----------|
| `QUAL-01` | SATISFIED | The touched scene/runtime/inspection paths were cleaned up to remove the analyzer findings that phase 81 surfaced, and the curated evidence projects now build with warnings as errors. |
| `QUAL-02` | SATISFIED | `quality-gate-evidence` now validates the packaged consumer build plus curated Core warning-clean surfaces, and benchmark docs explicitly describe labeled merge-time review semantics. |
| `QUAL-03` | SATISFIED | `README.md`, `docs/releasing.md`, `docs/benchmark-gates.md`, CI workflow definitions, and repository tests now describe the same alpha-ready definition of green. |

## Residual Risks

- The strict warning gate is intentionally scoped to curated consumer/Core surfaces; it is not yet a whole-repo warnings-as-errors policy.
- Benchmark review is actionable but still not a numeric blocker, so human judgment remains part of the release/merge process.

## Verdict

Phase 83 is complete, and the repo now has a tighter, auditable definition of green for the compatibility-hardening milestone.
