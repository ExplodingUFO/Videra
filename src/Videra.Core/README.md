# Videra.Core

[English](README.md) | [中文](../../docs/zh-CN/modules/videra-core.md)

`Videra.Core` is the platform-agnostic rendering core of Videra. It provides the shared abstractions, scene engine, import pipeline, and software fallback path used by the higher-level packages.

Current status: `alpha`. This package is the right starting point only for core-only consumption. Most desktop applications should start with `Videra.Avalonia` plus a matching `Videra.Platform.*` package.

## Responsibilities

- Rendering abstractions such as `IGraphicsBackend`, `IResourceFactory`, and `ICommandExecutor`
- Scene lifecycle management through `VideraEngine`
- Camera, grid, axis, and wireframe helpers
- Model import for `.gltf`, `.glb`, and `.obj`
- Render-style presets and software fallback rendering

## Key Types

- `VideraEngine`
- `Object3D`
- `GraphicsBackendFactory`
- `IGraphicsBackend`
- `IResourceFactory`
- `ICommandExecutor`

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
- `package discovery` and `plugin loading` remain out of scope.

## Typical Use

Configure GitHub Packages before installing the package:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Install `Videra.Core` directly when you need the core scene and backend abstractions without the Avalonia UI layer:

```bash
dotnet add package Videra.Core --version 0.1.0-alpha.1 --source github-ExplodingUFO
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
- `SharpGLTF.Toolkit` for model import

## Related Docs

- [Repository README](../../README.md)
- [Extensibility Contract](../../docs/extensibility.md)
- [Architecture](../../ARCHITECTURE.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/videra-core.md)

