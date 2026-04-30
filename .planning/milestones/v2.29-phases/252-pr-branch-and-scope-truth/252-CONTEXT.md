# Phase 252: PR Branch and Scope Truth - Context

**Gathered:** 2026-04-27  
**Status:** Ready for execution  
**Mode:** Autonomous

<domain>
## Phase Boundary

Establish the remote branch and PR boundary for the existing v2.28 streaming-performance commits.

</domain>

<decisions>
## Implementation Decisions

- Use the current branch `v2.28-streaming-performance-hardening` as the PR branch because it already contains the completed v2.28 commits.
- Keep the PR scope limited to the five existing streaming-performance commits.
- Do not add code changes unless branch/PR truth inspection exposes a real issue.
- Preserve the user request for small, maintainable tasks and clean worktree state.

</decisions>

<code_context>
## Existing Code Insights

- `origin/master..HEAD` contains five commits:
  - `819e2f7 feat: harden columnar scatter streaming contract`
  - `99c18a1 feat: expose scatter streaming diagnostics`
  - `8cb99a1 feat: expose scatter interaction quality`
  - `a754020 perf: add surfacecharts streaming benchmarks`
  - `d4810a6 docs: close surfacecharts streaming truth`
- The branch is ahead of `origin/master` by five commits and not behind.
- GitHub currently has no PR associated with this branch.

</code_context>

<specifics>
## Specific Ideas

- Push the branch to `origin`.
- Create a PR against `master` with explicit scope, verification, and non-goals.
- Record the PR URL and branch truth in verification.

</specifics>

<deferred>
## Deferred Ideas

- CI failure triage belongs to Phase 253.
- Merge and cleanup belong to Phase 254.

</deferred>
