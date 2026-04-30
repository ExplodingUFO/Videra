# Phase 76 Summary 02

- Updated `PickingService.TryResolveMeasurementAnchor(...)` to consume richer hit truth through `hit.WorldPoint` instead of reconstructing object hits from bounds distance.
- Added focused Core tests proving a slanted triangle now resolves the exact surface point rather than the bounding-box entry plane.
- Preserved the viewer-first interaction contract: public selection and annotation flows stay object-level even though the internal hit truth is now more precise.

