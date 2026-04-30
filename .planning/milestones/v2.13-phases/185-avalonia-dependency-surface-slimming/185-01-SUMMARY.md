# Phase 185 Summary: Avalonia Dependency Surface Slimming

## Outcome

`Videra.Avalonia` no longer carries `Videra.Import.Gltf` / `Videra.Import.Obj` by default.

The shipped Avalonia viewer line now uses one explicit importer seam:

- `VideraViewOptions.ModelImporter` provides importer-backed loading when a host opts in.
- `SceneImportService` no longer hard-wires importer packages.
- importer-side reuse cache resets when the configured importer changes.

Repository source consumers that actually load models were updated to stay coherent on that explicit path:

- `Videra.MinimalSample`
- `Videra.ExtensibilitySample`
- `Videra.Demo`
- focused Avalonia and integration tests

This phase intentionally did **not** close:

- package-consumer smoke installation
- docs/support wording
- repository guardrails and package-size/install-surface evidence

Those stay for Phases `186-187`.
