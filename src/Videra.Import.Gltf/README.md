# Videra.Import.Gltf

`Videra.Import.Gltf` is the dedicated glTF/GLB scene import package for Videra viewer/runtime paths.

Current status: `alpha`. Use this package when you need `.gltf` / `.glb` ingestion on top of `Videra.Core` without taking the Avalonia UI layer.

## Install

The default public consumer path is `nuget.org`:

```bash
dotnet add package Videra.Import.Gltf
```

Current `alpha` and contributor `preview` validation can still use `GitHub Packages`, but that feed is not the default public install route:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Import.Gltf --version 0.1.0-alpha.7 --source github-ExplodingUFO
```

`Videra.Avalonia` already brings this package transitively for `LoadModelAsync(...)` and `LoadModelsAsync(...)`, so Avalonia hosts do not need to add it manually on the default viewer path.

## Typical Use

```csharp
using Videra.Import.Gltf;
using Videra.Core.Graphics.Software;
using Videra.Core.Scene;

var asset = GltfModelImporter.Import("Models/reference-cube.glb");
var sceneObject = SceneUploadCoordinator.Upload(asset, new SoftwareResourceFactory());
```

Use `Import(...)` as the public importer contract. Route runtime upload through `SceneUploadCoordinator` on top of the imported asset when you need an `Object3D`.

## Validation

Repository validation entrypoints:

```bash
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

Focused importer evidence:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ModelImporter"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ModelImporter"
```

## Related Docs

- [Repository README](../../README.md)
- [Videra 1.0 Capability Matrix](../../docs/capability-matrix.md)
- [Package Matrix](../../docs/package-matrix.md)
- [Architecture](../../ARCHITECTURE.md)
