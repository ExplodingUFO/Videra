---
phase: 09-render-pipeline-inventory-and-contract-extraction
completed: 2026-04-08
requirements_completed:
  - PIPE-01
  - PIPE-02
---

# Phase 9 Summary

## Outcome

Phase 9 converted the existing frame path from implicit engine/session flow into an explicit, testable, and documented render-pipeline contract. The runtime now exposes stable stage vocabulary, captures per-frame pipeline truth, and mirrors that truth into Avalonia diagnostics without overclaiming public extensibility.

## Delivered Changes

### 09-01: Engine-level frame plan and pipeline snapshot
- `VideraEngine` now builds an explicit frame plan before rendering and executes it through a stable stage order.
- The extracted stage vocabulary is `PrepareFrame`, `BindSharedFrameState`, `GridPass`, `SolidGeometryPass`, `WireframePass`, `AxisPass`, `PresentFrame`.
- `LastPipelineSnapshot` records the effective profile, stage list, and pass participation for the last completed frame.
- New integration tests lock the behavior for standard, overlay, and wireframe-only paths.

### 09-02: Diagnostics consume pipeline truth
- `RenderSession` now retains the last pipeline snapshot after a successful `RenderOnce`.
- `VideraView.BackendDiagnostics` now mirrors `RenderPipelineProfile`, `LastFrameStageNames`, and `UsesSoftwarePresentationCopy`.
- A small bitmap-factory seam was added so software-presentation diagnostics can be validated in headless integration tests without changing normal runtime behavior.

### 09-03: Architecture/docs truth and repository guards
- Root architecture and `Videra.Core` package docs now describe the same stable stage vocabulary and diagnostics contract as the code.
- The Chinese `Videra.Core` module doc mirrors the same pipeline truth.
- Repository tests now guard both the vocabulary and the boundary that Phase 9 documents observable pipeline truth only, not public custom-pass APIs.

## Requirements Closed In This Phase

- `PIPE-01`: developers can now identify the frame-stage order and consumed diagnostics through explicit contract types, docs, and tests.
- `PIPE-02`: render orchestration is now organized around explicit stage/pass planning instead of remaining hidden inside scattered conditional flow.

## Key Files

### Runtime / Contracts
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineStage.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderFramePlan.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineSnapshot.cs`
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`

### Tests / Docs
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEnginePipelineContractTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- `ARCHITECTURE.md`
- `src/Videra.Core/README.md`
- `docs/zh-CN/modules/videra-core.md`

## Result

Phase 9 establishes a reliable baseline for the next milestone steps. The render pipeline is now observable, named, and guarded, which creates a safer foundation for Phase 10 decoupling and later public extensibility work.
