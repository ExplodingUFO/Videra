# Phase 185 Context: Avalonia Dependency Surface Slimming

## Milestone

`v2.13 Package Install Boundary Tightening`

## Requirement

`PIS-02`: Reduce the default dependency/install surface of `Videra.Avalonia` so viewer-only hosts stop carrying importer packages by default, while keeping the work inside package/install-surface reshaping instead of widening runtime or renderer behavior.

## Scope

- Remove direct importer package coupling from `Videra.Avalonia`.
- Keep `LoadModelAsync(...)` / `LoadModelsAsync(...)` on the same Avalonia viewer line, but stop relying on transitive importer availability.
- Add only the smallest explicit importer configuration seam needed for source-tree samples and focused runtime tests.
- Leave package-consumer smoke, docs, support wording, and repository guardrails to later phases.

## Starting Point

- `Videra.Avalonia` currently referenced `Videra.Import.Gltf` and `Videra.Import.Obj` directly.
- `SceneImportService` hard-wired glTF/OBJ importer dispatch.
- Source-tree samples and integration tests assumed importer-backed loading was always available on a plain `VideraView`.
- `v2.13` explicitly deferred package-consumer install proof and docs/guardrail truth to later phases.
