# Phase 165: Public API Contract Guardrails - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Add a minimal deterministic public API drift guard for the shipped public package surface before release-candidate review.

This phase is intentionally contract-only. It must not add compatibility layers, migration adapters, runtime features, rendering breadth, chart breadth, or publish behavior changes.
</domain>

<decisions>
## Implementation Decisions

- Guard the current public package line through a source-controlled manifest plus repository tests.
- Scope the first guard to top-level public types per shipped public package. This is deterministic, reviewable, and small enough for the current release-candidate closure phase.
- Do not introduce Microsoft API compatibility tooling or baseline package restore in this phase; that would widen release infrastructure and add more moving parts than the phase needs.
- Keep platform backend packages in the manifest even when their public top-level type list is empty, so accidental public type leaks become visible.
</decisions>

<code_context>
## Existing Code Insights

- `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs` already guards package, docs, release workflow, and metadata truth, but it is large and should not absorb another distinct contract.
- `HostingBoundaryTests.cs` already checks high-level public API boundary leakage, but it does not maintain an exact public API baseline.
- `eng/` already owns release-candidate evidence inputs such as `package-size-budgets.json`, making it the right location for a small public API contract manifest.
</code_context>

<specifics>
## Specific Ideas

- Add `eng/public-api-contract.json`.
- Add `tests/Videra.Core.Tests/Repository/PublicApiContractRepositoryTests.cs`.
- Verify package IDs, project paths, source roots, and exact top-level public type lists.
- Keep helper methods local to the new test file to avoid broad test infrastructure changes.
</specifics>

<deferred>
## Deferred Ideas

- Member-level API signature baselines.
- Binary API compatibility against previously published packages.
- Automated public API manifest regeneration tooling.
- Compatibility shims or transition adapters for intentional breaking changes.
</deferred>
