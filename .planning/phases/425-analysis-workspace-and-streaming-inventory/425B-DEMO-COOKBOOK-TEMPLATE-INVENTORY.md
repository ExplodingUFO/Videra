# Phase 425B Demo Cookbook and Package Template Inventory

Task: `Videra-7tqx.1.2`

Scope: read-only inventory for v2.64 scenario workflow documentation and package-consumer template planning. This file intentionally does not change demo code, package code, tests, or Beads state.

## Owners and Files

| Surface | Owner file(s) | Current responsibility |
| --- | --- | --- |
| SurfaceCharts demo app | `samples/Videra.SurfaceCharts.Demo/README.md` | Canonical repository-only demo guide, visible scenario list, demo coverage map, support-summary route, and explicit non-goals. |
| Demo scenario catalog | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs` | Bounded visible scenario ids and labels: first chart, cache-backed streaming, analytics proof, waterfall, scatter, bar, and contour. |
| Cookbook gallery catalog | `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` | Recipe groups and snippets mapped to existing demo scenarios; current groups are first chart, styling, interactions, live data, linked axes, Bar, Contour, and export. |
| Scatter streaming proof data | `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs` and `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingEvidence.cs` | Deterministic replace, append, and FIFO-trim scatter scenarios plus evidence records. |
| Demo support evidence | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` | Copyable chart-local support summary, including scenario id/name/update mode and rendering/status evidence for the active path. |
| Demo UI shell | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` and `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` | Current bounded gallery/source selector, scatter scenario selector, snapshot button, diagnostics panels, and copy-support-summary workflow. |
| Cookbook recipe docs | `samples/Videra.SurfaceCharts.Demo/Recipes/*.md` | Detailed copy-adapt recipes for first chart, cache-backed surface, waterfall, linked views, scatter/live data, Bar, Contour, support evidence, and PNG snapshot. |
| Consumer handoff doc | `docs/surfacecharts-release-cutover.md` | Consumer-facing package consumption, cookbook entry points, demo coverage map, migration boundary, support artifact list, and troubleshooting route. |
| Package matrix | `docs/package-matrix.md` | Public package truth for `Videra.SurfaceCharts.*`, repository-only demo/smoke boundaries, and install-story split. |
| Root package/cookbook summary | `README.md` | Scenario-based install table, minimal SurfaceCharts cookbook, package promise, demo/support routing, and current non-goals. |
| Packaged SurfaceCharts smoke | `smoke/Videra.SurfaceCharts.ConsumerSmoke/*` | Package-reference-only first-chart smoke app that writes `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`, and `chart-snapshot.png`. |
| Consumer smoke runner | `scripts/Invoke-ConsumerSmoke.ps1` and `scripts/ConsumerSmokeSupportArtifacts.ps1` | Package build/run validation and support-artifact routing for `-Scenario SurfaceCharts`. |
| Package validation | `scripts/Validate-Packages.ps1` and `eng/package-size-budgets.json` | SurfaceCharts package id/dependency/budget guardrails. |
| Cookbook/package contract tests | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbook*`, `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs`, and `tests/Videra.Core.Tests/Repository/PackageDocsContractTests.cs` | Text and repository contract tests for cookbook coverage, consumer-smoke packaging, package docs, and scope boundaries. |

## Existing Reuse Points

- The demo already has a bounded scenario workflow vocabulary: `Start here: In-memory first chart`, `Explore next: Cache-backed streaming`, `Try next: Analytics proof`, `Try next: Waterfall proof`, `Try next: Scatter proof`, `Try next: Bar chart proof`, and `Try next: Contour plot proof`.
- The cookbook gallery is already scenario-addressable through `CookbookRecipeCatalog.cs`, with each recipe carrying a group, title, scenario id, optional scatter scenario id, and snippet. Phase 429 can extend this model without inventing a new demo shell.
- Detailed recipe files already cover the current single-chart and feature-specific paths: first chart, cache-backed surface, waterfall, axes/linked views, scatter/live data, Bar, Contour, support evidence, and PNG snapshot.
- Scatter streaming has deterministic replace, append, and FIFO-trim scenario data and evidence-only records. This is reusable for streaming/high-density documentation, but should not be rewritten into benchmark guarantees.
- Package-consumer proof already exists as a package-reference-only smoke app. It is a better seed for package-consumer templates than the repository demo because it avoids `ProjectReference` and writes the support artifact set expected by `Invoke-ConsumerSmoke.ps1`.
- `docs/surfacecharts-release-cutover.md`, `docs/package-matrix.md`, and `README.md` already establish the package boundary: consumer entry package is `Videra.SurfaceCharts.Avalonia`; add `Videra.SurfaceCharts.Processing` only for cache-backed surfaces; demo and smoke apps stay repository-only.
- Existing tests already pin cookbook and package truth. New scenario docs/templates should add or update similarly focused text-contract tests rather than relying on broad manual review.

## Gaps for Phase 429

### Multi-Chart Analysis

- There is no dedicated multi-chart analysis cookbook scenario that shows several `VideraChartView` instances used together for comparison.
- Existing linked-view guidance uses a two-chart snippet in `axes-and-linked-views.md`, but the visible demo scenario remains a single active proof path rather than a multi-panel analysis layout.
- Current support summaries describe one active chart path. They do not define how to capture evidence for a multi-chart analysis workflow without creating fake aggregate support proof.

### Linked Interaction

- `LinkViewWith` is documented and tested as an explicit disposable two-chart link, but the demo does not expose a visible linked-interaction workflow with two charts moving together.
- Built-in interaction docs focus on one chart at a time. Phase 429 should clarify which gestures/state are shared across linked charts and which remain host-owned.
- Any linked-interaction template should avoid implying chart-owned workspace state, replay, or annotation persistence.

### Streaming and High-Density Scenario Docs

- Streaming docs currently center on scatter replace/append/FIFO and cache-backed surface tiles. There is no scenario cookbook that compares these as a workflow: cache-backed surface first, then high-volume scatter, then evidence collection.
- The demo has evidence-only streaming counters, but the docs should more clearly separate proof data from performance guarantees and benchmark thresholds.
- High-density behavior is documented as `Pickable=false` by default and evidence-only benchmarks, but there is no package-consumer template that teaches the high-volume path without pulling in the full demo.

### Package-Consumer Templates

- There is no source template directory or `dotnet new` template for SurfaceCharts consumers.
- The closest reusable template is `smoke/Videra.SurfaceCharts.ConsumerSmoke`, but it is a validation app, not a polished consumer starter. It includes artifact writing, timeout/shutdown behavior, and smoke-specific environment variables that a starter template should not inherit wholesale.
- Existing consumer docs show package install commands and snippets, but they do not provide a complete minimal project layout for first chart, cache-backed surface, linked two-chart analysis, or high-density scatter.

## Risks and Non-Goals

- Do not turn `Videra.SurfaceCharts.Demo` into a broad workbench shell or generic chart editor. The current value is bounded, scenario-first evidence.
- Do not make compatibility or parity claims. The cookbook is Videra-native and not a ScottPlot, old alpha API, or adapter layer.
- Do not reintroduce old controls such as `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`; `VideraChartView` remains the shipped chart control.
- Do not add a direct public `VideraChartView.Source` data-loading path; keep data loading on `Plot.Add.*`.
- Do not claim PDF/vector export, backend expansion, OpenGL/WebGL support, or compositor-native Wayland embedding from these docs.
- Do not use smoke/demo artifacts as fake support evidence. A scenario is supported only where the demo, smoke app, snapshot result, or support summary actually emits that evidence.
- Do not promote streaming/high-density evidence into benchmark guarantees until benchmark thresholds and CI history support that claim.
- Do not modify Beads state from this inventory slice; other workers own separate inventory files and task-state updates.

## Candidate Write Scopes for Phase 429

| Candidate scope | Suggested files | Parallelization notes |
| --- | --- | --- |
| Scenario cookbook doc expansion | `samples/Videra.SurfaceCharts.Demo/README.md`, selected `samples/Videra.SurfaceCharts.Demo/Recipes/*.md`, and `docs/surfacecharts-release-cutover.md` | Safe to parallelize with package-template work if one worker owns cookbook docs and another owns template/smoke docs. |
| Visible linked-analysis demo | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml`, `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`, and possibly `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs` | Should be isolated from docs-only workers because it touches the main demo shell. Keep it bounded to one explicit linked-analysis scenario. |
| Cookbook catalog sync | `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` and `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs` | Can be a focused follow-up after docs/design decisions are stable. Avoid mixing with unrelated UI changes. |
| Streaming/high-density evidence docs | `samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md`, `README.md`, and `docs/surfacecharts-release-cutover.md` | Safe docs-only slice if it does not alter scenario data or benchmark contracts. |
| Package-consumer starter template | New template/sample files under a deliberately named docs/sample/template path, plus `docs/package-matrix.md`, `docs/surfacecharts-release-cutover.md`, and package-doc tests | Keep separate from smoke validation changes. Base public install shape on `smoke/Videra.SurfaceCharts.ConsumerSmoke`, but strip smoke-only timeout/artifact behavior. |
| Packaged smoke contract hardening | `smoke/Videra.SurfaceCharts.ConsumerSmoke/*`, `scripts/Invoke-ConsumerSmoke.ps1`, and `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs` | Not safe to parallelize with template edits if both workers touch the smoke project. Reserve for validation gaps, not starter-template polish. |
| Package validation/docs guardrails | `scripts/Validate-Packages.ps1`, `eng/package-size-budgets.json`, `tests/Videra.Core.Tests/Repository/PackageDocsContractTests.cs`, and `docs/package-matrix.md` | Can run independently from demo UI work if file ownership is explicit. |

## Suggested Focused Validation

Targeted search commands used for this inventory:

```powershell
rg --files | rg "(?i)(demo|sample|cookbook|recipe|template|package|consumer|smoke|gallery|docs|425|429|stream|scenario|multi|linked|high-density|density|chart)"
rg -n "(?i)(cookbook|recipe|scenario|multi[- ]?chart|linked|streaming|high[- ]?density|package template|consumer|smoke|gallery|demo)" docs src samples tests .planning -g "*.md" -g "*.cs" -g "*.axaml" -g "*.csproj"
rg -n "(?i)(Cookbook|Scenario|SurfaceDemoScenarios|Scatter|Stream|Linked|Support summary|Capture Snapshot|PackageReference|Videra.SurfaceCharts)" samples/Videra.SurfaceCharts.Demo smoke/Videra.SurfaceCharts.ConsumerSmoke docs/surfacecharts-release-cutover.md docs/package-matrix.md README.md scripts/Invoke-ConsumerSmoke.ps1 scripts/ConsumerSmokeSupportArtifacts.ps1
rg -n "(?i)(template|package template|dotnet new|ProjectReference|PackageReference|packaged|consumer smoke|SurfaceCharts)" README.md docs/package-matrix.md docs/surfacecharts-release-cutover.md smoke/Videra.SurfaceCharts.ConsumerSmoke scripts/Invoke-ConsumerSmoke.ps1 scripts/Validate-Packages.ps1 eng/package-size-budgets.json
rg -n "(?i)(multi[- ]?chart|linked|LinkViewWith|shared|sync|streaming|high[- ]?density|high-volume|FIFO|append|replace|DataLogger3D)" README.md samples/Videra.SurfaceCharts.Demo docs/surfacecharts-release-cutover.md tests/Videra.Core.Tests/Samples tests/Videra.SurfaceCharts.Avalonia.IntegrationTests src/Videra.SurfaceCharts.Avalonia src/Videra.SurfaceCharts.Core
```

Suggested Phase 429 validation after actual docs/demo/template edits:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~PackageDocsContractTests" --no-restore
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~SurfaceChartsHighPerformancePathTests" --no-restore
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~VideraChartViewPlotApiTests" --no-restore
pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/consumer-smoke
pwsh -File ./scripts/Validate-Packages.ps1 -Configuration Release
git diff --check
```

Use the smoke and package-validation commands only when Phase 429 changes package-consumer templates, smoke behavior, or package docs. Docs-only cookbook edits can usually stay on the focused text-contract tests plus `git diff --check`.
