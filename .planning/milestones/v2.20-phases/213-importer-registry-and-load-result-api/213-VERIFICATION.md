# Phase 213 Verification

## Commands

- `dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~ModelImporterRegistryTests|FullyQualifiedName~SceneImportServiceTests"` — passed, 15 tests.
- `dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj -c Release` — passed, 44 tests.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ModelImporterRegistryTests|FullyQualifiedName~PackageDocsContractTests|FullyQualifiedName~AlphaConsumerIntegrationTests"` — passed, 12 tests.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests.CanonicalViewerPackageStackDocs_ShouldShareSameBoundaryAndInstallStory"` — passed, 1 test.
- `dotnet test tests\Videra.Core.IntegrationTests\Videra.Core.IntegrationTests.csproj -c Release` — passed, 168 passed / 2 skipped.
- `dotnet build Videra.slnx -c Release` — passed with 2 existing SurfaceCharts test warnings.
- `dotnet build smoke\Videra.ConsumerSmoke\Videra.ConsumerSmoke.csproj -c Release --ignore-failed-sources -p:RestoreAdditionalProjectSources=...\artifacts\phase213-smoke-packages-unique -p:VideraConsumerPackageVersion=0.1.0-alpha.7-phase213` — passed; emitted warnings for an existing user-level NuGet source pointing at missing `.worktrees\phase206\artifacts\packages`.
- `git diff --check` — passed; Git reported CRLF normalization warnings only.

## Notes

- A first smoke build attempt without a unique package version resolved cached `0.1.0-alpha.7` packages and failed against stale APIs. Repacked with unique `0.1.0-alpha.7-phase213` packages for compile verification.
- Temporary local package artifacts were removed after verification.

