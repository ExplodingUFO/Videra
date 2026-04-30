# 212-01 Summary: Package and Install Contract Truth

**Completed:** 2026-04-26
**Commit:** `7039c6f`

## Changes

- Added `PackageDocsContractTests` to lock explicit importer install docs against Avalonia package-reference truth.
- Added a root README "Install by scenario" table for viewer-only, viewer + OBJ, viewer + glTF, SurfaceCharts, and Core-only paths.
- Corrected stale `Videra.Import.Gltf` / `Videra.Import.Obj` README text that claimed transitive Avalonia importer dependencies.
- Corrected matching stale architecture wording.

## Verification

- RED: `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~PackageDocsContractTests"` failed before the docs fix.
- GREEN: same command passed after the docs fix: 3/3 tests passed.
- `git diff --check` reported no whitespace errors.

## Known Residual

- Broader `FullyQualifiedName~Repository` currently fails on unrelated existing drift in capability-matrix and benchmark-threshold repository tests from the prior baseline. Phase 212 did not modify those areas.
