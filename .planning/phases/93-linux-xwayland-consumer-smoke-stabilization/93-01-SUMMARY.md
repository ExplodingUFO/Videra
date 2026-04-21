---
requirements_completed:
  - SMOKE-03
---

# Phase 93 Summary 01

- Isolated the actual failing path to `consumer-smoke.yml` / `publish-public.yml` rather than `native-validation.yml`.
- Historical artifact evidence showed the nominal `linux-xwayland` smoke job resolved `display=X11`, not `XWayland`, and emitted no result JSON.
- Added repository guards so the XWayland smoke workflows now have to print `DISPLAY`, `WAYLAND_DISPLAY`, `XDG_RUNTIME_DIR`, and `XDG_SESSION_TYPE` and launch through `bash -lc`.
