# Phase 38 Research

## Key Context

1. By Phase 37 the shell/runtime split and unified scene owner existed, but the interaction controller still carried manipulation and picking semantics directly in Avalonia code.
2. The user-supplied milestone strategy required Avalonia to stay a routing layer. That implied moving the reusable math and semantic decisions into Core services while keeping the existing routed/native pointer shell intact.
3. Existing interaction integration tests already covered selection, additive box select, controlled annotation requests, and keyboard-capable routing helpers, so the safest path was to preserve those observable behaviors while moving the logic inward.
