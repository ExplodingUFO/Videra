# Phase 330: SurfaceCharts Probe Output Evidence Polish - Context

**Gathered:** 2026-04-28  
**Status:** Complete  
**Bead:** Videra-k38

## Boundary

Add a bounded SurfaceCharts chart-local probe evidence formatter over existing public probe and overlay formatting contracts. Do not expose internal overlay state, move chart semantics into `VideraView`, or add chart families.

## Ownership

- `src/Videra.SurfaceCharts.Avalonia/Controls`
- focused `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests`

## Non-Goals

- No viewer-runtime coupling, broad chart-family expansion, backend expansion, compatibility layer, or hidden fallback/downshift path.
