# Phase 346: Fluent Authoring Placement and Style Affordances

## Bead

`Videra-x9g`

## Outcome

Added narrow placement and inline style affordances to the existing `SceneAuthoringBuilder`; no duplicate builder or runtime object model was introduced.

## Changes

- Added `SceneAuthoringPlacement` as a small Core value type for position, rotation, and scale.
- Added placement overloads for authored mesh, plane, cube, sphere, and repeated instances.
- Added color overloads for common plane/cube/sphere calls that create a matte material named after the authored object.
- Preserved output as `SceneDocument` / `InstanceBatchEntry` truth.

## Verification

Worker verification:

```bash
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SceneAuthoringBuilder"
```

Result: passed, 17/17.

Integration verification:

```bash
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SceneAuthoringBuilder|FullyQualifiedName~ScenePresetTests"
```

Result: passed, 21/21.

## Handoff

Phase 348 can build on this by validating non-finite placement-derived transforms and instance placement input shapes explicitly. Do not add auto-repair or fallback placement behavior.
