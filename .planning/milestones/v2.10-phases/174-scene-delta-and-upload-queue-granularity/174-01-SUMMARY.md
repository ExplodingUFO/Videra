# Phase 174 Summary: Scene Delta and Upload Queue Granularity

## Outcome

Scene delta now carries explicit retained-entry change kinds, and scene uploads now coalesce by entry id while prioritizing attached dirty entries on the interactive hot path.

## Completed

- Replaced the coarse retained-entry reupload bucket with `SceneDeltaChangeKind` and `SceneDeltaChange`.
- Changed `SceneUploadQueue` from de-duplicated FIFO into one pending request per entry id with stable-order coalescing.
- Made queue draining prefer attached dirty entries when interactive, while still keeping backend rebind and normal upload work on the same queue path.
- Added focused tests for delta classification, request coalescing, and attached-dirty upload priority.

## Commit

`d03289c feat(174): prioritize coalesced scene uploads`

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~SceneUploadQueueTests|FullyQualifiedName~SceneDeltaPlannerTests|FullyQualifiedName~SceneResidencyRegistryTests|FullyQualifiedName~RuntimeFramePreludeTests"`
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~LoadModelAsync_ValidPath_ReturnsLoadedObjectAndAddsItToScene|FullyQualifiedName~LoadModelAsync_BeforeBackendReady_CreatesDeferredSceneObjectAndRetainsImportedAsset|FullyQualifiedName~DeferredImportedSceneObject_RehydratesWhenGraphicsResourcesBecomeAvailable"`
- `git -C ".worktrees/v2.10-phase174-delta-queue" diff --check`

## Next

Start Phase 175: Primitive-Centric Runtime Truth and Guardrails.
