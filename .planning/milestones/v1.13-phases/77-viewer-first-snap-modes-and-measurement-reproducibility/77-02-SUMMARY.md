# Phase 77 Summary 02

- Added `VideraMeasurementSnapService` in Core and routed `PickingService` measurement-anchor resolution through it so snapping stays geometry-driven instead of controller-specific.
- Implemented `Free`, `Vertex`, `EdgeMidpoint`, `Face`, and `AxisLocked` over the richer Phase 76 hit truth, with `Vertex` / `EdgeMidpoint` constrained to the resolved primitive and `AxisLocked` constraining the second anchor to the dominant world-axis delta.
- Extended deterministic picking tests to prove the snap modes change resolved anchors on slanted-triangle geometry rather than only changing labels or UI state.
