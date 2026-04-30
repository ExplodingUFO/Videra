# Support Evidence Recipe

This recipe explains what to copy from `Videra.SurfaceCharts.Demo` when filing
or triaging SurfaceCharts issues. The support summary is evidence-only text from
the active demo path. It is intentionally separate from
`VideraDiagnosticsSnapshotFormatter`.

## Copy Workflow

1. Run the sample app.
2. Reproduce the issue on the matching proof path:
   `Start here: In-memory first chart`, `Explore next: Cache-backed streaming`,
   `Try next: Analytics proof`, `Try next: Waterfall proof`,
   `Try next: Scatter proof`, `Try next: Bar chart proof`, or
   `Try next: Contour plot proof`.
3. Click `Copy support summary`.
4. Attach the copied text as `surfacecharts-support-summary.txt`.
5. If the issue is about image export, also attach the PNG from the
   `Plot.SavePngAsync` / `CaptureSnapshotAsync` path and the snapshot manifest
   fields visible in the support summary.

## Required Fields

The copied text should begin with `SurfaceCharts support summary` and include
these fields when the active proof path supports them:

- `GeneratedUtc`
- `EvidenceKind`
- `EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.`
- `ChartControl`
- `EnvironmentRuntime`
- `AssemblyIdentity`
- `BackendDisplayEnvironment`
- `CacheLoadFailure`
- `Plot path`
- `Plot details`
- `SeriesCount`
- `ActiveSeries`
- `ChartKind`
- `ColorMap`
- `PrecisionProfile`
- `OutputEvidenceKind`
- `OutputCapabilityDiagnostics`
- `SnapshotStatus`
- `SnapshotPath`
- `SnapshotWidth`
- `SnapshotHeight`
- `SnapshotFormat`
- `SnapshotBackground`
- `SnapshotOutputEvidenceKind`
- `SnapshotDatasetEvidenceKind`
- `SnapshotActiveSeriesIdentity`
- `SnapshotCreatedUtc`
- `DatasetEvidenceKind`
- `DatasetSeriesCount`
- `DatasetActiveSeriesIndex`
- `DatasetActiveSeriesMetadata`

Scatter proofs add deterministic scenario fields such as `ScenarioId`,
`ScenarioName`, `ScenarioUpdateMode`, `ScenarioFifoCapacity`, and
`ScenarioPickable`. Contour proofs add `ContourRenderingStatus`. Bar proofs
surface grouped-bar status through the rendering-path panel: `Series`,
`Categories`, `Bars`, and `Layout`.

## Truth Rules

The support summary describes the active chart state. It does not claim stable
performance numbers, benchmark guarantees, PDF/vector export, a new backend, or
a fallback/downshift path. If the cache-backed source fails to load, the summary
keeps `CacheLoadFailure` visible and the previous Plot path remains active; the
demo does not silently switch scenarios to hide the failure.

For packaged smoke handoff, collect `consumer-smoke-result.json`,
`diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`,
`chart-snapshot.png`, and trace/stdout/stderr/environment logs.
