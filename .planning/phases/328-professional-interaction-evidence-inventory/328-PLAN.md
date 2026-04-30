# Phase 328: Professional Interaction Evidence Inventory - Plan

**Bead:** Videra-7l7  
**Goal:** Map exact implementation boundaries before changing APIs, samples, or docs.

## Inventory Findings

### Viewer Interaction Evidence

Current capability:

- `src/Videra.Avalonia/Controls/VideraInspectionState.cs` is the typed state root for camera, selection, annotations, clipping, snap mode, and measurements.
- `src/Videra.Avalonia/Runtime/VideraViewRuntime.Inspection.cs` captures/applies that state.
- `src/Videra.Avalonia/Controls/VideraInspectionBundleService.cs` serializes inspection state and writes diagnostics/snapshot artifacts.
- `src/Videra.Avalonia/Controls/Interaction/VideraInteractionDiagnostics.cs` exposes capability flags.
- `samples/Videra.InteractionSample` demonstrates the interaction workflow.

Gap:

- There is no small deterministic formatter/report object for interaction evidence. Hosts either inspect the typed state directly or export a full bundle. Workbench currently formats scene evidence separately from actual interaction state.

Recommended Phase 329 scope:

- Add a small `VideraInteractionEvidence` / `VideraInteractionEvidenceFormatter` in `src/Videra.Avalonia/Controls`.
- Input should be existing `VideraInspectionState` plus optional `VideraInteractionDiagnostics`.
- Output should include selected count, primary selection id, annotation count/kinds, measurement count/labels, clipping count, snap mode, and camera values.
- Keep it formatting/report-only. Do not mutate runtime, attach event subscriptions, or invent a project format.
- Tests should cover deterministic formatting and null/empty state behavior.

### SurfaceCharts Probe Output Evidence

Current capability:

- `SurfaceProbeInfo` is public Core truth with sample coordinates, axis coordinates, value, approximate flag, world position, tile key, and distance.
- `SurfaceProbeResult` is a minimal public projection.
- `SurfaceChartOverlayOptions` and numeric presets format axis/legend/probe labels.
- `SurfaceProbeOverlayState` and `SurfaceProbeOverlayPresenter` format hovered/pinned probe readouts internally.
- Existing integration tests cover hover, pinned probes, delta readouts, and numeric presets.

Gap:

- Probe evidence is not reusable outside internal overlay state. Support summaries and workbench reports cannot consume a public chart-local probe evidence formatter.

Recommended Phase 330 scope:

- Add a chart-local public probe evidence contract in `src/Videra.SurfaceCharts.Avalonia/Controls`, because the formatter should honor `SurfaceChartOverlayOptions`.
- Candidate shape: `SurfaceChartProbeEvidence` plus `SurfaceChartProbeEvidenceFormatter.Create(...)` / `Format(...)`.
- Inputs should be hovered/current `SurfaceProbeInfo?`, pinned probes, and `SurfaceChartOverlayOptions`.
- Output should include evidence kind, probe status, hovered probe readout, pinned count, pinned readouts, and delta-vs-first-pin text when applicable.
- Do not expose `SurfaceProbeOverlayState` or move probe semantics into `VideraView`.
- Tests should live in SurfaceCharts Avalonia integration tests or focused sample repository tests.

### Workbench Report Workflow

Current capability:

- `samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs` formats scene evidence, chart output evidence, diagnostics snapshot status, and diagnostics snapshot text.
- Workbench loads authored scene evidence with retained primitives, instance batches, selection, annotations, and measurements.

Gap:

- The support capture does not include typed viewer interaction evidence or probe evidence.

Recommended Phase 331 scope:

- Extend workbench support capture to include new viewer interaction evidence and chart probe evidence.
- Keep Workbench sample-first and public-API-only.
- Avoid live per-frame UI churn; refresh on explicit copy/refresh/load events.

### Docs and Guardrails

Current capability:

- `docs/alpha-feedback.md`, `src/Videra.Avalonia/README.md`, SurfaceCharts README files, and `docs/ROADMAP.generated.md` already document support evidence boundaries.
- `scripts/Export-BeadsRoadmap.ps1` now separates Ready and Blocked Beads.

Gap:

- v2.46-specific interaction evidence vocabulary does not exist yet.

Recommended Phase 332 scope:

- Update docs after 329-331 land.
- Keep Beads roadmap regenerated from `.beads/issues.jsonl`.
- Add or update tests/guardrails only where they protect the new evidence contracts.

## Parallelization

- Phase 329 and Phase 330 can run in parallel after Phase 328 closes.
- Phase 329 owns `src/Videra.Avalonia/Controls`, focused Avalonia/Core tests, and viewer docs if needed.
- Phase 330 owns `src/Videra.SurfaceCharts.Avalonia/Controls`, SurfaceCharts Avalonia tests, and chart-local docs if needed.
- Phase 331 waits for both and owns `samples/Videra.AvaloniaWorkbenchSample`.
- Phase 332 waits for all implementation phases and owns docs/roadmap/guardrails.

## Verification

- Phase 328 verification is source inventory plus Beads dependency alignment.
- No product code is changed in Phase 328.
