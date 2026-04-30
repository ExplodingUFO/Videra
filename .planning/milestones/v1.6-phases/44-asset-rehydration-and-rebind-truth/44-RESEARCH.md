# Phase 44 Research

## Key Context

1. Once import and upload were separated, the next risk was rebind truth: runtime still needed a clean way to upload deferred objects through the active resource path without silently treating software resources as the steady state.
2. Backend or surface recreation should not require a fresh import if the runtime already retained imported assets in `SceneDocument`.
3. The existing engine/render-world seam was close, but it needed explicit rehydration support for pre-existing `Object3D` instances.
