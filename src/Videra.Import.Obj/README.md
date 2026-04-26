# Videra.Import.Obj

`Videra.Import.Obj` is the dedicated Wavefront OBJ scene import package for Videra viewer/runtime paths.

Current status: `alpha`. Use this package when you need `.obj` ingestion on top of `Videra.Core` without taking the Avalonia UI layer.

## Install

The default public consumer path is `nuget.org`:

```bash
dotnet add package Videra.Import.Obj
```

Current `alpha` and contributor `preview` validation can still use `GitHub Packages`, but that feed is not the default public install route:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Import.Obj --version 0.1.0-alpha.7 --source github-ExplodingUFO
```

Avalonia hosts must install `Videra.Import.Obj` explicitly when they need `.obj` importer-backed `LoadModelAsync(...)` or `LoadModelsAsync(...)` behavior, then opt in through `VideraViewOptions.ModelImporter`. `Videra.Avalonia` does not carry importer packages by default.

## Typical Use

```csharp
using Videra.Import.Obj;
using Videra.Core.Graphics.Software;
using Videra.Core.Scene;

var asset = ObjModelImporter.Import("Models/reference-cube.obj");
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
