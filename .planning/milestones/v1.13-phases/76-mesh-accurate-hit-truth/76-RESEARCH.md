# Phase 76 Research

- `SceneHitTestService` is still bounds-based: it ray-tests `Object3D.WorldBounds` and only returns `ObjectId`, object instance, and distance.
- `PickingService.TryResolveMeasurementAnchor(...)` currently derives object-hit anchors from `origin + direction * distance`, so measurement precision inherits the bounds hit, not triangle truth.
- `Object3D` already retains both source and active `MeshPayload` instances, and `CpuMeshRetentionPolicy` explicitly preserves CPU payloads for reupload and picking.
- CPU-side clipping mutates the active payload on `Object3D`, which makes active-payload-based hit testing the narrowest way to keep clipping and picking truth aligned.
- Current public interaction semantics are object-level and host-owned; the safe Phase 76 move is to enrich internal hit truth without changing `VideraSelectionRequest`, `AnnotationRequestedEventArgs`, or `VideraInteractionMode`.
