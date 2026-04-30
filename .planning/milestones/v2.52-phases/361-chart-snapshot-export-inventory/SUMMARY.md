---
phase: 361-chart-snapshot-export-inventory
plan: 01
subsystem: SurfaceCharts
tags: [inventory, snapshot, export, evidence, chart]
depends_on: []
provides: [chart-snapshot-export-inventory]
affects: [362, 363, 364, 365]
tech_stack:
  added: []
  patterns: [evidence-contract, snapshot-manifest, chart-local-export]
key_files:
  created:
    - .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md
  modified: []
decisions:
  - "Chart-local Plot-owned snapshot export is the target model (not viewer-level)"
  - "VideraSnapshotExportService is viewer-only; chart snapshot needs new Plot-level contract"
  - "Plot3DOutputCapabilityDiagnostic explicitly marks ImageExport as unsupported — this is the primary gap"
  - "Doctor parses PerformanceLab visual evidence but has no chart snapshot artifact parsing"
  - "Consumer smoke records ImageExport=unsupported but cannot produce/validate snapshot artifacts"
metrics:
  duration: "pending"
  completed: "2026-04-29"
  tasks_completed: 2
  tasks_total: 2
---

# Phase 361: Chart Snapshot Export Inventory Summary

Complete inventory of chart export/snapshot surfaces across VideraChartView, Plot3D, evidence contracts, demo/support, consumer smoke, Doctor, tests, and guardrails. Classifies bitmap snapshot/export gaps as implement, document, defer, or reject. Defines target examples, non-goals, and handoff for Phase 362+.

## Inventory

### 1. Chart Screenshot/Export Surfaces

#### VideraSnapshotExportService (viewer-level)

- **File:** `src/Videra.Avalonia/Runtime/VideraSnapshotExportService.cs`
- **Namespace:** `Videra.Avalonia.Runtime`
- **Visibility:** `internal static`
- **Public API:**
  ```csharp
  Task ExportAsync(
      string path,
      uint width,
      uint height,
      VideraEngine sourceEngine,
      IReadOnlyList<Object3D> sceneObjects,
      VideraSelectionState selectionState,
      IReadOnlyList<VideraAnnotation> annotations,
      IReadOnlyList<VideraMeasurement> measurements,
      VideraViewOverlayState overlayState,
      ISoftwareBackend? preferredReadbackBackend,
      ILogger logger,
      CancellationToken cancellationToken)
  ```
- **Current capability:** Produces PNG via SkiaSharp. Operates on `VideraEngine` scene objects (viewer-level 3D engine), NOT on `VideraChartView.Plot` SurfaceCharts. Uses software backend fallback. Renders overlays (selection outlines, labels, measurements).
- **Gap status:** This is the viewer-level snapshot path. It does NOT operate on SurfaceCharts/Plot3D. A new chart-local Plot-owned snapshot contract is needed.

#### VideraSnapshotExportResult (viewer-level result)

- **File:** `src/Videra.Avalonia/Controls/VideraSnapshotExportResult.cs`
- **Namespace:** `Videra.Avalonia.Controls`
- **Public API:** `Path`, `Width`, `Height`, `Duration`, `Failure`, `Succeeded`. Factory methods: `Success()`, `Failed()`.
- **Current capability:** Result type for viewer snapshot export. Records path, dimensions, duration, and failure.
- **Gap status:** This is for the Videra viewer, not for SurfaceCharts. A new chart-local result type is needed.

#### Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs` (lines 152-168)
- **Namespace:** `Videra.SurfaceCharts.Avalonia.Controls`
- **Public API:** `internal static IReadOnlyList<Plot3DOutputCapabilityDiagnostic> CreateUnsupportedExportDiagnostics()`
- **Current capability:** Returns diagnostics for three capabilities:
  - `ImageExport` — `isSupported: false`, code: `plot-output.export.image.unsupported`
  - `PdfExport` — `isSupported: false`, code: `plot-output.export.pdf.unsupported`
  - `VectorExport` — `isSupported: false`, code: `plot-output.export.vector.unsupported`
- **Gap status:** **PRIMARY GAP.** Chart snapshot export is explicitly marked as unsupported. This method must be updated in Phase 362-63 to report `ImageExport` as supported once the snapshot path exists.

### 2. Output Evidence Contract

#### Plot3DOutputEvidence (chart-local)

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs`
- **Namespace:** `Videra.SurfaceCharts.Avalonia.Controls`
- **Public API (properties):**
  - `EvidenceKind` → `"plot-3d-output"` (stable)
  - `SeriesCount` — number of series attached to plot
  - `ActiveSeriesIndex` — draw-order index of active series (-1 if empty)
  - `ActiveSeriesName` — optional host-facing name
  - `ActiveSeriesKind` — `Plot3DSeriesKind?` (Surface/Waterfall/Scatter)
  - `ActiveSeriesIdentity` — deterministic identity: `{Kind}:{Name}:{Index}`
  - `ColorMapStatus` — `Plot3DColorMapStatus` enum (Applied/NotApplicable/Unavailable)
  - `ColorMapEvidence` — `SurfaceChartOutputEvidence?` (palette/precision for surface/waterfall)
  - `PrecisionProfile` — deterministic numeric precision description
  - `RenderingEvidence` — `Plot3DRenderingEvidence?`
  - `OutputCapabilityDiagnostics` — `IReadOnlyList<Plot3DOutputCapabilityDiagnostic>`
- **Current capability:** Provides deterministic chart-local output evidence for text/metadata support. No snapshot artifact field exists.
- **Gap status:** No snapshot artifact or manifest metadata fields. Needs extension for Phase 362-63.

#### Plot3DRenderingEvidence

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs` (lines 174-257)
- **Public API (properties):**
  - `RenderingKind` — `"surface-rendering-status"` or `"scatter-rendering-status"`
  - `BackendKind` — `SurfaceChartRenderBackendKind`
  - `IsReady`, `IsFallback`, `FallbackReason`
  - `ViewWidth`, `ViewHeight`
- **Current capability:** Created from `SurfaceChartRenderingStatus` or `ScatterChartRenderingStatus`. Records rendering backend state.
- **Gap status:** No snapshot dimension or artifact identity fields. Will be complemented by manifest in Phase 362.

#### Plot3DColorMapStatus enum

- **Values:** `Applied`, `NotApplicable`, `Unavailable`
- **Current capability:** Indicates whether color-map evidence applies to the active output.
- **Gap status:** Complete for current scope.

#### SurfaceChartOutputEvidence (Core)

- **File:** `src/Videra.SurfaceCharts.Core/SurfaceChartOutputEvidence.cs`
- **Namespace:** `Videra.SurfaceCharts.Core`
- **Public API (properties):**
  - `EvidenceKind` — stable evidence kind string
  - `PaletteName` — palette name
  - `ColorStops` — `IReadOnlyList<string>` (ARGB hex)
  - `PrecisionProfile` — deterministic precision description
  - `SampleFormattedLabels` — `IReadOnlyList<string>`
- **Current capability:** Palette/precision evidence. Used by `Plot3DOutputEvidence.ColorMapEvidence`.
- **Gap status:** Complete for current scope. Will be linked from manifest in Phase 362.

### 3. Dataset Evidence Contract

#### Plot3DDatasetEvidence

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
- **Namespace:** `Videra.SurfaceCharts.Avalonia.Controls`
- **Public API (properties):**
  - `EvidenceKind` → `"Plot3DDatasetEvidence"` (stable)
  - `PlotRevision` — monotonically increasing plot revision
  - `ActiveSeriesIndex` — draw-order index (-1 if empty)
  - `PrecisionProfile` — deterministic precision description
  - `Series` — `IReadOnlyList<Plot3DSeriesDatasetEvidence>`
- **Current capability:** Deterministic dataset evidence snapshot from Plot-authored series.
- **Gap status:** Complete for current scope. Will be linked from manifest in Phase 362.

#### Plot3DSeriesDatasetEvidence

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs` (lines 78-359)
- **Public API (properties):**
  - `Index`, `IsActive`, `Identity` (format: `PlotSeries[{Index}]:{Kind}:{Name}`)
  - `Name`, `Kind` (`Plot3DSeriesKind`)
  - `Width`, `Height`, `SampleCount`, `SeriesCount`, `PointCount`
  - `ColumnarSeriesCount`, `ColumnarPointCount`, `PickablePointCount`
  - `StreamingAppendBatchCount`, `StreamingReplaceBatchCount`, `StreamingDroppedPointCount`, `LastStreamingDroppedPointCount`, `ConfiguredFifoCapacity`
  - `HorizontalAxis`, `VerticalAxis`, `DepthAxis` — `SurfaceAxisDatasetEvidence?`
  - `ValueRange` — `SurfaceValueRangeDatasetEvidence?`
  - `SamplingProfile` — deterministic string
  - `ColumnarSeries` — `IReadOnlyList<ScatterColumnarSeriesDatasetEvidence>`
- **Current capability:** Per-series dataset evidence covering surface, waterfall, and scatter.
- **Gap status:** Complete for current scope.

#### SurfaceAxisDatasetEvidence

- **Public API:** `Label`, `Unit`, `Minimum`, `Maximum`, `ScaleKind` (`SurfaceAxisScaleKind`)
- **Gap status:** Complete.

#### SurfaceValueRangeDatasetEvidence

- **Public API:** `Minimum`, `Maximum`
- **Gap status:** Complete.

#### ScatterColumnarSeriesDatasetEvidence

- **Public API:** `Index`, `Label`, `PointCount`, `IsSortedX`, `ContainsNaN`, `Pickable`, `FifoCapacity`, `AppendBatchCount`, `ReplaceBatchCount`, `TotalAppendedPointCount`, `TotalDroppedPointCount`, `LastDroppedPointCount`
- **Gap status:** Complete.

### 4. Demo/Support/Consumer Smoke

#### WorkbenchSupportCapture (AvaloniaWorkbenchSample)

- **File:** `samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs`
- **Namespace:** `Videra.AvaloniaWorkbenchSample`
- **Public API:**
  - `FormatSceneEvidence(WorkbenchSceneEvidence)` — scene status, node/instance counts
  - `FormatChartEvidence(WorkbenchChartEvidence)` — output evidence + probe evidence formatting
  - `FormatInteractionEvidence(WorkbenchInteractionEvidence)` — viewer interaction evidence
  - `FormatSupportCapture(generatedUtc, scene, interaction, chart, diagnostics)` — full support capture
- **Current capability:** Formats scene evidence, chart evidence (output + probe), interaction evidence, diagnostics snapshot. No snapshot artifact or manifest fields.
- **Gap status:** No snapshot artifact fields. Needs extension for Phase 364.

#### DemoSupportReportBuilder (Videra.Demo)

- **File:** `samples/Videra.Demo/Services/DemoSupportReportBuilder.cs`
- **Namespace:** `Videra.Demo.Services`
- **Public API:**
  - `FormatImportReport(ModelLoadBatchResult?)` — import report
  - `BuildDiagnosticsBundle(diagnostics, capabilities, loadedModelCount, lastImport, settings)` — diagnostics bundle
  - `BuildMinimalReproduction(diagnostics, lastImport, settings)` — minimal reproduction metadata
- **Current capability:** Builds diagnostics bundle and minimal reproduction metadata. No snapshot artifact fields.
- **Gap status:** No snapshot action or artifact fields. Needs extension for Phase 364.

#### PerformanceLabEvidenceSnapshotBuilder (Videra.Demo)

- **File:** `samples/Videra.Demo/Services/PerformanceLabEvidenceSnapshotBuilder.cs`
- **Namespace:** `Videra.Demo.Services`
- **Public API:** `Build(scenario, mode, objectCount, pickable, diagnosticsText, diagnostics, generatedUtc)`
- **Current capability:** Builds Performance Lab snapshot evidence text (EvidenceKind: `PerformanceLabDatasetProof`). No bitmap snapshot artifact.
- **Gap status:** No bitmap snapshot artifact. This is Performance Lab evidence, separate from chart snapshot export.

#### SurfaceCharts.ConsumerSmoke

- **File:** `smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs`
- **Namespace:** `Videra.SurfaceCharts.ConsumerSmoke.Views`
- **Key methods:**
  - `CreateSupportSummary()` — builds support summary text with: EvidenceKind, ChartControl, OutputEvidenceKind, OutputCapabilityDiagnostics, DatasetEvidenceKind, rendering status, etc.
  - `CreateDiagnosticsSnapshot()` — diagnostics text
  - `CreateReport()` — consumer smoke report record
- **Current capability:** Creates support summary with output evidence kind, dataset evidence kind, rendering status, chart control info. No snapshot artifact or manifest. Explicitly records: `OutputCapabilityDiagnostics: ImageExport=plot-output.export.image.unsupported;Supported=False`
- **Gap status:** Records ImageExport as unsupported but cannot produce/validate snapshot artifacts. Needs extension for Phase 364.

### 5. Doctor Parsing Surfaces

#### Invoke-VideraDoctor.ps1

- **File:** `scripts/Invoke-VideraDoctor.ps1`
- **Key functions:**
  - `Get-SurfaceChartsSupportReport` — parses `surfacecharts-support-summary.txt` with fields: status (present/missing/unavailable), supportSummaryPath, generatedAtUtc, evidenceKind, chartControl, environmentRuntime, assemblyIdentity, seriesCount, activeSeries, chartKind, colorMap, precisionProfile, outputEvidenceKind, outputCapabilityDiagnostics, datasetEvidenceKind, datasetSeriesCount, datasetActiveSeriesIndex, datasetActiveSeriesMetadata, renderingStatusPresent, isStructuredComplete, missingFields
  - `Get-PerformanceLabVisualEvidence` — parses `performance-lab-visual-evidence-manifest.json` with status, manifestPath, screenshotPaths, diagnosticsPaths, entries
- **Current capability:** Doctor parses Performance Lab visual evidence (screenshots, diagnostics) and SurfaceCharts support report (text fields). Doctor has NO snapshot artifact parsing for chart-local PNG exports.
- **Gap status:** Needs chart-local snapshot artifact/manifest parsing in Phase 364.

### 6. Tests and Guardrails

#### SurfaceChartOutputEvidenceTests

- **File:** `tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs`
- **Tests:**
  - `Create_ReportsPaletteStopsAndPrecisionProfile` — verifies palette stops, precision profile, sample labels
  - `Create_UsesColorMapRangeAsSampleLabels` — verifies color map range as labels
  - `Create_WithExplicitPrecisionProfile_UsesSuppliedFormatterWithoutFallback` — verifies custom precision profile
  - `Create_RejectsNonFiniteSampleLabels` — verifies NaN/Infinity rejection
- **Current capability:** Tests palette stops, precision profile, non-finite rejection.
- **Gap status:** No snapshot-related tests. Will need tests in Phase 363.

#### VideraSnapshotExportServiceTests

- **File:** `tests/Videra.Avalonia.Tests/Rendering/VideraSnapshotExportServiceTests.cs`
- **Tests:**
  - `ExportAsync_UsesPreferredReadbackBackend_WithoutCloneFallback` — verifies fast-path export with preferred readback backend
- **Current capability:** Tests Videra viewer snapshot export (not chart-local).
- **Gap status:** Tests viewer-level export only. Chart-local snapshot tests needed in Phase 363.

#### VideraDoctorRepositoryTests

- **File:** `tests/Videra.Core.Tests.Repository/VideraDoctorRepositoryTests.cs`
- **Tests:**
  - `VideraDoctor_ShouldRemainRepoOnlyAndNonMutating` — verifies script structure, docs, no mutating commands
  - `VideraDoctorDocs_ShouldReferenceActualValidationScriptsContractsAndArtifacts` — verifies docs reference actual scripts
  - `VideraDoctor_ShouldEmitHumanAndStructuredReports` — verifies JSON report structure, checks, validations, evidence packet, visual evidence, support report fields
  - `VideraDoctor_ShouldReportUnavailableValidationPrerequisites` — verifies unavailable state
  - `VideraDoctor_ShouldReportMissingVisualEvidenceAsOptionalEvidence` — verifies missing visual evidence state
  - `VideraDoctor_ShouldReportMissingSurfaceChartsSupportReportAsOptionalEvidence` — verifies missing support report state
  - `VideraDoctor_ShouldReportPresentSurfaceChartsSupportReport` — verifies present support report with all fields
  - `VideraDoctor_ShouldReportPartialSurfaceChartsSupportReport` — verifies partial support report with missing fields
  - `VideraDoctor_ShouldReportPresentVisualEvidenceArtifacts` — verifies present visual evidence with screenshots
  - `VideraDoctor_ShouldReportUnavailableVisualEvidenceStateDistinctFromMissing` — verifies unavailable vs missing distinction
- **Current capability:** Verifies Doctor script structure, report format, visual evidence parsing, support report parsing.
- **Gap status:** No chart snapshot artifact parsing tests. Will need extension in Phase 364.

#### SurfaceChartProbeEvidenceTests (referenced in CONTEXT.md)

- **File:** `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartProbeEvidence.cs` (formatter tests in test project)
- **Current capability:** Probe evidence formatting and readout creation.
- **Gap status:** Complete for current scope.

#### Guardrails

- `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` — explicitly marks ImageExport/PdfExport/VectorExport as unsupported
- Doctor tests verify `OutputCapabilityDiagnostics` field in support summary (e.g., `ImageExport=plot-output.export.image.unsupported;Supported=False`)
- **Gap status:** Guardrail exists but marks export as unsupported. Must be updated when snapshot path is implemented.

### 7. Existing Snapshot Infrastructure (NOT chart-local)

#### RenderPipelineSnapshot / RenderSessionSnapshot / RenderCapabilitySnapshot

- These are rendering pipeline snapshots (backend diagnostics), not chart export artifacts.
- Used by `VideraDiagnosticsSnapshotFormatter` for backend diagnostics formatting.
- **Gap status:** Not relevant to chart snapshot export.

#### PerformanceLabVisualEvidence*

- Performance Lab screenshot capture tooling (`Invoke-PerformanceLabVisualEvidence.ps1`)
- Produces PNG screenshots and diagnostics text for viewer/scatter scenarios
- Has manifest (`performance-lab-visual-evidence-manifest.json`) with entries, status, screenshotPaths, diagnosticsPaths
- **Gap status:** Separate from chart snapshot export. Performance Lab captures viewer-level screenshots, not chart-local SurfaceChart snapshots.

## Gap Classification

### implement

1. **Chart-local Plot-owned snapshot request/result contract** — No Plot-level snapshot API exists; `VideraSnapshotExportService` is viewer-level only
2. **PNG/bitmap artifact production from `VideraChartView.Plot`** — `CreateUnsupportedExportDiagnostics` marks ImageExport as unsupported
3. **Deterministic snapshot manifest metadata** — No manifest contract exists linking output evidence, dataset evidence, active series identity, dimensions, artifact identity
4. **Doctor parsing for chart-local snapshot artifacts** — Doctor only parses PerformanceLab visual evidence, not chart snapshot artifacts
5. **Consumer smoke snapshot validation** — Consumer smoke records `ImageExport=unsupported` but cannot produce/validate snapshot artifacts
6. **Demo snapshot action and support summary fields** — No snapshot action or artifact fields in demo/support

### document

1. **Snapshot export scope boundaries** — What's in/out for chart-local snapshot export
2. **Plot-owned API usage examples** — How to use the new Plot-owned snapshot API

### defer

1. **PDF/vector export** — Future milestone (not PNG/bitmap)
2. **Publication layout editor** — Out of scope for v2.52
3. **Visual-regression screenshot gates** — Out of scope for v2.52
4. **Additional chart families beyond surface/waterfall/scatter** — Out of scope for v2.52

### reject

1. **Old `SurfaceChartView`/`WaterfallChartView`/`ScatterChartView` public views** — `VideraChartView` is the single shipped control
2. **Direct public `Source` API** — `Plot.Add.*` is the public data-loading path
3. **Generic plotting engine semantics** — Scoped to current 3D chart model
4. **Backend expansion or renderer rewrite** — Snapshot stays chart-local
5. **Hidden fallback/downshift behavior** — Unsupported output = explicit diagnostics
6. **God-code workbench** — Demo/support stays bounded and sample-first
7. **Compatibility shims for removed alpha chart APIs** — Removed APIs stay removed
8. **Direct `VideraEngine`/`VideraSnapshotExportService` coupling** — Chart snapshot is Plot-owned, not viewer-engine-owned

## Target Examples

```csharp
// Phase 362 target: Plot-owned snapshot request
var request = new PlotSnapshotRequest(
    width: 1920,
    height: 1080,
    scale: 2.0,           // DPI scale
    background: PlotSnapshotBackground.Transparent,
    format: PlotSnapshotFormat.Png);

// Phase 363 target: Capture through Plot contract
var result = await chartView.Plot.CaptureSnapshotAsync(request);

// Result includes artifact + manifest
Console.WriteLine(result.Path);              // "/output/chart-snapshot.png"
Console.WriteLine(result.Manifest.Width);    // 1920
Console.WriteLine(result.Manifest.Height);   // 1080
Console.WriteLine(result.Manifest.OutputEvidenceKind);  // "plot-3d-output"
Console.WriteLine(result.Manifest.DatasetEvidenceKind); // "Plot3DDatasetEvidence"
Console.WriteLine(result.Manifest.ActiveSeriesIdentity); // "PlotSeries[0]:Surface:MyChart"
```

## Non-Goals

| Non-Goal | Reason | Gate |
|----------|--------|------|
| Old chart view compatibility | VideraChartView is the single shipped control | reject |
| Direct public Source API | Plot.Add.* is the public data-loading path | reject |
| PDF/vector export | This milestone is PNG/bitmap only | defer |
| Generic plotting engine | Scoped to current 3D chart model | reject |
| Backend expansion/rewrite | Snapshot stays chart-local | reject |
| Hidden fallback/downshift | Unsupported output = explicit diagnostics | reject |
| God-code workbench | Demo/support stays bounded and sample-first | reject |
| Compatibility shims | Removed alpha APIs stay removed | reject |

## Handoff

| Phase | Owner Surface | Write Boundary | Key Contracts |
|-------|---------------|----------------|---------------|
| 362 | `Plot3D` + new types in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` | Plot snapshot request/result types, manifest contract | `PlotSnapshotRequest`, `PlotSnapshotResult`, `PlotSnapshotManifest` |
| 363 | `VideraChartView` + Avalonia rendering path | Snapshot capture implementation | `Plot.CaptureSnapshotAsync()`, PNG artifact production |
| 364 | Demo/Smoke/Doctor scripts | Support summary fields, Doctor parsing, consumer smoke validation | Updated support summary format, Doctor report schema |
| 365 | Docs/guardrails/scripts | Documentation, guardrail scripts, public API contract | Guardrail scripts, docs updates |

**Note:** `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` must be updated in Phase 362-63 to report `ImageExport` as supported once the snapshot path exists.

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INV-01 | Phase 361 | Addressed — full inventory of all chart export/snapshot surfaces |
| INV-02 | Phase 361 | Addressed — gaps classified as implement/document/defer/reject |
| INV-03 | Phase 361 | Addressed — target examples show Plot-owned snapshot API |
| INV-04 | Phase 361 | Addressed — non-goals explicitly reject all 8 out-of-scope categories |
| VER-01 | Phase 361 | Addressed — Beads ownership, dependencies, status, handoff notes recorded |
| VER-03 | Phase 361 | Addressed — clean Beads status, phase completion documented |
