# Phase 253: GitHub CI Reality and Failure Closure - Context

**Gathered:** 2026-04-27  
**Status:** Ready for execution  
**Mode:** Autonomous

<domain>
## Phase Boundary

Observe required GitHub CI checks for PR #90 and close failures with the smallest justified code or CI-contract changes.

</domain>

<decisions>
## Implementation Decisions

- Treat GitHub checks as source of truth for this phase.
- Fix only failures that are caused by the PR implementation or an incorrect repository CI/test contract.
- Do not add new product features while closing CI.
- If checks are still running, wait and poll rather than guessing.

</decisions>

<code_context>
## Existing Code Insights

- PR #90 is open, mergeable, and points from `v2.28-streaming-performance-hardening` to `master`.
- Initial check rollup shows 18 checks in progress across CI, native validation, consumer smoke, release dry-run, and benchmark gates.

</code_context>

<specifics>
## Specific Ideas

- Use `gh pr checks 90 --watch` or repeated `gh pr view 90 --json statusCheckRollup`.
- Classify failures by check name, log URL, and likely cause.
- Apply focused fixes directly on the PR branch only if needed.

</specifics>

<deferred>
## Deferred Ideas

- Merging belongs to Phase 254.
- Final evidence and next milestone recommendations belong to Phase 255.

</deferred>
