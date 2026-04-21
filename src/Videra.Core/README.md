# Videra.Core

[English](README.md) | [中文](../../docs/zh-CN/modules/videra-core.md)

`Videra.Core` is the platform-agnostic viewer/runtime kernel of Videra. It provides the shared abstractions, scene engine, and software fallback path used by the higher-level packages.

Current status: `alpha`. This package is the right starting point only for core-only consumption. Most desktop applications should start with `Videra.Avalonia` plus a matching `Videra.Platform.*` package.

## Responsibilities

- Rendering abstractions such as `IGraphicsBackend`, `IResourceFactory`, and `ICommandExecutor`
- Scene lifecycle management through `VideraEngine`
- Backend-neutral scene/material runtime contracts through `SceneDocument`, `ImportedSceneAsset`, `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler`
- Camera, grid, axis, and wireframe helpers
- Render-style presets and software fallback rendering

The current shipped viewer/runtime baseline is static glTF/PBR: imported assets can carry UV-backed texture bindings, metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, and tangent-aware mesh data through explicit runtime contracts. Animation, skeletons, and morph targets remain outside this baseline.

The `v1.20` product boundary treats `Core`, `Import`, `Backend`, `UI adapter`, and `Charts` as separate layers. `Videra.Core` is the runtime kernel in that split; use [docs/capability-matrix.md](../../docs/capability-matrix.md) for the explicit layer matrix.

## Key Types

- `VideraEngine`
- `SceneDocument`
- `ImportedSceneAsset`
- `SceneNode`
- `MeshPrimitive`
- `MaterialInstance`
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

When consumed through `Videra.Avalonia`, the control diagnostics mirror the same information through `RenderPipelineProfile`, `LastFrameStageNames`, and `UsesSoftwarePresentationCopy`.

Stable feature vocabulary:

- `Opaque`
- `Transparent`
- `Overlay`
- `Picking`
- `Screenshot`

Public feature truth flows through `RenderCapabilitySnapshot.SupportedFeatureNames`, `RenderPipelineSnapshot.FeatureNames`, `BackendDiagnostics.LastFrameFeatureNames`, and `BackendDiagnostics.SupportedRenderFeatureNames`.

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

Those advanced seams remain on the abstractions for compatibility and test doubles, but the shipped native backends manage shader compilation and resource binding internally and may throw `UnsupportedOperationException` there. This minimum-contract documentation does not imply an `OpenGL` product promise.

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
- For Core-first backend resolution, `GraphicsBackendFactory.ResolveBackend(...)` uses `AllowSoftwareFallback` to choose between a software backend with `FallbackReason` populated and an explicit failure.
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

