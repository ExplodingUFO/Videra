# Extensibility Contract

This page is the English source of truth for Videra's public render extensibility surface. Start with [samples/Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md) when you want a copyable reference, then use this page to confirm the lifecycle and fallback contract.

## Public Entry Points

- `VideraView.Engine` is the public extensibility root.
- `RegisterPassContributor(...)` adds work to a stable pass slot without taking ownership of the full pipeline.
- `RegisterFrameHook(...)` attaches deterministic callbacks at `FrameBegin`, `SceneSubmit`, or `FrameEnd`.
- `RenderCapabilities` exposes the Core-side capability snapshot and last pipeline snapshot.
- `BackendDiagnostics` exposes readiness, resolved backend, fallback state, pipeline diagnostics, and backend-neutral `LastFrameObjectCount` / `LastFrameOpaqueObjectCount` / `LastFrameTransparentObjectCount`.
- `LoadModelAsync(...)` and `FrameAll()` are the narrow sample's scene-loading and framing steps.

## Recommended Flow

The supported public flow is:

1. Create or obtain a `VideraView`.
2. Configure backend options, including whether `AllowSoftwareFallback` is enabled.
3. Use `VideraView.Engine` to call `RegisterPassContributor(...)`.
4. Use `VideraView.Engine` to call `RegisterFrameHook(...)`.
5. Wait for `BackendDiagnostics.IsReady` or `BackendReady` before loading content.
6. Call `LoadModelAsync(...)`.
7. Call `FrameAll()`.
8. Inspect `RenderCapabilities` and `BackendDiagnostics`.

`LoadModelsAsync(...)` follows the same public boundary but with stricter scene semantics: import work can partially succeed, while the active scene is replaced only when every requested file succeeds.

```csharp
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

view.Options = new VideraViewOptions
{
    Backend =
    {
        PreferredBackend = GraphicsBackendPreference.Auto,
        AllowSoftwareFallback = true
    }
};

view.Engine.RegisterPassContributor(RenderPassSlot.SolidGeometry, contributor);
view.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context =>
{
    Console.WriteLine(context.HookPoint);
});

if (view.BackendDiagnostics.IsReady)
{
    var loadResult = await view.LoadModelAsync("Assets/reference-cube.obj");
    if (loadResult.Succeeded)
    {
        view.FrameAll();
    }
}

var capabilities = view.RenderCapabilities;
var diagnostics = view.BackendDiagnostics;
```

The concrete sample lives at `samples/Videra.ExtensibilitySample`, and its main flow is implemented in `samples/Videra.ExtensibilitySample/Views/MainWindow.axaml.cs`.

## Behavior Matrix

| Scenario | Registration contract | Capability and diagnostics contract | Loading and framing guidance |
| --- | --- | --- | --- |
| Ready | `RegisterPassContributor(...)` and `RegisterFrameHook(...)` register normally and participate in the next frame. | `RenderCapabilities.IsInitialized` is `true`. `BackendDiagnostics.IsReady` is `true`, and the backend/profile fields describe the active runtime. `LastFrameObjectCount` / `LastFrameOpaqueObjectCount` / `LastFrameTransparentObjectCount` are backend-neutral scene diagnostics, not draw-call metrics. | `LoadModelAsync(...)` and `FrameAll()` are the expected public flow once the view is ready. |
| Pre-initialization | Registrations are allowed before the first backend-ready frame and are retained until rendering starts. | `RenderCapabilities` is queryable but reports `IsInitialized = false`. `BackendDiagnostics.IsReady` is `false`, with no native fallback reason yet. | Wait for `BackendReady` or `BackendDiagnostics.IsReady` before calling `LoadModelAsync(...)` and `FrameAll()`. |
| `disposed` engine | Additional `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, and `RegisterFrameHook(...)` calls are ignored as a `no-op`. | `RenderCapabilities` remains queryable and reports `IsInitialized = false`. If a frame completed earlier, the last pipeline snapshot can still be retained after disposal. Treat `BackendDiagnostics` as last-known view/session state, not as a reactivation mechanism. | Do not expect new rendering work from the disposed engine or view. Create a new view/engine if you need a fresh session. |
| Backend unavailable | No registration contract changes are added by the render-pipeline diagnostics surface. | Backend availability remains a separate backend-resolution diagnostic. The render-pipeline contract stays limited to contributor/hook support, feature names, frame stages, and last-frame object counts. | Do not call `LoadModelAsync(...)` or `FrameAll()` until the view is ready. |

## Scene Loading Truth

- `SceneDocument` is the viewer-scene contract behind the public loading helpers, and public scene-entry truth is surfaced through `SceneDocumentEntry`, `ModelLoadResult.Entry`, and `ModelLoadBatchResult.Entries`.
- `LoadModelAsync(...)` imports a backend-neutral asset first, then uploads it only when a ready resource factory is available.
- `LoadModelsAsync(...)` uses bounded parallel import and replaces the active scene only when every requested import succeeds.
- `SceneDocumentStore` publishes desired-scene versions, `SceneDeltaPlanner` computes add/remove work plus typed retained-entry changes, and `SceneResidencyRegistry` keeps pending/resident/dirty upload state internal to the runtime.
- `SceneUploadQueue` coalesces upload work by entry id, prefers attached dirty entries during interactive draining, and still drains GPU upload work during the render/session cadence instead of synchronously allocating GPU resources on the public API path.
- Backend rebind restores the scene from retained imported assets and scene objects; it does not rely on a steady-state software staging path.

## Scope Boundaries

- `VideraEngine` is the only public extensibility root.
- `VideraViewRuntime`, `RenderSessionOrchestrator`, `RenderSession`, and `VideraViewSessionBridge` are internal orchestration seams.
- `SceneDocumentStore`, `SceneDeltaPlanner`, `SceneResidencyRegistry`, `SceneUploadQueue`, `SceneUploadCoordinator`, `IGraphicsDevice`, `IRenderSurface`, and `LegacyGraphicsBackendAdapter` are internal contracts; they inform diagnostics and docs truth, not public extension points.
- The public contract is intentionally C#-first and in-process.
- `package discovery` and `plugin loading` remain out of scope.
- Public samples and docs should not rely on internal-only types such as `SoftwareBackend`.

## Related Docs

- [Repository README](../README.md)
- [Architecture](../ARCHITECTURE.md)
- [Videra.Core](../src/Videra.Core/README.md)
- [Videra.Avalonia](../src/Videra.Avalonia/README.md)
- [Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md)
