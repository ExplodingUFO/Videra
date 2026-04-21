# Phase 18: Demo, Docs, and Repository Truth for Professional Charts - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning
**Source:** auto discuss-phase capture; recommended defaults selected inline

<domain>
## Phase Boundary

Phase 18 locks the outward-facing truth for the professional surface-chart slice that Phases 14-17 already built. It covers the independent chart demo, English and Chinese documentation entrypoints, and repository guards that keep the chart boundary and shipped behavior honest. It does not reopen renderer, scheduler, cache, overlay, or interaction architecture except where the demo/docs layer needs small, truth-preserving presentation changes.

</domain>

<decisions>
## Implementation Decisions

### Demo story and UX surface
- **D-01:** `Videra.SurfaceCharts.Demo` remains the single public chart demo. Phase 18 should deepen that independent demo instead of routing chart onboarding back through `Videra.Demo` or `VideraView`.
- **D-02:** The demo should show the currently shipped chart story directly in the sample UI: source switching, overview/detail viewport presets, axis/legend overlays, hover readout, `Shift + LeftClick` pinned probes, and renderer/fallback truth. The current data-path controls stay, but Phase 18 should add lightweight visibility for the newer chart behaviors instead of leaving them implicit.
- **D-03:** The demo should use a balanced product-facing surface, not an exhaustive diagnostics lab. Small status/help panels and behavior-specific copy are preferred over a large matrix of backend/debug toggles.

### Limitation wording and claim discipline
- **D-04:** Phase 18 must describe branch reality, not roadmap intent. If a behavior is still host-driven or only partially interactive, the demo and docs must say so explicitly rather than inheriting older or more ambitious milestone prose.
- **D-05:** The shipped chart story for this checkout is: independent module family, chart-local renderer seam, `GPU-first` renderer with explicit `software fallback`, axis/legend overlays, hover probe plus `Shift + LeftClick` pinning, view-aware residency/cache path, and host-driven overview/detail viewport selection. Do not claim a finished free-camera orbit/pan/dolly workflow or compositor-native Wayland support.
- **D-06:** Limitation language should stay narrow and concrete. It should identify what is not finished without drowning out the professional-chart behaviors that now are shipped.

### Documentation layering and localization
- **D-07:** Detailed surface-chart truth should live in the English module/demo READMEs that directly match the shipping artifacts: `samples/Videra.SurfaceCharts.Demo/README.md`, `src/Videra.SurfaceCharts.Avalonia/README.md`, `src/Videra.SurfaceCharts.Core/README.md`, and `src/Videra.SurfaceCharts.Processing/README.md`.
- **D-08:** The root English README and `docs/zh-CN/README.md` should act as routing/index pages with high-signal chart summaries, links, and key capability/limitation statements. They should not carry a second long-form chart contract that can drift from module pages.
- **D-09:** Chinese documentation remains a concise mirror where English is authoritative, but it must preserve the same boundary and capability truth: independent demo, no `VideraView` mode, shipped overlays/probe/rendering behavior, current limitation wording, and the current Linux `XWayland compatibility` claim.

### Public module boundary
- **D-10:** `Videra.SurfaceCharts.Rendering` stays a chart-local implementation detail in public onboarding. Phase 18 should explain renderer truth through the `Videra.SurfaceCharts.Avalonia` control docs and the demo, not introduce `Rendering` as a separate public entry module.
- **D-11:** Public chart docs should continue to present the module family as `Core`, `Avalonia`, `Processing`, and the independent demo. The rendering package can be referenced where needed to explain internals, but it should not become a new top-level onboarding target.

### Repository guard strategy
- **D-12:** Repository verification should combine exact sentence/link guards with behavior-level sample tests. That is the preferred proof surface for Phase 18; whole-file snapshot tests are not.
- **D-13:** Guards should lock three kinds of truth together: the sibling boundary from `VideraView`, the shipped chart capability/limitation language in docs, and the actual sample behavior exposed by `Videra.SurfaceCharts.Demo`.
- **D-14:** Milestone claims added or updated in Phase 18 should be frozen through repository tests where possible, especially when root docs or localization pages summarize shipped chart behavior.

### the agent's Discretion
- The exact UI treatment for renderer status, probe instructions, and dataset/path summaries in the demo, as long as it stays lightweight and product-facing.
- Which English/Chinese pages should carry one-line summaries versus short bullet lists, provided detailed truth still lives in the module/demo READMEs.
- The specific repository-test split between sample-behavior tests, documentation-term constants, and repository-architecture guards.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and milestone state
- `.planning/PROJECT.md` — current milestone state, active chart risks, and the decision to keep chart docs/demo truthful and separate from `VideraView`
- `.planning/REQUIREMENTS.md` — `DEMO-01` plus the already-validated chart requirements from Phases 14-17 that docs/demo must now reflect accurately
- `.planning/ROADMAP.md` — Phase 18 goal, plan split, dependencies, and success criteria
- `.planning/STATE.md` — current project position after Phase 17 and the recommended next step for Phase 18
- `AGENTS.md` — repository-wide working rules and documentation/editing constraints

### Prior phase context that already fixed chart truth
- `.planning/phases/08-demo-completion-and-user-feedback-truthfulness/08-CONTEXT.md` — demo-truth precedent: user-visible status first, exact capability gating, and docs/tests that match real behavior
- `.planning/phases/11-public-extensibility-apis/11-CONTEXT.md` — doc layering precedent: public entrypoints should be explicit, boundary-safe, and reinforced with repository guards rather than vague prose
- `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-CONTEXT.md` — chart-local renderer seam, `GPU-first` path, explicit `software fallback`, and Linux host-limit truth
- `.planning/phases/17-large-dataset-residency-cache-evolution-and-optional-rust-spike/17-CONTEXT.md` — scheduler/cache/statistics/native-seam decisions that the demo/docs layer must describe without overclaiming
- `.planning/phases/17-large-dataset-residency-cache-evolution-and-optional-rust-spike/17-VERIFICATION.md` — verified Phase 17 truths for residency, cache I/O, statistics, and benchmark/native-boundary language

### Original surface-chart product intent
- `docs/plans/2026-04-13-surface-charts-design.md` — original product boundary, independent demo requirement, and the rule that chart semantics stay separate from `VideraView`
- `docs/plans/2026-04-13-surface-charts-implementation.md` — implementation-plan precedent that README/sample docs and repository verification are part of the shipped chart slice

### Current public chart-facing docs
- `README.md` — current English entrypoint that still contains outdated surface-chart limitation wording and must be reconciled with Phase 15-17 reality
- `docs/zh-CN/README.md` — Chinese routing page that must mirror the same chart boundary/capability truth without contradiction
- `src/Videra.SurfaceCharts.Core/README.md` — current English chart-core contract page
- `src/Videra.SurfaceCharts.Avalonia/README.md` — current English control-layer truth for renderer seam, overlays, and limitations
- `src/Videra.SurfaceCharts.Processing/README.md` — current English processing truth for cache, batch reads, statistics, and optional native seam
- `docs/zh-CN/modules/videra-surfacecharts-core.md` — current Chinese core module mirror
- `docs/zh-CN/modules/videra-surfacecharts-avalonia.md` — current Chinese Avalonia module mirror; currently lags behind shipped axis/legend/probe/rendering truth
- `docs/zh-CN/modules/videra-surfacecharts-processing.md` — current Chinese processing module mirror; currently lags behind batch/statistics/native-seam truth
- `samples/Videra.SurfaceCharts.Demo/README.md` — current English independent-demo truth and limitation wording

### Current demo implementation and verification surface
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` — current demo UX structure, chart surface, and sample-side status panels
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` — current source switching, viewport presets, dataset summary, and cache-path presentation
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs` — public control surface, `RenderingStatus`, and `RenderStatusChanged`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs` — current chart-local probe input path and pinned-probe gesture
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs` — current axis, legend, hover probe, and pinned-probe overlay composition
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs` — current demo layout/asset/repository existence guards
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs` — current headless sample behavior coverage for source and viewport switching
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` — current repo guards for boundary, renderer truth, verify scripts, and native-boundary rules
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryLayoutTests.cs` — current repo guard for module-family project/readme layout
- `tests/Videra.Core.Tests/Repository/SurfaceChartsDocumentationTerms.cs` — current documentation-term constants used by repository tests

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` and `MainWindow.axaml.cs`: already provide a usable independent demo shell with source selection, viewport selection, status text, cache-path text, and dataset summary panels.
- `SurfaceChartView.RenderingStatus` / `RenderStatusChanged`: already expose control-visible backend/fallback truth that the demo can surface more explicitly without inventing a new diagnostics API.
- `SurfaceChartView.Overlay.cs`: the control already renders axes, legend, hover probe, and pinned probes, so Phase 18 can make these behaviors more legible in the demo/docs rather than building them from scratch.
- `SurfaceChartsDemoConfigurationTests` and `SurfaceChartsDemoViewportBehaviorTests`: already provide a sample-behavior verification seam that Phase 18 can extend for UX/truth coverage.
- `SurfaceChartsRepositoryArchitectureTests` plus `SurfaceChartsDocumentationTerms`: already establish the repo pattern of exact-sentence documentation guards for chart boundary and renderer truth.

### Established Patterns
- Public chart onboarding is README-driven: root README routes, module READMEs hold artifact-level truth, and repository tests lock critical sentences/links.
- Chinese docs are concise mirrors where English is authoritative, but the mirrors are still expected to preserve boundary truth and quick-start links.
- The chart product line stays separate from `VideraView`; demo and docs are expected to reinforce that sibling boundary rather than blur it.
- Verification prefers deterministic tests over screenshot/snapshot-only checks: repository assertions for docs/layout plus headless Avalonia tests for sample behavior.

### Integration Points
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` is the natural place to add lightweight user-facing explanations for probe gestures, rendering status, or current limitations.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` is the seam for surfacing `RenderingStatus`, active source details, and any current-path text updates.
- `README.md` and `docs/zh-CN/README.md` are the routing entrypoints that need their chart summaries aligned with the module/demo READMEs.
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` is the right guard point for exact chart-boundary and documentation-truth assertions.
- `tests/Videra.Core.Tests/Samples/*.cs` is the right verification surface for independent-demo UX and behavior expectations.

</code_context>

<specifics>
## Specific Ideas

- `[auto]` Selected all gray areas: demo story surface, limitation wording, documentation layering, public module boundary, and repository guard strategy.
- `[auto]` Demo story surface → selected the recommended middle path: keep the current independent sample and source/viewport flow, but make shipped overlay/probe/rendering behavior explicit in the sample UI instead of turning the demo into a broad diagnostics workbench.
- `[auto]` Limitation wording → selected the recommended truth-first option: describe only the current checkout reality and do not market unfinished orbit/pan/dolly or compositor-native Wayland support as shipped.
- `[auto]` Documentation layering → selected the recommended routing model: English module/demo READMEs hold the detailed chart contract; root README and Chinese entrypoints summarize and route.
- `[auto]` Public module boundary → selected the recommended boundary-safe option: keep `Videra.SurfaceCharts.Rendering` as an implementation detail documented through `Videra.SurfaceCharts.Avalonia`, not a new public onboarding surface.
- `[auto]` Repository guards → selected the recommended mixed proof surface: exact sentence/link guards plus demo behavior tests instead of whole-file snapshots.
- `[auto]` No phase-matching todos were found, so nothing was folded into scope.

</specifics>

<deferred>
## Deferred Ideas

- Adding a full diagnostics dashboard that enumerates every backend/fallback combination live in the demo.
- Reopening chart runtime or camera-interaction implementation work from earlier phases.
- Promoting `Videra.SurfaceCharts.Rendering` into a separate public module family entrypoint.
- Broad localization expansion beyond the chart pages and entrypoints needed for truthful parity in this phase.

</deferred>

---

*Phase: 18-demo-docs-and-repository-truth-for-professional-charts*
*Context gathered: 2026-04-14*
