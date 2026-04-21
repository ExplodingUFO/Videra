# Phase 45 Research

## Key Context

1. By the end of v1.5, the device/surface seam existed, but the built-in platform backends still mostly relied on `LegacyGraphicsBackendAdapter` to participate in that v2 contract.
2. The migration target for this phase was not a wholesale backend rewrite; it was direct device/surface ownership for built-in backends while keeping compatibility for non-migrated or custom legacy backends.
3. Diagnostics truth had to stay stable throughout the migration so fallback reason, backend identity, and software-copy semantics still projected correctly.
