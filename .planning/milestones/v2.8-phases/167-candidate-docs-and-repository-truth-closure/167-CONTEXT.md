# Phase 167: Candidate Docs and Repository Truth Closure - Context

**Gathered:** 2026-04-23
**Status:** Ready for planning
**Mode:** Autonomous

<domain>
## Phase Boundary

Align candidate-facing docs, changelog/release-note guidance, localized entry docs, and repository guardrails with the `v2.8` release-candidate contract closure truth.

</domain>

<decisions>
## Implementation Decisions

- Keep the phase documentation-only plus repository-truth-test focused.
- Do not add runtime, rendering, chart, backend, package publishing, compatibility, fallback, or migration behavior.
- Treat `.github/workflows/release-dry-run.yml`, `scripts/Invoke-ReleaseDryRun.ps1`, `eng/public-api-contract.json`, and `scripts/Validate-Packages.ps1` as the canonical release-candidate evidence path.
- Keep `.planning` local-only.

</decisions>

<code_context>
## Existing Code Insights

- Phase 165 added `eng/public-api-contract.json` and `PublicApiContractRepositoryTests`.
- Phase 166 added the read-only release dry-run script/workflow and `ReleaseDryRunRepositoryTests`.
- Candidate-facing docs currently describe release flows and package matrices but do not consistently mention the dry-run evidence path.

</code_context>

<specifics>
## Specific Ideas

- Add concise release-candidate evidence wording to README, capability matrix, package matrix, support matrix, release policy, releasing runbook, changelog, and Chinese entry docs.
- Add a focused repository test file instead of widening an already-large repository readiness test file.

</specifics>

<deferred>
## Deferred Ideas

- Actual public publishing, release tag creation, new package IDs, and runtime feature work remain out of scope.

</deferred>
