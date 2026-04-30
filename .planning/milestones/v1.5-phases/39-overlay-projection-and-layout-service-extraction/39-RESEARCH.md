# Phase 39 Research

## Key Context

1. The bridge previously adapted runtime state and also owned 3D-to-2D overlay math. That mixed two different responsibilities: synchronization and geometric projection/layout.
2. Phase 38 already moved interaction semantics into Core services. Overlay projection was the next matching extraction: selection outlines, annotation anchors, and layout/collision math could also move into Core while the bridge stayed an adapter.
3. The migration had to preserve hover, selection, and annotation visuals, which already had integration coverage through `SelectionOverlayIntegrationTests` and `VideraViewSessionBridgeIntegrationTests`.
