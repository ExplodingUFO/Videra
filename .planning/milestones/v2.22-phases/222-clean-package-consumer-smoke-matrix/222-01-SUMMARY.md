# Phase 222 Summary: Clean Package Consumer Smoke Matrix

**Status:** complete
**Commit:** `502eb01`

## Changed

- Extended `scripts/Invoke-ConsumerSmoke.ps1` with explicit `ViewerOnly`, `ViewerObj`, `ViewerGltf`, and `SurfaceCharts` scenarios.
- Added per-scenario package metadata to consumer smoke reports: scenario, package version, package IDs, model format, project path, and support artifact paths.
- Updated `smoke/Videra.ConsumerSmoke` so `Videra.Import.Obj` and `Videra.Import.Gltf` are conditional package references selected by `VideraConsumerSmokeModelFormat`.
- Added `smoke/Videra.ConsumerSmoke/Assets/reference-triangle.gltf` for the clean glTF consumer path.
- Updated `scripts/Invoke-VideraDoctor.ps1` and Doctor repository tests to use the explicit `ViewerObj` consumer smoke default.
- Added repository guard coverage for the scenario matrix and tracked glTF asset.

## Verification

- Passed targeted repository tests: `AlphaConsumerIntegrationTests` and `VideraDoctorRepositoryTests`.
- Passed build-only smoke matrix for `ViewerOnly`, `ViewerObj`, `ViewerGltf`, and `SurfaceCharts`.
- Passed warnings-as-errors build-only smoke matrix for `ViewerOnly`, `ViewerObj`, `ViewerGltf`, and `SurfaceCharts`.
- Passed full solution build with `-p:RestoreIgnoreFailedSources=true`.

## Known Residuals

- Full graphical smoke execution remains host/session dependent and is still handled by the existing consumer smoke workflows.
- Solution build still reports two pre-existing SurfaceCharts CS0067 warnings.
