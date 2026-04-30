# Phase 213 Summary

## Completed

- Added Core importer contracts in `Videra.Core.Scene`.
- Added `ObjModelImporter.Create()` and `GltfModelImporter.Create()` registry adapters.
- Added `VideraViewOptions.ModelImporters` and fluent `UseModelImporter(...)`.
- Updated Avalonia import runtime to use registered importers, preserve the legacy delegate path, and return diagnostics/import duration.
- Added `ModelLoadFileResult` and per-file batch results for partial failure visibility.
- Updated root/package/Avalonia/import docs plus samples and consumer smoke to use the registry path.
- Added focused tests for registry options, importer selection, no-match failure, OBJ registry adapter results, and indexed batch results.

## Commit

- `04dda78 api: add registered model importers`

