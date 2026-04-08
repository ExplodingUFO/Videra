# Videra.Demo

[English](README.md) | [中文](../../docs/zh-CN/modules/demo.md)

`Videra.Demo` is the sample Avalonia application used to exercise Videra in a real desktop workflow.

## What It Shows

- `VideraView` integration in an Avalonia window
- Backend-ready initialization through `DemoSceneBootstrapper`
- Automatic default demo cube seeding and framing when the backend becomes ready
- High-level model loading with `LoadModelAsync` / `LoadModelsAsync`
- Scene framing with `FrameAll()` after successful load
- Quick camera utilities with `Frame All` and `Reset Camera`
- Render-style and wireframe switching
- Grid visibility, color, and height controls
- Basic object transform editing
- Backend diagnostics via `BackendDiagnostics`, including readiness, native-host binding, and fallback details

## Runtime Behavior

By default, the demo waits for `VideraView` to finish backend initialization, attaches the importer, and seeds a default demo cube scene through the high-level scene API.

If the default demo cube cannot be created, the demo stays backend-ready, reports the failure in the status area, and keeps model import available. Model import remains available while the status area carries the last scene-bootstrap error.

`PreferredBackend="Auto"` keeps the backend preference aligned with the current platform:

- Windows: Direct3D 11
- Linux: Vulkan
- macOS: Metal

`Program.cs` keeps Windows host options but no longer forces `VIDERA_BACKEND`, so the demo follows the same public backend-selection path described in the repository docs.

For model loading, the demo now calls:

```csharp
var result = await View3D.LoadModelsAsync(paths);
if (result.Succeeded)
{
    View3D.FrameAll();
}

var diagnostics = View3D.BackendDiagnostics;
```

Import results are summarized in the status area. Partial success keeps the loaded models in the scene and reports the last failure message alongside the success count.

`Import Model`, `Frame All`, and `Reset Camera` follow command/capability readiness rather than a raw `IsBackendReady` XAML assumption, so the visible controls stay aligned with the live importer and viewport wiring.

## Run

```bash
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## Validation

Repository-wide validation:

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

Demo-only build check:

```bash
dotnet build samples/Videra.Demo/Videra.Demo.csproj -c Release
```

## Main Files

```text
Videra.Demo/
├── Assets/
├── Converters/
├── Services/
│   ├── AvaloniaModelImporter.cs
│   ├── DemoSceneBootstrapper.cs
│   └── DemoMeshFactory.cs
├── ViewModels/
│   ├── CameraViewModel.cs
│   └── MainWindowViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
├── App.axaml
├── App.axaml.cs
└── Program.cs
```

## Related Docs

- [Repository README](../../README.md)
- [Videra.Avalonia](../../src/Videra.Avalonia/README.md)
- [Chinese Demo Doc](../../docs/zh-CN/modules/demo.md)
