# Phase 275: Worktree Coordination Contract - Context

**Gathered:** 2026-04-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Document and validate the worktree redirect pattern that lets parallel phase/worktree agents share one Beads issue truth while keeping Git branches and file ownership isolated.

</domain>

<decisions>
## Implementation Decisions

### Redirect Pattern
- Use `.beads/redirect` in worktrees to point back to the main checkout `.beads` directory.
- Treat redirect files as local-only runtime coordination files that must not be committed.
- Validate with `bd worktree list` plus `bd context --json` from a representative worktree.

### Branch Isolation
- Keep Git branch/worktree ownership separate from shared Beads issue state.
- Do not introduce a monolithic orchestration script or speculative worktree manager.

### the agent's Discretion
Fold the worktree guidance into the existing Beads coordination doc instead of creating a second coordination document.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `docs/beads-coordination.md` is the canonical coordination doc from Phase 274.
- `bd worktree list` already reports old phase worktrees as `redirect -> Videra`.

### Established Patterns
- Maintainer docs prefer command snippets and explicit expected output shape.
- Local-only files are documented as non-versioned rather than managed by product code.

### Integration Points
- Add worktree coordination under `docs/beads-coordination.md`.

</code_context>

<specifics>
## Specific Ideas

No specific requirements beyond keeping the redirect pattern simple and verifiable.

</specifics>

<deferred>
## Deferred Ideas

- Dedicated validation script belongs to Phase 277.

</deferred>
