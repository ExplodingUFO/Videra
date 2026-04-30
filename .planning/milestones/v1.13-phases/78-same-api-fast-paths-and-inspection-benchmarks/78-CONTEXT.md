# Phase 78: Same-API Fast Paths and Inspection Benchmarks - Context

**Gathered:** 2026-04-19  
**Status:** Ready for planning and execution  
**Mode:** Autonomous

## Phase Boundary

Phase 78 keeps the current public inspection contract intact while making the implementation less conservative. The stable scope is narrower than the original “GPU clipping preview” idea: land fast paths that are already supportable in the current architecture, then add benchmark evidence so the next performance step is driven by data instead of guesswork.

## Implementation Decisions

### Public contract
- **D-01:** `ClippingPlanes` stays unchanged on `VideraView`; fast-path work must remain internal to the clipping payload/runtime path.
- **D-02:** `ExportSnapshotAsync(...)` stays unchanged on `VideraView`; fast-path work must stay inside runtime/export services.
- **D-03:** No new “fast mode” flags or backend-selection APIs are added in this phase.

### Stable fast paths
- **D-04:** The clipping fast path is a cached deterministic truth path: repeated normalized plane signatures should reuse clipped payload results instead of recomputing every frame.
- **D-05:** Snapshot export should prefer live-frame readback when the current render session already exposes compatible pixels and the export size matches the live frame.
- **D-06:** Both fast paths must fall back transparently to the current truthful implementations when the optimized path is unavailable.

### Benchmark evidence
- **D-07:** Add a dedicated inspection benchmark suite to the existing `Videra.Viewer.Benchmarks` project instead of creating a separate benchmark harness.
- **D-08:** The benchmark suite should cover the three new pressure points directly: hit testing, clipping payload generation, and snapshot export.

### the agent's Discretion
- Exact cache-key shape for clipped payload reuse as long as normalized plane semantics stay deterministic.
- Whether the snapshot fast path is wired from `RenderSession`, runtime inspection helpers, or the export service, provided the public API and fallback behavior remain unchanged.

## Specific Ideas

- Reuse `ConditionalWeakTable` caches keyed by mesh payload to keep acceleration state tied to payload lifetime and avoid manual invalidation infrastructure.
- Keep benchmark inputs small and deterministic so dry runs remain cheap enough for workflow evidence.
- Record the narrower implementation truth explicitly in the phase docs so follow-up work can decide whether a future GPU clip-preview path is still worth adding.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 78 goal, requirements, and success criteria.
- `.planning/REQUIREMENTS.md` — `PERF-12`, `PERF-13`, and `PERF-14`.
- `docs/plans/2026-04-18-viewer-inspection-workflow-design.md` — inspection-fidelity design intent and same-API constraint.

### Current implementation seams
- `src/Videra.Core/Inspection/VideraClipPayloadService.cs` — current CPU truth path for clipping payload generation.
- `src/Videra.Avalonia/Runtime/VideraSnapshotExportService.cs` — current deterministic snapshot export implementation.
- `src/Videra.Avalonia/Rendering/RenderSession.cs` — live render-session seam that can expose reusable frame pixels.
- `benchmarks/Videra.Viewer.Benchmarks` — the existing benchmark workflow this phase should extend.

## Existing Code Insights

### Reusable Assets
- `MeshPayload` already acts as the stable geometry identity for cached acceleration and clip truth reuse.
- `RenderSession` already knows the live frame size and render orchestration state, making it the narrowest place to expose a preferred readback backend.
- Existing benchmark infrastructure already produces trend artifacts; the missing piece is an inspection-specific suite.

### Established Patterns
- Phase 76 already uses `ConditionalWeakTable` to cache mesh hit acceleration, so the same lifetime model fits clipped payload caching.
- Snapshot export already treats software export as the ground truth, which makes a preferred live-readback branch safe as a capability optimization rather than a new correctness contract.
- The repo already treats dry benchmark runs as evidence-generating workflow steps rather than single authoritative numbers.

### Integration Points
- `VideraViewRuntime.Inspection` already decides how snapshot export is invoked from the public inspection surface.
- `VideraClipPayloadService` is the only place that needs to understand cached clipped meshes.
- `Videra.Viewer.Benchmarks` is already the accepted home for scene/viewer performance evidence.

## Deferred Ideas

- A backend-specific GPU clipping preview path while plane drags are in flight.
- Readback optimization for export sizes that do not match the live frame size.
- Any new public tuning knobs for performance behavior.

---

*Phase: 78-same-api-fast-paths-and-inspection-benchmarks*  
*Context gathered: 2026-04-19*
