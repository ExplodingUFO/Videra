# Phase 187 Verification

## Commands

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release -m:1 --filter "FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~HostingBoundaryTests|FullyQualifiedName~PackageSizeBudgetRepositoryTests"`
- `pwsh -NoProfile -File scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario Viewer -BuildOnly -OutputRoot artifacts/consumer-smoke-phase187`
- `pwsh -NoProfile -File scripts/Validate-Packages.ps1 -PackageRoot artifacts/consumer-smoke-phase187/packages -ExpectedVersion 0.1.0-alpha.7.consumer-smoke`
- `git diff --check`

## Result

PASS
