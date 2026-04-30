# Phase 109 Context

## Goal

Make scene/material participation explicit through one render-feature vocabulary that the runtime, pass contributors, frame hooks, and host diagnostics can share directly.

## Constraints

- Keep the contract backend-neutral and viewer-first.
- Do not add compatibility shims, fallback layers, or speculative abstraction seams.
- Keep the bridge explicit: runtime-owned render features, host-owned diagnostics projection.
- Avoid widening backend interfaces or reopening package-boundary work from `v1.20`.

## Relevant Code

- `src/Videra.Core/Graphics/RenderPipeline/RenderFramePlan.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineSnapshot.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderPassContributionContext.cs`
- `src/Videra.Core/Graphics/RenderPipeline/Extensibility/RenderFrameHookContext.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`

## Definition of Done

- A stable `RenderFeatureSet` exists for `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`.
- Runtime snapshots and extensibility contexts expose active features directly.
- Host diagnostics expose supported and last-frame feature names without adding adapter layers.
- Repository integration tests lock the vocabulary and bridge truth in place.
