# Phase 202 Verification

## Checks

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ConsumerSmokeConfigurationTests|FullyQualifiedName~WpfSmokeConfigurationTests"`
  - Result: PASS (`3/3`)
- `pwsh -File scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -LightingProofHoldSeconds 10 -OutputRoot artifacts/consumer-smoke/windows/v2.17-phase202`
  - Result: PASS
  - Evidence: `artifacts/consumer-smoke/windows/v2.17-phase202/consumer-smoke-result.json` includes `"EmissiveNormalProofObjectName": "ConsumerSmokeEmissiveNormalProofQuad"` and `"LightingProofHoldSeconds": 10`
- `pwsh -File scripts/Invoke-WpfSmoke.ps1 -Configuration Release -LightingProofHoldSeconds 10 -OutputRoot artifacts/test-results/verify/wpf-smoke/v2.17-phase202`
  - Result: PASS
  - Evidence: `artifacts/test-results/verify/wpf-smoke/v2.17-phase202/wpf-smoke-diagnostics.txt` includes `EmissiveNormalProofObjectName: WpfSmokeEmissiveNormalProofQuad`
- `git diff --check`
  - Result: PASS (no whitespace errors; CRLF warnings only)

## Result

PASS
