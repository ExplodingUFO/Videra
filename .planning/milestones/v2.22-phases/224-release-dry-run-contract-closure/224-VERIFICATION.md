# Phase 224 Verification

## Commands

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-ReleaseDryRun.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts/release-dry-run-phase224`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet build Videra.slnx -c Release -p:RestoreIgnoreFailedSources=true`
- `git diff --check`

## Result

- ReleaseDryRun repository tests passed: 8/8.
- Real release dry run passed for `0.1.0-alpha.7`.
- Dry run produced 11 package files and 11 symbol package files.
- Package validation passed and wrote package size evidence.
- Release candidate evidence index generation passed.
- ReleaseDryRun plus Doctor repository tests passed: 12/12.
- Full solution build passed.
- `git diff --check` passed.

## Residuals

- Full solution build still reports the two pre-existing SurfaceCharts integration-test CS0067 warnings for unused recording host events.
