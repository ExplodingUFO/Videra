# Videra.Core

[English](README.md) | [中文](../../docs/zh-CN/modules/videra-core.md)

`Videra.Core` is the platform-agnostic viewer/runtime kernel of Videra. It provides the shared abstractions, `SceneDocument`-backed imported-asset truth, scene engine, and software fallback path used by the higher-level packages.

Current status: `alpha`. This package is the right starting point only for core-only consumption. Most desktop applications should start with `Videra.Avalonia` plus a matching `Videra.Platform.*` package.

## Responsibilities

- Rendering abstractions such as `IGraphicsBackend`, `IResourceFactory`, and `ICommandExecutor`
- Scene lifecycle management through `VideraEngine`
- Backend-neutral scene/material runtime contracts through `SceneDocument`, `ImportedSceneAsset`, `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler`
- Camera, grid, axis, and wireframe helpers
- Render-style presets and software fallback rendering

The current shipped viewer/runtime baseline is static glTF/PBR with one bounded style-driven broader-lighting baseline on the native static-scene path: imported assets can carry UV-backed texture bindings, per-primitive non-Blend material participation, metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, occlusion texture binding/strength, `KHR_texture_transform` offset/scale/rotation plus texture-coordinate override, and tangent-aware mesh data through explicit runtime contracts. The canonical runtime path may expand one imported entry into multiple internal runtime objects so mixed opaque and transparent primitive participation survives the runtime bridge without widening the public surface into a broader transparency system. The current renderer path consumes baseColor texture sampling, occlusion texture binding/strength, emissive inputs, and normal-map-ready inputs on the bounded static-scene seam, including `KHR_texture_transform` offset/scale/rotation and texture-coordinate override where those bindings request them. This remains a bounded renderer-consumption seam rather than a broader lighting/shader/backend promise. This does not imply an `OpenGL` product promise. Animation, skeletons, morph targets, broader lighting systems beyond the bounded broader-lighting baseline, shadows, environment maps, post-processing, extra UI adapters, and Wayland/OpenGL/WebGL/backend API expansion remain outside this baseline.

Use [docs/capability-matrix.md](../../docs/capability-matrix.md) for the explicit layer matrix.

## Key Types

- `VideraEngine`
- `SceneDocument`
- `ImportedSceneAsset`
- `SceneNode`
- `MeshPrimitive`
- `MaterialInstance`
- `InstanceBatchDescriptor` / `InstanceBatchEntry` for the first same-mesh/same-material instance-batch contract
- `Texture2D`
- `Sampler`
- `Object3D`
- `GraphicsBackendFactory`
- `IGraphicsBackend`
- `IResourceFactory`
- `ICommandExecutor`
- `IGraphicsDevice`
- `IRenderSurface`

## Current Render Pipeline Contract

`VideraEngine` now exposes the current frame shape as read-only contract truth. After a completed draw, `LastPipelineSnapshot` records the effective pipeline profile and executed stage list for the last frame.

Stable stage vocabulary:

- `PrepareFrame`
- `BindSharedFrameState`
- `GridPass`
- `SolidGeometryPass`
- `WireframePass`
- `AxisPass`
- `PresentFrame`

Current profiles:

- `Standard`
- `StandardWithWireframeOverlay`
- `WireframeOnly`

When consumed through `Videra.Avalonia`, the control diagnostics mirror the same information through `RenderPipelineProfile`, `LastFrameStageNames`, `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, `LastFrameTransparentObjectCount`, and `UsesSoftwarePresentationCopy`. Those last-frame counts are backend-neutral scene diagnostics, not draw-call metrics. Renderer-cost fields such as `LastFrameDrawCallCount`, `LastFrameInstanceCount`, `LastFrameVertexCount`, and `PickableObjectCount` are nullable so unsupported backend paths can report `Unavailable` instead of inventing measurements.

Stable feature vocabulary:

- `Opaque`
- `Transparent`
- `Overlay`
- `Picking`
- `Screenshot`

`Transparent` means alpha mask rendering plus deterministic alpha blend ordering for per-primitive carried alpha sources; broader transparency work stays deferred. Public feature truth flows through `RenderCapabilitySnapshot.SupportedFeatureNames`, `RenderPipelineSnapshot.FeatureNames`, `BackendDiagnostics.LastFrameFeatureNames`, `BackendDiagnostics.SupportedRenderFeatureNames`, `BackendDiagnostics.LastFrameObjectCount`, `BackendDiagnostics.LastFrameOpaqueObjectCount`, `BackendDiagnostics.LastFrameTransparentObjectCount`, and `TransparentFeatureStatus`.

The first instance-batch contract is intentionally narrow: `InstanceBatchDescriptor` accepts one `MeshPrimitive`, one matching `MaterialInstance`, per-instance `Matrix4x4` transforms, optional per-instance `RgbaFloat` colors, optional per-instance `Guid` object ids, and a `Pickable` flag. `SceneDocument.AddInstanceBatch(...)` records an `InstanceBatchEntry` with batch-level bounds. Multi-geometry batching, per-instance material overrides, transparent `Blend` material sorting, GPU-driven culling, indirect draw, and ECS-style ownership are outside this contract.

## Built-in Backend Minimum Contract

The shipped native backends (`D3D11`, `Vulkan`, and `Metal`) intentionally share a narrow common contract instead of exposing every backend-specific graphics feature through the public abstractions.

Portable built-in backend expectations:

- `CreateVertexBuffer(...)`, `CreateIndexBuffer(...)`, and `CreateUniformBuffer(...)`
- `CreatePipeline(...)` for the current viewer pipeline shape
- Direct buffer binding through `SetVertexBuffer(...)` and `SetIndexBuffer(...)`
- Draw submission, viewport/scissor control, clear, and best-effort depth-state toggles via `SetDepthState(...)` / `ResetDepthState()`

Non-portable advanced seams:

- `CreateShader(...)`
- `CreateResourceSet(...)`
- `SetResourceSet(...)`

Those advanced seams remain on the abstractions for test doubles, but the shipped native backends manage shader compilation and resource binding internally and may throw `UnsupportedOperationException` there.

Capability truth for those advanced seams is explicit. Query `GetRenderCapabilities()` and check `SupportsShaderCreation`, `SupportsResourceSetCreation`, and `SupportsResourceSetBinding` before calling `CreateShader(...)`, `CreateResourceSet(...)`, or `SetResourceSet(...)`. Backends that do not implement the optional capability-provider interfaces report these advanced flags as `false`.

## Public Extensibility Contract

Phase 11 adds a narrow public extensibility surface in Core:

- `IRenderPassContributor`
- `RegisterPassContributor(...)`
- `ReplacePassContributor(...)`
- `RegisterFrameHook(...)`
- `GetRenderCapabilities()`

The public hook vocabulary is `RenderFrameHookPoint` with `FrameBegin`, `SceneSubmit`, and `FrameEnd`.

Scope boundary:

- `VideraEngine` is the public extensibility root.
- The API is intentionally Core-first and in-process.
- Internal session/orchestration types from `Videra.Avalonia` are not part of the public extension contract.
- Shipped onboarding lives in [docs/extensibility.md](../../docs/extensibility.md) and [samples/Videra.ExtensibilitySample](../../samples/Videra.ExtensibilitySample/README.md).
- Before initialization and after disposal, `GetRenderCapabilities()` remains queryable so host apps can inspect the stable support flags without inferring internal state.
- After the engine is `disposed`, `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, and `RegisterFrameHook(...)` are ignored as a `no-op`.
- For Core-first backend resolution, `GraphicsBackendFactory.ResolveBackend(...)` defaults to explicit failure for unavailable native backends. Set `AllowSoftwareFallback = true` only when a host intentionally wants a software backend with `FallbackReason` populated.
- `SceneDocument` keeps imported assets backend-neutral until a ready resource factory uploads them, and backend recovery restores scene resources from retained scene truth instead of a steady-state software staging path.
- `ImportedSceneAsset.Metrics` and retained deferred mesh state give the runtime enough budget/recovery metadata to queue uploads and rebuild scene resources after backend recreation.
- `package discovery` and `plugin loading` remain out of scope.

## Typical Use

The default public consumer path is `nuget.org`:

```bash
dotnet add package Videra.Core
```

Install `Videra.Core` directly when you need the runtime kernel and backend abstractions without the Avalonia UI layer.

Add `Videra.Import.Gltf` and/or `Videra.Import.Obj` when you need `.gltf` / `.glb` / `.obj` ingestion on the core path without taking `Videra.Avalonia`.

Current `alpha` and contributor `preview` validation can still use `GitHub Packages`, but that feed is not the default public install route:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Core --version 0.1.0-alpha.7 --source github-ExplodingUFO
```

Most desktop applications should start with `Videra.Avalonia` instead.

## Validation

Repository validation entrypoints:

```bash
# Unix shell
./scripts/verify.sh --configuration Release

# PowerShell
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

Core-focused test runs:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release
```

## Requirements

- .NET 8

## Related Docs

- [Repository README](../../README.md)
- [Hosting Boundary](../../docs/hosting-boundary.md)
- [Videra 1.0 Capability Matrix](../../docs/capability-matrix.md)
- [Extensibility Contract](../../docs/extensibility.md)
- [Architecture](../../ARCHITECTURE.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/videra-core.md)

