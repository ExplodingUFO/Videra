# Phase 276: Issue Lifecycle and Handoff Proof - Context

**Gathered:** 2026-04-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Prove the Beads issue lifecycle with real Docker-backed issue state: create, claim, create discovered follow-up, close follow-up, observe database state, and document session handoff expectations.

</domain>

<decisions>
## Implementation Decisions

### Lifecycle Proof
- Use real Beads issues rather than synthetic markdown evidence.
- Record the observed issue ids and dependency relation in a repo-owned JSON proof file.
- Close proof follow-ups so no artificial ready work is left open.

### Handoff
- Distinguish Git state from Dolt/Beads state.
- Keep the proof non-product and non-release-gating.

### the agent's Discretion
Use a simple JSON proof artifact under `eng/` and documentation updates under the existing Beads coordination page.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `docs/beads-coordination.md` already owns service and worktree coordination truth.
- `.beads/issues.jsonl` is the tracked issue export.

### Established Patterns
- Release evidence and repository truth files live under `eng/` when they are machine-readable repository contracts.
- Maintainer docs describe what to run and what not to overclaim.

### Integration Points
- Add `eng/beads-lifecycle-proof.json`.
- Extend `docs/beads-coordination.md` with lifecycle and handoff guidance.

</code_context>

<specifics>
## Specific Ideas

Use `discovered-from` dependency type to prove follow-up capture without markdown TODOs.

</specifics>

<deferred>
## Deferred Ideas

- Automated validation of proof schema belongs to Phase 277.

</deferred>
