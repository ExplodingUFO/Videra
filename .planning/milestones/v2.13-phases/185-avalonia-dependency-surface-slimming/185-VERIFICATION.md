# Phase 185 Verification

## Commands

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release -m:1 --filter "FullyQualifiedName~SceneImportServiceTests|FullyQualifiedName~SceneRuntimeCoordinatorTests"`
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release -m:1 --no-restore --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraInspectionBundleIntegrationTests"`
- `dotnet build samples/Videra.MinimalSample/Videra.MinimalSample.csproj -c Release`
- `dotnet build samples/Videra.ExtensibilitySample/Videra.ExtensibilitySample.csproj -c Release`
- `dotnet build samples/Videra.Demo/Videra.Demo.csproj -c Release`
- `git diff --check`

## Result

PASS
