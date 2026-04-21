# Phase 40 Research

## Key Context

1. The monolithic `IGraphicsBackend` seam bundled device lifetime, render-surface binding, and frame lifecycle into one interface. That was serviceable for one view, but it left no clean internal seam for multi-surface or offscreen work.
2. The milestone strategy explicitly forbade widening public orchestration surface. The migration therefore had to stay internal and compatibility-first: introduce device/surface/frame abstractions, then bridge every existing backend through an adapter.
3. The safest place to consume the new seam was `RenderSessionOrchestrator` and `VideraEngine`. Existing backends stay untouched because `LegacyGraphicsBackendAdapter` turns the old v1 backend contract into the new device/surface split.
