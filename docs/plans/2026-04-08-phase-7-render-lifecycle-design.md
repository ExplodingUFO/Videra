# Phase 7 Render Lifecycle Safety Design

**Date:** 2026-04-08

## Context

Phase 6 closed the packaging and distribution truth gap. The next remaining reliability problem is runtime-facing rather than packaging-facing:

- [RenderSession](F:/CodeProjects/DotnetCore/Videra/src/Videra.Avalonia/Rendering/RenderSession.cs) mixes render-loop ticks, backend initialization, handle binding, resize, suspend, and dispose through implicit field combinations.
- [VideraEngine](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/VideraEngine.cs), [VideraEngine.Rendering.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/VideraEngine.Rendering.cs), and [VideraEngine.Resources.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/VideraEngine.Resources.cs) currently rely on a lock and a disposed flag, but do not expose a clear lifecycle contract.
- The software backend still leaves [ICommandExecutor.SetDepthState](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs) and `ResetDepthState` as no-ops in [SoftwareCommandExecutor](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs), while [WireframeRenderer](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs) assumes those calls are meaningful.
- `RenderStylePreset.Wireframe` writes `Material.WireframeMode` into style uniforms, but the actual solid versus wireframe pass selection is still driven only by `WireframeRenderer.Mode`.

The user-approved direction for Phase 7 is:

- lifecycle safety first
- disposed objects should prefer harmless `no-op` behavior
- a larger structural cleanup is acceptable
- software backend and wireframe/style contract consistency comes immediately after lifecycle safety

## Goals

- Make `VideraEngine` and `RenderSession` lifecycle states explicit and testable
- Ensure render-loop ticks, handle rebinds, resize, suspend, and dispose cannot race into stale resource access
- Define a single honest contract for disposed and not-ready states
- Make software depth-state APIs real in the software backend
- Make wireframe modes map to real depth semantics
- Remove the current mismatch where wireframe-related style intent exists in uniforms but does not control real render behavior

## Non-Goals

- No Wayland support
- No package or distribution changes
- No broad renderer rewrite that moves all orchestration out of `RenderSession`
- No typed Metal binding replacement in this phase
- No public demo/UX cleanup; that remains Phase 8

## Approaches Considered

### 1. Minimal locking and guard patching

Keep the current structure, add more `_disposed` checks and a few more locks, then patch the software backend depth behavior.

**Pros**

- Smallest patch set
- Lowest short-term merge risk

**Cons**

- Lifecycle remains implicit
- `RenderSession` still depends on field combinations rather than named states
- Hard to reason about future regressions

### 2. Dual lifecycle state machines

Introduce explicit lifecycle states for both `VideraEngine` and `RenderSession`, then build software-depth and wireframe/style contract fixes on top of that stable model.

**Pros**

- Matches the approved direction of a larger structural cleanup
- Gives Phase 7 a stable foundation instead of another patch layer
- Lets tests target state transitions rather than accidental implementation details

**Cons**

- Larger initial refactor
- Requires more new tests before implementation

### 3. Full render coordinator rewrite

Turn `RenderSession` into a more complete render coordinator and reduce `VideraEngine` to a largely passive scene container.

**Pros**

- Could produce the cleanest long-term architecture

**Cons**

- Pulls too much future architecture work into Phase 7
- Risky relative to the current phase goal

## Recommendation

Use approach 2.

It is large enough to actually fix the lifecycle model, but still scoped to the Phase 7 goal. The key principle is to make state transitions explicit before fixing software-depth and wireframe/style behavior.

## Design

## Lifecycle Model

### `VideraEngine`

Add an internal explicit lifecycle state, with these states:

- `Uninitialized`
- `Active`
- `Suspended`
- `Disposed`

Contract:

- `Initialize` transitions `Uninitialized` or `Suspended` to `Active`
- `Suspend` transitions `Active` to `Suspended`
- `Dispose` always wins and transitions to `Disposed`
- after `Disposed`, `Draw`, `Resize`, `AddObject`, `RemoveObject`, `ClearObjects`, and `Suspend` become `no-op`
- `Initialize` against `Disposed` is also `no-op`

This keeps user-facing behavior forgiving while making internal behavior deterministic.

### `RenderSession`

Add an explicit session lifecycle state, with these states:

- `Detached`
- `WaitingForSize`
- `WaitingForHandle`
- `Ready`
- `Faulted`
- `Disposed`

Contract:

- `Attach` records backend intent and advances the session toward `WaitingForSize` or `WaitingForHandle`
- `Resize` only updates cached dimensions until the session can safely initialize
- `BindHandle` can advance a native session from `WaitingForHandle` to `Ready`
- `Suspend` tears down active rendering and returns to a waiting state rather than leaving partially active fields behind
- `Dispose` stops the render loop, clears callbacks, tears down backend/bitmap/engine state exactly once, and forces all later entrypoints to `no-op`
- `RenderOnce` operates only from a stable `Ready` snapshot

The critical change is that backend initialization, render-loop ticks, and handle lifetime are no longer inferred from a mix of `_backend`, `_width`, `_height`, and `HandleState`, but from named states.

## Concurrency and Ownership Rules

- `Dispose` is terminal and dominates all in-flight transitions
- render-loop callbacks must read a stable session snapshot first and stop work if the state changed before drawing
- resource teardown and render operations may still serialize internally, but serialization is now guarded by explicit lifecycle state rather than raw field presence
- `RenderSession` owns render-loop and backend activation state
- `VideraEngine` owns scene/resource orchestration state

## Software Backend Depth Contract

Phase 7 makes the software backend honor the same depth-state API shape that native executors already expose.

Required semantics:

- `SetDepthState(false, false)` means no depth test and no depth writes
- `SetDepthState(true, false)` means depth test only
- `SetDepthState(true, true)` means depth test and depth writes
- `ResetDepthState()` restores the default solid-pass behavior: depth test on, depth writes on

This behavior must apply to point, line, and triangle draw paths in [SoftwareCommandExecutor.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs).

## Wireframe Contract

Phase 7 keeps `WireframeRenderer.Mode` as the low-level pass selector, but defines its depth behavior explicitly:

- `Overlay`: no depth test, no depth writes
- `VisibleOnly`: depth test on, depth writes off
- `AllEdges`: hidden pass uses no depth test and no depth writes, visible pass uses depth test on and depth writes off
- `WireframeOnly`: depth test on, depth writes on, and the solid pass must be skipped

## Style and Wireframe Consistency

The current system has two competing wireframe signals:

- `Material.WireframeMode` in style parameters
- `WireframeRenderer.Mode` in render orchestration

Phase 7 resolves that mismatch with this contract:

- `WireframeRenderer.Mode` remains the explicit pass-level override
- if the explicit wireframe mode is `None` but `StyleService.CurrentParameters.Material.WireframeMode` is `true`, the engine derives an effective wireframe mode of `WireframeOnly`
- therefore `RenderStylePreset.Wireframe` finally affects real rendering behavior even when no explicit wireframe override is set
- explicit non-`None` wireframe mode still wins over style-derived default behavior

This avoids a silent dead uniform while preserving the existing style preset concept.

## Testing Strategy

### Unit-level

- add lifecycle contract tests for disposed `no-op` behavior
- add software depth-state tests for lines and triangles
- add tests for effective wireframe-mode resolution

### Integration-level

- `RenderSession` tests for dispose-during-tick, suspend-before-rebind, and no reactivation after dispose
- `VideraEngine` tests for suspend/resume and post-dispose `no-op`
- software-backend integration tests that inspect rendered output differences between `Overlay`, `VisibleOnly`, `AllEdges`, and `WireframeOnly`

### Repository-level

- only add architecture assertions where helpful
- do not replace behavior tests with grep-only checks

## Execution Split

Phase 7 should be split into three plans:

1. lifecycle state machines and `no-op` contract
2. software depth-state and wireframe mode implementation
3. style-to-wireframe contract and Avalonia-facing consistency

They should execute sequentially, not in parallel.

## Risks

- large lifecycle refactors can accidentally break attach/resize ordering
- disposed `no-op` semantics can hide mistakes if state transitions are not tested carefully
- software-depth fixes may expose hidden assumptions in existing integration tests

## Mitigations

- lock behavior first with failing tests
- keep state enums internal until behavior is validated
- run the existing full repository verification after each plan

## Success Criteria

- `VideraEngine` and `RenderSession` have explicit, test-backed lifecycle states
- disposed entrypoints are harmless `no-op`
- software depth-state APIs are real and exercised by tests
- wireframe mode semantics are implemented consistently across software rendering
- `RenderStylePreset.Wireframe` and actual rendering behavior are no longer disconnected
