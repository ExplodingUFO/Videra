# Phase 202: Repo-Owned Emissive/Normal-Map Proof and Validation - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Prove the new emissive/normal-map renderer-consumption slice on the existing repository-owned desktop proof hosts and keep the `10`-second survival rule explicit on every validation app used in this phase.

</domain>

<decisions>
## Implementation Decisions

### Proof hosts

- Reuse the existing desktop proof hosts already on the viewer path.
- Keep validation limited to `ConsumerSmoke` viewer mode and `WpfSmoke` unless a narrower proof seam becomes impossible.
- Preserve explicit `LightingProofHoldSeconds=10` on every desktop app used in milestone validation.

### Non-goals

- No new proof hosts, demo families, or package lines.
- No docs/support/repository-guardrail wording changes in this phase.
- No widening of renderer/material abstractions beyond the proof evidence needed for the shipped slice.

</decisions>

<code_context>
## Existing Code Insights

- `v2.15-v2.16` already established the repo-owned proof-host and explicit `10`-second survival pattern.
- `v2.17` now consumes emissive and normal-map-ready truth through the CPU-side static-scene bake seam.
- The proof phase should therefore be about bounded scene/evidence wiring, not new renderer contracts.

</code_context>

<specifics>
## Specific Ideas

- Use one small scene or proof asset that makes emissive and perturbed normals visible on the current static-scene path.
- Emit proof diagnostics or support snapshots that make the new slice inspectable without widening public claims.
- Keep the same validation habit: actual desktop host run plus explicit `10`-second hold.

</specifics>

<deferred>
## Deferred Ideas

- Additional proof hosts
- Broader demo/product storytelling
- Docs/guardrail wording work (Phase 203)

</deferred>
