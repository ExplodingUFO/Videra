# Phase 217 Context

Milestone: `v2.21 Repo Doctor and Quality Gate Closure`
Requirement: `RDG-01`
Branch: `v2.21-phase217`
Commit: `c3bd924`
Date: `2026-04-26`

## Starting Point

The benchmark threshold contract was internally inconsistent:

- `benchmarks/benchmark-thresholds.json` already defined seven committed hard-gate thresholds.
- `docs/benchmark-gates.md` described the seven-threshold hard gate.
- `BenchmarkThresholdRepositoryTests` still expected only two Viewer thresholds and therefore failed against the real committed threshold file.
- The benchmark runbook also had duplicated future-escalation guidance.

## Assumptions

- The source-controlled threshold file is the committed hard-gate truth.
- Phase 217 should not widen thresholds, remove thresholds, or retune numeric budgets.
- Broader release-readiness wording drift is left for Phase 221 unless it blocks benchmark-gate truth directly.

## Out of Scope

- New benchmark families or measurements.
- Threshold budget changes.
- Release-readiness documentation cleanup outside benchmark gate contract truth.
