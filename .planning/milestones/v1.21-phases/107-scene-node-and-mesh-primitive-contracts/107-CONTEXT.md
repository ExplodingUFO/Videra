# Phase 107: Scene Node and Mesh Primitive Contracts

**Gathered:** 2026-04-21  
**Status:** Completed

## Phase Boundary

Introduce one backend-neutral scene truth that separates:

- scene-node identity and hierarchy
- local transform semantics
- reusable mesh-primitive ownership
- the existing deferred upload/runtime path

The phase must not widen into materials, textures, render-feature vocabulary, or backend-specific public scene objects.

## Decisions

- `ImportedSceneAsset` becomes the public scene truth for `Nodes` and `Primitives`; the combined upload payload stays internal and derived once.
- `SceneNode` stores parent linkage plus local transform rather than introducing a heavier graph object.
- `MeshPrimitive` owns reusable geometry identity and mesh data; shared instances remain represented by multiple nodes referencing the same primitive id.
- Importers continue to preserve practical partial-import behavior when malformed glTF primitives appear beside valid ones.

## Code Context

- Existing runtime/reupload path still depends on one deferred `MeshPayload` through `SceneObjectFactory` and `SceneUploadCoordinator`.
- OBJ and glTF importers were previously flattening directly into one `MeshData`.
- Benchmarks and focused tests referenced the old `ImportedSceneAsset.MeshData` shape.

## Success Shape

- Public contracts expose `SceneNode`, `SceneNodeId`, `MeshPrimitive`, and `MeshPrimitiveId`.
- Viewer/runtime code still uses one internal derived payload for deferred upload/reupload.
- OBJ/glTF importers produce scene-node/primitive truth.
- Tests cover hierarchy wiring, shared primitive reuse, partial-import behavior, and flattened transformed geometry.
