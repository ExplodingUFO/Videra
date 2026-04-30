# Phase 78 Summary 02

- Exposed the active render session's software backend through `RenderSession` and taught `VideraViewRuntime.Inspection` to prefer it when snapshot export can reuse the live frame directly.
- Refactored `VideraSnapshotExportService` into explicit “capture pixels” and “save with overlay” steps so the preferred readback path and the existing fallback path share the same final encoding logic.
- Preserved the old deterministic fallback intact: if the live readback path is unavailable or incompatible, export still clones into the software path and writes the same outward artifact contract.
