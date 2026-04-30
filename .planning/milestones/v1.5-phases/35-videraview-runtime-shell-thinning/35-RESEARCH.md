# Phase 35 Research

## Key Context

1. The public shell already exposed stable diagnostics, render-capability, selection, annotation, and scene APIs. The remaining problem was ownership: `VideraView` still directly held session, bridge, overlay, native-host, and input coordination.
2. A thin-shell extraction only works if the shell keeps the same public API while runtime state moves inward. That means `VideraViewRuntime` must own `RenderSession`, `VideraViewSessionBridge`, overlay state, and native-host/input lifecycle coordination without widening public surface.
3. The safest slice was to keep `VideraView` as the forwarding shell, move orchestration to runtime partials, and lock compatibility through `VideraView*IntegrationTests` that read diagnostics, scene, and interaction truth through the public control.
