---
status: active
phase: 398
bead: Videra-v258.3
---

# Phase 398 Plan

## Success Criteria

- Release dry-run evidence declares the allowed states: `pass`, `fail`, `skipped`, and `manual-gate`.
- Publish/tag/GitHub release actions are recorded with visible commands, explicit approval inputs, `failClosedDefault=true`, and `actionTaken=false`.
- Public preflight fails when release-action gate evidence is missing or malformed.
- Focused repository tests validate the script contract.
- A local dry-run/preflight command produces pass evidence without publishing, tagging, or creating a GitHub release.

## Implementation

1. Extend `Invoke-ReleaseDryRun.ps1` with release-action gate evidence.
2. Extend `New-ReleaseCandidateEvidenceIndex.ps1` so the evidence index preserves the release-action gate state.
3. Extend `Invoke-PublicReleasePreflight.ps1` so manual gates are accepted as explicit non-mutating state and malformed/missing gates fail closed.
4. Surface preflight gate state in `Invoke-FinalReleaseSimulation.ps1`.
5. Add focused tests in `ReleaseDryRunRepositoryTests`.

## Verification

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~ReleaseDryRunRepositoryTests`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseDryRun.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts\phase398-release-dry-run`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-PublicReleasePreflight.ps1 -ExpectedVersion 0.1.0-alpha.7 -EvidenceRoot artifacts\phase398-preflight-evidence -ReleaseDryRunRoot artifacts\phase398-release-dry-run -BenchmarkRoot artifacts\phase398-preflight-evidence\benchmarks -ConsumerSmokeRoot artifacts\phase398-preflight-evidence\consumer-smoke -NativeValidationRoot artifacts\phase398-preflight-evidence\native-validation -OutputRoot artifacts\phase398-public-release-preflight -SkipRepositoryStateCheck`
