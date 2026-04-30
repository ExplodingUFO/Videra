# Phase 419B Demo Gallery and Support Workflow Inventory

Bead: `Videra-jyrv`
Scope: docs-only inventory for Phase 422 planning. No product code changes were made or recommended here.

## Current Demo And Support Map

| Owner | File(s) | Current responsibility | Notes for Phase 422 |
| --- | --- | --- | --- |
| Demo app shell | `samples/Videra.SurfaceCharts.Demo/Program.cs`, `App.axaml`, `App.axaml.cs`, `Videra.SurfaceCharts.Demo.csproj` | Standalone repository-only Avalonia sample for the SurfaceCharts package family. | The project references `Videra.SurfaceCharts.Avalonia` and `Videra.SurfaceCharts.Processing`; it is not an installable product dependency. |
| Demo visual layout | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` | Two-column app layout, chart host stack, source selector, scatter scenario selector, cookbook gallery, status/diagnostics panels, support summary panel, and bounded snapshot button. | Visible source choices are in XAML; source-index contracts are duplicated in code-behind and recipe catalog. |
| Demo workflow orchestration | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` | Owns chart lookup, source selection, scene application, in-memory/cache/analytics/waterfall/scatter/bar/contour data creation, Fit/Reset, support-summary copy, recipe-snippet copy, PNG snapshot capture, diagnostics text, and active-chart routing. | This is the largest ownership hotspot: 1194 lines, several responsibilities, and multiple duplicated source-index constants. |
| Cookbook recipe catalog | `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` | Defines 8 gallery recipes: First chart, Styling, Interactions, Live data, Linked axes, Bar, Contour, Export. Maps each recipe to a source index, optional scatter scenario id, and copyable snippet. | Good split from code-behind, but still depends on integer source indexes that must match `MainWindow`. |
| Scatter scenario catalog | `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs` | Defines replace, append, and FIFO-trim columnar streaming scenarios and creates deterministic `ScatterColumnarSeries` data. | Scenario model is already small and reusable for demo/support evidence; keep it demo-owned unless API inventory recommends public changes. |
| Scatter evidence service | `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingEvidence.cs` | Produces evidence-only records for scenario and `DataLogger3D` live-window behavior. | Useful support truth seam; not a benchmark guarantee. |
| Support summary service | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` | Creates the copyable support summary for surface/waterfall/bar, scatter, and contour paths. Includes runtime, assembly, backend/display environment, series, chart kind, output/dataset evidence, snapshot fields, rendering status, and active dataset/path details. | Strong support-evidence truth seam, but at 445 lines it is starting to own formatting policy for every chart kind. |
| Path evidence service | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoPathEvidence.cs` | Creates evidence records for in-memory pyramid and cache-backed paths. | Small and focused; keep as-is unless support evidence is consolidated later. |
| Demo README | `samples/Videra.SurfaceCharts.Demo/README.md` | Repository reference app instructions, current demo path list, cookbook overview, support summary semantics, and intentional non-goals. | Clearly states the demo is bounded and not a generic chart editor/workstation. |
| Recipe docs | `samples/Videra.SurfaceCharts.Demo/Recipes/*.md` | Detailed copy-adapt recipes for first chart, cache-backed surface, waterfall, linked axes, scatter/live data, bar, contour, support evidence, and PNG snapshot. | Current catalog has 9 detailed files; the in-app catalog has 8 groups and treats support evidence/cache/waterfall as supporting docs or visible paths rather than direct gallery groups. |
| Cache sample asset | `samples/Videra.SurfaceCharts.Demo/Assets/sample-surface-cache/sample.surfacecache.json`, `.bin` | Committed manifest+payload sidecar used by the cache-backed streaming source. | Supports lazy cache story; failures are reported without hidden scenario/path fallback. |
| Demo contract tests | `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs` | Pins demo existence, README tokens, XAML controls, cookbook groups, support summary fields, no old controls, no `VideraView`, and visible Bar/Contour proof paths. | Broad token test; good guardrail, but future demo refactors need careful test updates. |
| Cookbook coverage tests | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs` | Pins root README, demo README, release handoff, visible proof labels, recipe files, and blocked migration-boundary claims. | This is the main cross-doc coverage matrix. |
| Recipe-specific tests | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookFirstSurfaceRecipeTests.cs`, `SurfaceChartsCookbookScatterLiveRecipeTests.cs`, `SurfaceChartsCookbookWaterfallLinkedRecipeTests.cs`, `SurfaceChartsCookbookBarContourSnapshotRecipeTests.cs` | Pins individual recipe content and forbidden implementation tokens. | Treat as validation inventory input; do not weaken without validation owner alignment. |
| Scatter evidence tests | `tests/Videra.Core.Tests/Samples/ScatterStreamingScenarioEvidenceTests.cs` | Pins scenario evidence and `DataLogger3D` evidence-only behavior. | Relevant if Phase 422 changes scenario selectors or live-data demo flow. |
| Demo viewport/behavior tests | `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs`, `SurfaceChartsHighPerformancePathTests.cs`, `SurfaceChartsPerformanceTruthTests.cs` | Pins demo path behavior and performance-truth wording. | Consult validation owner before changing assertions. |
| Packaged support smoke | `smoke/Videra.SurfaceCharts.ConsumerSmoke/**`, `scripts/Invoke-ConsumerSmoke.ps1`, `scripts/ConsumerSmokeSupportArtifacts.ps1` | Packaged first-chart support proof, writes `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`, `chart-snapshot.png`, logs, and environment snapshot. | This is support evidence truth, not demo-gallery UX. Keep separated from repository demo planning. |
| Consumer handoff docs | `docs/surfacecharts-release-cutover.md`, `docs/alpha-feedback.md`, root `README.md` | Consumer-facing package, migration, support artifact, and troubleshooting route. | These docs explicitly block compatibility layers, hidden fallback, vector/PDF export, and backend expansion. |

## Current Gallery And Workflow Shape

- Visible source selector paths: in-memory first chart, cache-backed streaming, analytics proof, waterfall proof, scatter proof, bar chart proof, and contour plot proof.
- Scatter scenario selector: replace 100k, append 100k plus 25k update, FIFO-trim 100k plus 50k update with 100k capacity.
- Cookbook selector groups: First chart, Styling, Interactions, Live data, Linked axes, Bar, Contour, Export.
- Support workflow: reproduce `Start here` first, switch only as needed, copy support summary, optionally capture a 1920x1080 PNG snapshot.
- Packaged support workflow: run `scripts/Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts`; collect JSON, diagnostics, support summary, chart snapshot, logs, and environment file.

## UX And Workflow Gaps

- Source selection and recipe selection are linked by integer indexes. This makes gallery growth fragile because XAML labels, `MainWindow` constants, and `CookbookRecipeCatalog` constants must stay in sync manually.
- In-app gallery groups and detailed recipe docs are not one-to-one. Cache-backed, waterfall, support evidence, and PNG snapshot have detailed files, while the in-app selector groups primarily map to cookbook intent and source paths.
- The scatter scenario selector remains visible even when a non-scatter source is active. The code ignores changes unless scatter is active, but the UI affordance can imply that scenario selection affects all paths.
- The support summary panel is useful but dense. It mixes active path evidence, environment truth, dataset truth, snapshot truth, and chart-kind-specific diagnostics in one text block.
- Snapshot capture is available from the support summary panel, but the current workflow does not make the relationship between "capture first" and "copy summary includes snapshot fields" obvious.
- Bar and contour proofs are bounded and useful, but their panels do not yet explain limitations as clearly as surface/scatter paths do.
- Packaged consumer smoke currently belongs to support truth, not demo UX. Phase 422 should avoid pulling smoke orchestration into the repository demo shell.

## God-Code And Ownership Risks

- `MainWindow.axaml.cs` is the main god-code risk. It owns UI wiring, source routing, active chart selection, synthetic data generation, chart configuration, recipe side effects, support refresh, copy operations, and snapshot capture.
- Source routing is duplicated across `MainWindow.axaml`, `MainWindow.axaml.cs`, and `CookbookRecipeCatalog.cs`. The current integer-index contract is the highest-risk demo-only seam.
- Support summary formatting is centralized in `SurfaceDemoSupportSummary.cs`. That is better than code-behind formatting, but the service now knows all chart-kind branches and could grow into another large policy file.
- Synthetic data factories in `MainWindow.axaml.cs` are useful for readability today, but they are not UI behavior. Moving them behind demo-only scenario descriptors would reduce churn without touching product APIs.
- Tests heavily pin strings across README, release handoff, XAML, and code-behind. This protects boundaries, but it can make small UX wording changes feel larger than they are.

## Candidate Improvements

### Demo-only workflow

- Introduce a demo-only `SurfaceDemoScenario` descriptor that owns id, label, chart kind, recipe group mapping, and apply delegate. This should replace integer source-index coupling without changing product APIs.
- Hide or disable the scatter scenario selector when a non-scatter path is active, and show the selected scenario in support text only for scatter.
- Split `MainWindow.axaml.cs` by demo responsibility using partial files or small demo services: scenario apply flow, sample data factories, support actions, and UI text refresh.
- Add a small gallery/status affordance that shows "support summary includes last snapshot: yes/no" after `Capture Snapshot`.
- Add a compact "visible proof coverage" section to the demo README that maps each in-app selector path to the detailed recipe file(s).

### Support evidence truth

- Keep `SurfaceDemoSupportSummary` evidence-only and separate from `VideraDiagnosticsSnapshotFormatter`.
- Extract chart-kind-specific support summary blocks into small internal helpers if Phase 422 touches summary breadth.
- Keep cache load failure explicit: previous Plot path remains active, and there is no hidden scenario/data-path fallback.
- Preserve packaged smoke artifact ownership in `smoke/Videra.SurfaceCharts.ConsumerSmoke` and `scripts/Invoke-ConsumerSmoke.ps1`; the repository demo should remain manual repro/reference.
- Align any summary-field changes with the validation inventory owner because `Invoke-ConsumerSmoke.ps1` and sample tests assert required prefixes.

### Out of scope

- No generic chart workbench, benchmark editor, or end-user workstation shell.
- No compatibility layer for ScottPlot or removed alpha APIs.
- No old `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView` wrappers.
- No direct public `VideraChartView.Source` loading API.
- No hidden fallback/downshift behavior for failed paths.
- No PDF/vector export or broader image export beyond bounded PNG chart snapshots.
- No OpenGL/WebGL/backend expansion or compositor-native Wayland promise.

## Recommended Phase 422 Child Beads

### 422-demo-scenario-descriptors

- Type: task
- Category: demo-only workflow
- Depends on: `Videra-jyrv`; coordinate with `Videra-mula` before changing any public API-facing snippets.
- Goal: Replace integer source-index coupling with a demo-only scenario descriptor table used by the source selector and cookbook recipes.
- Write scope: `samples/Videra.SurfaceCharts.Demo/Services/*`, `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`, focused sample tests under `tests/Videra.Core.Tests/Samples/*`.
- Validation commands:
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests`
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests`
  - `git diff --check`
- Handoff notes: Keep descriptor ids stable and demo-owned. Do not add public product configuration or generic workbench abstractions.

### 422-demo-mainwindow-split

- Type: task
- Category: demo-only workflow
- Depends on: `422-demo-scenario-descriptors`.
- Goal: Reduce `MainWindow.axaml.cs` ownership by moving sample data factories and support actions into demo-only partials or small internal services.
- Write scope: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow*.cs`, optional new files under `samples/Videra.SurfaceCharts.Demo/Services/`, focused tests only if string ownership changes.
- Validation commands:
  - `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj -c Release`
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests`
  - `git diff --check`
- Handoff notes: This is a code-ownership cleanup, not a UX redesign. Preserve current labels, support fields, and recipe behavior unless the bead explicitly owns a visible change.

### 422-scatter-selector-affordance

- Type: task
- Category: demo-only workflow
- Depends on: `422-demo-scenario-descriptors`.
- Goal: Make scatter scenario selection visibly scoped to the scatter proof path.
- Write scope: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml`, `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`, focused sample UI/string tests.
- Validation commands:
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests`
  - `git diff --check`
- Handoff notes: Disable/hide the selector or make its inactive state explicit. Do not add scenario fallback or auto-downshift behavior.

### 422-support-summary-snapshot-state

- Type: task
- Category: support evidence truth
- Depends on: `Videra-i8zb` validation inventory, because summary prefixes and artifacts are guarded by tests and scripts.
- Goal: Clarify the demo workflow around PNG snapshot capture and copied support summary snapshot fields.
- Write scope: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml`, `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`, `samples/Videra.SurfaceCharts.Demo/Recipes/support-evidence.md`, `samples/Videra.SurfaceCharts.Demo/Recipes/png-snapshot.md`, focused sample tests.
- Validation commands:
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookBarContourSnapshotRecipeTests`
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests`
  - `git diff --check`
- Handoff notes: Keep PNG-only bitmap boundary. Do not introduce PDF/vector/export generalization.

### 422-demo-coverage-map-doc

- Type: task
- Category: support evidence truth
- Depends on: `Videra-jyrv` and `Videra-i8zb`.
- Goal: Add a compact README/cutover coverage map that links each visible demo path and support artifact to the recipe/test surface that proves it.
- Write scope: `samples/Videra.SurfaceCharts.Demo/README.md`, `docs/surfacecharts-release-cutover.md`, focused sample coverage tests.
- Validation commands:
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests`
  - `git diff --check`
- Handoff notes: Keep wording bounded: current visible proofs, current artifacts, current non-goals. Do not claim public release approval.

### 422-consumer-smoke-truth-alignment

- Type: task
- Category: support evidence truth
- Depends on: `Videra-i8zb`; coordinate with API owner if `Plot.Add.Bar` or `Plot.Add.Contour` packaged-smoke expectations change.
- Goal: Audit whether packaged SurfaceCharts smoke should remain first-chart-only with supplemental Bar/Contour status or become a separate explicit support scenario.
- Write scope: `smoke/Videra.SurfaceCharts.ConsumerSmoke/**`, `scripts/Invoke-ConsumerSmoke.ps1`, `scripts/ConsumerSmokeSupportArtifacts.ps1`, focused tests under `tests/Videra.Core.Tests/Samples/*`.
- Validation commands:
  - `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests`
  - `pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly`
  - `git diff --check`
- Handoff notes: This is support evidence truth, not demo-gallery UX. Avoid broadening the repository demo into packaged smoke orchestration.

## Explicit Recommendation

Do not build a generic workbench, compatibility layer, migration adapter, or old-control wrapper in Phase 422. The better route is a small demo-only scenario descriptor and focused ownership split that keeps `VideraChartView`, `Plot.Add.*`, cookbook snippets, support summaries, and packaged smoke evidence honest without expanding product scope.
