# Phase 9 Research: Render Pipeline Inventory and Contract Extraction

**Date:** 2026-04-08  
**Phase:** 9  
**Requirements:** `PIPE-01`, `PIPE-02`

## Executive Summary

Phase 9 should **not** jump directly to a public plugin API. The current codebase still hides the actual frame contract inside `VideraEngine.Rendering.cs`, while `RenderSession` and `VideraView` mix rendering concerns with host/runtime concerns. The highest-value next step is to extract an explicit engine-level pipeline vocabulary and frame-plan/snapshot model that later phases can consume.

Recommended approach:

1. Introduce an explicit internal render-pipeline contract in `Videra.Core`
2. Route `VideraEngine` through that contract without changing shipped behavior
3. Surface a stable summary of the contract through diagnostics and docs

This directly satisfies `PIPE-01` and `PIPE-02` while keeping `PIPE-03` and the public extension API for later phases.

## Current Reality

### What Exists Today

- `VideraEngine` already has a meaningful but implicit frame sequence:
  - prepare frame
  - bind shared uniforms/state
  - render grid
  - render solid objects
  - render wireframe overlay if active
  - render axis helper
  - end/present frame
- `RenderSession` already acts as the runtime shell around engine draw calls, software copy, backend attach, resize, and render-loop ownership
- `VideraView` already acts as the user-facing orchestration shell around session attach, native host binding, options, and diagnostics
- `VideraBackendDiagnostics` already gives a typed place to surface backend/runtime truth

### Main Gaps

1. The pipeline sequence is real, but it is not modeled as a first-class contract.
2. Diagnostics can tell callers which backend they got, but not which pipeline profile/stages actually ran.
3. The current `Draw()` implementation is still a helper-call script, not a frame-plan object model.
4. There is no contract artifact that later public extensibility APIs can safely build on.

## Design Options

### Option A: Public plugin system immediately

Add public pass registration and frame hooks now.

**Pros**
- Fastest path to user-visible extensibility

**Cons**
- Hard-locks current implicit pipeline as public API before it has been normalized
- High risk of leaking `VideraEngine` implementation detail as long-term package contract
- Mixes `PIPE-*` and `EXT-*` scopes too early

**Verdict**
- Reject for Phase 9

### Option B: Internal pipeline contract first, diagnostics second

Introduce explicit pipeline vocabulary and frame-plan/snapshot types in `Videra.Core`, then let `RenderSession` / `VideraView` consume a stable summary.

**Pros**
- Minimal behavior risk
- Gives future phases a stable foundation
- Keeps external API surface small while still improving explainability
- Maps cleanly to `PIPE-01` and `PIPE-02`

**Cons**
- External developers do not yet get a full extension API in this phase

**Verdict**
- Recommended

### Option C: Full host-agnostic orchestration extraction immediately

Pull orchestration out of both engine and session in one phase.

**Pros**
- Potentially cleaner end-state sooner

**Cons**
- Directly overlaps with Phase 10
- Much larger migration risk
- Makes Phase 9 too broad to verify cleanly

**Verdict**
- Defer to Phase 10

## Recommended Architecture For Phase 9

### 1. Stable stage vocabulary

Define a stable engine-level stage vocabulary in `Videra.Core`, using explicit names instead of current helper names:

- `PrepareFrame`
- `BindSharedFrameState`
- `GridPass`
- `SolidGeometryPass`
- `WireframePass`
- `AxisPass`
- `PresentFrame`

This vocabulary is stable enough for tests, diagnostics, and docs, while still allowing internal helper refactors later.

### 2. Frame-plan and frame-snapshot model

Recommended internal types:

- `RenderPipelineStage`
- `RenderPipelinePassKind` or `RenderPipelineProfile`
- `RenderFramePlan`
- `RenderPipelineSnapshot`

The plan should describe what passes are active for a frame. The snapshot should record what contract is currently active and which stages actually executed.

The important point is not the exact type names, but that `VideraEngine` stops encoding the contract only as ad hoc method order.

### 3. Engine stays the owner in Phase 9

`VideraEngine` should remain the owner of frame sequencing in this phase. The change is to make the sequencing explicit and inspectable, not to move ownership to `RenderSession` or `VideraView`.

That keeps the phase small and avoids stepping on `PIPE-03`.

### 4. Session/view consume summary only

`RenderSession` and `VideraView` should consume a read-only summary of the active pipeline contract, for example:

- pipeline profile
- last frame stage names
- whether software presentation copy happened

This is enough for diagnostics and docs without exposing a premature public customization API.

## Boundaries To Preserve

- Do not redesign `IGraphicsBackend` or `ICommandExecutor` for extensibility in this phase
- Do not move render-loop ownership out of `RenderSession` yet
- Do not turn `VideraView` into a new pipeline coordinator
- Do not add new rendering features while extracting the contract

## Verification Strategy

### Core integration tests

Add integration tests that prove:

- `VideraEngine.Draw()` records the expected stage sequence for a normal frame
- wireframe-enabled frames record `WireframePass`
- wireframe-disabled frames do not claim a `WireframePass`
- the extracted stage vocabulary remains stable

### Session/view diagnostics tests

Add integration tests that prove:

- `RenderSession.RenderOnce()` captures the latest pipeline summary
- `VideraView.BackendDiagnostics` exposes the same pipeline truth
- software presentation and native presentation can be distinguished in diagnostics

### Repository guard tests

Add repository-level assertions that prove:

- contract types exist in `src/Videra.Core/Graphics/RenderPipeline/`
- architecture docs mention the stable stage vocabulary
- docs do not overclaim that custom public pipeline extension is already supported

## Concrete Planning Recommendation

Split the phase into three execution plans:

1. **09-01** — codify pipeline vocabulary and engine-level frame plan/snapshot contract
2. **09-02** — wire session/view diagnostics to the extracted contract
3. **09-03** — document the contract and add repository guards

This sequence is narrow enough to verify and broad enough to materially improve maintainability.

## Risks and Mitigations

### Risk: naming churn leaks into public API too early

**Mitigation:** keep the initial contract types internal or read-only where possible; expose only stable summary data through diagnostics.

### Risk: plan extraction becomes a no-op wrapper around existing code

**Mitigation:** require tests and architecture guards that look for explicit contract types and explicit frame-plan creation/execution paths.

### Risk: phase drifts into Phase 10 or 11

**Mitigation:** explicitly defer host-agnostic orchestration and public extension APIs; treat any such work as future-facing seam design only.

## Recommendation

Proceed with **Option B**. Phase 9 should make the current pipeline explicit, testable, diagnosable, and documentable. That is the cleanest foundation for later decoupling and public extensibility work.
