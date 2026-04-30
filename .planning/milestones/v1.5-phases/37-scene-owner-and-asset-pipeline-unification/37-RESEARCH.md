# Phase 37 Research

## Key Context

1. Scene truth was previously split between `Items` binding behavior, direct engine object mutation, and import paths that immediately uploaded GPU resources. That made backend rebind and resource recreation harder to reason about.
2. The minimal unification is a backend-neutral `SceneDocument` and `ImportedSceneAsset` path: import once on the CPU side, then upload through a coordinator when runtime resources are available.
3. Because `VideraView` already exposes async model-loading APIs, the migration could happen behind runtime internals as long as scene replacement and clear operations updated both the engine and the new scene document truth.
