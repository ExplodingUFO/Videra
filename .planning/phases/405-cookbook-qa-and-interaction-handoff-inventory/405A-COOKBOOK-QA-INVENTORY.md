---
phase: 405
title: "Cookbook QA Surface Inventory"
bead: Videra-6iu
status: inventory
created_at: 2026-04-30
scope: "Phase 405 cookbook QA surface inventory only"
---

# Phase 405A Cookbook QA Inventory

## Scope

This is a read-only inventory of the current SurfaceCharts cookbook docs, demo,
and test surfaces after the v2.59 cookbook work. It does not approve product
runtime changes, broaden SurfaceCharts scope, or edit any product/demo/test
source. The only Phase 405 write surface is this file.

## Current Cookbook Docs, Demo, and Test Surfaces

### Docs and public navigation

- `README.md:104` introduces the "Minimal SurfaceCharts cookbook".
- `README.md:106` frames ScottPlot 5 as recipe-discovery inspiration only and
  names the current cookbook surface: `VideraChartView`, `Plot.Add.*`,
  `Plot.Axes`, chart-local interaction/profile APIs, linked 3D views,
  `DataLogger3D`, and PNG-only chart snapshots.
- `README.md:173` routes additional bounded snippets to
  `samples/Videra.SurfaceCharts.Demo/README.md`.
- `README.md:294` starts the SurfaceCharts onboarding section.
- `README.md:298` points support repros at `Copy support summary` and keeps
  `Videra.SurfaceCharts.Demo` repository-only.
- `README.md:300` points consumers at
  `docs/surfacecharts-release-cutover.md` for package consumption, release
  notes, cookbook, migration, support artifacts, and troubleshooting.
- `docs/index.md:17` links the SurfaceCharts release cutover from the public
  documentation start list.
- `docs/index.md:43` starts the chart package/sample list, and
  `docs/index.md:56` links `Videra.SurfaceCharts.Demo` as a repository-only
  sample/demo.

### Demo README

- `samples/Videra.SurfaceCharts.Demo/README.md:12` lists the default
  `Start here: In-memory first chart` path.
- `samples/Videra.SurfaceCharts.Demo/README.md:17` and
  `samples/Videra.SurfaceCharts.Demo/README.md:18` list bounded Bar and Contour
  proof paths.
- `samples/Videra.SurfaceCharts.Demo/README.md:19` defines the cookbook gallery
  as first chart, styling, interactions, live data, linked axes, Bar, Contour,
  and export recipes mapped to isolated setup paths.
- `samples/Videra.SurfaceCharts.Demo/README.md:21` keeps ScottPlot 5 as
  inspiration only and explicitly rejects compatibility/parity.
- `samples/Videra.SurfaceCharts.Demo/README.md:38` starts the ordered
  "Start Here" workflow, with Bar and Contour called out at lines 44-45.
- `samples/Videra.SurfaceCharts.Demo/README.md:53` starts the copyable
  "Cookbook Recipes" section.
- `samples/Videra.SurfaceCharts.Demo/README.md:55` states that selecting a
  recipe switches to a bounded visible result when one exists and that the
  visible snippet matches the documentation.
- `samples/Videra.SurfaceCharts.Demo/README.md:243` starts "What The Demo Shows
  Today".
- `samples/Videra.SurfaceCharts.Demo/README.md:276` starts "What The Demo Does
  Not Show Yet".

### Visible demo shell

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:36` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:43` declare one
  `VideraChartView` surface each for surface, waterfall, scatter, Bar, and
  Contour.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:69` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:70` expose
  `Try next: Bar chart proof` and `Try next: Contour plot proof`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:74` exposes the
  scatter stream selector.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:96` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:103` expose the
  cookbook gallery, recipe selector/status/snippet, and copy button.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:121`,
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:211`, and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:307` expose the
  view-state, rendering diagnostics, and support-summary panels.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:312` exposes the
  bounded snapshot button.

### Demo code-behind

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:139` binds the
  recipe selector to `CookbookRecipes.All`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:155` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:554` wire and
  implement `OnCopyRecipeSnippetClicked`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:343` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:352` preserve
  the no-hidden-fallback cache failure behavior: report the failure and keep
  the previous Plot path active.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:410` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:417` apply the
  bounded Bar proof through `Plot.Add.Bar`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:426` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:433` apply the
  bounded Contour proof through `Plot.Add.Contour`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:536` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1136` wire and
  generate the support summary.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:570` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:594` implement
  the PNG snapshot button through `Plot.SavePngAsync`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:864` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:874` hold the
  sample Bar and Contour data builders.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1144` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1145` label
  support summary values as evidence-only, not benchmark guarantees.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1607` starts
  the `CookbookRecipes` collection, with Bar, Contour, and Export snippets
  anchored at lines 1736, 1761, and 1771.

### Release cutover and migration/support handoff

- `docs/surfacecharts-release-cutover.md:15` names the supported cookbook
  surfaces including axes, overlay options, interaction/profile APIs, linked
  views, `DataLogger3D`, and PNG-only snapshot APIs.
- `docs/surfacecharts-release-cutover.md:47` starts the cookbook entry-point
  list.
- `docs/surfacecharts-release-cutover.md:58` keeps export bounded to PNG chart
  snapshots.
- `docs/surfacecharts-release-cutover.md:60` rejects ScottPlot API
  compatibility, parity, adapters, and migration layers.
- `docs/surfacecharts-release-cutover.md:62` starts the migration boundary.
- `docs/surfacecharts-release-cutover.md:78` keeps hidden scenario/data-path
  fallback excluded.
- `docs/surfacecharts-release-cutover.md:82` and
  `docs/surfacecharts-release-cutover.md:105` define the support handoff and
  troubleshooting route.

### Text-contract tests

- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:9`
  starts the cookbook docs/demo alignment test.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:20`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:34`
  assert Bar/Contour docs alignment and reject the old unexplained
  `new ScatterColumnarData(x, y, z)` snippet shape.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:39`
  starts the independent demo entry-point test.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:75`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:77`
  check root README Bar, Contour, and `SavePngAsync` coverage.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:100`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:118`
  check demo README cookbook, support, and snippet coverage.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:130`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:167`
  check visible demo UI strings and reject `VideraView` in the SurfaceCharts
  demo shell.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:194`
  and `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:233`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:235`
  check support-summary, Bar, Contour, and PNG snapshot code-behind coverage.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:265`
  and `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:279`
  keep support summary refresh and no-GPU-driven-culling non-goal wording in
  the contract test.

## Archived v2.59 Evidence

- `.planning/milestones/v2.59-phases/401-interaction-and-cookbook-experience-inventory/401B-COOKBOOK-INVENTORY.md`
  originally found that Bar/Contour proof paths were visible but not yet fully
  represented in README, release-cutover, or cookbook recipe coverage.
- `.planning/milestones/v2.59-phases/401-interaction-and-cookbook-experience-inventory/401C-VALIDATION-INVENTORY.md`
  recorded the focused cookbook validation suite and warned not to run demo
  build and demo configuration tests concurrently in the same worktree.
- `.planning/milestones/v2.59-phases/403-cookbook-demo-conversion/403-SUMMARY.md`
  records that Bar/Contour cookbook recipes, docs alignment, tightened live
  scatter/linked-axis snippets, and text-contract coverage were completed.
- `.planning/milestones/v2.59-phases/404-interaction-cookbook-final-verification/404-VERIFICATION.md`
  records a passing final suite: 46 SurfaceCharts Avalonia integration tests,
  24 Core sample/demo contract tests, demo build, snapshot scope guardrail,
  generated roadmap check, and `git diff --check`.
- `.planning/milestones/v2.59-ROADMAP.md` marks Phase 401 through Phase 404
  complete and keeps v2.59 scoped to ScottPlot5-inspired ergonomics without
  compatibility layers.

## Copyability and Drift Risks

1. The current cookbook surface is better aligned than the Phase 401 baseline:
   root README, demo README, release cutover, visible demo selector, code-behind
   recipe list, and text-contract tests all now mention Bar and Contour.
2. Copyability is still split across several surfaces. The root README gives a
   compact first-chart/live-scatter/export path, the demo README has the fuller
   recipe catalog, and the demo UI copies individual snippets. A consumer can
   still miss required `using` statements, package references, or host-control
   setup when copying from only one surface.
3. The demo UI snippets are intentionally short. That keeps them readable, but
   it leaves compile-ready cookbook assurance dependent on text-contract checks
   rather than snippet compilation.
4. The visible proof path and support summary are coupled by string contracts.
   `SurfaceChartsDemoConfigurationTests` catches many drift cases, but it does
   not enumerate every `CookbookRecipes.All` item against every demo README
   heading through a structured parser.
5. Release cutover still carries `v2.58` in its title and navigation text while
   it is also the current consumer-facing SurfaceCharts cutover page. That is a
   naming/versioning drift risk for v2.60 handoff unless the next phase either
   deliberately preserves it as historical or creates a new current page.
6. Support-summary evidence is intentionally evidence-only. It should not drift
   into benchmark, pixel-perfect visual-regression, backend fallback, replay,
   PDF/vector export, or installable-demo claims.

## Residual Gaps

- No compile test currently extracts and builds every README/demo UI snippet as
  standalone consumer code.
- No structured inventory currently maps each `CookbookRecipes.All` record to a
  demo README heading, release cutover entry, and visible proof path without
  relying on broad string containment.
- No current Phase 405 evidence reruns the demo build or focused Core sample
  tests; this inventory is read-only and only validates its own diff/status.
- The consumer-facing cutover page title remains versioned as v2.58 even though
  it is referenced as the current route from README/docs index.
- The repo has separate packaged smoke support artifacts and repository-only
  demo support summaries; cookbook QA should keep those routes distinct.

## Minimal Candidate Phase 406 Slices

### 406A: Snippet Compileability Harness

Create a focused test or fixture that extracts curated cookbook snippets into a
minimal SurfaceCharts consumer compile check. Keep it text/snippet focused; do
not launch the Avalonia demo.

Acceptance:

- First chart, styling, interactions, live data, linked axes, Bar, Contour, and
  export snippets are either compile-checked or explicitly tagged as
  illustrative.
- Required package/reference assumptions are documented near the harness.
- No product runtime behavior changes are made.

### 406B: Structured Cookbook Coverage Matrix

Replace part of the broad string-contract coverage with a small structured
coverage table or test fixture that maps recipe group, visible proof path, demo
README heading, release cutover entry, and support-summary evidence fields.

Acceptance:

- Adding or renaming a recipe fails a focused coverage test until README/cutover
  coverage is updated.
- The test remains read-only and does not instantiate UI controls.
- Repository-only demo wording remains separate from packaged consumer smoke
  wording.

### 406C: Current Cutover Naming Decision

Make a small docs-only decision about whether `docs/surfacecharts-release-cutover.md`
stays a v2.58 historical cutover page or becomes a version-neutral/current
SurfaceCharts consumer handoff.

Acceptance:

- README and docs index wording are consistent with the decision.
- Public release approval boundaries remain unchanged.
- No publish/tag/GitHub Release action is implied.

## Non-Goals and Fallback Exclusions

- No product runtime, rendering, interaction, or package API changes.
- No edits to demo XAML/code-behind, tests, README/docs, Beads state, roadmap,
  state files, or other phase files in this Phase 405 inventory bead.
- No ScottPlot compatibility layer, parity claim, adapter, or migration shim.
- No old `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`
  controls.
- No direct public `VideraChartView.Source` API.
- No generic plotting engine, chart editor, product workbench, or god-code demo.
- No hidden cache/scenario/data-path fallback.
- No PDF/vector export and no image export beyond bounded PNG chart snapshots.
- No backend expansion, OpenGL/WebGL addition, or compositor-native Wayland
  embedding promise.
- No benchmark/performance guarantee from cookbook snippets, screenshots, or
  support summaries.
- No package dependency on `Videra.SurfaceCharts.Demo`; it remains
  repository-only.

## Validation Commands

Inventory-only validation run for Phase 405:

```powershell
git diff --check -- .planning\phases\405-cookbook-qa-and-interaction-handoff-inventory\405A-COOKBOOK-QA-INVENTORY.md
git status --short
```

Focused validation candidates for any Phase 406 cookbook QA implementation:

```powershell
dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj
dotnet restore samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests|InteractionSampleConfigurationTests|DemoInteractionContractTests" --no-restore
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short
```

Run the demo build and `SurfaceChartsDemoConfigurationTests` sequentially in the
same worktree to avoid the PDB contention noted by the archived v2.59
validation inventory.
