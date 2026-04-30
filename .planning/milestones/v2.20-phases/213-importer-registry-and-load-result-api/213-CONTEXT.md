# Phase 213 Context

## Goal

Evolve importer-backed model loading from a single synchronous path delegate into a cancellation-aware importer registry with structured import results, diagnostics, duration, and per-file batch outcomes.

## Constraints

- Keep `Videra.Import.Gltf` and `Videra.Import.Obj` explicit packages; do not add transitive importer dependencies to `Videra.Avalonia`.
- Keep the existing `ModelImporter` delegate usable for current hosts while teaching the registry path as the canonical DX.
- Avoid broad render/import architecture changes.

## Requirements

- `PDX-02` Async importer registry API
- `PDX-03` Load model result and partial-failure diagnostics

