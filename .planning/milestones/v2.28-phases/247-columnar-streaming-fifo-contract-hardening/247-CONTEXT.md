# Phase 247: Columnar Streaming/FIFO Contract Hardening - Context

**Gathered:** 2026-04-27
**Status:** Ready for execution
**Mode:** Autonomous

<domain>
## Phase Boundary

Tighten the SurfaceCharts columnar scatter API around append/FIFO semantics and validation without introducing a generic streaming framework.
</domain>

<decisions>
## Implementation Decisions

- Keep the work inside `ScatterColumnarSeries` and existing scatter tests.
- Add observable streaming counters directly on the columnar series so later diagnostics can reuse them.
- Make `IsSortedX` an enforced non-decreasing contract for append streams while allowing replacement to reset the range.
</decisions>

<deferred>
## Deferred Ideas

- Demo diagnostics, rendering status, and benchmark evidence are handled by later v2.28 phases.
</deferred>
