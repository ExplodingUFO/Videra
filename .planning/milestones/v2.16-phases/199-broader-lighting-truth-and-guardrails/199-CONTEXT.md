# Phase 199: Broader-Lighting Truth and Guardrails - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Close `v2.16` by aligning repository docs, support/native-validation wording, package/readme surfaces, and repository guardrails with the shipped bounded broader-lighting baseline and the explicit repo-owned `10`-second desktop proof hosts.

</domain>

<decisions>
## Implementation Decisions

### Bounded scope only

- Touch docs and repository guard tests only.
- Do not widen into runtime, shader, backend, packaging, or workflow behavior changes unless a doc/test mismatch forces it.
- Keep the wording honest: `v2.16` broadens the bounded style-driven lighting baseline through `FillIntensity`, but does not claim shadows, environment maps, post-processing, emissive/normal-map renderer consumption, animation, or broader advanced-runtime breadth.

### Proof-host truth

- The `10`-second hold language should describe repository-owned desktop proof hosts and validation evidence, not a broader public runtime promise.
- The docs must stay clear that `WpfSmoke` and `SurfaceCharts.ConsumerSmoke` remain repository-only proof/support surfaces.

</decisions>

<code_context>
## Existing Code Insights

- Root docs and package READMEs still use `direct-lighting baseline` wording from `v2.15`.
- Native-validation/support docs still frame the documented hold as direct-lighting proof rather than repo-owned broader-lighting proof-host evidence.
- Repository architecture/release/native-validation tests already lock many of these strings, so updates must keep docs and tests in sync.

</code_context>

<specifics>
## Specific Ideas

- Update root/docs/package mirrors to say `bounded broader-lighting baseline` where appropriate.
- Update support/native-validation wording so the documented `10`-second hold is described as repo-owned desktop proof-host evidence.
- Keep the same deferred list explicit across English and Chinese mirrors.

</specifics>

<deferred>
## Deferred Ideas

- No new runtime proof host behavior.
- No new public samples or package lines.
- No advanced-lighting breadth beyond the shipped bounded broader-lighting baseline.

</deferred>
