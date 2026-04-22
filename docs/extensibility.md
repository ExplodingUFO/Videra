# Extensibility Contract

This page is the English source of truth for Videra's public render extensibility surface. Start with [samples/Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md) when you want a copyable reference, then use this page to confirm the lifecycle and fallback contract.

## Public Entry Points

- `VideraView.Engine` is the public extensibility root.
- `RegisterPassContributor(...)` adds work to a stable pass slot without taking ownership of the full pipeline.
- `RegisterFrameHook(...)` attaches deterministic callbacks at `FrameBegin`, `SceneSubmit`, or `FrameEnd`.
- `RenderCapabilities` exposes the Core-side capability snapshot and last pipeline snapshot.
- `BackendDiagnostics` exposes readiness, resolved backend, fallback state, and pipeline diagnostics.
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
| Ready | `RegisterPassContributor(...)` and `RegisterFrameHook(...)` register normally and participate in the next frame. | `RenderCapabilities.IsInitialized` is `true`. `BackendDiagnostics.IsReady` is `true`, and the backend/profile fields describe the active runtime. | `LoadModelAsync(...)` and `FrameAll()` are the expected public flow once the view is ready. |
| Pre-initialization | Registrations are allowed before the first backend-ready frame and are retained until rendering starts. | `RenderCapabilities` is queryable but reports `IsInitialized = false`. `BackendDiagnostics.IsReady` is `false`, with no native fallback reason yet. | Wait for `BackendReady` or `BackendDiagnostics.IsReady` before calling `LoadModelAsync(...)` and `FrameAll()`. |
| `disposed` engine | Additional `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, and `RegisterFrameHook(...)` calls are ignored as a `no-op`. | `RenderCapabilities` remains queryable and reports `IsInitialized = false`. If a frame completed earlier, the last pipeline snapshot can still be retained after disposal. Treat `BackendDiagnostics` as last-known view/session state, not as a reactivation mechanism. | Do not expect new rendering work from the disposed engine or view. Create a new view/engine if you need a fresh session. |
| Native unavailable with software fallback | Registrations still use the same public surface because the engine runs on the software backend instead of failing the API shape. | With `AllowSoftwareFallback = true`, `BackendDiagnostics.IsUsingSoftwareFallback` is `true`, `ResolvedBackend` becomes `Software`, and `FallbackReason` describes why the native backend could not be used. `RenderCapabilities` continues to expose the same contributor/hook support flags. | The narrow sample can still call `LoadModelAsync(...)` and `FrameAll()` after readiness, but host apps should surface the fallback state to users. |
| Native unavailable without software fallback | No registration contract changes, but no ready session is created. The same unavailable reason is surfaced as a failure instead of a fallback. | With `AllowSoftwareFallback = false`, backend resolution fails instead of populating `FallbackReason` on a software path. In Core-first flows, `GraphicsBackendFactory.ResolveBackend(...)` throws the same unavailable reason. In Avalonia flows, the view stays not ready and surfaces the failure through `InitializationFailed` / `LastInitializationError`. | Do not call `LoadModelAsync(...)` or `FrameAll()` until the missing package/runtime issue is fixed and the view can initialize normally. |

## Scene Loading Truth

- `SceneDocument` is the viewer-scene contract behind the public loading helpers, and public scene-entry truth is surfaced through `SceneDocumentEntry`, `ModelLoadResult.Entry`, and `ModelLoadResult.Entries`.
- `LoadModelAsync(...)` imports a backend-neutral asset first, then uploads it only when a ready resource factory is available.
- `LoadModelsAsync(...)` uses bounded parallel import and replaces the active scene only when every requested import succeeds.
- `SceneDocumentStore` publishes desired-scene versions, `SceneDeltaPlanner` computes add/remove/reupload work, and `SceneResidencyRegistry` keeps pending/resident/dirty upload state internal to the runtime.
- `SceneUploadQueue` drains GPU upload work during the render/session cadence instead of synchronously allocating GPU resources on the public API path.
- Backend rebind and fallback recovery restore the scene from retained imported assets and scene objects; they do not rely on a steady-state software staging path.

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
