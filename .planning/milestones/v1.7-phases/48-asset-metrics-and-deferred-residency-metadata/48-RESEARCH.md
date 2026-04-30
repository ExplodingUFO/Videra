# Phase 48 Research

## Key Context

1. The upload queue and residency budget needed object-level byte estimates before they could become explicit runtime policy.
2. Imported assets were already backend-neutral, so metrics belonged on `ImportedSceneAsset` rather than in Avalonia runtime glue.
3. Deferred/recreate readiness had to stay internal to `Object3D` so the public viewer and engine contracts did not widen.
