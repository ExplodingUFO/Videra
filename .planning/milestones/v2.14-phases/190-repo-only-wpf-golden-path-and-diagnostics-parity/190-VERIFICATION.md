# Phase 190 Verification

## Checks

- `dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj`
  - Result: PASS
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~WpfSmokeConfigurationTests|FullyQualifiedName~RepositoryNativeValidationTests"`
  - Result: PASS (`24/24`)
- `pwsh -NoProfile -File scripts\Invoke-WpfSmoke.ps1 -Configuration Release -OutputRoot artifacts\wpf-smoke-phase190`
  - Result: PASS
- `git diff --check`
  - Result: PASS (CRLF warnings only)

## Result

PASS
