# Phase 347: Primitive and Material Preset Polish

## Bead

`Videra-1wn`

## Outcome

Added two narrow renderer-neutral presets that improve static-scene authoring without widening runtime scope.

## Changes

- Added `SceneMaterials.Emissive(...)` using existing `MaterialEmissive` truth.
- Added `SceneGeometry.BoxOutline(...)` as deterministic line-topology bounds/selection-style geometry.
- Added focused tests for the new material and geometry presets.

## Verification

Worker verification:

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter FullyQualifiedName~ScenePresetTests
```

Result: passed, 4/4.

Integration verification:

```bash
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SceneAuthoringBuilder|FullyQualifiedName~ScenePresetTests"
```

Result: passed, 21/21.

## Handoff

Phase 348 should keep preset failures explicit. `BoxOutline(...)` argument validation throws for invalid dimensions; do not replace that with fallback dimensions.
