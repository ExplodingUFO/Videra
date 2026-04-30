# Phase 226 Verification

## Commands

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseCandidateTruthRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseCandidateTruthRepositoryTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet build Videra.slnx -c Release -p:RestoreIgnoreFailedSources=true`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-VideraDoctor.ps1 -OutputRoot artifacts/doctor-phase226`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ReleaseDryRun.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts/release-dry-run-phase226`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ConsumerSmoke.ps1 -Scenario ViewerOnly -BuildOnly -TreatWarningsAsErrors -OutputRoot artifacts/consumer-smoke-phase226/ViewerOnly`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ConsumerSmoke.ps1 -Scenario ViewerObj -BuildOnly -TreatWarningsAsErrors -OutputRoot artifacts/consumer-smoke-phase226/ViewerObj`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ConsumerSmoke.ps1 -Scenario ViewerGltf -BuildOnly -TreatWarningsAsErrors -OutputRoot artifacts/consumer-smoke-phase226/ViewerGltf`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts -BuildOnly -TreatWarningsAsErrors -OutputRoot artifacts/consumer-smoke-phase226/SurfaceCharts`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~PublicApiContractRepositoryTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-native-validation.ps1 -Platform Windows -Configuration Release`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~PublicApiContractRepositoryTests|FullyQualifiedName~ReleaseCandidateTruthRepositoryTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `git diff --check`

## Result

- Closeout classification tests passed: 3/3.
- Combined readiness tests passed before blocker fixes: 38/38.
- Full solution build passed.
- Doctor generated the Phase 226 evidence packet.
- Release dry-run for `0.1.0-alpha.7` passed and produced 11 packages plus 11 symbol packages.
- Consumer smoke build-only matrix passed for `ViewerOnly`, `ViewerObj`, `ViewerGltf`, and `SurfaceCharts`.
- Public API contract and closeout classification tests passed after blocker fixes: 5/5.
- Windows native validation passed, including Windows platform tests 29/29 and WPF smoke.
- Final combined readiness tests passed after blocker fixes: 40/40.
- `git diff --check` passed; Git reported LF-to-CRLF working-copy warnings for touched files only.

## Residuals

- Full solution build still reports the two pre-existing SurfaceCharts integration-test CS0067 warnings for unused recording host events.
- Full BenchmarkDotNet threshold gates were not rerun in this phase; they remain covered by the established benchmark validation path.
