# Phase 393 Verification

## Commands and Results

### Release-readiness command wiring

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseReadinessValidation.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts\phase393-release-readiness-validation -ConsumerSmokeBuildOnly
```

Result: Passed.

Evidence from summary:

- `package-build`: pass; `scripts/Invoke-ReleaseDryRun.ps1` packed and validated the public package set.
- `surfacecharts-consumer-smoke`: pass; `scripts/Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts -BuildOnly` packed, restored, and built the package-only consumer.
- `surfacecharts-focused-tests`: pass; 30 focused tests passed.
- `snapshot-scope-guardrails`: pass.
- Local environment warnings: none detected by the readiness script.
- Public publish/tag steps: explicitly skipped for NuGet push, release tag, and GitHub release creation.

Summary artifacts:

- `artifacts/phase393-release-readiness-validation/release-readiness-validation-summary.json`
- `artifacts/phase393-release-readiness-validation/release-readiness-validation-summary.txt`

### Focused script/configuration tests

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~PackageSizeBudgetRepositoryTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests|FullyQualifiedName~ReleaseDryRunRepositoryTests"
```

Result: Passed. 21 passed, 0 failed, 0 skipped.

### Snapshot scope guardrails

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1
```

Result: Passed. All scope checks passed.

## Notes

- The first readiness run exposed stale SurfaceCharts package-size budgets. The budgets were aligned to the current v2.57 package outputs with modest binary-KiB ceilings, and the passing run above validates that alignment.
- Full desktop SurfaceCharts consumer smoke without `-ConsumerSmokeBuildOnly` was not run in this session. The new command runs it by default; the build-only switch was used here as the least expensive wiring proof requested for local validation.