# Phase 249: Interaction Quality Refine Policy - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous, scoped from roadmap and current code

<domain>
## Phase Boundary

Use the existing SurfaceCharts interaction-quality concept to distinguish interaction-time updates from settled/refine work.

The narrow target is `ScatterChartView`, because `SurfaceChartView` and `WaterfallChartView` already expose `InteractionQuality` and `InteractionQualityChanged`.
</domain>

<decisions>
## Implementation Decisions

- Reuse `SurfaceChartInteractionQuality` instead of adding a scatter-specific enum.
- Keep behavior chart-local in `Videra.SurfaceCharts.Avalonia`; do not touch `VideraView`.
- Use deterministic pointer state transitions: left-drag start is `Interactive`, release/capture-lost is `Refine`.
- Avoid timers and async settle policy in this phase.
</decisions>

<code_context>
## Existing Code Insights

- `ScatterChartView` already tracks `_isInteracting` and publishes `IsInteracting` through `ScatterChartRenderingStatus`.
- The SurfaceCharts demo currently says scatter does not expose `InteractionQuality`.
- Existing scatter integration tests route pointer events through reflection and can verify the state transition without a live UI.
</code_context>

<specifics>
## Specific Ideas

- Add `InteractionQuality` to `ScatterChartRenderingStatus`.
- Add `InteractionQuality` and `InteractionQualityChanged` to `ScatterChartView`.
- Update demo panels and support summary to report actual scatter interaction quality.
- Add focused tests for drag transitions and event order.
</specifics>

<deferred>
## Deferred Ideas

- Interaction timers, progressive refinement scheduling, and renderer-quality knobs are deferred until a concrete expensive scatter render path exists.
- New chart families and viewer/chart unification remain out of scope.
</deferred>
