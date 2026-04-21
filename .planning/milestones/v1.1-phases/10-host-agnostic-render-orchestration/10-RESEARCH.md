# Phase 10 Research: Host-Agnostic Render Orchestration

**Date:** 2026-04-08  
**Phase:** 10  
**Requirements:** `PIPE-03`, `MAIN-01`

## Executive Summary

Phase 10 should **not** jump to public extensibility yet. The right next move is to split host-agnostic session orchestration away from Avalonia-specific presentation and view glue, while keeping `VideraEngine` as the owner of frame-plan execution.

Recommended approach:

1. Extract an internal `RenderSessionOrchestrator` plus typed `RenderSessionInputs` / `RenderSessionSnapshot`
2. Shrink `RenderSession` into a runtime shell that hosts render-loop and software-presentation adapters
3. Introduce `VideraViewSessionBridge` so `VideraView` stops being a hidden orchestration root
4. Lock the new boundary with headless integration tests and repository architecture guards

This directly satisfies `PIPE-03` and `MAIN-01` while keeping `EXT-*` work for Phase 11.

## Current Reality

### What Exists Today

- `VideraEngine` now exposes explicit frame-plan truth through `LastPipelineSnapshot`
- `RenderSession` already owns a coherent lifecycle state machine and can render without `VideraView`
- `VideraView` already acts as the outer UI shell and native host surface
- Diagnostics already surface backend truth, display-server truth, and pipeline truth

### Main Gaps

1. `RenderSession` still mixes orchestration with Avalonia-specific presentation concerns such as `WriteableBitmap` and `DispatcherRenderLoopDriver`.
2. `VideraView` still directly coordinates attach / resize / handle / diagnostics refresh in many places, which keeps orchestration knowledge inside the control.
3. There is no headless orchestration type that can be tested without `VideraView`, `NativeControlHost`, or Avalonia bitmap infrastructure.
4. There is no architecture guard preventing `VideraView` from regaining direct orchestration ownership later.

## Design Options

### Option A: Keep current structure and only add more tests

Add extra tests around `RenderSession` and `VideraView`, but do not change internal boundaries.

**Pros**
- Lowest immediate code churn
- Minimal short-term migration risk

**Cons**
- Does not satisfy `PIPE-03`; orchestration is still coupled to presentation/view glue
- Leaves `VideraView` as a hidden coordination root
- Makes Phase 11 public API design start from a still-muddy ownership model

**Verdict**
- Reject

### Option B: Internal orchestration core plus thin adapters

Extract a host-agnostic orchestration core from `RenderSession`, then let `RenderSession` and `VideraView` become thinner adapters.

**Pros**
- Directly addresses `PIPE-03`
- Preserves current public behavior while improving internal ownership
- Keeps `VideraEngine` frame ownership stable
- Creates a clear foundation for Phase 11 public APIs

**Cons**
- Moderate refactor cost
- Needs careful regression coverage to preserve lifecycle and diagnostics truth

**Verdict**
- Recommended

### Option C: Move orchestration and extension model into `Videra.Core` immediately

Pull all session/view orchestration into `Videra.Core` and start designing public extension APIs in the same phase.

**Pros**
- Potentially cleaner end-state sooner
- Fewer intermediate internal seams

**Cons**
- Conflates Phase 10 and Phase 11
- Risks dragging Avalonia/backend composition concerns into the wrong layer
- Higher blast radius for a phase that is supposed to improve maintainability incrementally

**Verdict**
- Reject for Phase 10

## Recommended Architecture

### 1. `RenderSessionOrchestrator`

Introduce an internal orchestration type that owns:

- session lifecycle state
- requested backend + backend options
- current handle/size/render-scale inputs
- backend activation / suspend / rebind / dispose
- last backend resolution
- last initialization error
- display-server diagnostics
- last pipeline snapshot
- a single render entrypoint that runs one frame

Boundary rule:

- `RenderSessionOrchestrator` must not depend on `WriteableBitmap`, `DispatcherTimer`, or `NativeControlHost`

### 2. Typed inputs and snapshot

Introduce:

- `RenderSessionInputs`
- `RenderSessionSnapshot`

The inputs model should make session decisions explicit. The snapshot should expose the current session truth to both `RenderSession` and `VideraViewSessionBridge` without forcing them to inspect internal fields.

### 3. `RenderSession` becomes a runtime shell

After extraction, `RenderSession` should keep:

- `IRenderLoopDriver`
- bitmap creation via `_bitmapFactory`
- software framebuffer copy into the bitmap
- request-render callback
- event emission such as `BackendReady`

It should delegate orchestration state and backend lifecycle decisions to `RenderSessionOrchestrator`.

### 4. `VideraViewSessionBridge`

Introduce a bridge type between `VideraView` and session/orchestrator responsibilities.

The bridge should own:

- backend-options snapshot construction
- attach/resize/handle synchronization
- retry policy for initialization attempts
- diagnostics projection from `RenderSessionSnapshot`

`VideraView` should keep:

- Avalonia property surface
- UI tree / native host visual composition
- pointer input and camera helpers
- scene convenience APIs such as `LoadModelAsync`, `FrameAll`, `ResetCamera`

### 5. `VideraEngine` remains the frame owner

Phase 10 should not move frame-plan ownership out of `VideraEngine`. The point is to decouple session/view orchestration, not to re-litigate Phase 9’s pipeline ownership decision.

## Boundaries To Preserve

- Do not add public `RegisterPass(`, `FrameHook`, `IRenderPassContributor`, or capability-query APIs in this phase
- Do not redesign `IGraphicsBackend` for plugin-style extensibility yet
- Do not change platform truth or backend selection semantics
- Do not break Phase 7’s dispose/no-op contract
- Do not remove or degrade Phase 9 pipeline diagnostics truth

## Verification Strategy

### Headless orchestration tests

Add tests that prove:

- orchestration can progress through waiting / ready / faulted / disposed states without constructing `VideraView`
- software and native-handle activation paths can both be driven through typed inputs
- render-once captures pipeline truth through the orchestrator path
- dispose remains idempotent and blocks backend recreation

### View-bridge integration tests

Add tests that prove:

- `VideraViewSessionBridge` can project diagnostics from the session snapshot
- backend-option changes, handle create/destroy, and size changes flow through the bridge instead of ad hoc view logic
- the existing `VideraView` user-facing helpers still behave after the bridge extraction

### Repository architecture guards

Add guards that prove:

- `RenderSessionOrchestrator`, `RenderSessionInputs`, `RenderSessionSnapshot`, and `VideraViewSessionBridge` exist
- `RenderSessionOrchestrator` does not contain `WriteableBitmap`, `DispatcherTimer`, or `NativeControlHost`
- `VideraView` references the bridge and no longer directly calls low-level `_renderSession.Attach(` / `_renderSession.BindHandle(` / `_renderSession.Resize(` paths
- architecture docs describe the new boundary without overclaiming public extensibility

## Risks and Mitigations

### Risk: extraction becomes superficial and leaves logic duplicated

**Mitigation:** require repository guards that verify low-level session calls have moved out of `VideraView`, not just wrapped in more helper methods.

### Risk: headless orchestration type starts depending on Avalonia types anyway

**Mitigation:** add grep-verifiable architecture tests for forbidden type names in `RenderSessionOrchestrator.cs`.

### Risk: lifecycle regressions from moving state ownership

**Mitigation:** keep Phase 7 contract tests in the verification matrix and require dispose/rebind/render-once no-op coverage in new orchestration tests.

### Risk: phase drifts into public API design

**Mitigation:** explicitly forbid `RegisterPass(`, `FrameHook`, `IRenderPassContributor`, and capability-query public surface in Phase 10 plans.

## Concrete Planning Recommendation

Split Phase 10 into three execution plans:

1. **10-01** — extract `RenderSessionOrchestrator` + typed input/snapshot model and prove it works headlessly
2. **10-02** — add `VideraViewSessionBridge` and slim `VideraView` back to UI-shell responsibilities
3. **10-03** — add regression coverage, architecture docs, and repository guards for the new boundary

This sequence is narrow enough to verify and broad enough to materially improve maintainability before Phase 11.

## Recommendation

Proceed with **Option B**. Phase 10 should establish a testable, host-agnostic orchestration core and a thinner Avalonia shell. That is the cleanest path to later public extensibility without freezing today’s accidental implementation boundaries into long-term API.
