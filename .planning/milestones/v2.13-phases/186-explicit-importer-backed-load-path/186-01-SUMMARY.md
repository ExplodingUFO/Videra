# Phase 186 Summary: Explicit Importer-Backed Load Path

## Outcome

The packaged Avalonia consumer path now opts into importer-backed loading explicitly.

What changed:

- `smoke/Videra.ConsumerSmoke` now directly references `Videra.Import.Obj`.
- `smoke/Videra.ConsumerSmoke` now sets `VideraViewOptions.ModelImporter = static path => ObjModelImporter.Import(path)` before calling `LoadModelAsync(...)`.
- `AlphaConsumerIntegrationTests` now lock that packaged install/use truth.

This phase intentionally did **not** close:

- broader docs/support wording alignment
- package-size/install-surface evidence updates
- repository-wide boundary/guardrail wording cleanup

Those stay for Phase `187`.
