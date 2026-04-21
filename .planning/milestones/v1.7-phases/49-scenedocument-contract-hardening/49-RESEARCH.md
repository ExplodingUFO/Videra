# Phase 49 Research

## Key Context

1. The scene pipeline could not become delta-driven until document entries had stable identity, versioning, and explicit ownership semantics.
2. The right boundary was to keep `SceneDocument` immutable and internalize entry identity rather than expanding the public scene API.
3. Runtime-owned imported objects and external objects needed different lifetime rules, especially for removal and rehydration paths.
