# Phase 76: Mesh-Accurate Hit Truth - Context

**Gathered:** 2026-04-19
**Status:** Ready for planning and execution
**Mode:** Autonomous

## Phase Boundary

Phase 76 replaces bounds-based inspection hits with richer mesh-accurate hit truth while preserving the current public object-level selection story. The phase must improve the internal truth used by picking-dependent inspection workflows without turning `VideraView` into an editor or freezing a new primitive-level public API.

## Implementation Decisions

### Hit truth shape
- **D-01:** Core hit testing should return a richer record containing `ObjectId`, the resolved object instance, world hit point, world normal, primitive index, and hit distance.
- **D-02:** The richer hit result becomes the default truth for measurement and internal inspection resolution paths; public selection requests remain object-level.
- **D-03:** Geometry normals for hit truth should come from the intersected triangle face transformed into world space, not from interpolated vertex normals.

### Mesh accuracy strategy
- **D-04:** Triangle meshes with retained CPU payloads should use mesh-accurate ray intersection.
- **D-05:** Mesh-accurate intersection should be accelerated by a per-mesh local-space AABB tree/BVH built lazily from the active mesh payload.
- **D-06:** The acceleration structure should follow the active mesh payload so clipping-aware payload changes automatically affect inspection truth.

### Compatibility and fallbacks
- **D-07:** Non-triangle meshes or objects without a retained mesh payload should keep the existing object-level fallback behavior instead of failing selection outright.
- **D-08:** `SceneHitTestResult.PrimaryHit.ObjectId` and the current `VideraSelectionRequest` semantics stay stable so host-owned selection does not change shape in this phase.
- **D-09:** Annotation requests continue to emit object anchors on object hits and world-point anchors on misses; this phase improves hit resolution truth but does not widen the public annotation-anchor model.

### the agent's Discretion
- Exact BVH node layout and split heuristic.
- Whether the acceleration cache lives on `MeshPayload` directly or beside it as an internal cache keyed by payload identity.
- Whether diagnostics need a new internal field for precise-vs-fallback hit usage in this phase, provided the public contract stays narrow.

## Specific Ideas

- Reuse the earlier chart-side lesson from Phase 24: shared camera/ray math plus truthful 3D hit data is more valuable than adding new UI affordances.
- Treat `Object3D` active payload as the source of pick truth so CPU-side clipping and future export/support flows all see the same geometry truth.
- Prove the benefit with meshes whose bounds intersection differs materially from the triangle surface, not with axis-aligned quads where bounds and geometry coincide.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone and public contract
- `.planning/ROADMAP.md` — Phase 76 goal, requirements, and milestone-level success criteria for mesh-accurate hit truth.
- `.planning/REQUIREMENTS.md` — `HIT-01`, `HIT-02`, and `HIT-03`.
- `docs/plans/2026-04-18-viewer-inspection-workflow-design.md` — current inspection workflow design, including the admitted bounds-based measurement limitation.
- `README.md` — current public interaction and inspection story that must remain viewer-first and object-level.

### Prior related phases
- `.planning/phases/24-true-3d-picking-and-probe-truth/24-CONTEXT.md` — prior project pattern for moving from approximate to truthful picking.
- `.planning/phases/38-core-interaction-picking-and-selection-services/38-VERIFICATION.md` — current object-level interaction semantics that must remain stable.
- `.planning/RETROSPECTIVE.md` — v1.12 inspection workflow retrospective and the decision to deepen inspection rather than widen product scope.

## Existing Code Insights

### Reusable Assets
- `src/Videra.Core/Selection/SceneHitTestService.cs`: current object-level hit ordering and fallback seam.
- `src/Videra.Core/Picking/PickingService.cs`: central consumer seam for selection, annotation, and measurement hit resolution.
- `src/Videra.Core/Graphics/Object3D.cs`: retained source payload, active payload, world matrix, and clipping-aware mesh mutation seam.
- `src/Videra.Core/Graphics/MeshPayload.cs`: authoritative retained vertex/index data for exact ray/triangle work.

### Established Patterns
- `CpuMeshRetentionPolicy.KeepForReuploadAndPicking` already states that CPU payload retention exists partly for picking truth.
- `VideraInteractionController` keeps object selection public and host-owned; internal hit truth can improve without changing emitted selection shape.
- `VideraViewRuntime` keeps inspection orchestration inside runtime helpers instead of widening `VideraView` or `VideraEngine`.

### Integration Points
- `SceneHitTestResult` is the narrow typed seam that can carry richer hit truth through Core without forcing public `VideraView` API changes.
- `TryResolveMeasurementAnchor(...)` should switch from `origin + direction * distance` to the richer world hit point.
- Existing Core/Avalonia/integration tests already cover selection, annotation, measurement, and inspection workflows and can carry richer assertions.

## Deferred Ideas

- Public primitive-level selection, triangle handles, or editor-style hit affordances.
- GPU/depth-buffer-assisted picking or backend-specific pick acceleration.
- Richer annotation anchor kinds that preserve object identity plus exact surface point.

---

*Phase: 76-mesh-accurate-hit-truth*
*Context gathered: 2026-04-19*
