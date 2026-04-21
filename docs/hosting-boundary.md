# Hosting Boundary

This page is the canonical answer to “how do the public viewer packages compose, and which seams stay internal?”

Use [Videra 1.0 Capability Matrix](capability-matrix.md) for the shipped-vs-deferred product scope and [Package Matrix](package-matrix.md) for distribution truth. This page is about ownership boundaries inside the shipped viewer path.

## Canonical Viewer Composition

The normal viewer stack is:

1. `Videra.Core`
2. `Videra.Import.Gltf` and/or `Videra.Import.Obj`
3. `Videra.Avalonia`
4. One matching `Videra.Platform.*` package

`Videra.SurfaceCharts.*` is the shipped public chart package family. It is not part of the `VideraView` hosting/runtime path.

### What each layer owns

| Layer | Public role | Key types |
| --- | --- | --- |
| `Core` | Viewer/runtime kernel | `VideraEngine`, `SceneDocument`, `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, `Sampler`, `IGraphicsBackend`, `IResourceFactory`, `ICommandExecutor` |
| `Import` | CPU-side scene asset ingestion | `GltfModelImporter`, `ObjModelImporter`, `ImportedSceneAsset` |
| `UI adapter` | Avalonia host shell | `VideraView`, `VideraViewOptions`, `BackendDiagnostics`, `RenderCapabilities` |
| `Backend` | Native graphics implementation | matching `Videra.Platform.Windows` / `Linux` / `macOS` backend package |
| `Charts` | Shipped public chart package family | `SurfaceChartView` and `Videra.SurfaceCharts.*` |

## Scene and Material Runtime Model

The shipped viewer path keeps one direct runtime model:

- `Videra.Import.*` parses files into backend-neutral `ImportedSceneAsset` catalogs.
- Those catalogs are composed from `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler`.
- `Videra.Avalonia` retains them in `SceneDocument` while `SceneResidencyRegistry` and `SceneUploadQueue` decide when the active backend can realize them.
- The shipped viewer/runtime baseline on that path is static glTF/PBR: UV-backed texture bindings, metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, tangent-aware mesh data, and repeated unchanged imports that can reuse retained imported scene assets while those retained assets stay available.
- The shared render-feature vocabulary on that path is `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`.
- Host apps observe the result through public diagnostics and capability surfaces rather than through importer-specific or backend-specific types.

That boundary is intentionally narrower than a general 3D runtime. Animation, skeletons, morph targets, and broader advanced-runtime expansion stay out of scope here.

## Internal Seam Owners

The public viewer API is intentionally smaller than the internal runtime/session/native-host graph.

These seams stay internal to `Videra.Avalonia`:

- `VideraViewRuntime`: view-local coordinator
- `VideraViewSessionBridge`: translation between public view state and session/runtime state
- `RenderSession`: render-loop shell and ready/fallback/runtime diagnostics bridge
- `RenderSessionOrchestrator`: backend/device/surface lifecycle owner
- `INativeHostFactory` and `IVideraNativeHost`: native handle creation/binding seam
- `SceneImportService`, `SceneRuntimeCoordinator`, `SceneDocumentStore`, `SceneDeltaPlanner`, `SceneResidencyRegistry`, and `SceneUploadQueue`: scene/runtime coordination and deferred upload path

The product rule is simple: these types can evolve as internal composition seams without becoming public extension roots.

## Public API Rules

- `VideraView` is the public Avalonia shell.
- `LoadModelAsync(...)` and `LoadModelsAsync(...)` stay path-based; they do not expose `Videra.Import.*` types in the public surface.
- `Videra.Avalonia` does not expose native-host handles, `RenderSession`, `RenderSessionOrchestrator`, or `VideraViewRuntime` as public API.
- Import packages stay layered on `Videra.Core`; they do not take dependencies on Avalonia, platform packages, or chart assemblies.
- Backend-specific public API still stops at the existing viewer/runtime abstractions; this boundary does not introduce new raw backend handles or device types.
- Render feature truth stays public through `RenderCapabilities.SupportedFeatureNames`, `BackendDiagnostics.LastFrameFeatureNames`, and `BackendDiagnostics.SupportedRenderFeatureNames` instead of exposing pass-local or backend-local switches.

## Why This Boundary Exists

This split keeps the public viewer path readable and maintainable:

- `Core` stays reusable as the runtime kernel.
- `Import` stays explicit instead of hiding inside `Videra.Core`.
- `Videra.Avalonia` stays the thin host shell instead of becoming a general runtime framework.
- Native-host complexity stays behind internal seams where it belongs.
- `SurfaceCharts` can deepen independently without coupling to `VideraView` hosting/runtime internals.

## Non-Goals

- No new public host-factory API in this phase
- No public `RenderSession` / `VideraViewRuntime` promotion
- No backend-specific public handle/device contract widening
- No attempt to generalize Avalonia hosting into a multi-UI abstraction layer here

## Related Docs

- [README](../README.md)
- [Architecture](../ARCHITECTURE.md)
- [Package Matrix](package-matrix.md)
- [Support Matrix](support-matrix.md)
- [Videra.Avalonia README](../src/Videra.Avalonia/README.md)
- [Videra.Core README](../src/Videra.Core/README.md)
