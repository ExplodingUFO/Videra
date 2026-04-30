# Phase 243: Instance Batch Runtime and Picking Proof - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous continuation from v2.27 roadmap

<domain>
## Phase Boundary

Wire the first retained instance-batch contract through the viewer runtime enough to prove it is real runtime state: callers can add a batch, `FrameAll()` includes its bounds, diagnostics expose retained batch/instance counts, and click picking returns stable per-instance identity.

</domain>

<decisions>
## Implementation Decisions

- Keep this as a minimal runtime proof, not a renderer rewrite.
- Do not fake GPU draw-call or submitted-instance counters. Renderer-cost fields stay unavailable unless the active backend reports them.
- Treat `InstanceBatchEntry` as retained scene-document truth alongside normal `SceneDocumentEntry` objects.
- Picking may return a hit without an `Object3D`; consumers that need mesh-specific object data must fall back to the world point.

</decisions>

<code_context>
## Existing Code Insights

- `SceneDocument` already retains `InstanceBatches` from Phase 242.
- `VideraViewRuntime` owns `SceneRuntimeCoordinator`, camera framing, diagnostics projection, and interaction routing.
- `SceneHitTestService` already owns mesh-accurate object picking and can reuse `MeshPrimitive.Payload` for batch picking.
- `VideraBackendDiagnostics` already separates backend-reported nullable renderer-cost metrics from residency-derived metrics.

</code_context>

<specifics>
## Specific Ideas

- Add `VideraView.AddInstanceBatch(...)` and runtime/coordinator forwarding.
- Expose coordinator/runtime `InstanceBatches` for framing and interaction.
- Extend `SceneHitTestRequest` and `SceneHitTestResult.SceneHit` with batch inputs and `InstanceIndex`.
- Add retained `InstanceBatchCount` and `RetainedInstanceCount` diagnostics.
- Include pickable batch instances in `PickableObjectCount` without claiming backend draw-call support.

</specifics>

<deferred>
## Deferred Ideas

- Real GPU instanced draw path.
- Draw-call reduction measurement.
- Upload-byte accounting for instance buffers.
- Selection overlay outline rendering for per-instance selections.
- Box selection over instance batches.

</deferred>
