# Phase 293: SurfaceCharts Support Report UX - Context

**Gathered:** 2026-04-28  
**Status:** Implemented  
**Bead:** Videra-0w9.2

## Boundary

Refine the existing `Videra.SurfaceCharts.Demo` support summary using existing diagnostics truth. Do not create a parallel diagnostics model, merge with viewer diagnostics, or widen chart/rendering behavior.

## Decisions

- Keep the support summary as visible/copyable text in the existing demo panel.
- Add environment and identity fields because chart counters were already sufficient.
- Keep evidence-only wording and maintain SurfaceCharts separate from `VideraDiagnosticsSnapshotFormatter`.

## Handoff

Phase 295 can consume these support-report fields when aligning Doctor/support evidence:

- `ChartControl`
- `EnvironmentRuntime`
- `AssemblyIdentity`
- `BackendDisplayEnvironment`
- `CacheLoadFailure`
