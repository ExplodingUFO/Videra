# Phase 200: Emissive/Normal-Map Scope Lock - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Freeze `v2.17` to one bounded renderer-consumption slice: consume already-retained emissive and normal-map-ready material truth on the existing static-scene viewer/runtime path, without widening into a broader lighting or material system.

</domain>

<decisions>
## Implementation Decisions

### Milestone Shape

- `Phase 201`: minimum emissive/normal-map renderer contract
- `Phase 202`: repo-owned proof and explicit `10`-second validation
- `Phase 203`: docs/support/repository truth and guardrails

### Explicit Non-Goals

- No shadows, environment maps, post-processing, fullscreen pass framework, or new render-target/resource-set abstraction.
- No generic lighting framework, material-system rewrite, or broader shader-system public model.
- No animation, skeletons, morph targets, mixers, extra UI adapters, backend/platform expansion, importer breadth widening, or chart-family expansion.
- No compatibility shims, migration adapters, downgrade paths, or transitional dual-path renderer contracts.

</decisions>

<code_context>
## Existing Code Insights

- `v2.15-v2.16` already proved the advanced-runtime line can stay bounded on the existing style/uniform seam.
- Emissive and normal-map-ready material truth already exists as retained runtime truth, but is not yet part of the shipped renderer-consumption baseline.
- Repository-owned desktop proof hosts already carry the explicit `10`-second survival pattern and should stay the only proof surface.

</code_context>

<specifics>
## Specific Ideas

- Keep the next implementation phase scoped to renderer/shader consumption only.
- Reuse the existing proof hosts rather than creating a new sample or validation shell.
- Keep the docs/guardrail pass honest about what becomes shipped renderer truth versus what remains retained-only truth.

</specifics>

<deferred>
## Deferred Ideas

- Shadows, environment maps, post-processing
- Broader lighting/material-system work
- Additional proof hosts or public package reshaping

</deferred>
