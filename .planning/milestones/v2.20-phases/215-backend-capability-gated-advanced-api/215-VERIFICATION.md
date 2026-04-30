# Phase 215 Verification

## Commands

- `dotnet test tests\Videra.Core.IntegrationTests\Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngineExtensibilityIntegrationTests|FullyQualifiedName~VideraViewExtensibilityIntegrationTests|FullyQualifiedName~VideraViewSessionBridgeIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests.BackendDiagnostics_ShouldExposeLinuxDisplayServerResolutionFields"` — passed, 24 tests.
- `dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests"` — passed, 1 test.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests|FullyQualifiedName~ExtensibilitySampleConfigurationTests|FullyQualifiedName~DemoConfigurationTests"` — passed, 71 tests.
- `dotnet test tests\Videra.Platform.Windows.Tests\Videra.Platform.Windows.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~D3D11BackendSmokeTests"` — passed, 6 tests.
- `dotnet test tests\Videra.Platform.Linux.Tests\Videra.Platform.Linux.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~VulkanBackendSmokeTests"` — skipped 4 tests on the current Windows host.
- `dotnet test tests\Videra.Platform.macOS.Tests\Videra.Platform.macOS.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~MetalBackendSmokeTests"` — skipped 4 tests on the current Windows host.
- `dotnet build Videra.slnx -c Release` — passed with 0 warnings and 0 errors.
- `git diff --check` — passed; Git reported CRLF normalization warnings only.

## Notes

- A first Core integration test run failed during concurrent restore with `The file already exists`; rerunning the same filter serially passed.
- The implementation is capability reporting only. It does not add fallback behavior or broaden the backend abstraction.

