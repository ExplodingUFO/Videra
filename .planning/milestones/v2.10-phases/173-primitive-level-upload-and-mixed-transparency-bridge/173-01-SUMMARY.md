# Phase 173 Summary: Primitive-Level Upload and Mixed Transparency Bridge

## Outcome

Imported runtime entries now expand into multiple internal runtime objects, so mixed opaque and transparent primitive participation no longer fails at the runtime bridge.

## Completed

- Added `ImportedSceneRuntimeObjectBuilder` to expand one imported asset into one deferred runtime object per primitive instance.
- Updated `SceneDocumentEntry`, `SceneDocument`, residency state, upload draining, engine attachment, and clipping application to carry multiple runtime objects per imported entry.
- Kept the public single-object convenience path unchanged while moving the Avalonia runtime hot path onto the multi-object imported bridge.
- Added focused repository tests for runtime-object expansion, document flattening, upload of multi-object imported entries, and coordinator-level mixed-alpha entry creation.

## Commit

`95dab82 feat(173): split imported runtime objects by primitive`

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~Object3DTests"`
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~SceneUploadQueueTests|FullyQualifiedName~SceneDocumentTruthTests|FullyQualifiedName~SceneRuntimeCoordinatorTests|FullyQualifiedName~SceneResidencyRegistryTests|FullyQualifiedName~SceneDeltaPlannerTests"`
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~LoadModelAsync_ValidPath_ReturnsLoadedObjectAndAddsItToScene|FullyQualifiedName~LoadModelAsync_BeforeBackendReady_CreatesDeferredSceneObjectAndRetainsImportedAsset|FullyQualifiedName~DeferredImportedSceneObject_RehydratesWhenGraphicsResourcesBecomeAvailable"`
- `git -C ".worktrees/v2.10-phase173-primitive-bridge" diff --check`

## Next

Start Phase 174: Scene Delta and Upload Queue Granularity.
