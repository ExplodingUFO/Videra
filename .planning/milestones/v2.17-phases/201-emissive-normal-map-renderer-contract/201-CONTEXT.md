# Phase 201: Emissive/Normal-Map Renderer Contract - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Add the minimum renderer-consumption contract needed for emissive and normal-map-ready material truth on the shipped static-scene path, without introducing a generic material system, new resource bindings, or a wider lighting framework.

</domain>

<decisions>
## Implementation Decisions

### Smallest viable seam

- Reuse the existing CPU-side material bake path that already consumes baseColor and occlusion.
- Consume emissive by baking it into retained vertex color inputs on the static-scene path.
- Consume normal textures by baking them into retained vertex normals using existing tangent and texture-coordinate data.
- Keep both `ImportedSceneAsset.Payload` and deferred runtime-object paths aligned so the same static-scene truth reaches the renderer either way.

### Explicit non-goals

- No new GPU texture resource bindings or render-target abstractions.
- No generic material-system or lighting-system public contract.
- No shadow, environment, post-process, animation, or host/package expansion.

</decisions>

<code_context>
## Existing Code Insights

- `MaterialTextureColorBaker` already samples textures on the CPU and applies baseColor + occlusion to vertex colors.
- `ImportedSceneAssetPayloadBuilder` currently builds `asset.Payload` from primitive payloads plus alpha only; emissive and normal-map-ready truth are not consumed there today.
- `ImportedSceneRuntimeObjectBuilder` likewise consumes baseColor + occlusion only.
- Tangent data and texture coordinate sets already flow through mesh/runtime truth, so normal-map sampling can stay inside the current static-scene path.

</code_context>

<specifics>
## Specific Ideas

- Extend the existing texture baker with emissive color contribution.
- Add one narrow normal-texture baker/helper instead of widening `MaterialTextureColorBaker` into a god file.
- Update focused scene/import/render tests so they prove both payload and deferred-object paths consume the same truth.

</specifics>

<deferred>
## Deferred Ideas

- Physically broader emissive/shading models
- GPU-side texture consumption or per-material shader resource sets
- Shadows, environment maps, post-processing

</deferred>
