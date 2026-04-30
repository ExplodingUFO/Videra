# Phase 274: Beads Service Contract and Agent Onboarding - Context

**Gathered:** 2026-04-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Lock the repository-facing Beads service contract and agent onboarding guidance around the Docker-backed `Videra` Dolt database. This phase documents the main checkout service contract and command workflow only; worktree redirect validation, lifecycle proof, and guardrail automation are later phases.

</domain>

<decisions>
## Implementation Decisions

### Service Contract
- Document `dolt-sql-server`, `127.0.0.1:3306`, database `Videra`, user `root`, and project id `cf27bb80-40f6-4ba7-95f7-bc455a484d7b`.
- Treat mismatched Dolt project identity as a hard coordination boundary rather than falling back to `AgentDialog` or another database.
- Keep the contract repository-facing and local-tooling oriented; do not make Beads part of product runtime or build prerequisites.

### Agent Onboarding
- Link agent entry files to one canonical coordination document.
- Cover ready/show/claim/create discovered work/close/export/sync commands.
- Keep GSD as planning authority and Beads as task coordination authority.

### the agent's Discretion
Use the smallest docs surface that satisfies the phase; defer scripts, tests, and lifecycle proof to later phases.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `AGENTS.md` and `CLAUDE.md` already contain Beads integration blocks.
- `docs/index.md` is the long-lived docs entry point.
- `.beads/config.yaml` and `.beads/metadata.json` already point at the Docker-backed `Videra` database.

### Established Patterns
- Repository docs are concise, maintainer-facing, and explicit about non-goals.
- Tooling docs distinguish local support/coordination commands from product/runtime behavior.

### Integration Points
- New docs should live under `docs/` and be linked from `docs/index.md`.
- Agent entry files should point to the canonical docs page instead of duplicating every detail.

</code_context>

<specifics>
## Specific Ideas

No additional user-specific choices beyond maintaining small, direct, non-god-code changes.

</specifics>

<deferred>
## Deferred Ideas

- Worktree redirect validation belongs to Phase 275.
- Issue lifecycle proof belongs to Phase 276.
- Validation script and repository guardrails belong to Phase 277.

</deferred>
