# Phase 277: Coordination Validation and Guardrails - Context

**Gathered:** 2026-04-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Add explicit validation and repository guardrails for Beads coordination without making normal product builds or CI depend on the local Beads Docker service.

</domain>

<decisions>
## Implementation Decisions

### Validation Surface
- Add a manually invoked PowerShell script under `scripts/`.
- Check `bd context --json`, `bd doctor`, `bd worktree list`, and Docker-backed Dolt metadata.
- Do not start Docker containers or mutate product/build/release state.

### Guardrails
- Add repository tests that statically verify the docs, proof JSON, ignored runtime files, and explicit-only script boundary.
- Ensure normal `verify.ps1` and GitHub workflows do not invoke the Beads coordination validation script.

### the agent's Discretion
Keep tests narrow and repository-contract focused; do not add CI workflows or product code.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `docs/beads-coordination.md` owns the coordination contract.
- `eng/beads-lifecycle-proof.json` owns lifecycle proof data.
- Repository tests under `tests/Videra.Core.Tests/Repository` already lock docs/scripts/guardrails.

### Established Patterns
- Repository guardrails use static file assertions for docs/scripts that should not run during normal CI.
- PowerShell scripts live under `scripts/` and are invoked explicitly from docs.

### Integration Points
- Add `scripts/Test-BeadsCoordination.ps1`.
- Add `BeadsCoordinationRepositoryTests`.
- Extend `docs/beads-coordination.md` with explicit validation guidance.

</code_context>

<specifics>
## Specific Ideas

Validation is explicit-only: maintainers run it when checking local Beads coordination, not as part of product build or release validation.

</specifics>

<deferred>
## Deferred Ideas

- Beads remote sync policy remains deferred until a remote is configured.

</deferred>
