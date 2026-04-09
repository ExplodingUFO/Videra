# Selection, Highlight, and Annotation Overlay Design

**Date:** 2026-04-09

## Context

`v1.1` shipped an explicit render-pipeline contract, host-agnostic orchestration, and a first public extensibility surface. That work improved maintainability, but it still needs pressure from a real user-facing feature.

The next step should validate the shipped architecture with a feature that is:

- visible to end users
- meaningful for host applications embedding `VideraView`
- compatible with Videra's current product direction as a viewer/presentation engine rather than a modeling editor
- likely to expose weak spots in the new pass-contributor, frame-hook, diagnostics, and interaction boundaries

Selection, highlight, box selection, and lightweight annotations fit that goal well.

## Goals

- Add object-level selection for `Object3D` instances
- Support single-select, multi-select, and box selection from the start
- Support lightweight text annotations bound to either a scene object or a world-space point
- Keep host applications as the owner of selection and annotation state
- Provide built-in default interaction for `VideraView`
- Split 3D overlay rendering from 2D overlay UI so text/layout logic does not pollute the render core
- Prefer clean architectural boundaries over compatibility; breaking changes are acceptable

## Non-Goals

- No sub-mesh, triangle, or primitive-level picking
- No modeling-editor behavior such as vertex editing, gizmos, or mesh authoring
- No rich annotation templates in the first release
- No plugin/package system redesign in this phase
- No attempt to introduce a heavyweight scene graph beyond the current `Object3D` collection model

## Product Direction

This work should reinforce that Videra is a rendering/viewer library for presentation and motion, not a modeling tool.

That means:

- selection is object-level
- annotations are lightweight
- the default interaction model stays viewer-first
- the design should remain useful for demos, product viewers, simulators, and runtime visualization, not just editor-like workflows

## Design Overview

The recommended design is a four-layer split:

1. `Videra.Core.Selection` for pure picking, box-selection, and annotation-anchor calculations
2. `Videra.Core.Selection.Rendering` for 3D overlay rendering contracts and contributors
3. `Videra.Avalonia.Controls.Interaction` for built-in default interaction behavior and state machines
4. `VideraView` as the public controlled shell that receives host-owned state and emits structured user intent

This is intentionally not a "selection manager owns everything" design. The host application remains the source of truth. Videra computes, visualizes, and emits structured requests.

## Why This Approach

Three broad approaches were considered:

1. Thin viewer shell plus pure core query services and overlay contributors
2. A heavier scene-interaction subsystem that owns selection and annotation state internally
3. Primitive APIs only, with the host expected to implement all interaction

The first approach is preferred because it gives Videra a complete user-facing feature without turning the control into an editor framework or leaving the host to assemble too much by hand.

## Architectural Boundaries

### 1. `Videra.Core.Selection`

This layer should be pure computation and must not depend on Avalonia types.

Responsibilities:

- stable object identity contracts
- object-level hit testing
- box selection
- annotation anchor projection
- ordering and filtering of hit/selection results

Inputs should be limited to:

- `Object3D`
- camera state
- viewport size
- screen-space coordinates or rectangles
- world-space points

This layer should not know about:

- pointer events
- modifier keys
- styled properties
- controls
- text editing UX

### 2. `Videra.Core.Selection.Rendering`

This layer converts host-owned selection and annotation truth into renderable overlay inputs.

Responsibilities:

- selected-object highlight overlay
- hover highlight overlay
- 3D annotation anchor markers and line origins
- translation of overlay state into pass-contributor work

It should not decide what is selected or what annotations exist. It only renders what the host truth says should be shown.

### 3. `Videra.Avalonia.Controls.Interaction`

This shipped under the `Controls/Interaction` namespace and folder so the public interaction types stay colocated with `VideraView`.

This layer interprets Avalonia input into structured requests.

Responsibilities:

- built-in default interaction for `Navigate`, `Select`, and `Annotate`
- pointer-down / drag / release state transitions
- click vs drag threshold handling
- modifier-aware selection requests
- annotation placement requests

This layer should remain thin. It must not become the home of picking algorithms or business-state ownership.

### 4. `VideraView`

`VideraView` remains the public shell.

Responsibilities:

- expose public state inputs such as selection and annotations
- expose public events for user intent
- wire the interaction controller to the core query services
- pass host truth into 3D and 2D overlay rendering
- surface diagnostics and capability state

`VideraView` must not become a god object for selection logic, overlay layout, and input state machines.

## Object Model and Identity

The current core scene model is a collection of `Object3D` instances. That is sufficient for the first release of this feature and aligns with the product direction.

To make controlled selection and annotations reliable, `Object3D` needs a stable public identity.

Recommended change:

- add `Object3D.Id`
- keep it immutable after construction
- prefer `Guid` or a similarly stable opaque identity type

This identity becomes the durable key for:

- selection state
- annotation bindings
- host persistence
- event payloads

The design should not rely on raw object references as long-lived identity.

## Public API Model

The public surface should separate host-owned state from control-emitted intent.

### Controlled State Inputs

Expose public inputs on `VideraView` such as:

- `SelectionState` (`VideraSelectionState`)
- `Annotations`
- `InteractionMode`
- `InteractionOptions`

These represent the truth the control should render and interpret against.

### Structured Intent Outputs

Expose public events such as:

- `SelectionRequested`
- `AnnotationRequested`
- `InteractionDiagnosticsChanged`

These should not mutate host state automatically. They describe what the user asked to do.

### State Ownership Rule

Host applications own:

- which objects are selected
- which object is primary
- which annotations exist
- annotation text changes and persistence

Videra owns:

- hit-test computation
- box-selection computation
- anchor projection
- overlay rendering
- built-in gesture interpretation

## Selection Model

The first release should support object-level selection only.

Suggested semantics:

- single click selects one object
- modifier click adds/removes from selection
- box selection returns a structured ordered result set
- host application decides whether to accept or rewrite the result

Recommended support types:

- `SelectionState` / `VideraSelectionState`
- `SelectionOperation` / `VideraSelectionOperation`
- `SelectionRequest` / `VideraSelectionRequest`
- `SelectionResultCandidate`

The public contract should allow both additive and replacing selection semantics without exposing internal controller details.

## Annotation Model

The first release should support lightweight text annotations only.

Recommended split:

- `NodeAnnotation` bound to `Object3D.Id`
- `WorldPointAnnotation` bound to a `Vector3`

Shared fields:

- `Id`
- `Text`
- `Color`
- `IsVisible`

The content model should remain intentionally small so the architecture can validate anchor, projection, and overlay behavior before richer annotation UI is considered.

## Interaction Model

The built-in behavior should remain viewer-first, not editor-first.

### Public Interaction Modes

Use explicit modes:

- `Navigate`
- `Select`
- `Annotate`

Even in a viewer-oriented control, explicit modes reduce ambiguity between camera movement, selection, and annotation placement.

### Navigate Mode

- left drag rotates camera
- right drag pans
- wheel zooms
- hover feedback may exist, but selection is not committed

### Select Mode

- left click requests single-select
- `Ctrl` / `Shift` + left click requests additive/toggle selection
- left drag above threshold requests box selection
- `Ctrl` / `Shift` + drag requests additive box selection
- right drag and wheel continue to control camera

### Annotate Mode

- left click on object requests `NodeAnnotation`
- left click on empty scene hit point requests `WorldPointAnnotation`
- right drag and wheel continue to control camera
- box selection is disabled in this mode

### Interaction State Machine

The internal controller should stay minimal, with states like:

- `Idle`
- `PointerDownPending`
- `DraggingSelectionBox`
- `PendingAnnotationPlacement`

It should hold only short-lived gesture state, not durable selection or annotation data.

## Core Query Design

### Hit Testing

The first release should use object-level ray-vs-bounds hit testing:

- build a world-space ray from the camera and pointer position
- intersect the ray against each object's `WorldBounds`
- sort by hit distance
- return the nearest match and optional candidate list

This is sufficient for:

- click select
- hover highlight
- object annotation placement

Higher-precision picking can be added later behind the same contract.

### Box Selection

The first release should project object world bounds into screen space:

- project the eight corners of `WorldBounds`
- derive a screen-space rectangle
- compare against the user drag rectangle

Support two query policies from the start:

- `Touch`
- `FullyInside`

`Touch` should be the default because it matches viewer-style expectations.

### Annotation Anchor Projection

Unify both annotation types under a common projected anchor contract.

Recommended internal anchor forms:

- `NodeAnchor(ObjectId)`
- `WorldPointAnchor(Vector3)`

Projection results should include:

- screen position
- visibility
- clip status
- whether overlay rendering should degrade or hide

## Overlay Rendering Design

Selection and annotation overlays should be split into two rendering layers.

### 3D Overlay

Rendered through pass contributors in the engine.

Used for:

- selected-object highlight
- hover highlight
- anchor markers
- 3D line origins

### 2D Overlay

Rendered through the Avalonia overlay layer.

Used for:

- selection rectangle
- annotation text panels
- label positioning
- line endpoints and UI-aligned adorners

This split is important:

- pure 3D rendering is a poor home for text layout and interactive label UI
- pure 2D overlay is a poor home for truthful object highlighting and anchor depth-aware visuals

## Breaking Change Guidance

Breaking changes are acceptable in this feature if they improve separation of concerns.

Allowed:

- reshaping `VideraView` interaction APIs
- extracting controller logic out of current input files
- adding stable identity to `Object3D`
- changing internal view/session/input wiring to support a clean interaction architecture

Not allowed:

- preserving old structure by stuffing new logic into `VideraView`
- adding compatibility shims that keep god-code patterns alive

## Testing Strategy

### Core Unit Tests

Add headless tests for:

- `Object3D.Id` stability
- hit-test ray/bounds ordering
- box-selection rectangle policies
- annotation anchor projection
- object and world-point annotation visibility logic

### Core Integration Tests

Add rendering-oriented tests for:

- selected-object highlight overlay truth
- hover highlight overlay truth
- anchor marker rendering behavior
- annotation overlay state translation

Prefer software backend or deterministic framebuffer assertions where practical.

### Avalonia Interaction Tests

Add interaction tests for:

- mode switching
- single select
- additive select
- box select
- annotation placement on object
- annotation placement on world point
- empty-space behavior

These tests should assert emitted requests and control diagnostics, not internal fields.

### Repository Guards

Add repository tests that lock:

- public API vocabulary
- docs/sample alignment
- localization parity where applicable

### Sample Validation

Do not overload the current extensibility sample.

Add a dedicated interaction-oriented sample that demonstrates:

- host-owned selection state
- host-owned annotations
- default interaction modes
- overlay feedback path

## Implementation Order

The branch shipped in this order:

1. Add stable object identity and pure selection/query contracts in core
2. Add annotation anchor projection and controlled public interaction models
3. Add overlay state plus the 3D/2D overlay split
4. Extract the dedicated Avalonia interaction controller and host bridge
5. Add the focused interaction sample, documentation, and repository guards
6. Run repository-level verification and finalize the plan notes

## Shipped Notes

The final branch structure added a few details that are now part of the intended architecture:

- `SceneBoundsProjector` centralizes screen-space bounds projection for selection and overlay work.
- `VideraView.InteractionHost.cs` separates the host/controller bridge from the main `VideraView` partials.
- `VideraSelectionState`, `VideraSelectionRequest`, and `VideraSelectionOperation` are the authoritative public interaction type names.

## Risks

- `VideraView.Input.cs` is already responsible for camera interaction and will become unmaintainable if new feature logic is appended directly
- object identity must be made explicit before selection and annotations can be trustworthy
- 2D and 3D overlay responsibilities will blur if text, label layout, and highlight rendering are not separated early
- exposing convenience APIs that also own state would move Videra toward editor-like coupling

## Expected Outcome

If implemented this way, Videra gets a user-facing feature that:

- validates the `v1.1` extensibility and orchestration architecture
- stays aligned with Videra as a viewer/presentation library
- increases end-user value without dragging the repo into editor-like complexity
- improves maintainability by splitting algorithms, interaction, and rendering concerns cleanly
