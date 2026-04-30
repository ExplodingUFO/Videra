---
phase: 11-public-extensibility-apis
completed: 2026-04-08
requirements_completed:
  - EXT-01
  - EXT-02
  - EXT-03
---

# Phase 11 Summary

## Outcome

Phase 11 shipped the first public render-pipeline extensibility surface. `VideraEngine` is now the stable public owner for pass contributors, pass replacement, frame hooks, and runtime capability queries, while `VideraView` exposes matching host-app diagnostics/query truth without leaking internal session seams.

## Delivered Changes

### 11-01: Core public extensibility contract
- Added `IRenderPassContributor`, `RenderPassContributionContext`, `RenderFrameHookPoint`, `RenderFrameHookContext`, and `RenderCapabilitySnapshot` under `Videra.Core.Graphics.RenderPipeline.Extensibility`.
- Extended `VideraEngine` with public `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, `RegisterFrameHook(...)`, and `GetRenderCapabilities()` APIs.
- Reworked frame execution so stable pass slots and lifecycle hook points flow through the new contributor/hook dispatch path while preserving existing default rendering behavior.
- Added core integration coverage for contributor registration, built-in slot replacement, frame-hook ordering, and runtime capability queries.

### 11-02: Avalonia host-app path and diagnostics projection
- Extended `VideraView` with public `RenderCapabilities` so host apps can query pipeline/backend/capability truth through a stable UI-facing surface.
- Extended `VideraBackendDiagnostics` with capability-projection fields that expose whether pass contributors, pass replacement, frame hooks, and pipeline snapshots are available.
- Kept `RenderSession`, `RenderSessionOrchestrator`, and `VideraViewSessionBridge` internal while using them to assemble diagnostics and capability projection.
- Added host-app integration coverage that exercises the new extensibility/query APIs through `VideraView.Engine`, `VideraView.RenderCapabilities`, and `VideraView.BackendDiagnostics`.

### 11-03: Docs, guards, and final regression matrix
- Updated root architecture docs, package READMEs, and Chinese mirrors to document the shipped public extensibility contract instead of treating it as future work.
- Switched repository guards from negative assertions ("do not mention public extensibility") to positive contract assertions that pin the chosen API names and keep internal seams internal.
- Preserved the explicit "no package discovery / no plugin loading" boundary so Phase 11 does not overclaim a future package model.

## Requirements Closed In This Phase

- `EXT-01`: library users can register or replace render-pass contributors through public `VideraEngine` APIs.
- `EXT-02`: library users can register stable frame lifecycle hooks and receive deterministic hook contexts.
- `EXT-03`: host apps can query pipeline/backend/capability/runtime truth through public Core and Avalonia surfaces.

## Key Files

### Runtime / Contracts
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/IRenderPassContributor.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderPassContributionContext.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderFrameHookPoint.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderFrameHookContext.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderCapabilitySnapshot.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`

### Tests / Docs
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineExtensibilityIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewExtensibilityIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSessionBridgeIntegrationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- `ARCHITECTURE.md`
- `src/Videra.Core/README.md`
- `src/Videra.Avalonia/README.md`
- `docs/zh-CN/ARCHITECTURE.md`
- `docs/zh-CN/modules/videra-core.md`
- `docs/zh-CN/modules/videra-avalonia.md`

## Result

Phase 11 closes the gap between the internal render-pipeline contract work in Phases 9-10 and a real public developer surface. The repository now ships, documents, and guards a coherent extensibility/query contract that Phase 12 can build on with samples and compatibility guidance.
