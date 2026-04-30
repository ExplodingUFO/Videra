# Phase 194 Verification

Verified in worktree `v2.15-phase194-lighting-proof-diagnostics`.

## Commands

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ConsumerSmokeConfigurationTests|FullyQualifiedName~WpfSmokeConfigurationTests|FullyQualifiedName~RepositoryNativeValidationTests"`
- `pwsh -File .\scripts\Invoke-ConsumerSmoke.ps1 -Scenario Viewer -Configuration Release -OutputRoot artifacts/consumer-smoke/windows-lighting-proof -LightingProofHoldSeconds 10`
- `pwsh -File .\scripts\Invoke-WpfSmoke.ps1 -Configuration Release -OutputRoot artifacts/test-results/verify/wpf-smoke-lighting-proof -LightingProofHoldSeconds 10`
- `git diff --check master...HEAD`

## Results

- Focused repository tests passed: `27/27`.
- Viewer packaged consumer smoke passed with proof hold enabled.
  - `consumer-smoke-result.json` recorded `LightingProofHoldSeconds = 10`.
  - `consumer-smoke-trace.log` contains `Lighting proof hold active for 10 seconds before shutdown.`
- WPF smoke passed with proof hold enabled.
  - end-to-end measured runtime was `14.27s`, which cleared the requested `10`-second survival window on this host.
- `git diff --check master...HEAD` passed with no whitespace errors.

## Evidence Paths

- `artifacts/consumer-smoke/windows-lighting-proof/consumer-smoke-result.json`
- `artifacts/consumer-smoke/windows-lighting-proof/consumer-smoke-trace.log`
- `artifacts/consumer-smoke/windows-lighting-proof/diagnostics-snapshot.txt`
- `artifacts/test-results/verify/wpf-smoke-lighting-proof/wpf-smoke-diagnostics.txt`
