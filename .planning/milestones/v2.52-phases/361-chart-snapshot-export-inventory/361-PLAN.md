---
phase: 361-chart-snapshot-export-inventory
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md
autonomous: true
requirements:
  - INV-01
  - INV-02
  - INV-03
  - INV-04
  - VER-01
  - VER-03
must_haves:
  truths:
    - "Inventory covers all chart export/snapshot surfaces in VideraChartView, Plot3D, and evidence contracts"
    - "Output evidence and dataset evidence contracts are cataloged with their current snapshot capabilities"
    - "Demo, support, consumer smoke, and Doctor surfaces are mapped"
    - "Tests and guardrails related to chart output/export are listed"
    - "Gaps are classified as implement, document, defer, or reject"
    - "Non-goals explicitly reject old chart views, direct Source, PDF/vector export, generic plotting engine, backend expansion, hidden fallback/downshift, and god-code"
    - "Handoff identifies implementation owners and write boundaries"
  artifacts:
    - path: ".planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md"
      provides: "Complete inventory with gap classification, target examples, non-goals, and handoff"
      contains: "## Inventory"
  key_links:
    - from: ".planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md"
      to: "Phase 362-365 implementation plans"
      via: "Gap classification drives implementation priorities"
      pattern: "implement|document|defer|reject"
---

<objective>
Map the current chart screenshot/export, output evidence, dataset evidence, demo/support, consumer smoke, Doctor, docs, tests, and guardrail surfaces before changing code. Classify bitmap snapshot/export gaps as implement, document, defer, or reject. Define target examples and non-goals. Produce SUMMARY.md with handoff for Phase 362+.

Purpose: Establish a complete baseline of existing chart export/evidence surfaces so implementation phases know exactly what exists, what's missing, and what's out of scope.
Output: `.planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md`
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/phases/361-chart-snapshot-export-inventory/361-CONTEXT.md
</context>

<tasks>

<task type="auto">
  <name>Task 1: Scan and Inventory All Chart Export/Snapshot Surfaces</name>
  <files>
    .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md
  </files>
  <read_first>
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Properties.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs
    - src/Videra.SurfaceCharts.Core/SurfaceChartOutputEvidence.cs
    - src/Videra.Avalonia/Runtime/VideraSnapshotExportService.cs
    - src/Videra.Avalonia/Controls/VideraSnapshotExportResult.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartProbeEvidence.cs
    - samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs
    - samples/Videra.Demo/Services/DemoSupportReportBuilder.cs
    - samples/Videra.Demo/Services/PerformanceLabEvidenceSnapshotBuilder.cs
    - smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs
    - tests/Videra.Core.Tests/Repository/VideraDoctorRepositoryTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs
    - tests/Videra.Avalonia.Tests/Rendering/VideraSnapshotExportServiceTests.cs
    - scripts/Invoke-VideraDoctor.ps1
  </read_first>
  <action>
    Scan all chart export/snapshot surfaces in the codebase and produce a structured inventory in SUMMARY.md. The inventory must cover these sections:

    **1. Chart Screenshot/Export Surfaces:**
    - `VideraSnapshotExportService` (Videra.Avalonia.Runtime) — the existing Avalonia-level snapshot export path for 3D viewer scenes. Note it operates on `VideraEngine` scene objects, NOT on `VideraChartView.Plot` SurfaceCharts. Catalog its signature: `ExportAsync(path, width, height, VideraEngine, sceneObjects, selectionState, annotations, measurements, overlayState, preferredReadbackBackend, logger, cancellationToken)`. Note it produces PNG via SkiaSharp, uses software backend fallback, and renders overlays (selection outlines, labels, measurements).
    - `VideraSnapshotExportResult` (Videra.Avalonia.Controls) — result type with Path, Width, Height, Duration, Failure, Succeeded. Note this is for the Videra viewer, not for SurfaceCharts.
    - `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` — currently returns diagnostics for ImageExport, PdfExport, VectorExport all as `isSupported: false`. This is the gap: chart snapshot export is explicitly marked unsupported.

    **2. Output Evidence Contract:**
    - `Plot3DOutputEvidence` (chart-local) — fields: EvidenceKind, SeriesCount, ActiveSeriesIndex/Name/Kind/Identity, ColorMapStatus, ColorMapEvidence, PrecisionProfile, RenderingEvidence, OutputCapabilityDiagnostics.
    - `Plot3DRenderingEvidence` — fields: RenderingKind, BackendKind, IsReady, IsFallback, FallbackReason, ViewWidth, ViewHeight. Created from `SurfaceChartRenderingStatus` or `ScatterChartRenderingStatus`.
    - `Plot3DColorMapStatus` enum: Applied, NotApplicable, Unavailable.
    - `SurfaceChartOutputEvidence` (Core) — palette/precision evidence: EvidenceKind, PaletteName, ColorStops, PrecisionProfile, SampleFormattedLabels.

    **3. Dataset Evidence Contract:**
    - `Plot3DDatasetEvidence` — fields: EvidenceKind, PlotRevision, ActiveSeriesIndex, PrecisionProfile, Series (list of Plot3DSeriesDatasetEvidence).
    - `Plot3DSeriesDatasetEvidence` — per-series: Index, IsActive, Identity, Name, Kind, Width, Height, SampleCount, SeriesCount, PointCount, columnar scatter fields, axis metadata, ValueRange, SamplingProfile, ColumnarSeries.
    - `SurfaceAxisDatasetEvidence` — Label, Unit, Minimum, Maximum, ScaleKind.
    - `SurfaceValueRangeDatasetEvidence` — Minimum, Maximum.
    - `ScatterColumnarSeriesDatasetEvidence` — Index, Label, PointCount, IsSortedX, ContainsNaN, Pickable, FifoCapacity, batch counts, dropped counts.

    **4. Demo/Support/Consumer Smoke:**
    - `WorkbenchSupportCapture` (AvaloniaWorkbenchSample) — formats scene evidence, chart evidence (output + probe), interaction evidence, diagnostics snapshot. No snapshot artifact or manifest fields.
    - `DemoSupportReportBuilder` (Videra.Demo) — builds diagnostics bundle and minimal reproduction metadata. No snapshot artifact fields.
    - `PerformanceLabEvidenceSnapshotBuilder` (Videra.Demo) — builds Performance Lab snapshot evidence text. No bitmap snapshot artifact.
    - `SurfaceCharts.ConsumerSmoke` — creates support summary with output evidence kind, dataset evidence kind, rendering status, chart control info. No snapshot artifact or manifest. Note: `OutputCapabilityDiagnostics: ImageExport=plot-output.export.image.unsupported;Supported=False` is explicitly recorded.

    **5. Doctor Parsing Surfaces:**
    - `Invoke-VideraDoctor.ps1` — parses `surfaceChartsSupportReport` with fields: status (present/missing/unavailable), supportSummaryPath, renderingStatusPresent, isStructuredComplete, missingFields, generatedAtUtc, evidenceKind, chartControl, etc. Also parses `performanceLabVisualEvidence` with status, manifestPath, screenshotPaths, diagnosticsPaths, entries.
    - Doctor tests verify: present/partial/missing/unavailable states for both visual evidence and support report.
    - Note: Doctor currently has NO snapshot artifact parsing for chart-local PNG exports.

    **6. Tests and Guardrails:**
    - `SurfaceChartOutputEvidenceTests` — tests palette stops, precision profile, non-finite rejection.
    - `VideraSnapshotExportServiceTests` — tests Videra viewer snapshot export (not chart-local).
    - `VideraDoctorRepositoryTests` — verifies Doctor script structure, report format, visual evidence parsing, support report parsing.
    - `SurfaceChartProbeEvidenceTests` — probe evidence formatting.
    - Guardrails: `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` explicitly marks ImageExport/PdfExport/VectorExport as unsupported. Doctor tests verify `OutputCapabilityDiagnostics` field in support summary.

    **7. Existing Snapshot Infrastructure (NOT chart-local):**
    - `RenderPipelineSnapshot`, `RenderSessionSnapshot`, `RenderCapabilitySnapshot` — these are rendering pipeline snapshots, not chart export artifacts.
    - `VideraDiagnosticsSnapshotFormatter` — formats backend diagnostics, not chart output.
    - `PerformanceLabVisualEvidence*` — Performance Lab screenshot capture tooling (separate from chart snapshot export).

    Write all findings to SUMMARY.md under a `## Inventory` heading with subsections for each surface category. For each surface, record: file path, public API surface, current snapshot/export capability, and gap status.
  </action>
  <verify>
    <automated>grep -c "## Inventory" .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md</automated>
  </verify>
  <done>SUMMARY.md contains complete inventory of all chart export/snapshot surfaces with file paths, API surfaces, current capabilities, and gap status for each category.</done>
</task>

<task type="auto">
  <name>Task 2: Classify Gaps, Define Non-Goals, Target Examples, and Handoff</name>
  <files>
    .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md
  </files>
  <read_first>
    - .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md
    - .planning/ROADMAP.md
    - .planning/REQUIREMENTS.md
    - .planning/phases/361-chart-snapshot-export-inventory/361-CONTEXT.md
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
  </read_first>
  <action>
    Append to SUMMARY.md the following sections after the Inventory:

    **## Gap Classification:**
    Classify each identified gap as implement, document, defer, or reject:

    - **implement:**
      - Chart-local Plot-owned snapshot request/result contract (gap: no Plot-level snapshot API exists; `VideraSnapshotExportService` is viewer-level only)
      - PNG/bitmap artifact production from `VideraChartView.Plot` (gap: `CreateUnsupportedExportDiagnostics` marks ImageExport as unsupported)
      - Deterministic snapshot manifest metadata linking output evidence, dataset evidence, active series identity, dimensions, artifact identity (gap: no manifest contract exists)
      - Doctor parsing for chart-local snapshot artifacts (gap: Doctor only parses PerformanceLab visual evidence, not chart snapshot artifacts)
      - Consumer smoke snapshot validation (gap: consumer smoke records `ImageExport=unsupported` but cannot produce/validate snapshot artifacts)
      - Demo snapshot action and support summary fields (gap: no snapshot action or artifact fields in demo/support)

    - **document:**
      - Snapshot export scope boundaries (what's in/out)
      - Plot-owned API usage examples

    - **defer:**
      - PDF/vector export (future milestone)
      - Publication layout editor
      - Visual-regression screenshot gates
      - Additional chart families beyond surface/waterfall/scatter

    - **reject:**
      - Old `SurfaceChartView`/`WaterfallChartView`/`ScatterChartView` public views
      - Direct public `Source` API
      - Generic plotting engine semantics
      - Backend expansion or renderer rewrite
      - Hidden fallback/downshift behavior
      - God-code workbench
      - Compatibility shims for removed alpha chart APIs

    **## Target Examples:**
    Define concise target examples showing what Plot-owned snapshot export and manifest usage looks like AFTER implementation (Phase 362-363). Use pseudocode-style C#:

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

    **## Non-Goals:**
    Explicitly list what v2.52 does NOT do (per INV-04, REQUIREMENTS.md Out of Scope table):

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

    **## Handoff:**
    Identify implementation owners and write boundaries for Phase 362+:

    | Phase | Owner Surface | Write Boundary | Key Contracts |
    |-------|---------------|----------------|---------------|
    | 362 | `Plot3D` + new types in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` | Plot snapshot request/result types, manifest contract | `PlotSnapshotRequest`, `PlotSnapshotResult`, `PlotSnapshotManifest` |
    | 363 | `VideraChartView` + Avalonia rendering path | Snapshot capture implementation | `Plot.CaptureSnapshotAsync()`, PNG artifact production |
    | 364 | Demo/Smoke/Doctor scripts | Support summary fields, Doctor parsing, consumer smoke validation | Updated support summary format, Doctor report schema |
    | 365 | Docs/guardrails/scripts | Documentation, guardrail scripts, public API contract | Guardrail scripts, docs updates |

    Record that `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` must be updated in Phase 362-63 to report ImageExport as supported once the snapshot path exists.

    Update the `## Traceability` section to mark Phase 361 requirements (INV-01 through INV-04, VER-01, VER-03) as addressed.
  </action>
  <verify>
    <automated>grep -c "## Gap Classification" .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md && grep -c "## Non-Goals" .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md && grep -c "## Handoff" .planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md</automated>
  </verify>
  <done>SUMMARY.md contains gap classification (implement/document/defer/reject), target examples showing Plot-owned snapshot API, non-goals table rejecting out-of-scope work, and handoff with implementation owners and write boundaries for Phase 362-365.</done>
</task>

</tasks>

<verification>
- SUMMARY.md exists at `.planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md`
- Contains `## Inventory` with subsections for all 7 surface categories
- Contains `## Gap Classification` with implement/document/defer/reject labels
- Contains `## Target Examples` with Plot-owned snapshot API pseudocode
- Contains `## Non-Goals` with explicit rejection table
- Contains `## Handoff` with implementation owners and write boundaries
- All Phase 361 requirements (INV-01, INV-02, INV-03, INV-04, VER-01, VER-03) addressed
</verification>

<success_criteria>
1. Every chart export/snapshot surface in the codebase is cataloged with file path and current capability
2. Gaps are classified as implement (6 items), document (2 items), defer (4 items), reject (8 items)
3. Target examples show concise Plot-owned snapshot export and manifest usage
4. Non-goals explicitly reject all 8 out-of-scope categories from REQUIREMENTS.md
5. Handoff identifies 4 implementation phases with owner surfaces and write boundaries
</success_criteria>

<output>
After completion, create `.planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md`
</output>
