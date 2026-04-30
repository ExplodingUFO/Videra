# Phase 330: SurfaceCharts Probe Output Evidence Polish - Summary

**Status:** Complete  
**Bead:** Videra-k38  
**Implementation commit:** 76ce702f25b77f8321ed19d2cdc7b08e504fe6f7  
**Merged on master via:** Phase 330 merge commit

## Outcome

Added a chart-local probe evidence contract:

- `SurfaceChartProbeEvidence`
- `SurfaceChartProbeEvidenceStatus`
- `SurfaceChartProbeEvidenceFormatter`

The formatter reports:

- evidence kind
- probe status
- hovered readout
- pinned probe count and readouts
- delta versus the first pinned probe when available

Existing overlay readout logic now delegates to the formatter helpers, keeping probe vocabulary consistent.

## Verification

- Worker ran full `Videra.SurfaceCharts.Avalonia.IntegrationTests`: passed 116/116.
- Worker ran focused `SurfaceChartProbeEvidenceTests`: passed 3/3.
