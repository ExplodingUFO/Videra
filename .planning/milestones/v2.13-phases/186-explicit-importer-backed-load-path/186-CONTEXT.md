# Phase 186 Context: Explicit Importer-Backed Load Path

## Why This Phase Exists

Phase 185 removed importer packages from the default `Videra.Avalonia` dependency surface and introduced `VideraViewOptions.ModelImporter` as the one explicit importer seam.

The remaining install-truth gap is packaged consumer smoke:

- `smoke/Videra.ConsumerSmoke` still depended on `Videra.Avalonia` plus platform packages only.
- its window still called `LoadModelAsync(...)` without explicitly configuring `ModelImporter`.

That left the packaged install story behind the source-tree truth.

## Scope

- add one explicit importer package to packaged consumer smoke
- wire one explicit `ObjModelImporter.Import(...)` callback into `VideraViewOptions.ModelImporter`
- add focused repository guard coverage for that packaged install truth

## Out of Scope

- release/publish workflow changes
- broader docs/support wording alignment
- package-size evidence and repository-wide guardrail cleanup
- new helpers, fallback import discovery, or compatibility adapters
