# Selection, Highlight, and Annotation Overlay Implementation Notes

**Date:** 2026-04-09

## Goal

Ship object-level selection, box selection, lightweight annotations, and split overlay rendering for `VideraView` while keeping durable state owned by the host application.

## Shipped Architecture

### Core Selection And Projection

The feature landed as pure scene-query and projection services under `src/Videra.Core/Selection`.

Key shipped pieces:

- `Object3D.Id` provides a durable public object identity.
- `SceneHitTestService` and `SceneBoxSelectionService` provide object-level hit testing and box selection.
- `SceneBoundsProjector` centralizes screen-space bounds projection used by selection queries.
- `AnnotationAnchorProjector` resolves object and world-point anchors into projected overlay coordinates.

Annotation contracts live under `src/Videra.Core/Selection/Annotations` and overlay render-state models live under `src/Videra.Core/Selection/Rendering`.

### Avalonia Public Interaction Surface

The public controlled-state and intent surface shipped under `src/Videra.Avalonia/Controls/Interaction`.

Key public types:

- `VideraInteractionMode`
- `VideraInteractionOptions`
- `VideraSelectionState`
- `VideraSelectionRequest`
- `VideraSelectionOperation`
- `VideraAnnotation`
- `VideraNodeAnnotation`
- `VideraWorldPointAnnotation`
- `SelectionRequestedEventArgs`
- `AnnotationRequestedEventArgs`
- `VideraInteractionDiagnostics`
- `VideraEmptySpaceSelectionBehavior`

`VideraView` exposes these models as the public host-facing contract, and `VideraView.InteractionHost.cs` keeps the controller wiring separate from the rest of the control surface.

### Overlay Split

The shipped overlay split follows the design intent:

- `SelectionOverlayContributor` and `AnnotationAnchorOverlayContributor` render 3D overlay truth inside the engine.
- `VideraView.Overlay.cs` and `VideraViewOverlayState.cs` translate projected selection and annotation data into the Avalonia 2D overlay layer.

This keeps 3D highlight truth in the render pipeline while leaving label/layout concerns in the UI layer.

### Interaction Controller

Viewer-style gesture handling shipped through `VideraInteractionController` and related resolver/router helpers under `src/Videra.Avalonia/Controls/Interaction`.

The final structure keeps:

- camera gestures forwarded through `VideraView.Input.cs`
- short-lived pointer state in the controller
- selection and annotation intent resolution in dedicated helper types
- durable selection and annotation truth on host-owned public properties

## Final Structure Versus Original Draft

The shipped branch differs from the earlier draft in a few places that matter for future maintenance:

- interaction code lives in `src/Videra.Avalonia/Controls/Interaction`, not a separate `src/Videra.Avalonia/Interaction` root
- selection request and operation types were added to support the public intent contract cleanly
- `SceneBoundsProjector` was introduced so screen-space projection logic is shared instead of duplicated
- `VideraView.InteractionHost.cs` was added to keep the host/controller bridge out of the main control file

These are the paths and names future changes should treat as authoritative.

## Validation Coverage

The shipped work is covered by focused core and integration tests including:

- object identity, hit testing, box selection, and annotation projection
- overlay contributor and `VideraView` integration behavior
- public interaction contract and sample/documentation repository guards

Repository-level validation also runs through `verify.ps1` so build, tests, and sample/demo compilation remain part of the release check.

## Shipped Outcome

This branch now records the selection-overlay work as a delivered feature set rather than an execution checklist. The useful long-lived notes are:

- host applications own durable selection and annotation state
- Videra computes queries, projects anchors, renders overlays, and emits structured user intent
- 3D overlay rendering and 2D overlay UI stay intentionally split
- the final public API and file layout are the ones listed above
