# Viewer Inspection Workflow Design

## Goal

Turn `VideraView` into a viewer-first inspection surface by adding a narrow workflow band:

1. typed section/clipping planes
2. lightweight measurements
3. camera/view-state capture and restore
4. truthful snapshot export

The work must stay inside the existing viewer boundary. `VideraEngine` remains the extensibility root. `VideraViewRuntime`, `RenderSession`, and scene/runtime coordinators remain internal seams.

## Scope Boundaries

- No editor-style gizmos
- No transform or modeling tools
- No new public pass-contributor surface
- No compositor-native Wayland work
- No new deep performance milestone

## Chosen Approach

### Clipping

Use a CPU-side clipping path that derives an active mesh payload from a retained source payload and reuses the existing scene residency/upload pipeline.

Why this approach:

- It works across the existing hardware and software backends.
- It pressures the scene document, residency, upload queue, overlay, and diagnostics seams without requiring shader- or backend-specific clipping infrastructure first.
- It keeps clipping on the viewer path instead of leaking engine-level pass configuration into the default consumer API.

Tradeoff:

- CPU-side clipping is not the most efficient long-term implementation for complex scenes.
- It is the smallest truthful implementation that matches the current architecture and alpha scope.

### Measurement

Build measurements as a viewer-first workflow that reuses the current picking and annotation anchor seams. Measurements are public, typed records, but runtime-owned by default.

Why this approach:

- The repo already has host-facing `SelectionState`, `Annotations`, and object/world anchor infrastructure.
- Distance and height-delta measurements fit the current product line without drifting into editor semantics.
- Overlay projection can reuse the same camera/world truth already used for annotation labels.

Tradeoff:

- Current object picking is bounds-based, so measurement anchors will initially inherit that accuracy profile.

### Camera State and Snapshot Export

Add a typed `VideraViewState` capture/restore API on top of the current orbit camera contract, and implement inspection snapshot export through a software render/export path that reuses retained scene truth plus the same overlay projection used on-screen.

Why this approach:

- `FrameAll()`, `ResetCamera()`, and the current diagnostics shell already form the public happy path.
- A typed view-state makes the inspection workflow reproducible for hosts.
- A software export path avoids blocking on native backend readback support while still producing a truthful artifact in alpha.

Tradeoff:

- Snapshot export is not expected to be the fastest path.
- The priority is deterministic, cross-backend correctness.

## Public Contract Shape

### `VideraView`

Add high-level inspection APIs and properties:

- `ClippingPlanes`
- `Measurements`
- `CaptureViewState()`
- `RestoreViewState(...)`
- `ExportSnapshotAsync(...)`

The default story remains:

`Options -> LoadModelAsync -> FrameAll/ResetCamera -> inspection workflow -> BackendDiagnostics`

`Engine`, frame hooks, and pass contributors remain advanced/extensibility features.

### Diagnostics

Diagnostics snapshot/export should include:

- clipping active state and plane count
- measurement count
- last snapshot export path / status

## Internal Structure

### Runtime

Keep `VideraViewRuntime` as shell/orchestrator. Add dedicated inspection services under viewer/runtime or core as needed rather than expanding `VideraViewRuntime.Scene.cs`.

### Core

Add narrow, typed inspection primitives in Core where they express domain truth:

- clip plane and clipped payload helpers
- measurement records/calculators
- view-state record

## Testing Strategy

- TDD for each public contract slice
- Core tests for clipping helpers, measurements, and view-state
- Avalonia/runtime tests for overlay, queue, and export coordination
- integration tests for viewer inspection workflows
- repo/sample guard tests for docs and minimal sample alignment

## Success Criteria

1. Hosts can enable clipping, create measurements, capture/restore view state, and export snapshots without dropping to engine internals.
2. Hardware backends and software fallback present the same inspection truth.
3. Public docs and samples teach a single inspection happy path.
