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
- Backend diagnostics via `BackendDiagnostics`, including readiness, native-host binding, fallback details, and `LastFrameObjectCount` / `LastFrameOpaqueObjectCount` / `LastFrameTransparentObjectCount`
- A copyable support panel with a diagnostics bundle, per-file import report, and minimal reproduction metadata
- A focused `Scene Pipeline Lab` panel that calls out `SceneDocument` versioning, retained `SceneNode` / `MeshPrimitive` / `MaterialInstance` / `Texture2D` / `Sampler` catalogs, the shipped static glTF/PBR baseline, tangent-aware retained assets, repeated unchanged imports that can reuse retained imported scene assets while they remain retained, deferred upload, render-feature diagnostics, residency counts, atomic scene replacement, and backend-rebind truth
- A focused `Performance Lab` panel for normal-object versus retained instance-batch datasets, pickable toggle coverage, pick latency evidence, retained instance diagnostics, and a copyable support snapshot

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

Import results are summarized in the status area and expanded in the support panel. Batch import uses bounded parallel import, and the active scene is replaced only when every requested file succeeds. If any file fails, the active scene stays unchanged and the status area carries the last failure message alongside the success count. The support panel keeps the last per-file import report, including success/failure state, importer diagnostics, import duration, and asset metrics when available.

`Copy Diagnostics Bundle` copies a support artifact with OS/runtime information, package versions, backend diagnostics, render capabilities, current demo settings, loaded model count, and the latest import report. `Copy Repro Metadata` copies a smaller reproduction snapshot with scene paths, current settings, and the backend diagnostics snapshot.

The `Performance Lab` panel generates bounded datasets at selectable counts. `NormalObjects` creates regular `Object3D` entries with the current resource factory; `InstanceBatch` records one retained `InstanceBatchDescriptor` through `VideraView.AddInstanceBatch(...)`. The panel reports build/frame-time proxy, pick latency, draw-call availability, upload bytes, resident bytes, retained instance count, pickable count, and per-instance pick identity where available. `Copy Snapshot` copies the current lab settings plus the backend diagnostics snapshot for support and release-validation notes.

`Import Model`, `Frame All`, and `Reset Camera` follow command/capability readiness rather than a raw `IsBackendReady` XAML assumption, so the visible controls stay aligned with the live importer and viewport wiring.

The `Scene Pipeline Lab` copy in the side panel is deliberate. It projects three internal truths into a user-visible onboarding surface without turning `Videra.Demo` into another broad workstation shell:

- `SceneDocument` is the runtime scene truth, not a mirror of `Engine.SceneObjects`
- imported assets stay CPU-side until a ready resource factory uploads them through the scene upload queue
- imported scene truth stays backend-neutral as `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler` catalogs
- the retained scene/material runtime baseline is static glTF/PBR: UV-backed baseColor texture bindings plus per-primitive non-Blend material participation, metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, occlusion texture binding/strength, `KHR_texture_transform` offset/scale/rotation plus texture-coordinate override, and tangent-aware retained mesh data
- the current renderer path consumes baseColor texture sampling, occlusion texture binding/strength, emissive inputs, and normal-map-ready inputs on the bounded static-scene seam, including `KHR_texture_transform` offset/scale/rotation and texture-coordinate override where those bindings request them; this remains a bounded renderer-consumption seam rather than a broader lighting/shader/backend promise
- mixed Blend/non-Blend imports remain guarded until transparent primitives are independently sortable
- repeated unchanged imports can reuse retained imported scene assets while they remain retained instead of rebuilding ad hoc importer-shaped state
- backend diagnostics surface `document version`, `pending`, `resident`, `dirty`, and `failed` scene-upload counts
- backend diagnostics also surface `SupportedRenderFeatureNames`, `LastFrameFeatureNames`, `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, `LastFrameTransparentObjectCount`, and `TransparentFeatureStatus` so the public viewer path shows `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot` truth directly, with `Transparent` meaning alpha mask rendering plus deterministic alpha blend ordering for per-primitive carried alpha sources; those counts are backend-neutral scene diagnostics, not draw-call metrics
- backend diagnostics keep backend availability separate from render-pipeline metrics while the scene survives backend recreation
- animation, skeletons, morph targets, lights, shadows, post-processing, extra UI adapters, and Wayland/OpenGL/WebGL/backend API expansion are deliberately outside this demo path

## Run

```bash
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## Validation

Repository-wide validation:

```bash
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
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

