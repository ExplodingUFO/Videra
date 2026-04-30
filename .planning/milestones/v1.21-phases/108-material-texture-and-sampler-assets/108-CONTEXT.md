# Phase 108: Material, Texture, and Sampler Assets

**Gathered:** 2026-04-21  
**Status:** Complete

## Phase Boundary

Introduce backend-neutral material, texture, and sampler runtime assets for imported scenes without widening backend APIs or changing the existing upload/runtime bridge shape.

## Decisions

- Keep the first slice at the scene/import contract level; do not reopen `IResourceFactory`, backend upload APIs, or public host controls.
- Preserve current vertex-color rendering behavior; the new assets are catalogued and carried through the runtime path, not rendered yet.
- Store texture payloads as encoded image content plus width, height, and sRGB intent so the contract matches the actual imported data.

## Code Context

- `ImportedSceneAsset` already carried node/primitive scene truth from Phase 107, so Phase 108 could extend that catalog rather than invent a second asset path.
- OBJ and glTF importers were the right seam for material/texture/sampler capture because the backend layer still uploads flattened mesh payloads only.
- Existing scene residency and import-service tests already exercised the imported-asset handoff path and could be extended directly.

## Deferred Ideas

- No backend texture upload or render-path binding work in this phase.
- No PBR, UV-driven shading, or broader static glTF runtime work in this phase.
