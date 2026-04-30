# Phase 323: Professional Output and Scene Semantics Inventory - Context

**Gathered:** 2026-04-28  
**Status:** Ready for planning/execution  
**Bead:** Videra-5wz

<domain>
## Phase Boundary

v2.45 should improve publishable/support-ready output semantics without widening renderer/backend scope. The current high-value paths are:

- Core static-scene authoring: `SceneAuthoringBuilder`, `SceneGeometry`, `SceneAuthoringDiagnostic`, retained `SceneDocument` / `InstanceBatchEntry` truth.
- SurfaceCharts output/evidence: chart-local palette/precision/status/support summary contracts.
- Optional workbench sample: public-API composition and structured support capture.

This phase is inventory only. No product API changes are required here.
</domain>

<decisions>
## Implementation Decisions

- Phase 324 should own Core-only authoring semantics. Candidate: add a small scale guide/scale bar helper, or a validation diagnostic that explains non-batched repeated visuals. Keep it retained-scene truth.
- Phase 325 should own SurfaceCharts-only output evidence. Candidate: add a small reusable evidence formatter/summary for color-map palette + numeric precision/output metadata, then use it from demo/workbench later.
- Phase 326 should own the optional workbench sample only after 324/325 land.
- Phase 327 should own public docs, generated roadmap, and guardrail expectations.

Phases 324 and 325 can run in parallel after this inventory because their write sets are disjoint.
</decisions>

<code_context>
## Existing Code Insights

### Static-scene authoring

- `src/Videra.Core/Scene/SceneAuthoringBuilder.cs` supports primitive helpers, `AddAxisTriad(...)`, point clouds, and `AddInstances(...)`.
- `src/Videra.Core/Scene/SceneGeometry.cs` owns primitive mesh helpers including `AxisLine(...)`.
- `tests/Videra.Core.Tests/Scene/SceneAuthoringBuilderTests.cs` already covers retained document truth, instance-batch truth, axis triads, and validation diagnostics.
- Gap: authored scenes can show axes/grid/markers but lack a professional scale semantic such as a scale bar/guide. A scale guide is smaller and safer than labels, text billboards, UI overlays, or runtime gizmos.

### SurfaceCharts output/evidence

- `src/Videra.SurfaceCharts.Core/ColorMaps/SurfaceColorMapPresets.cs` now has `CreateProfessional()`.
- `src/Videra.SurfaceCharts.Avalonia/README.md` documents precision and palette presets.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` builds support summaries inline and includes runtime/chart/status details.
- Gap: palette/precision evidence is still partly string-built in samples. A small chart-local evidence formatter can make output support evidence deterministic without adding export infrastructure or chart families.

### Workbench output workflow

- `samples/Videra.AvaloniaWorkbenchSample/Views/MainWindow.axaml.cs` composes authored scene, OBJ load, diagnostics snapshot, selection/annotation/measurement, and SurfaceCharts precision/palette evidence using public APIs.
- `samples/Videra.AvaloniaWorkbenchSample/WorkbenchSupportCapture.cs` formats scene and chart evidence.
- Gap: after 324/325, workbench should demonstrate the new scene semantic and chart output evidence in one coherent copy/export workflow.

### Guardrails

- `docs/capability-matrix.md`, README, and repository tests already emphasize no ECS, no runtime gizmos, no hidden fallback/downshift, no broad backend/chart expansion.
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` validates generated roadmap determinism from `.beads/issues.jsonl`.
</code_context>

<specifics>
## Handoff Boundaries

| Phase | Bead | Suggested ownership | Verification |
| --- | --- | --- | --- |
| 324 | Videra-uxg | `src/Videra.Core/Scene/*`, `tests/Videra.Core.Tests/Scene/*`, maybe `samples/Videra.MinimalAuthoringSample/*` docs | `SceneAuthoringBuilderTests` |
| 325 | Videra-ct5 | `src/Videra.SurfaceCharts.Core/*` or chart-local evidence helper, `tests/Videra.SurfaceCharts.Core.Tests/*`, maybe SurfaceCharts README | SurfaceCharts Core focused tests |
| 326 | Videra-ki0 | `samples/Videra.AvaloniaWorkbenchSample/*` only, consuming 324/325 public APIs | workbench sample build |
| 327 | Videra-422 | docs, generated roadmap, Beads roadmap test expectations | `BeadsPublicRoadmapTests` |

## Recommended Phase 324 Shape

Implement `SceneGeometry.ScaleBar(...)` and `SceneAuthoringBuilder.AddScaleBar(...)` as line topology retained primitives. Keep parameters minimal: name, length, optional tick height, material/color, transform. Avoid labels/text because that would pull in UI/overlay/font semantics.

## Recommended Phase 325 Shape

Implement a small chart-local output/evidence formatter, for example `SurfaceChartOutputEvidence` or `SurfaceChartEvidenceFormatter`, that reports palette name/color stops and numeric precision profile in deterministic lines. Avoid image export, file IO, or viewer diagnostics coupling.
</specifics>

<deferred>
## Deferred Ideas

- Text labels, billboards, runtime manipulators, scene graph editor, ECS, update loops.
- Chart screenshot/export pipeline, pixel-perfect visual regression, broad chart-family expansion.
- Backend/device/render-host changes.
- Compatibility layers or hidden fallback/downshift behavior.
</deferred>
