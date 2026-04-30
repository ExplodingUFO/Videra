# Phase 69 Verification

- Diagnostics snapshot is now a stable, copy-pasteable public artifact.
- Minimal sample and consumer smoke both produce the same snapshot contract.
- Support docs and issue templates now ask for that exact artifact.
- Validation:
  - `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests"`
  - `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -OutputRoot artifacts/consumer-smoke/local-v1.11`
