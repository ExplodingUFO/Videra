# Phase 198 Verification

## Checks

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ConsumerSmokeConfigurationTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~WpfSmokeConfigurationTests|FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~AlphaConsumerIntegrationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"`
  - Result: PASS (`57/57`)
- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario Viewer -OutputRoot artifacts/test-results/verify/viewer-proof-10s -LightingProofHoldSeconds 10`
  - Result: PASS
- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/test-results/verify/surfacecharts-proof-10s -LightingProofHoldSeconds 10`
  - Result: PASS
- `pwsh -File ./scripts/Invoke-WpfSmoke.ps1 -Configuration Release -OutputRoot artifacts/test-results/verify/wpf-proof-10s -LightingProofHoldSeconds 10`
  - Result: PASS
- `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly -OutputRoot artifacts/test-results/verify/surfacecharts-buildonly -LightingProofHoldSeconds 10`
  - Result: PASS
- `dotnet build smoke/Videra.WpfSmoke/Videra.WpfSmoke.csproj -c Release`
  - Result: PASS
- `git diff --check master...HEAD`
  - Result: PASS

## Result

PASS
