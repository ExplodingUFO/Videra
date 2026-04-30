# Phase 364: Demo Smoke Doctor Snapshot Evidence - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Bead:** Videra-lu9.4

<domain>
## Phase Boundary

Refresh demo, consumer smoke, support summaries, and Doctor parsing around snapshot artifacts and manifests. Demo exposes a bounded snapshot action and support-summary fields. Consumer smoke validates snapshot manifest evidence. Doctor parses snapshot present/missing/unavailable/failed states without launching UI.

</domain>

<decisions>
## Implementation Decisions

### Evidence Integration
- Demo: Add `CaptureSnapshot` button in demo MainWindow that calls `Plot.CaptureSnapshotAsync` — bounded, single action
- Support summary: Add `SnapshotStatus`, `SnapshotPath`, `SnapshotManifest*` fields to support summary format
- Doctor: Parse `SnapshotStatus` (present/missing/unavailable/failed) + manifest fields — not full manifest JSON
- Consumer smoke: Call `CaptureSnapshotAsync` after chart ready, validate manifest in support summary

### the agent's Discretion
- Follow existing support summary format pattern (line-based key:value)
- Doctor parsing follows existing `Get-SurfaceChartsSupportReport` pattern
- Demo snapshot action bounded to single click — no editor, no batch, no configuration UI
- Consumer smoke snapshot runs after chart readiness confirmed

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `WorkbenchSupportCapture` — existing chart evidence formatter pattern
- `SurfaceChartProbeEvidenceFormatter` — reference for formatting evidence as text
- `Invoke-VideraDoctor.ps1` — Doctor parsing with `Get-SurfaceChartsSupportReport` function
- Consumer smoke `MainWindow.axaml.cs` — existing support summary writing pattern
- Phase 362 contract types: `PlotSnapshotRequest`, `PlotSnapshotResult`, `PlotSnapshotManifest`
- Phase 363 implementation: `Plot3D.CaptureSnapshotAsync`

### Established Patterns
- Support summary: line-based `Key: Value` format in `surfacecharts-support-summary.txt`
- Doctor: PowerShell functions that parse support summary fields
- Consumer smoke: writes support summary after chart readiness confirmed
- Demo: support report builder in `DemoSupportReportBuilder.cs`

### Integration Points
- `Plot3D.CaptureSnapshotAsync` — called from demo and smoke
- `PlotSnapshotResult` — success/failure status for summary
- `PlotSnapshotManifest` — metadata fields for Doctor parsing
- Doctor `Get-SurfaceChartsSupportReport` — needs snapshot field parsing

</code_context>

<specifics>
## Specific Ideas

- Phase 363 implemented CaptureSnapshotAsync; Phase 364 integrates it into demo/smoke/Doctor
- Support summary snapshot fields: SnapshotStatus (present/failed/unavailable), SnapshotPath, SnapshotWidth, SnapshotHeight, SnapshotFormat, SnapshotBackground, SnapshotOutputEvidenceKind, SnapshotDatasetEvidenceKind, SnapshotActiveSeriesIdentity, SnapshotCreatedUtc
- Doctor should report snapshot as a new section alongside existing support report

</specifics>

<deferred>
## Deferred Ideas

- Snapshot gallery/history in demo (future)
- Snapshot comparison/visual regression (future)
- Batch snapshot capture (future)

</deferred>
