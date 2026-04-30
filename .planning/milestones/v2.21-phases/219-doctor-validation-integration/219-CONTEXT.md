# Phase 219 Context

Milestone: `v2.21 Repo Doctor and Quality Gate Closure`
Requirement: `RDG-03`
Branch: `v2.21-phase219`
Commit: `6cf0580`
Date: `2026-04-26`

## Starting Point

Phase 218 introduced the repo-only Doctor snapshot command, but validation integration was limited to script presence and support-artifact paths. Doctor did not yet model opt-in validator execution or distinguish skipped, unavailable, failed, and passing validation paths.

## Assumptions

- Doctor should orchestrate or reference existing validators; it should not reimplement validation logic.
- Default Doctor execution remains non-mutating and should not run expensive or host-sensitive validators.
- Explicit run switches are acceptable for opt-in validation execution.

## Out of Scope

- Running full package validation, benchmarks, smoke tests, or native validation as part of default Doctor.
- Publishing packages, pushing tags, changing feed configuration, or mutating user setup.
- Adding new validators beyond the existing repository scripts and demo diagnostics references.
