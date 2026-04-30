# Phase 77: Viewer-First Snap Modes and Measurement Reproducibility - Context

**Gathered:** 2026-04-19
**Status:** Ready for planning and execution
**Mode:** Autonomous

## Phase Boundary

Phase 77 adds explicit measurement snap modes on top of the richer hit truth from Phase 76. The work must stay viewer-first: no gizmos, no transform handles, no authoring workflow. The phase should make measurement intent more repeatable and ensure inspection-state capture/restore preserves both the current snap mode and the resulting measurements.

## Implementation Decisions

### Public seam
- **D-01:** Keep snap mode on `VideraInteractionOptions.MeasurementSnapMode` so the viewer-first public surface stays grouped under controlled interaction options instead of widening `VideraView`.
- **D-02:** Persist `MeasurementSnapMode` through `VideraInspectionState` so capture/restore preserves the active measurement mode.
- **D-03:** Keep the existing `VideraInteractionMode.Measure` flow; snap mode refines how anchors are chosen, not how the overall interaction model works.

### Snap semantics
- **D-04:** `Free` uses the richer hit truth when an object is hit and falls back to the current camera-facing plane when the click misses scene geometry.
- **D-05:** `Face` uses the exact mesh hit point on object hits and otherwise follows the same miss behavior as `Free`.
- **D-06:** `Vertex` and `EdgeMidpoint` snap only within the resolved hit primitive from Phase 76; this keeps the implementation truthful and local instead of searching the full mesh.
- **D-07:** `AxisLocked` applies only when completing a measurement from an existing first anchor; it constrains the second point to the dominant axis delta from the first anchor and may emit a world-point anchor even if the raw hit started on an object.

### Architecture
- **D-08:** Snap calculations belong in a dedicated Core helper/service consumed by `PickingService`, not inside `VideraInteractionController`.
- **D-09:** Diagnostics do not need a new public field in this phase unless verification proves the missing visibility hurts support or sample clarity.

### the agent's Discretion
- Exact naming and placement of the snap helper/service.
- Whether `Face` and `Free` should currently differ only on miss behavior, as long as the public enum remains stable and truthful.
- Whether created measurements should remember the snap mode that produced them, provided the current mode and resulting anchors already round-trip reproducibly.

## Specific Ideas

- Extend `InteractionSample` just enough to switch snap modes and show the current mode in the measurement summary; keep the richer docs sweep for Phase 79.
- Prefer deterministic geometry-local snaps over more magical mesh-global searches to avoid surprising users and to keep the implementation cheap.
- Use the same slanted-triangle test geometry from Phase 76 to prove the snap modes change the resolved anchor, not just the labels.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone and prior phase
- `.planning/ROADMAP.md` — Phase 77 goal, requirements, and success criteria.
- `.planning/REQUIREMENTS.md` — `SNAP-01`, `SNAP-02`, and `SNAP-03`.
- `.planning/phases/76-mesh-accurate-hit-truth/76-VERIFICATION.md` — the richer hit truth Phase 77 builds on.
- `docs/plans/2026-04-18-viewer-inspection-workflow-design.md` — the existing viewer-first measurement story and non-goals.

### Current public surfaces
- `src/Videra.Avalonia/Controls/VideraView.cs` — current inspection/interaction public property surface.
- `src/Videra.Avalonia/Controls/VideraInspectionState.cs` — current persisted inspection state.
- `samples/Videra.InteractionSample/Views/MainWindow.axaml.cs` — the canonical public interaction sample that should demonstrate snap-mode switching without editor drift.

## Existing Code Insights

### Reusable Assets
- `src/Videra.Core/Picking/PickingService.cs`: already centralizes measurement-anchor resolution and now carries richer mesh hit truth.
- `src/Videra.Avalonia/Controls/Interaction/VideraInteractionController.cs`: owns the two-click measurement workflow but should stay thin.
- `src/Videra.Avalonia/Runtime/VideraViewRuntime.Inspection.cs`: capture/restore seam for persisted inspection state.
- `tests/Videra.Core.Tests/Picking/PickingServiceTests.cs`: existing Phase 76 slanted-triangle tests that can be extended to prove snap behavior.

### Established Patterns
- Inspection workflow state still lives on `VideraView` (`Measurements`, `ClippingPlanes`, inspection-state capture/restore), while snap intent hangs off `InteractionOptions` because it refines interaction behavior rather than stored geometry.
- Overlay and snapshot paths project only the stored measurement anchors, so if the created anchors are reproducible the rest of the pipeline follows.
- Reflection-based public-surface tests in `VideraViewExtensibilityIntegrationTests` and sample configuration tests will need updates because `VideraInteractionOptions` grows a new public property and enum contract.

### Integration Points
- `IVideraInteractionHost` is the narrow bridge between `VideraView` and `VideraInteractionController`; it can carry the active snap mode without widening the rest of the UI surface.
- `VideraInspectionState` and `CaptureInspectionState()/ApplyInspectionState(...)` are the only persistence seam needed to satisfy the restore requirement.
- `InteractionSample` can expose snap-mode switching with a small control/status update instead of a larger UI redesign.

## Deferred Ideas

- Angle/area/volume measurements or any richer analysis tool beyond the current two-point measurement workflow.
- Mesh-global nearest-vertex or nearest-edge snapping across the whole object instead of the resolved hit primitive.
- Editor-style visual snap guides, handles, or drag manipulators.

---

*Phase: 77-viewer-first-snap-modes-and-measurement-reproducibility*
*Context gathered: 2026-04-19*
