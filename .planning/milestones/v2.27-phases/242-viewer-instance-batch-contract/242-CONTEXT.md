# Phase 242: Viewer Instance Batch Contract - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous execution from v2.27 roadmap

<domain>
## Phase Boundary

Add a narrow public instance-batch contract for same mesh/material, per-instance transform/color/id, and batch-level bounds. This phase only defines retained scene truth and validation; it does not render, upload, pick, or benchmark instance batches.

</domain>

<decisions>
## Implementation Decisions

- Keep the contract in `Videra.Core.Scene` alongside `SceneDocument`, `MeshPrimitive`, and `MaterialInstance`.
- Use one `MeshPrimitive` and one matching `MaterialInstance`.
- Support per-instance `Matrix4x4`, optional `RgbaFloat` colors, optional `Guid` object ids, and `Pickable`.
- Reject transparent `Blend` material participation in v1 of the contract; opaque and alpha-mask remain allowed.
- Keep multi-geometry batching, per-instance material, transparent sorting, GPU-driven culling, indirect draw, and ECS-style ownership out of scope.

</decisions>

<code_context>
## Existing Code Insights

- `SceneDocument` is already the retained scene truth and public entry list.
- `MeshPrimitive` computes a mesh payload and already rejects invalid empty mesh payloads.
- `BoundingBox3` supports local bounds from vertices and transform/encapsulate operations.
- Runtime object expansion and upload remain separate from `SceneDocument` and belong to later phases.

</code_context>

<specifics>
## Specific Ideas

- Add `InstanceBatchDescriptor` with validation and computed batch bounds.
- Add `InstanceBatchEntry` as retained scene-document truth.
- Add `SceneDocument.InstanceBatches` and `SceneDocument.AddInstanceBatch(...)`.
- Add focused tests for valid descriptors, bounds, validation errors, and retained document entries.

</specifics>

<deferred>
## Deferred Ideas

- Runtime rendering and upload accounting belong to Phase 243.
- Picking returning `ObjectId + InstanceIndex` belongs to Phase 243.
- Benchmark evidence belongs to Phase 244.

</deferred>
