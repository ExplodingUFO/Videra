# Phase 358: Dataset Metadata and Reproducibility Evidence

**Status:** Complete locally
**Bead:** Videra-779.3
**Branch:** v2.51-phase-358-dataset-evidence

## Scope

Implemented deterministic dataset evidence owned by `VideraChartView.Plot` without adding output/report contracts, backend internals, compatibility APIs, fallback/downshift behavior, or generic plotting abstractions.

## Implementation

- Added `Plot3D.CreateDatasetEvidence()` as the Plot-owned dataset evidence entrypoint.
- Added immutable evidence types under `Videra.SurfaceCharts.Avalonia.Controls`:
  - `Plot3DDatasetEvidence`
  - `Plot3DSeriesDatasetEvidence`
  - `SurfaceAxisDatasetEvidence`
  - `SurfaceValueRangeDatasetEvidence`
  - `ScatterColumnarSeriesDatasetEvidence`
- Surface and waterfall evidence is derived from the current `Plot3DSeries.SurfaceSource.Metadata`.
- Scatter evidence is derived from the current `ScatterChartData.Metadata`, aggregate point counts, columnar counts, streaming counters, FIFO capacity, and pickable counts.
- Evidence reads the current Plot series list on each call, so add/remove/clear lifecycle changes produce deterministic updated identities and active-series state.

## Requirement Coverage

- **DATA-01:** Surface, waterfall, and scatter series report deterministic identity, name, kind, and shape metadata.
- **DATA-02:** Evidence includes dimensions or point counts, ranges, units, axis scale, sampling profile, precision profile, FIFO, streaming, and pickable information where relevant.
- **DATA-03:** Evidence is immutable data derived from public dataset metadata and current Plot series, not mutable runtime/backend internals.
- **DATA-04:** Focused lifecycle tests cover add/remove/empty evidence behavior.

## Handoff

- Phase 357 still owns chart output/report contracts.
- Phase 359 can consume `Plot3D.CreateDatasetEvidence()` for demo/support artifacts without needing renderer state.
