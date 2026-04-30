# Phase 21: Milestone Audit Truth and Summary Metadata Cleanup - Context

**Gathered:** 2026-04-16  
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 21 is a cleanup-only phase created after the v1.2 re-audit. The shipped code already closes the original milestone gaps, but the audit still recorded three non-blocking debts:

1. Phase 19/20 summaries do not declare `requirements-completed`
2. Historical verification files still read like live blockers unless the reader already knows Phase 20 landed
3. There is no deterministic guard that keeps milestone-artifact truth aligned

This phase fixes planning and verification truth without rewriting shipped product behavior.

</domain>

<decisions>
## Implementation Decisions

### Preserve history, clarify present truth
Phase 13/14 must keep their historical `gaps_found` record. The cleanup should add supersession notes, not erase the original audit evidence.

### Guard the planning truth in tests
The repository already uses `SurfaceChartsRepositoryArchitectureTests` to freeze chart boundary and docs truth. The same pattern should guard milestone-artifact consistency for Phase 19/20 summary metadata and the superseded verification wording.

### Keep requirement status unchanged
`VIEW-*` and `INT-*` are already satisfied in Phase 19/20. This phase must not remap or reopen those requirements.

</decisions>

<code_context>
## Existing Code Insights

- `tests/Videra.Core.Tests/Repository/SurfaceChartsDocumentationTerms.cs` holds exact phrases for chart docs/guard assertions.
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` is the existing deterministic guard point for root/demo/Chinese/module truth.
- `.planning/phases/19-...` and `.planning/phases/20-...` summary files currently omit `requirements-completed`.
- `.planning/phases/13-.../13-VERIFICATION.md`, `14-VERIFICATION.md`, `18-VERIFICATION.md`, and `19-VERIFICATION.md` still contain wording that can be misread as current milestone state unless Phase 20 context is already known.

</code_context>

<specifics>
## Specific Ideas

- Add `requirements-completed` to all Phase 19/20 summaries, matching the phase verification matrix
- Add explicit historical/superseded notes to 13/14/18/19 verification reports
- Add repository tests that fail if the summary metadata or supersession wording drifts

</specifics>

<deferred>
## Deferred Ideas

- Do not rewrite archived milestone files
- Do not remove the original `gaps_found` status from the historical Phase 13/14 verification reports

</deferred>
