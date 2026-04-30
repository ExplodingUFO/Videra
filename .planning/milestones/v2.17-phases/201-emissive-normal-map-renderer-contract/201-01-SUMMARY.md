# Phase 201 Summary: Emissive/Normal-Map Renderer Contract

## Outcome

`v2.17` now consumes emissive and normal-map-ready material truth on the existing static-scene renderer path without introducing a new material or resource-binding system:

- extended the existing CPU-side material bake seam so emissive contributes to retained vertex color inputs
- added a small normal-texture baker so retained vertex normals can be perturbed from tangent-space normal maps when tangent data is present
- kept `ImportedSceneAsset.Payload` and deferred runtime-object creation aligned so both static-scene renderer entry paths consume the same truth

The phase intentionally did not widen into:

- shader/backend/resource-binding expansion
- a generic material or lighting framework
- docs/support/repository guardrail changes

## Verification Shape

- focused scene and graphics tests for emissive and normal-map baking on both payload and deferred-runtime paths
- no whitespace regressions beyond expected CRLF warnings in the Windows checkout

## Next Phase

- Phase 202: repo-owned emissive/normal-map proof and validation
