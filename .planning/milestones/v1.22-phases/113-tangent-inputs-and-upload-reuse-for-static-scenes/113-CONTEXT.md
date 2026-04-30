# Phase 113: Tangent Inputs and Upload Reuse for Static Scenes - Context

**Gathered:** 2026-04-21  
**Status:** Ready for planning

## Phase Boundary

Make tangent-aware shading inputs and repeated static-scene reuse explicit on the shipped importer/runtime path. Keep the phase narrow:

- no renderer redesign
- no GPU texture API expansion
- no backend-specific material upload work
- no speculative cache framework

The goal is to retain truthful runtime data and make repeated imports flow through one predictable retained-asset path.

## Existing Code Insights

- `src/Videra.Core/Geometry/VertexPositionNormalColor.cs` defines `MeshData`, and it currently carries:
  - `Vertices`
  - `Indices`
  - `TextureCoordinateSets`
  - `Topology`
- `MeshData` has no tangent field, so tangent input cannot survive importer/runtime handoff today.
- `src/Videra.Import.Gltf/GltfModelImporter.cs` reads:
  - `POSITION`
  - `NORMAL`
  - `COLOR_0`
  - `TEXCOORD_0`
  - `TEXCOORD_1`
  but it does not read `TANGENT`.
- `src/Videra.Core/Graphics/MeshPayload.cs` only retains `VertexPositionNormalColor[]`, `uint[]`, and topology, so any future tangent data would still be dropped unless payload truth widens too.
- `src/Videra.Core/Scene/ImportedSceneAssetPayloadBuilder.cs` flattens primitives into one `MeshPayload` using transformed positions and normals only.
- `src/Videra.Core/Scene/SceneObjectFactory.cs` already reuses the same `ImportedSceneAsset.Payload` when multiple deferred `Object3D` instances are created from the same `ImportedSceneAsset`.
- `src/Videra.Avalonia/Runtime/Scene/SceneImportService.cs` currently calls the importer every time. Repeated imports of the same glTF file rebuild:
  - `ImportedSceneAsset`
  - material catalog
  - texture catalog
  - sampler catalog
  - payload ids and object graph
- `src/Videra.Import.Gltf/GltfTextureAssetReader.cs` and `GltfModelImporter` only deduplicate materials/textures/samplers within one import call via local dictionaries.
- `src/Videra.Avalonia/Runtime/Scene/SceneResidencyRegistry.cs` and `SceneUploadQueue.cs` track upload state per `SceneEntryId`, but they do not have an explicit imported-asset reuse identity beyond the retained `ImportedAsset` reference already stored on each entry.
- `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs` has no texture creation API, so this phase should stay at runtime-truth and retained-asset reuse level rather than inventing texture-upload abstractions the engine does not yet ship.

## Implementation Decisions

### Keep tangent truth explicit and direct

Do not overload `VertexPositionNormalColor` or add speculative multi-set tangent abstractions. Add one direct tangent-bearing runtime field to the mesh/runtime path and preserve it from importer through retained payloads.

### Reuse the retained imported asset instead of inventing a new upload layer

The repository already uses `ImportedSceneAsset` as the retained scene/material/texture truth. The narrowest reuse seam is to make repeated imports of the same unchanged static asset resolve to the same retained imported asset, so repeated `SceneObjectFactory.CreateDeferred(...)` calls reuse one payload and one material/texture/sampler catalog.

### Keep reuse bounded to static asset identity

If reuse is keyed, it should be keyed by normalized file identity plus file freshness, not by backend state, not by renderer internals, and not by speculative global cache policy.

## Expected Verification Shape

- Core tests for tangent retention through importer/runtime payloads
- Avalonia/runtime tests proving repeated imports of the same unchanged static glTF reuse one retained `ImportedSceneAsset`
- focused diff hygiene and existing upload/runtime tests kept green
