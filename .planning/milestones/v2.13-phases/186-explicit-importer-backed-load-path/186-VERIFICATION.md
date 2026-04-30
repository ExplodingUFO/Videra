# Phase 186 Verification

## Commands

- `pwsh -NoProfile -File scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario Viewer -OutputRoot artifacts/consumer-smoke-phase186`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~AlphaConsumerIntegrationTests"`
- `git diff --check`

## Result

PASS
