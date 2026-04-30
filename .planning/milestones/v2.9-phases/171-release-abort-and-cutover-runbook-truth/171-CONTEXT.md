# Phase 171 Context: Release Abort and Cutover Runbook Truth

## Milestone

`v2.9 Release Candidate Final Validation`

## Goal

Document and guard the release-candidate abort criteria and human cutover boundary.

## Boundary

This phase is documentation and repository truth only:

- abort criteria for failed candidate validation
- manual cutover checklist
- human approval boundary before publishing
- links from releasing docs and evidence index
- repository tests that prevent drift

## Decisions

- Add one focused runbook rather than expanding the publishing workflow.
- Keep all release actions manual and outside this milestone.
- Keep tags and publishing out of the release dry-run path.
- Do not add fallback release paths, compatibility routes, or alternate publish workflows.

## Deferred

- Actual package publishing and repository release tag creation remain deferred to a later human cutover.
- Any release automation beyond the existing workflows remains out of scope.
