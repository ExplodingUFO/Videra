# Phase 11 Research: Public Extensibility APIs

**Date:** 2026-04-08  
**Phase:** 11  
**Requirements:** `EXT-01`, `EXT-02`, `EXT-03`

## Executive Summary

Phase 11 should expose **public, typed, low-coupling extension APIs** on top of the pipeline contract from Phase 9 and the orchestration boundaries from Phase 10.

Recommended approach:

1. Add public Core contracts for custom pass contributors, frame lifecycle hooks, and runtime/capability queries
2. Keep `VideraEngine` as the owner of contributor registration, replacement, and hook dispatch
3. Keep Avalonia/session internals private; project runtime/backend truth through existing public surfaces such as `VideraView.BackendDiagnostics`
4. Update repository guards from “do not overclaim extensibility” to “positively enforce the shipped API names and boundaries”

This closes `EXT-01`, `EXT-02`, and `EXT-03` without collapsing the new internal seams back into a leaky API surface.

## Current Reality

### What Exists Today

- `VideraEngine` exposes explicit frame-plan and pipeline snapshot truth
- `RenderSessionOrchestrator` / `RenderSession` / `VideraViewSessionBridge` already separate internal orchestration concerns from the UI shell
- `VideraView.BackendDiagnostics` already exposes backend, display-server, and pipeline-summary truth as public read-only data
- Repository docs and tests currently still treat public pass registration and frame hooks as *not shipped*

### Main Gaps

1. There is no public contributor registry for replacing or inserting render-pass behavior.
2. There is no stable hook vocabulary for frame-begin, scene-submit, or frame-end interception.
3. There is no capability-query API that external consumers can use to inspect current runtime support in a typed way.
4. The current public query story is fragmented across `VideraEngine.LastPipelineSnapshot`, `VideraView.BackendDiagnostics`, and internal session snapshots.

## Design Options

### Option A: Expose current engine internals directly

Make internal engine/session state public or protected, and tell advanced users to subclass/override as needed.

**Pros**
- Lowest immediate implementation effort
- Minimal refactor to ship something quickly

**Cons**
- Breaks the low-coupling goal immediately
- Freezes accidental implementation details into public API
- Encourages consumers to depend on `RenderSession` / Avalonia / internal orchestration state
- Makes future refactors much more expensive

**Verdict**
- Reject

### Option B: Public Core contract layer plus thin runtime projection

Define a typed public extensibility contract in `Videra.Core`, let `VideraEngine` own registration/replacement/hook dispatch, and expose runtime/backend/capability truth through public read-only query surfaces.

**Pros**
- Aligns with the current architecture after Phases 9 and 10
- Keeps ownership clear: pipeline in Core, runtime/backend truth in Avalonia/session diagnostics
- Gives users stable extension points without exposing internal seams
- Preserves future freedom to refactor internals behind the contract

**Cons**
- Moderate design work up front
- Needs deliberate tests and docs to avoid overclaiming

**Verdict**
- Recommended

### Option C: Jump straight to a package/plugin ecosystem

Design a broader plugin discovery/loading model now, with pluggable assemblies and package-driven extension points.

**Pros**
- Potentially flexible long-term architecture
- Could support richer ecosystem scenarios later

**Cons**
- Far beyond current milestone scope
- Conflates stable public API design with deployment/discovery mechanics
- High risk of shipping an overdesigned, under-tested surface

**Verdict**
- Reject for Phase 11

## Recommended Architecture

### 1. Public Core extensibility contracts

Introduce public types in `Videra.Core` for:

- pass contributors
- frame hook points and contexts
- runtime/capability snapshots

These contracts should depend only on stable Core abstractions and read-only state:

- `ICommandExecutor`
- `IResourceFactory`
- pipeline snapshot / frame plan truth
- scene/camera/style data that is already stable enough to expose

They should **not** depend on Avalonia, `RenderSession`, `RenderSessionSnapshot`, `VideraView`, or concrete backend implementations.

### 2. Engine-owned registration and dispatch

`VideraEngine` should be the single public owner of:

- registering additional contributors
- replacing built-in contributor slots
- registering frame lifecycle hooks
- exposing current render/runtime capability information from the Core side

Important boundary:

- public extension APIs should target stable slot/hook identities, not private helper names

Recommended vocabulary split:

- pass slot / stage vocabulary continues to come from the stable pipeline contract
- frame hook vocabulary should use a separate enum such as `FrameBegin`, `SceneSubmit`, `FrameEnd`

This keeps lifecycle hooks distinct from pipeline stages and reduces semantic overload.

### 3. Public query model

Phase 11 should treat “capabilities” as runtime/library capability truth, not full hardware feature discovery.

A practical public query model is:

- `VideraEngine` exposes pipeline + extension/capability truth from the Core side
- `VideraView.BackendDiagnostics` continues to expose backend/session/display-server truth
- Avalonia projection may add capability-related fields, but must derive them from existing session/orchestrator truth or new typed snapshots, not parallel ad hoc state

### 4. Avalonia stays a projection layer

`VideraView` should not become the owner of custom-pass/hook registration logic.

Instead:

- host apps can use `VideraView.Engine` as the extension root
- `VideraView` remains the query/diagnostics shell for backend/runtime truth
- `RenderSessionSnapshot` and `VideraViewSessionBridge` stay internal

This preserves the host-agnostic/Core-first API direction.

## Boundaries To Preserve

- Do not expose `RenderSessionOrchestrator`, `RenderSessionSnapshot`, or `VideraViewSessionBridge` as public extension roots
- Do not move frame ownership out of `VideraEngine`
- Do not re-couple public APIs to Avalonia UI types
- Do not expand Phase 11 into sample/reference onboarding or plugin/package discovery
- Do not claim exhaustive GPU capability discovery if the implementation only provides runtime/library capability truth

## Verification Strategy

### Core extensibility tests

Add tests that prove:

- users can register an additional contributor through public API
- users can replace a built-in contributor slot through public API
- frame hooks run at stable hook points in deterministic order
- pipeline/runtime query snapshots are readable through public API after a completed frame

### Avalonia integration tests

Add tests that prove:

- a host app using `VideraView.Engine` can use the new public extension surface without touching internal session types
- `VideraView.BackendDiagnostics` or its successor continues to expose backend/pipeline truth while adding capability query data
- no new public API requires reflection into `RenderSessionSnapshot`

### Repository guard realignment

Update repository tests so they:

- stop forbidding the new shipped API vocabulary
- positively assert the chosen public type names and docs presence
- continue to forbid overclaiming package/plugin discovery or leaking internal orchestration types as public extension roots

## Risks and Mitigations

### Risk: the new public API leaks internal runtime objects

**Mitigation:** require typed public context/snapshot types and add repository guards that keep `RenderSessionSnapshot` internal-only.

### Risk: contributor and hook vocabularies become muddled

**Mitigation:** separate pass-stage identity from frame-hook identity; do not overload `RenderPipelineStage` for every lifecycle concern.

### Risk: capability query becomes an overclaimed hardware API

**Mitigation:** explicitly scope Phase 11 capabilities to runtime/library truth and document that boundary.

### Risk: docs and tests still reflect the “not shipped” story

**Mitigation:** reserve a dedicated plan for docs/guards/regression so the repository truth flips deliberately, not incidentally.

## Concrete Planning Recommendation

Split Phase 11 into three execution plans:

1. **11-01** — add public Core contributor/hook/query contracts and wire them into `VideraEngine`
2. **11-02** — project the new runtime/capability truth into public host-app usage paths without exposing internal session types
3. **11-03** — update architecture/docs/guards and add the final regression matrix for the new public API

## Recommendation

Proceed with **Option B**. Phase 11 should ship a typed, Core-first extensibility surface that respects the current pipeline and orchestration boundaries. That is the lowest-risk path to better usability without sacrificing maintainability.
