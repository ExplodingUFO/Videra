# Phase 41 Research

## Key Context

1. By Phase 40 the new device/surface seam existed, but `VideraEngine` still carried scene, pass registration, resource lifetime, and frame-state responsibilities directly on one large type.
2. The milestone strategy required zero public extensibility change. That means the engine can only split inward: keep `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, `RegisterFrameHook(...)`, capability snapshots, and pipeline snapshots stable while delegating the work to helpers.
3. Existing integration and repository architecture tests already assert the public contract. The decomposition therefore had to be judged by whether those tests still passed after introducing internal helpers.
