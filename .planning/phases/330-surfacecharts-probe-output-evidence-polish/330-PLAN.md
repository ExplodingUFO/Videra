# Phase 330: SurfaceCharts Probe Output Evidence Polish - Plan

**Bead:** Videra-k38

## Plan

1. Add a public chart-local `SurfaceChartProbeEvidence` model and status enum.
2. Add `SurfaceChartProbeEvidenceFormatter` that creates/formats evidence from hovered and pinned `SurfaceProbeInfo` values using `SurfaceChartOverlayOptions`.
3. Reuse formatter helpers from existing overlay presenter/state to avoid duplicate probe readout logic.
4. Cover hovered+pinned, pinned-only, and deterministic text formatting.

## Verification

Run focused SurfaceCharts Avalonia probe evidence tests and integration tests.
