# Phase 77 Research

- `Measurements`, `ClippingPlanes`, and inspection-state capture/restore already live on `VideraView`, so `MeasurementSnapMode` fits better on the inspection surface than inside `VideraInteractionOptions`.
- `PickingService` is already the choke point for measurement-anchor resolution; adding snap semantics there keeps `VideraInteractionController` from becoming another geometry-heavy shell.
- `VideraInspectionState` currently persists camera, selection, clipping, and measurements only; adding snap mode there is the narrowest way to make state restore deterministic.
- `InteractionSample` already has a dedicated measurement section and status text, so a small snap-mode control can land without a broader sample redesign.
