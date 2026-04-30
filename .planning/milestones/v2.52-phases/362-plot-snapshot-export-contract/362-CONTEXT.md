# Phase 362: Plot Snapshot Export Contract - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Bead:** Videra-lu9.2

<domain>
## Phase Boundary

Add a bounded chart-local snapshot request/result contract owned by Plot APIs. The contract captures dimensions, scale/DPI, background behavior, and target semantics without backend internals. Result includes deterministic manifest metadata linked to output and dataset evidence. Unsupported formats return explicit diagnostics rather than fallback behavior.

Key finding from Phase 361: `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` currently marks ImageExport as `isSupported: false` with diagnostic code `plot-output.export.image.unsupported`. Phase 362 creates the contract types that Phase 363 will implement.

</domain>

<decisions>
## Implementation Decisions

### Contract Shape
- Request type: `PlotSnapshotRequest` — matches existing `Plot3D*` naming convention
- Format enum: `PlotSnapshotFormat { Png }` — PNG only for v2.52, extensible for future formats
- Background enum: `PlotSnapshotBackground { Transparent, Opaque }` — minimal, covers chart needs
- Result type: `PlotSnapshotResult` with Path + Manifest — immutable record class

### Manifest and Diagnostics
- Manifest: `PlotSnapshotManifest` as separate type — reusable for Doctor/consumer smoke parsing
- Evidence linking: Manifest includes OutputEvidenceKind + DatasetEvidenceKind strings (not full objects)
- Unsupported format handling: Return `PlotSnapshotResult` with `Succeeded=false` + diagnostic code (no exceptions)
- Method name: `CaptureSnapshotAsync` on Plot3D — clear action verb

### the agent's Discretion
- Namespace: types go in `Videra.SurfaceCharts.Avalonia.Controls` alongside existing Plot3D types
- Internal constructor pattern: follow `Plot3DOutputEvidence` convention (internal ctor, public properties)
- No dependency on Videra.Avalonia or Videra.Core — chart-local only

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Plot3DOutputEvidence` — reference pattern for chart-local evidence types (internal ctor, public properties, EvidenceKind pattern)
- `Plot3DOutputCapabilityDiagnostic` — diagnostic pattern with Capability/IsSupported/DiagnosticCode/Message
- `Plot3DDatasetEvidence` — reference for dataset evidence structure
- `Plot3DRenderingEvidence` — reference for rendering status evidence

### Established Patterns
- Chart-local types live in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`
- Evidence types use `internal` constructors with `ArgumentNullException.ThrowIfNull` validation
- `EvidenceKind` strings use kebab-case: "plot-3d-output", "plot-3d-dataset"
- Diagnostic codes use dot-separated: "plot-output.export.image.unsupported"

### Integration Points
- `Plot3D` — main plot API, will host `CaptureSnapshotAsync` method
- `VideraChartView.Plot` — public entry point that exposes Plot3D
- `Plot3DOutputEvidence` — manifest links to this via EvidenceKind
- `Plot3DDatasetEvidence` — manifest links to this via EvidenceKind

</code_context>

<specifics>
## Specific Ideas

- Phase 361 SUMMARY.md contains target examples showing the expected API shape
- `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` must be updated to reflect ImageExport as supported once Phase 363 implements the capture path
- The manifest should include: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc

</specifics>

<deferred>
## Deferred Ideas

- PDF/vector export (future milestone)
- BMP/WebP format support (future)
- Custom color background option (future)

</deferred>
