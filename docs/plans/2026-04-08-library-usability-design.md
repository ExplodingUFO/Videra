# Library Usability Design

**Date:** 2026-04-08

## Context

Videra's rendering core and Avalonia control are already usable, but the public integration story still feels closer to an internal engine than to an embeddable viewer component.

The main friction points are:

- backend selection truth is split across public properties, environment variables, and internal fallback logic
- `VideraView` exposes low-level objects such as `Engine` and `IResourceFactory` as the primary escape path
- common viewer intentions such as "frame all", "reset camera", and "load a model into the scene" are not first-class APIs
- initialization and fallback state are difficult for host applications to observe and explain

This design adds a thin product-facing layer without re-architecting the rendering core.

## Goals

- Make the default Avalonia integration path understandable without requiring direct `VideraEngine` manipulation
- Replace hidden backend behavior with typed configuration and explicit diagnostics
- Add high-level scene and camera APIs that cover common 3D viewer workflows
- Preserve compatibility for existing consumers during the alpha period
- Keep the rendering core and backend implementations largely unchanged

## Non-Goals

- No picking, hit-testing, or selection system in this phase
- No drag-and-drop UX or editor tooling in this phase
- No package/distribution redesign in this phase
- No attempt to hide `VideraEngine` completely from advanced consumers

## Design Overview

The recommended approach is to add a usability facade on top of the current control and session flow:

1. `VideraViewOptions` becomes the primary typed configuration surface
2. `VideraBackendDiagnostics` exposes requested backend, resolved backend, readiness, fallback reason, and last failure
3. `VideraView` adds high-level scene and camera methods for common host-app use cases
4. Existing low-level APIs remain available as advanced escape hatches, but documentation shifts to the new high-level path

## Configuration Model

### New Types

Add the following public types in `src/Videra.Avalonia/Controls`:

- `VideraViewOptions`
- `VideraBackendOptions`
- `VideraInputOptions`
- `VideraDiagnosticsOptions`
- `VideraRenderLoopMode`

### Configuration Rules

`VideraViewOptions` becomes the preferred source of truth for:

- requested backend
- software fallback policy
- render-loop behavior
- environment override policy
- input behavior defaults
- auto-framing after model load

Existing styled properties such as `PreferredBackend`, `IsGridVisible`, `RenderStyle`, and `WireframeMode` remain for compatibility. During this phase:

- existing per-property setters still work
- the new options object is additive, not disruptive
- documentation shifts to the options-first path

### Environment Variable Policy

Environment variables such as `VIDERA_BACKEND` remain supported, but they should no longer act as an always-on hidden control plane.

Introduce an explicit policy in `VideraBackendOptions`, for example:

- `Disabled`
- `AllowOverrides`
- `PreferOverrides`

Default should be `Disabled` for host code. The demo can choose `AllowOverrides` for diagnostics and experimentation.

This makes backend resolution explainable instead of implicit.

## Diagnostics Model

### New Types

Add:

- `VideraBackendDiagnostics`
- `VideraBackendFailure`
- `VideraBackendStatusChangedEventArgs`

### Public Surface

`VideraView` should expose:

- `Diagnostics` or `BackendDiagnostics`
- `event EventHandler<VideraBackendStatusChangedEventArgs>? BackendStatusChanged`
- `event EventHandler<VideraBackendFailure>? InitializationFailed`

### Required Fields

The diagnostics snapshot should include:

- `RequestedBackend`
- `ResolvedBackend`
- `IsReady`
- `IsUsingSoftwareFallback`
- `FallbackReason`
- `NativeHostBound`
- `RenderLoopMode`
- `LastInitializationError`
- `EnvironmentOverrideApplied`

The goal is simple: a host app should be able to render a small status panel without parsing logs.

## High-Level Scene API

### New `VideraView` Methods

Add the following public methods on `VideraView`:

- `Task<ModelLoadResult> LoadModelAsync(string path, CancellationToken cancellationToken = default)`
- `Task<ModelLoadBatchResult> LoadModelsAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)`
- `void AddObject(Object3D obj)`
- `void ReplaceScene(IEnumerable<Object3D> objects)`
- `void ClearScene()`

### Structured Load Results

Loading should return structured results instead of only mutating state or logging:

- `ModelLoadResult`
- `ModelLoadBatchResult`
- `ModelLoadFailure`

These results should contain:

- loaded objects
- failed paths
- exception/error message
- elapsed time
- whether auto-framing ran

This makes host UI and demo UI much easier to build.

### Compatibility

`Items` remains supported, but it should stop being the only "MVVM-friendly" story. The new scene methods give non-MVVM consumers a straightforward path.

## High-Level Camera API

### New Methods

Add the following public methods:

- `void ResetCamera()`
- `bool FrameAll()`
- `bool Frame(Object3D obj)`
- `void SetViewPreset(ViewPreset preset)`

Add `ViewPreset` enum with:

- `Front`
- `Back`
- `Left`
- `Right`
- `Top`
- `Bottom`
- `Isometric`

### Bounds Strategy

To support framing, `Object3D` needs world-space bounds.

Introduce a small geometry type in `src/Videra.Core/Geometry`, for example:

- `BoundingBox3`

Then add:

- cached local bounds derived from mesh vertices
- `Object3D.GetWorldBounds()` or `Object3D.WorldBounds`

`FrameAll()` uses aggregate world bounds across current scene objects. If no objects are present, it returns `false` and does nothing.

### Why This Matters

These APIs map to how users think about a viewer:

- "show everything"
- "focus this object"
- "go back to a known view"

That is much more ergonomic than exposing only `Rotate`, `Zoom`, and `Pan`.

## Render Session Changes

`RenderSession` should stop hiding backend state transitions as implementation detail only.

It should publish enough structured information for `VideraView` to:

- know when initialization was attempted
- know what backend was requested
- know what backend was resolved
- know whether fallback occurred
- know why initialization failed

This does not require large backend changes. It mostly requires moving backend resolution into an explicit request/result model instead of implicit branching plus retries.

## Documentation Changes

After implementation, update:

- `README.md`
- `src/Videra.Avalonia/README.md`
- `samples/Videra.Demo/README.md`

The default public example should move away from:

`view.Engine.AddObject(...)`

and toward:

- `view.Options = ...`
- `await view.LoadModelAsync(...)`
- `view.FrameAll()`

## Rollout Plan

### Phase 1

Typed options and backend diagnostics.

### Phase 2

Scene loading facade and structured load results.

### Phase 3

Camera framing and view presets.

### Phase 4

Demo adoption and documentation cleanup.

## Risks

- Backward compatibility confusion if both old property-based configuration and new options coexist without clear precedence
- Bounds/framing quality may feel inconsistent if mesh bounds are computed too naively
- Diagnostics can become noisy if every retry is surfaced as an error instead of a state transition

## Mitigations

- Define precedence explicitly in tests and docs
- Start with axis-aligned bounds from vertex data only; do not over-engineer bounding volumes
- Distinguish between "attempt", "fallback", and "failure" in diagnostics

## Recommendation

Proceed with this usability layer before doing deeper product features such as picking or advanced interaction customization.

This phase gives the biggest improvement in user experience per unit of implementation risk.
