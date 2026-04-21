# Phase 57 Research

## Problem

Imported assets were already backend-neutral, but `Object3D` still cloned vertex and index arrays into multiple caches. That kept reupload and wireframe behavior working, but it duplicated CPU mesh memory for imported/deferred scenes.

## Findings

- `ImportedSceneAsset` already had a stable place to carry shared CPU-side geometry.
- `SceneObjectFactory` is the natural handoff point from imported asset to deferred `Object3D`.
- `Object3D` only needs one retained CPU payload for reupload, wireframe, and picking semantics.

## Decision

Phase 57 should introduce shared `MeshPayload`, teach imported assets and `SceneObjectFactory` to use it, and collapse `Object3D` onto that single retained payload plus explicit retention metadata.

