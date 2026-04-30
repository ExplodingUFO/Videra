# Phase 112: Metallic-Roughness, Alpha, Emissive, and Normal Material Semantics - Context

**Gathered:** 2026-04-21  
**Status:** Ready for planning

## Phase Boundary

Make the shipped static PBR material semantics explicit on the current viewer/runtime path by extending `MaterialInstance` and the glTF importer. Keep this phase contract-first: no textured shader path, no lighting expansion, no transparent sorting overhaul, and no backend-specific material logic.

## Existing Code Insights

- `src/Videra.Core/Scene/MaterialInstance.cs` currently exposes only `BaseColorFactor` and `BaseColorTexture`.
- `src/Videra.Core/Scene/MaterialTextureBinding.cs` already provides one direct runtime texture-usage contract with texture id, sampler id, texcoord set, and color-space intent.
- `src/Videra.Import.Gltf/GltfModelImporter.cs` already uses `Material.FindChannel("BaseColor")` and now retains base-color texture usage plus UV sets, but it does not yet map:
  - metallic/roughness factors or texture
  - alpha mode / alpha cutoff / double-sided
  - emissive factor or emissive texture
  - normal texture usage or normal scale
- `ImportedSceneAsset`, `SceneDocumentEntry`, `SceneImportService`, and the Avalonia scene runtime already retain imported material catalogs without consuming them on the current GPU upload path.
- `SceneObjectFactory` / `Object3D` still flatten only `MeshPayload`, so Phase 112 should preserve runtime truth rather than widen into renderer consumption.

## SharpGLTF API Truth

Using SharpGLTF `1.0.6`:

- `Material.Alpha`, `Material.AlphaCutoff`, and `Material.DoubleSided` are public properties.
- `Material.FindChannel(...)` exposes the relevant channel keys:
  - `BaseColor`
  - `MetallicRoughness`
  - `Normal`
  - `Emissive`
- For a sample material:
  - `MetallicRoughness.Parameter` came through as `<metallic, roughness, 0, 0>`
  - `Normal.Parameter.X` carried the normal scale
  - `Emissive.Parameter` carried emissive RGB

## Implementation Decisions

### Keep material semantics on `MaterialInstance`

This phase should extend the existing runtime material contract directly instead of inventing importer-shaped payloads or backend-specific structures.

### Reuse `MaterialTextureBinding` where it already fits

- base color and emissive textures are color textures -> `TextureColorSpace.Srgb`
- metallic-roughness and normal textures are data textures -> `TextureColorSpace.Linear`

Normal maps still need a scale value, so normal-map semantics need one additional direct runtime contract rather than abusing the generic binding.

### Keep runtime truth narrower than rendering truth

The shipped viewer/runtime path already retains imported catalogs. Phase 112 should prove that retained imported assets preserve the new material semantics through import and `SceneImportService`, without implying that the renderer consumes them yet.
