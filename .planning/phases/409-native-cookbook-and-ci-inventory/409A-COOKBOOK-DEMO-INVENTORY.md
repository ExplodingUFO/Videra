---
phase: 409
artifact: 409A-COOKBOOK-DEMO-INVENTORY
title: "Cookbook and Demo Surface Inventory"
scope: "Phase 409 cookbook/demo inventory only"
bead: Videra-5sh
status: complete
---

# Phase 409A Cookbook and Demo Surface Inventory

This is an evidence-only inventory of the current SurfaceCharts cookbook and
demo surfaces for v2.61 planning. It does not approve product, demo, test,
README, docs, roadmap, Beads, or source edits. It is scoped to the current
consumer/root/demo/cutover cookbook surfaces and the candidate Phase 410 slices
that could turn the existing snippet/navigation surface into detailed runnable
3D cookbook recipes.

## Current Planning Boundary

- `.planning/ROADMAP.md:1` names v2.61 as "Native SurfaceCharts Cookbook and
  CI Truth".
- `.planning/ROADMAP.md:3` sets the milestone goal: move toward
  ScottPlot5-style cookbook usability while preserving native Videra 3D chart
  semantics.
- `.planning/ROADMAP.md:8` and `.planning/ROADMAP.md:11` define the external
  ScottPlot reference as short `Plot.Add.*` ergonomics only, not API
  compatibility or parity.
- `.planning/ROADMAP.md:21` makes Phase 409 the inventory phase for current
  demo cookbook, native 3D chart APIs, performance-sensitive paths, CI gates,
  and anti-fake validation gaps.
- `.planning/ROADMAP.md:22` makes Phase 410 the implementation phase for
  detailed 3D cookbook demo recipes.
- `.planning/REQUIREMENTS.md:14` asks Phase 409 to inventory current demo
  cookbook recipes, snippet sources, visible demo surfaces, and validation
  coverage.
- `.planning/REQUIREMENTS.md:61` and `.planning/REQUIREMENTS.md:65` keep broader
  chart-family expansion and unrelated native backend architecture work out of
  scope.
- `.planning/milestones/v2.60-phases/406-cookbook-qa-hardening/406-SUMMARY.md:13`
  records that v2.60 added a focused cookbook coverage matrix.
- `.planning/milestones/v2.60-phases/408-v2.60-final-verification/408-VERIFICATION.md:15`
  records the final v2.60 focused cookbook test pass.

## Root, Docs, Demo, and Cutover Surfaces

- `README.md:104` starts the root "Minimal SurfaceCharts cookbook" section.
- `README.md:106` states the ScottPlot5 inspiration boundary and names the
  current Videra-native cookbook surface: `VideraChartView`, `Plot.Add.*`,
  `Plot.Axes`, chart-local interaction/profile APIs, linked 3D views,
  `DataLogger3D`, and PNG-only chart snapshots.
- `README.md:114`, `README.md:144`, `README.md:152`, and `README.md:169`
  include root snippets for `Plot.Add.Surface`, `Plot.Add.Scatter`,
  `Plot.Add.Bar`, and `Plot.Add.Contour`.
- `README.md:173` points users to `samples/Videra.SurfaceCharts.Demo/README.md`
  for bounded styling, interaction profile, live latest-window, linked-axis,
  Bar, Contour, and export recipes.
- `README.md:296` through `README.md:300` describe the repository-only demo
  route, support-summary workflow, packaged consumer smoke artifacts, and the
  cutover handoff route.
- `docs/index.md:5` points readers from public docs to the root README for the
  alpha support contract and shipped `VideraChartView` / `Plot.Add.Surface` /
  `Plot.Add.Waterfall` / `Plot.Add.Scatter` entrypoints.
- `docs/index.md:17` links `docs/surfacecharts-release-cutover.md` as the
  consumer-facing SurfaceCharts current handoff.
- `docs/index.md:49` and `docs/index.md:56` list the current consumer handoff
  and the repository-only `Videra.SurfaceCharts.Demo`.
- `docs/surfacecharts-release-cutover.md:3` defines the page as the current
  consumer-facing handoff for install path, release-note evidence, cookbook
  entry points, migration boundary, support artifacts, and troubleshooting.
- `docs/surfacecharts-release-cutover.md:14` and
  `docs/surfacecharts-release-cutover.md:15` enumerate the cookbook entrypoints
  and supported surfaces.
- `docs/surfacecharts-release-cutover.md:49` through
  `docs/surfacecharts-release-cutover.md:60` define the current cookbook route
  and the ScottPlot5 inspiration-only boundary.
- `docs/surfacecharts-release-cutover.md:84` through
  `docs/surfacecharts-release-cutover.md:103` define packaged support artifacts,
  repository-only repro order, `Copy support summary`, and anti-fake support
  constraints.
- `samples/Videra.SurfaceCharts.Demo/README.md:5` states the repository demo
  exercises `Plot.Add.Surface`, `Plot.Add.Waterfall`, `Plot.Add.Scatter`,
  `Plot.Add.Bar`, and `Plot.Add.Contour` on `VideraChartView`.
- `samples/Videra.SurfaceCharts.Demo/README.md:10` through
  `samples/Videra.SurfaceCharts.Demo/README.md:19` list the visible demo paths
  and cookbook gallery.
- `samples/Videra.SurfaceCharts.Demo/README.md:21` repeats the ScottPlot5
  inspiration-only boundary using Videra-native 3D APIs.
- `samples/Videra.SurfaceCharts.Demo/README.md:55` states the cookbook gallery
  selector groups recipes by intent and switches to a bounded visible result
  when one exists.
- `samples/Videra.SurfaceCharts.Demo/README.md:247` through
  `samples/Videra.SurfaceCharts.Demo/README.md:266` describe current demo
  capabilities, support-summary content, and evidence-only framing.
- `samples/Videra.SurfaceCharts.Demo/README.md:274` and
  `samples/Videra.SurfaceCharts.Demo/README.md:281` through
  `samples/Videra.SurfaceCharts.Demo/README.md:286` define visual-evidence and
  demo non-goal boundaries.

## Visible Demo Surfaces

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:24` gives the
  visible demo framing: cookbook gallery recipes for first chart, styling,
  interaction, live data, linked axes, Bar, Contour, and PNG export.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:37` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:43` declare the
  active `VideraChartView` surfaces for base surface, waterfall, scatter, Bar,
  and Contour.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:62` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:71` expose the
  first-chart Plot path selector: in-memory first chart, cache-backed streaming,
  analytics proof, waterfall proof, scatter proof, Bar proof, and Contour proof.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:74` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:77` expose the
  scatter stream selector.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:96` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:104` expose the
  `CookbookRecipeSelector`, status text, `Copy recipe snippet` button, and
  visible snippet text.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:307` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml:319` expose the
  support summary, `Copy support summary`, `Capture Snapshot`, and visible
  support text.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:139` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:156` wire the
  cookbook selector, source selector, copy recipe button, and support summary
  button.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1607` defines
  `CookbookRecipes.All`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1612` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1770` define the
  current recipe groups and snippets: First chart, Styling, Interactions, Live
  data, Linked axes, Bar, Contour, and Export.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:408` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:435` implement
  bounded Bar and Contour visible proof paths.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1240` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1276` build the
  visible support summary fields, including active chart, runtime/assembly,
  backend/display environment, cache failure, Plot path, series, output
  capability diagnostics, snapshot status, dataset evidence, view state,
  interaction quality, rendering status, overlay options, cache asset, and
  dataset.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1770` through
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:1783` keep the
  Export recipe on `Plot.SavePngAsync(...)` with a bounded 1920x1080 PNG path.

## Current Validation Surfaces

- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:9`
  checks the current consumer handoff alignment across root README, docs index,
  demo README, cutover page, XAML, and code-behind.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:29`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:44`
  assert each cookbook row appears in root docs, demo docs, cutover docs,
  code-behind snippets, and visible proof labels where applicable.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:45`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:56`
  pin support summary, packaged-vs-repository support routes, ScottPlot
  non-compatibility, no direct `Source` API, no hidden fallback, PNG-only export,
  no backend expansion, and no resurrected old chart view controls.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:59`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs:117`
  define the current cookbook coverage rows: First chart, Styling,
  Interactions, Live data, Linked axes, Bar, Contour, and Export.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:89`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:126`
  pin demo README cookbook headings, snippet tokens, support-summary language,
  Bar/Contour tokens, and ScottPlot non-parity wording.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:130`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:158`
  pin the visible XAML cookbook selector, snippet text, source selector,
  diagnostics panels, support summary, and host-command controls.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:171`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs:180`
  pin code-behind recipe groups and copy handler presence.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs:497`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs:546`
  validate visible support-summary projection and copy feedback.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs:555`
  through `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs:580`
  validate that copied support summaries refresh from the current Plot path and
  view state.

## Gaps for Detailed Runnable 3D Cookbook Recipes

These are gaps for Phase 410 selection, not defects to fix in Phase 409.

1. First chart: root and demo snippets show the smallest matrix surface, but
   the cookbook does not yet provide a full runnable host shell with imports,
   layout placement, lifecycle notes, expected visible result, and validation
   command in one recipe.
2. Surface: the current first chart covers `Plot.Add.Surface`, while the
   surface/cache-backed path is presented as a demo source. There is no
   detailed standalone recipe showing when to use matrix surface vs cache-backed
   tile source, what evidence should appear, and how to validate no hidden
   data-path fallback occurred.
3. Waterfall: the demo exposes `Try next: Waterfall proof` and Linked axes uses
   waterfall as the visible second chart result, but there is no standalone
   waterfall cookbook recipe with data shape, setup code, visible proof,
   support summary, and validation command.
4. Scatter: live data and scatter proof are present, including
   `DataLogger3D`, FIFO, and columnar evidence. The cookbook still lacks a
   layered runnable scatter progression from small point-object scatter to
   high-volume columnar/FIFO scatter with explicit anti-fake evidence.
5. Bar: Bar is bounded and visible in the demo, but there is no detailed 3D Bar
   recipe page/section that explains supported grouping semantics, color
   choices, evidence fields, and current limitations without implying a generic
   chart editor.
6. Contour: Contour is bounded and visible in the demo, but there is no
   detailed 3D Contour recipe that explains scalar-field construction, contour
   level defaults, expected visible proof, and validation commands.
7. Axes: axes and labels appear in snippets and the overlay panel, but there is
   no detailed axes recipe covering bounds, units, custom labels/formatters,
   legend/axis-side overlays, and how to inspect support evidence.
8. Styling: color map and overlay options are shown, but there is no full
   styling recipe that separates durable style APIs from demo-only visual
   choices and explicitly avoids theme/editor scope creep.
9. Interaction: current snippets show profile toggles, commands, and probe
   resolution. The cookbook still lacks a runnable interaction recipe that ties
   host buttons, built-in gestures, pinned probes, keyboard toolbar behavior,
   and support evidence together.
10. Live data: live scatter is present, but a detailed recipe should spell out
    replace vs append vs FIFO-trim behavior, retained counts, pickability,
    `InteractionQuality`, and the evidence fields that prove the path is live
    rather than static.
11. Linked views: `LinkViewWith` appears in the Linked axes snippet, but there
    is no detailed recipe covering lifetime/disposal, two-chart layout, sync
    expectations, and support-summary evidence.
12. Support evidence: the support summary is visible and copyable, but recipes
    do not yet tell users exactly which fields to check for each cookbook path
    and which fields are evidence-only rather than benchmark or fallback proof.
13. PNG snapshot: root/cutover/demo cover PNG-only export and the demo exposes a
    `Capture Snapshot` button, but there is no detailed recipe tying
    `SavePngAsync` / `CaptureSnapshotAsync`, manifest fields, support-summary
    snapshot fields, and packaged smoke artifacts into one runnable path.

## ScottPlot5 Inspiration Boundary

- Keep ScottPlot5 as an ergonomics reference for discoverable, short,
  copy-adapt recipe organization.
- Keep examples on Videra's native 3D chart model:
  `VideraChartView`, `Plot.Add.*`, `Plot.Axes`, chart-local interactions,
  `DataLogger3D`, linked 3D views, and PNG snapshot APIs.
- Do not promise ScottPlot compatibility, parity, adapters, migration tooling,
  wrapper controls, or API aliases.
- Do not add or document a direct public `VideraChartView.Source` loading path.
- Do not reintroduce removed old chart controls such as `SurfaceChartView`,
  `WaterfallChartView`, or `ScatterChartView`.
- Do not use ScottPlot examples to justify 2D plotting breadth, generic chart
  editor UI, PDF/vector export, WebGL/OpenGL/backend expansion, or hidden
  fallback behavior.

## Candidate Minimal Phase 410 Slices

1. `410A - Runnable first-chart and surface cookbook`: expand the smallest
   first-chart recipe into a complete runnable recipe, then add the
   matrix-surface vs cache-backed surface recipe boundary. Validation:
   `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsCookbookCoverageMatrixTests|SurfaceChartsDemoConfigurationTests" --no-restore`
   and `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore`.
2. `410B - Detailed waterfall, linked views, and axes recipe`: add a bounded
   waterfall recipe, explicit linked-view lifetime guidance, and axes/overlay
   expectations without changing product APIs. Validation:
   `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsCookbookCoverageMatrixTests|SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests" --no-restore`.
3. `410C - Scatter/live-data recipe hardening`: split small scatter, live
   `DataLogger3D`, replace/append/FIFO, pickability, and evidence fields into a
   detailed runnable flow. Validation:
   `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests" --no-restore`
   plus `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~ScatterDataLogger3DTests" --no-restore`.
4. `410D - Bar and Contour bounded recipe detail`: keep Bar and Contour as
   bounded demo proof paths while adding data-shape, visible-proof, and
   limitation wording. Validation:
   `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsCookbookCoverageMatrixTests|SurfaceChartsDemoConfigurationTests" --no-restore`.
5. `410E - Support evidence and PNG snapshot recipe`: connect `Copy support
   summary`, `SavePngAsync`, `CaptureSnapshotAsync`, manifest fields, and
   packaged smoke artifacts in cookbook text without adding export formats.
   Validation:
   `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsCookbookCoverageMatrixTests|SurfaceChartsDemoViewportBehaviorTests|SurfaceChartsConsumerSmokeConfigurationTests" --no-restore`
   and `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~PlotSnapshotContractTests|FullyQualifiedName~PlotSnapshotCaptureTests" --no-restore`.

## Phase 409 Verification Command

```powershell
git diff --check -- .planning/phases/409-native-cookbook-and-ci-inventory/409A-COOKBOOK-DEMO-INVENTORY.md
git status --short
```

## Non-Goals and Anti-Fake Constraints

- No product, demo, test, README, docs, roadmap, Beads, generated-roadmap, or
  state edits in this inventory task.
- No claim that snippets are compile-verified unless a focused compile or test
  command proves it in the implementing phase.
- No hidden data-path fallback: cache-backed failures must remain visible as
  support evidence, with the prior Plot path left active.
- No benchmark or GPU performance guarantee from cookbook snippets, screenshots,
  visual evidence, or support summaries.
- No pixel-perfect visual-regression claim from cookbook screenshots or PNG
  snapshot artifacts.
- No package dependency on `Videra.SurfaceCharts.Demo`; it remains
  repository-only.
- No public publish, tag, GitHub Release, or package-approval implication.
- No PDF/vector export and no image export beyond bounded PNG chart snapshots.
- No backend expansion, OpenGL/WebGL compatibility promise, or compositor-native
  Wayland support claim from cookbook work.
- No old chart view controls, compatibility wrappers, adapter layer, direct
  public `Source` loading API, or generic plotting engine.
