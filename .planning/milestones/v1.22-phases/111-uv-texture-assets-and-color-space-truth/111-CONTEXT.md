# Phase 111: UV, Texture Assets, and Color-Space Truth - Context

**Gathered:** 2026-04-21
**Status:** Ready for planning

## Phase Boundary

Thread static glTF UV coordinates, texture references, and explicit color-space intent through the importer/runtime path without introducing importer-shaped public types or reopening the viewer/runtime boundary.

## Existing Code Insights

- `src/Videra.Import.Gltf/GltfModelImporter.cs` currently imports `POSITION`, `NORMAL`, `COLOR_0`, base-color factor, and optional base-color texture/sampler references.
- `src/Videra.Import.Gltf/GltfTextureAssetReader.cs` creates `Texture2D` assets and currently hard-codes `isSrgb: true`.
- `src/Videra.Core/Geometry/VertexPositionNormalColor.cs` and `MeshData` do not currently retain UV data.
- `src/Videra.Core/Scene/MaterialInstance.cs` currently stores base-color texture usage as separate `Texture2DId?` and `SamplerId?` fields, with no explicit texcoord-set or color-space contract.
- `src/Videra.Core/Scene/ImportedSceneAsset.cs` already retains `Materials`, `Textures`, and `Samplers`, and `SceneImportService` preserves the imported asset on the viewer/runtime path.
- The current render/runtime path does not consume texture sampling yet, so this phase should lock runtime truth and importer retention rather than widen into a full textured shader path.

## Implementation Decisions

### Keep the phase contract-first

Phase 111 will make UV coordinates and texture-usage semantics explicit runtime truth. It will not add speculative GPU texture sampling, fallback layers, or backend-specific material abstractions.

### Favor one direct texture-usage contract

Instead of scattering `TextureId`, `SamplerId`, `texcoord`, and color-space intent across unrelated properties, use one narrow material texture-binding contract on `MaterialInstance`.

### Keep viewer/runtime proof on retained imported assets

The success condition is that imported glTF UV and texture/color-space truth survive `Import(...)`, `SceneImportService`, and `ImportedSceneAsset` retention on the current viewer path.
