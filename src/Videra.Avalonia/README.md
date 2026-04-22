# Videra.Avalonia

[English](README.md) | [中文](../../docs/zh-CN/modules/videra-avalonia.md)

`Videra.Avalonia` is the Avalonia integration layer for Videra. It exposes the `VideraView` control, coordinates backend selection, and bridges Avalonia with native host handles on each platform.

Current status: `alpha`. `Videra.Avalonia` is the entry package for Avalonia apps, but it no longer implicitly brings every native backend with it.

## Responsibilities

- Expose the `VideraView` control
- Host the internal `VideraViewRuntime` coordinator behind that public shell
- Connect Avalonia visual-tree lifecycle to backend initialization
- Coordinate backend preference and render-session creation
- Keep runtime coordination internal while `SceneDocument` carries public scene-entry truth through `SceneDocumentEntry`, `ModelLoadResult.Entry`, and `ModelLoadBatchResult.Entries`
- Retain backend-neutral `ImportedSceneAsset` catalogs built from `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler` until upload
- Map pointer input to camera interaction
- Manage native-host integration for Windows, Linux, and macOS

On that shipped viewer path, the current material/runtime baseline is static glTF/PBR: retained imported assets carry UV-backed texture bindings, metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, tangent-aware mesh data, and repeated unchanged imports that can reuse retained imported scene assets while those retained assets stay available. Animation, skeletons, and morph targets remain out of scope for this line.

## Install

The default public consumer path is `nuget.org`. Install the Avalonia entry package and exactly one matching platform package:

```bash
dotnet add package Videra.Avalonia
dotnet add package Videra.Platform.Windows
# or
dotnet add package Videra.Platform.Linux
# or
dotnet add package Videra.Platform.macOS
```

Current `alpha` and contributor `preview` validation can still use `GitHub Packages`, but that feed is not the default public install route:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Avalonia --version 0.1.0-alpha.7 --source github-ExplodingUFO
dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.7 --source github-ExplodingUFO
```

If no matching platform package is installed, the software fallback path can still help with diagnostics, but it does not install missing platform packages.

`PreferredBackend` and `VIDERA_BACKEND` only change backend preference. They do not install missing platform packages and do not replace matching-host native validation.

`Videra.Avalonia` depends on `Videra.Import.Gltf` and `Videra.Import.Obj` transitively so `LoadModelAsync(...)` and `LoadModelsAsync(...)` stay available on the default install path without extra package commands.

## Happy Path

```csharp
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;

View3D.Options = new VideraViewOptions
{
    Backend =
    {
        PreferredBackend = GraphicsBackendPreference.Auto,
        AllowSoftwareFallback = true
    }
};

var result = await View3D.LoadModelAsync("Models/reference-cube.obj");
if (!result.Succeeded && result.Failure is not null)
{
    Console.WriteLine(result.Failure.ErrorMessage);
}

var framed = View3D.FrameAll();
View3D.ResetCamera();

var diagnostics = View3D.BackendDiagnostics;
var diagnosticsSnapshot = VideraDiagnosticsSnapshotFormatter.Format(diagnostics);
```

`LoadModelAsync(...)` is the shortest public first-scene path. `LoadModelsAsync(...)` remains available when a host wants bounded parallel import with atomic replace semantics across a batch, and it replaces the active scene only when every requested file succeeds.

`VideraView.BackendDiagnostics` remains the backend/runtime diagnostics shell, and `VideraDiagnosticsSnapshotFormatter` turns it into the copy-pasteable alpha support artifact used by `Videra.MinimalSample` and `consumer smoke`. `VideraView.RenderCapabilities` and `VideraView.Engine` stay available, but they are not part of the default alpha happy path.

The shared render-feature vocabulary for those diagnostics is `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`, where `Transparent` means alpha mask rendering plus deterministic alpha blend ordering for per-object carried alpha sources. Hosts read that truth through `RenderCapabilities.SupportedFeatureNames`, `BackendDiagnostics.LastFrameFeatureNames`, `BackendDiagnostics.SupportedRenderFeatureNames`, and `TransparentFeatureStatus`.

For the copyable first-scene flow, see [samples/Videra.MinimalSample](../../samples/Videra.MinimalSample/README.md).

## Compatibility and Advanced Entry Points

Compatibility properties such as `PreferredBackend`, `RenderStyle`, `WireframeMode`, `WireframeColor`, and `IsGridVisible` remain available for existing hosts, but they are not the canonical alpha onboarding path. Prefer `View3D.Options = new VideraViewOptions { ... }` for new code.

## Extensibility

`VideraView.Engine` is the public extensibility root for custom contributors and frame hooks.

For the complete advanced flow, see [docs/extensibility.md](../../docs/extensibility.md) and [samples/Videra.ExtensibilitySample](../../samples/Videra.ExtensibilitySample/README.md). The narrow extensibility sample uses `VideraView.Engine`, `RegisterPassContributor(...)`, `RegisterFrameHook(...)`, `LoadModelAsync(...)`, `FrameAll()`, `RenderCapabilities`, and `BackendDiagnostics` together.
For scene-runtime diagnostics, [Videra.Demo](../../samples/Videra.Demo/README.md) includes the narrow `Scene Pipeline Lab` surface for document version, pending/resident upload counts, and backend rebind recovery.

Contract notes:

- After the engine is `disposed`, additional contributor and hook registrations are ignored as a `no-op`.
- `RenderCapabilities` remains queryable before initialization and after disposal.
- With `AllowSoftwareFallback = true`, `BackendDiagnostics.IsUsingSoftwareFallback` and `BackendDiagnostics.FallbackReason` explain native backend fallback.
- With `AllowSoftwareFallback = false`, the view stays not ready until the native backend issue is fixed; it does not silently recover through fallback.
- Scene loading uses retained imported assets and `SceneDocument` truth so backend rebind can restore scene resources without a steady-state software staging path.
- `SceneDocumentStore`, `SceneDeltaPlanner`, `SceneResidencyRegistry`, and `SceneUploadQueue` stay internal to `Videra.Avalonia`; they let `VideraViewRuntime` publish document deltas, queue uploads, and expose read-only scene residency counts through `BackendDiagnostics`.
- The retained asset catalog behind that path is `SceneNode` + `MeshPrimitive` + `MaterialInstance` + `Texture2D` + `Sampler`, surfaced to hosts as viewer/runtime truth rather than backend-specific resources.
- The shipped static glTF/PBR baseline on that path stays viewer-first rather than backend-first: repeated unchanged imports can reuse retained imported scene assets while they remain retained, while animation, skeletons, and morph targets stay deferred.
- `package discovery` and `plugin loading` remain out of scope.

## Interaction Contract

The `host owns` `SelectionState`, `Annotations`, and annotation state.

`VideraView` also supports a controlled interaction flow built around that ownership boundary.

```csharp
using System.Numerics;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Selection.Annotations;

var selectionState = new VideraSelectionState();
IReadOnlyList<VideraAnnotation> annotations = Array.Empty<VideraAnnotation>();

View3D.SelectionState = selectionState;
View3D.Annotations = annotations;
View3D.InteractionMode = VideraInteractionMode.Navigate;

View3D.SelectionRequested += (_, e) =>
{
    selectionState = new VideraSelectionState
    {
        ObjectIds = e.ObjectIds,
        PrimaryObjectId = e.PrimaryObjectId
    };

    View3D.SelectionState = selectionState;
};

View3D.AnnotationRequested += (_, e) =>
{
    annotations = e.Anchor.Kind == AnnotationAnchorKind.Object && e.Anchor.ObjectId is Guid objectId
        ? annotations.Concat(
        [
            new VideraNodeAnnotation
            {
                ObjectId = objectId,
                Text = "Object note"
            }
        ]).ToArray()
        : annotations.Concat(
        [
            new VideraWorldPointAnnotation
            {
                WorldPoint = e.Anchor.WorldPoint ?? Vector3.Zero,
                Text = "World note"
            }
        ]).ToArray();

    View3D.Annotations = annotations;
};
```

Contract notes:

- Built-in modes are `Navigate`, `Select`, `Annotate`, and `Measure`.
- Selection is `object-level`.
- `SelectionRequested` reports intent; the view does not mutate host state for you.
- `AnnotationRequested` supports object anchors and world-point anchors.
- `Measure` writes lightweight viewer-first probes to `VideraView.Measurements`.
- `InteractionOptions.MeasurementSnapMode` keeps snap behavior viewer-first with `Free`, `Vertex`, `EdgeMidpoint`, `Face`, and `AxisLocked`.
- `ClippingPlanes`, `CaptureInspectionState()`, `ApplyInspectionState(...)`, and `ExportSnapshotAsync(...)` stay on the public inspection surface.
- `CaptureInspectionState()` / `ApplyInspectionState(...)` now round-trip the typed inspection session, including host-owned annotations.
- `VideraInspectionBundleService` exports and restores replayable inspection bundles on top of that same typed session contract without widening `VideraView` into a larger project-format surface.
- Overlay responsibilities are split between `3D highlight/render state` and `2D label/feedback rendering`.

For the end-to-end public flow, see [samples/Videra.InteractionSample](../../samples/Videra.InteractionSample/README.md).

## Native Host Coverage

- Windows: child `HWND` for Direct3D 11
- Linux: X11 window for Vulkan, or XWayland compatibility inside Wayland sessions
- macOS: `NSView` for Metal

## Validation

Use the repository verification scripts for standard and native-host validation:

```bash
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

Linux and macOS native-host validation still require explicit opt-in switches and matching hosts.

## Related Docs

- [Repository README](../../README.md)
- [Hosting Boundary](../../docs/hosting-boundary.md)
- [Extensibility Contract](../../docs/extensibility.md)
- [Architecture](../../ARCHITECTURE.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/videra-avalonia.md)

