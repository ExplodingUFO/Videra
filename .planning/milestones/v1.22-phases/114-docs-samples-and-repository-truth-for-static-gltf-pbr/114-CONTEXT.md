# Phase 114 Context: Docs, Samples, and Repository Truth for Static glTF/PBR

## Goal

Make the shipped docs, sample truth, and repository guards describe one consistent static glTF/PBR baseline for the viewer/runtime path.

## What is already true in code

- Static glTF UV coordinates and texture references flow into explicit `Texture2D` and `Sampler` runtime assets.
- Viewer-path materials now carry metallic-roughness, alpha, emissive, and normal-map-ready semantics through explicit runtime contracts.
- Tangent-aware mesh data flows through the static glTF importer/runtime path.
- Repeated unchanged imports reuse retained imported scene assets instead of rebuilding ad hoc importer-shaped state each time.

## Phase boundary

This phase is documentation/sample/repository-truth only.

- Update public docs and package/readme guidance.
- Update demo/sample copy where the repository presents the shipped viewer/runtime baseline.
- Add or tighten repository guards so the docs and samples stay aligned.

## Non-goals

- No runtime behavior changes.
- No new renderer/backend APIs.
- No compatibility shims or fallback logic.
- No widening into animation, skeletons, morph targets, or broader advanced-runtime feature work.
