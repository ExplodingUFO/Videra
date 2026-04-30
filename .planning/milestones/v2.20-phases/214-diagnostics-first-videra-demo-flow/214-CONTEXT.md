# Phase 214 Context

## Goal

Turn `Videra.Demo` into a support and release-validation tool by adding copyable diagnostics, import reporting, and minimal reproduction metadata around the Phase 213 importer/load result path.

## Scope

- Keep `Videra.Demo` as a focused demo, not a broad workstation shell.
- Add copyable support text only; do not introduce a persistent editor/project format.
- Reuse existing backend diagnostics and render-capability snapshots.
- Preserve the explicit importer package boundary.

## Success Criteria

- Demo can copy a diagnostics bundle with environment, backend, package, import, render-capability, and loaded-model state.
- Demo import report lists each attempted file with success/failure, timing, diagnostics, and asset metrics when available.
- Minimal reproduction metadata captures scene paths, current settings, and diagnostics snapshot.

