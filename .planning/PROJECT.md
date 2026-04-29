# Videra 开源准备项目

## What This Is

Videra 是一个基于 `.NET 8` 的跨平台 3D 渲染引擎，提供 Windows (`D3D11`)、Linux (`Vulkan`)、macOS (`Metal`) 三个平台后端，包含软件渲染回退、viewer 宿主桥接和可扩展 render pipeline。

仓库同时维护与 `VideraView` 独立的 surface-chart 模块家族，用于离线大矩阵、曲面图和时频图一类可视化场景。viewer 主线在 `v1.5-v1.13` 已经完成壳层瘦身、scene truth、backend rehydration 基线、scene realisation / residency / upload closure、event-driven dirty、shared mesh payload、queue-aware upload budgeting、scene telemetry / benchmark evidence / coordinator cleanup、alpha consumer happy path / smoke / feedback surfaces / diagnostics snapshot productization、viewer-first inspection workflow，以及 inspection fidelity 深化；`v1.16` 把 `SurfaceCharts` 收成了 source-first adoption surface，而随后 `v1.17-v1.18` 已把 repair baseline、analytics-core contracts、render fast paths 和 benchmark hotspot evidence 合并回 `master`。接下来的主线不是追 Three.js 式通用 engine parity，而是先把 Videra 做成边界清楚、包结构清楚、viewer/runtime 定位清楚的 native 桌面 viewer + inspection + surface-chart 产品。

## Core Value

**跨平台 3D 渲染引擎的可靠性** — 在三个主流平台上稳定运行，并让代码、CI、文档、Demo、包发布边界、scene runtime 合同和可观测性讲同一套真相。

## Current State

- 最新完整归档 milestone：`v2.52 Professional Chart Snapshot Export`
- 最近完成 milestone：`v2.52 Professional Chart Snapshot Export`
- 当前 active milestone：`v2.53 Chart Type Expansion and Axis Semantics`
- 当前 focus：扩展 SurfaceCharts 图表类型 (Bar, Contour) 和轴语义 (Log, DateTime, Legend)。

## Current Milestone: v2.53 Chart Type Expansion and Axis Semantics

**Goal:** Add new chart types (Bar, Contour) and richer axis semantics (DateTime, log scale, custom formatters) to SurfaceCharts, with enhanced legend overlay.

**Target features:**
- Log scale Y axis and DateTime X axis support
- Custom tick formatters per axis
- Multi-series legend overlay with per-kind indicators
- Bar chart series type (grouped/stacked, 3D rectangular prisms)
- Contour plot series type (marching squares iso-lines)
- Integration evidence for all new chart types

## Completed Milestones

### v2.52 Professional Chart Snapshot Export (2026-04-29)

Shipped a bounded chart-local PNG/bitmap snapshot export path for Plot-authored SurfaceCharts:
- `PlotSnapshotRequest/Result/Manifest/Diagnostic/Format/Background` contract types
- `Plot3D.CaptureSnapshotAsync` with Avalonia RenderTargetBitmap + SkiaSharp PNG encoding
- Demo snapshot button, consumer smoke validation, Doctor parsing
- Guardrail script `Test-SnapshotExportScope.ps1` and scope boundary documentation
- 5 phases, 5 plans, 23 requirements, all verified passed

Archived: `.planning/milestones/v2.52-ROADMAP.md`, `.planning/milestones/v2.52-REQUIREMENTS.md`

## Recently Completed Milestone: v2.51 Professional Chart Output and Dataset Evidence

**Status:** completed locally on 2026-04-29.

**Goal:** Make Plot-authored SurfaceCharts easier to export, attach, reproduce, and diagnose as professional 3D chart outputs without widening backend/runtime scope or restoring removed alpha APIs.

**Delivered outcomes:**
- inventoried existing chart output, report, dataset metadata, demo, smoke, Doctor, and guardrail surfaces before changing APIs
- added a bounded chart-local output/report contract owned by `VideraChartView.Plot`
- exposed deterministic dataset metadata and reproducibility evidence for surface, waterfall, and scatter series
- refreshed demo and consumer smoke artifacts so professional chart output evidence can be attached to support reports
- closed docs, generated public roadmap, and repository guardrails around the chart-local output/evidence model

## Recently Completed Milestone: v2.50 Plot Authoring Usability and Professional Chart Polish

**Status:** completed locally on 2026-04-29.

**Goal:** Tighten the single `VideraChartView` + `Plot.Add.*` chart authoring model into a more maintainable, professional, ScottPlot-style 3D charting surface without restoring old chart APIs or widening backend/runtime scope.

**Delivered outcomes:**
- inventoried the Plot API, professional chart gaps, demo/docs/tests, support evidence, and guardrails before changing code
- polished Plot-owned series lifecycle, names, inspection, ordering, clear/remove semantics, and revision truth
- routed professional presentation presets, color maps, and numeric precision evidence through chart-local Plot APIs
- refreshed demo, consumer-smoke, Doctor, and support evidence with series identity, chart kind, color map, precision profile, and rendering status
- tightened docs and repository guardrails around the single `VideraChartView.Plot.Add.*` contract, without old chart controls, direct public `Source`, compatibility wrappers, hidden fallback/downshift, backend expansion, or god-code

## Recently Completed Milestone: v2.49 Scene Authoring Builder Polish

**Status:** completed locally on 2026-04-28.

**Goal:** Polish the existing Core `SceneAuthoring` layer into a more approachable, high-performance, static-scene authoring experience without creating a parallel builder or widening runtime scope.

**Delivered outcomes:**
- inventoried the existing authoring API surface and kept `SceneAuthoringBuilder` as the single public authoring path
- added fluent placement/color affordances plus material and geometry preset polish on existing Core-only contracts
- improved authoring diagnostics for invalid `AddInstances(...)` inputs without fallback geometry or silent repair
- refreshed `samples/Videra.MinimalAuthoringSample` as concise file-free scene proof
- added 1k/10k instance-aware authoring evidence around existing `InstanceBatchEntry` truth

## Recently Completed Milestone: v2.48 Plot-Driven Chart Runtime

**Status:** completed locally on 2026-04-28.

**Goal:** Make `VideraChartView.Plot.Add.*` the authoritative runtime data-loading path for SurfaceCharts and remove the remaining public direct `Source` API.

**Delivered outcomes:**
- made `Plot.Add.Surface`, `Plot.Add.Waterfall`, and `Plot.Add.Scatter` the public runtime data-loading path
- deleted the public `VideraChartView.Source` / `SourceProperty` API
- migrated demo, consumer smoke, tests, support evidence, docs, and guardrails to Plot-owned loading
- added repository guardrails preventing the old direct `Source` path from returning

## Recently Completed Milestone: v2.47 Single Plot View for 3D Charts

**Status:** completed locally on 2026-04-28.

**Goal:** Replace the current multi-view SurfaceCharts public model with one maintainable `VideraChartView` and a concise `Plot.Add.*` chart authoring API.

**Delivered outcomes:**
- inventoried and deleted old `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` public View components
- introduced `VideraChartView` as the single shipped chart control
- added `VideraChartView.Plot.Add.Surface(...)`, `.Waterfall(...)`, and `.Scatter(...)`
- migrated demo, consumer smoke, support summaries, docs, and guardrails to the single Plot View model
- moved professional color-map and overlay presentation settings to `VideraChartView.Plot`

## Recently Completed Milestone: v2.46 Professional Interaction Evidence

**Status:** completed locally on 2026-04-28.

**Goal:** Make Videra's authored scenes, viewer interactions, SurfaceCharts probes, and optional workbench reports easier to attach, reproduce, and diagnose without widening renderer/backend scope.

**Delivered outcomes:**
- inventoried professional interaction evidence gaps and implementation boundaries
- added deterministic viewer interaction evidence formatting on top of public inspection/session truth
- added chart-local SurfaceCharts probe evidence formatting
- strengthened the optional Avalonia workbench professional interaction report workflow
- regenerated the Beads public roadmap and aligned docs/guardrails around report-only interaction evidence

## Recently Completed Milestone: v2.45 Professional Output and Scene Semantics

**Status:** completed locally on 2026-04-28.

**Goal:** Make Videra's authored 3D scenes and SurfaceCharts outputs more publishable, support-ready, and semantically clear without widening renderer/backend scope.

**Delivered outcomes:**
- added bounded Core static-scene scale-bar helpers for professional authored scene semantics
- added deterministic SurfaceCharts professional output evidence contracts and formatting
- strengthened the optional Avalonia workbench professional output workflow with scale-bar and output evidence
- regenerated the Beads public roadmap from the closed v2.45 issue chain
- aligned docs and guardrails around professional output boundaries, excluding backend expansion, ECS, runtime gizmos, broad chart-family expansion, compatibility layers, hidden fallback/downshift behavior, and god-code

## Recently Completed Milestone: v2.44 Professional Visualization Quality

**Status:** completed locally on 2026-04-28.

**Goal:** Improve Videra's professional visualization product surface through bounded authoring semantics, SurfaceCharts presentation quality, workbench evidence, and guardrails without widening renderer/backend scope.

**Delivered outcomes:**
- added bounded Core static-scene axis triad authoring helpers
- promoted the SurfaceCharts professional palette into a tested chart-core preset
- strengthened the optional Avalonia workbench structured support capture
- regenerated the Beads public roadmap from the closed v2.44 issue chain
- aligned public docs and capability guardrails around professional visualization boundaries, excluding ECS, runtime gizmos, backend expansion, compatibility layers, and hidden downshift behavior

## Recently Completed Milestone: v2.43 Visual Workbench and Authoring Polish

**Status:** completed locally on 2026-04-28.

**Goal:** Productize the v2.42 authoring, instance, and chart presentation contracts into a more approachable adoption layer without widening renderer/backend scope.

**Delivered outcomes:**
- added `SceneGeometry.Sphere(...)` and `SceneAuthoringBuilder.AddSphere(...)`
- added `samples/Videra.MinimalAuthoringSample` for file-free authored scenes and instance-aware markers
- unified SurfaceCharts axis/legend/hover/pinned probe readouts through chart-local numeric precision policy
- added deterministic Beads public roadmap generation from `.beads/issues.jsonl`
- added optional `samples/Videra.AvaloniaWorkbenchSample` for authored scene evidence, OBJ evidence, diagnostics snapshot, support capture, and chart precision proof

## Recently Completed Milestone: v2.42 High-Performance Visual Authoring and Professional Charts

**Status:** completed locally on 2026-04-28.

**Goal:** Move Videra toward a high-performance, easy-to-use 3D visualization library and professional 3D charting library through a bounded static-scene authoring and chart-presentation vertical slice.

**Delivered outcomes:**
- added Core-only `SceneAuthoring`, `SceneAuthoringBuilder`, `SceneGeometry`, `SceneMaterials`, and validation diagnostics
- added static primitives and typed geometry helpers for cube, plane, grid, polyline, point cloud, and buffer geometry
- added `SceneAuthoringBuilder.AddInstances(...)` so repeated same-mesh/same-material authored geometry records one retained `InstanceBatchEntry`
- added SurfaceCharts chart-local numeric label presets, overlay presets, and color-map presets
- added concise proof docs and repository guardrails around authoring, professional chart presentation, and deferred engine/chart scope

## Recently Completed Milestone: v2.41 No-Downshift Contract Hardening

**Status:** completed locally on 2026-04-28.

**Goal:** Harden the no-downshift contract after v2.40 so viewer/backend failures stay explicit by default, intentional fallback remains opt-in, and docs/support evidence cannot drift back to silent recovery language.

**Delivered outcomes:**
- inventoried fallback/default/downshift wording and behavior across backend resolution, Avalonia options, samples, docs, Doctor/support evidence, and SurfaceCharts vocabulary
- tightened viewer/backend documentation and repository guardrails so software fallback is explicit opt-in rather than default recovery
- aligned Doctor/support evidence vocabulary around not-ready/initialization failure by default and `FallbackReason` only for actual fallback
- clarified SurfaceCharts chart-local fallback language and cache-load failure wording without deleting intentional chart-local fallback

## Recently Completed Milestone: v2.40 Redundancy and Fallback Reduction Audit

**Status:** completed locally on 2026-04-28.

**Goal:** Audit and reduce redundant compatibility seams, hidden fallback/downshift behavior, and god-code hotspots without broad architecture rewrites or new product scope.

**Delivered outcomes:**
- inventoried compatibility, legacy, fallback, bridge, adapter, obsolete, deprecated, and god-code signals
- removed obsolete viewer/backend compatibility seams where authoritative replacements were already shipped
- hardened selected SurfaceCharts fallback/downshift behavior so failures remain visible
- added a surgical god-code guardrail/refactor path without broad architecture churn
- followed up by making viewer software fallback explicit opt-in by default in Core/Avalonia contracts

## Previously Completed Milestone: v2.39 Support Evidence Contract Hardening

**Status:** completed locally on 2026-04-28.

**Goal:** Harden support evidence artifacts so producer/parser drift is caught near the producer and stays visible in Doctor, release, and support documentation without expanding renderer, chart, Demo, benchmark, or release scope.

**Delivered outcomes:**
- added producer-side validation for SurfaceCharts `surfacecharts-support-summary.txt` required structured fields
- extended Doctor SurfaceCharts support-report output with complete/partial/missing/unavailable field-completeness truth
- aligned release/support/preflight documentation and repository guardrails around required support-summary fields

## Archived Milestone: v2.38 SurfaceCharts Support Experience Closure

**Status:** completed and archived on 2026-04-28.

**Goal:** Improve SurfaceCharts and Performance Lab supportability through focused demo/reporting UX, lifecycle reliability guardrails, and evidence alignment without broad renderer, chart-family, or architecture expansion.

**Delivered outcomes:**
- mapped SurfaceCharts demo/support, Performance Lab, Doctor, support evidence, and lifecycle gaps before changing code
- added richer SurfaceCharts demo support report fields for chart identity, runtime, assembly, backend/display environment, and cache-load failure context
- added deterministic timeout/cancellation guardrails for SurfaceCharts headless integration test dispatch
- added passive Doctor discovery for SurfaceCharts support reports with evidence-only vocabulary
- closed an audit-discovered packaged consumer-smoke -> Doctor contract gap so `surfacecharts-support-summary.txt` exposes the structured fields Doctor parses

## Archived Milestone: v2.37 Dependabot Dependency Triage Closure

**Status:** completed and archived on 2026-04-28.

**Goal:** Triage and close the current Dependabot PR backlog through scoped compatibility checks, targeted remediation, CI observation, and branch cleanup without broad dependency policy changes or product feature work.

**Delivered outcomes:**
- inventoried all open Dependabot PRs with package family, version, overlap, branch, CI, and mergeability context
- verified accepted analyzer, test-tooling, SourceLink, and logging dependency updates through scoped compatibility checks
- merged accepted updates after checks passed and closed superseded robot PRs with rationale
- preserved Beads close-state export, pushed Docker Dolt Beads state, and confirmed clean ready/worktree state
- introduced no renderer, chart, Demo, release, package, or architecture expansion

## Archived Milestone: v2.36 Beads Remote Sync PR Closure

**Status:** completed and archived on 2026-04-28.

**Goal:** Use the newly configured Docker-backed Beads remote sync path to coordinate and close the current branch through PR creation, CI observation, targeted remediation, merge, and cleanup without adding product scope or release/publish behavior.

**Delivered outcomes:**
- verified Beads remote push/pull operating path for multi-agent handoff state
- superseded v2.35 planning beads replaced by one clean v2.36 Beads issue chain
- opened PR `#94` with Beads-backed handoff evidence and no unrelated local planning leakage
- observed all required PR checks and confirmed no targeted remediation was needed
- merged PR `#94`, closed Beads phase state, pushed Docker Dolt state, and cleaned milestone branches

## Next Milestone Goals

The next milestone should start from `$gsd-new-milestone`. Recommended direction: return to product/quality work now that Beads coordination is operational, keep tasks small, and use Beads plus isolated worktrees only where dependencies allow parallel execution.

## Most Recently Completed Milestone: v2.36 Beads Remote Sync PR Closure

**Goal:** Use the Docker-backed Beads remote sync path to close the current PR/CI/merge loop without adding product scope or release/publish behavior.

**Delivered outcomes:**
- verified the Docker Dolt Beads remote, GitHub remote, and non-interactive push path
- opened PR `#94` from the release-evidence readiness branch and confirmed the diff excluded local-only `.planning` artifacts
- observed PR checks through completion and recorded that all checks passed
- merged PR `#94` into `master`, then preserved the final Beads close-state export on `master`
- deleted the milestone PR branch and pushed Docker Dolt Beads state after cleanup

## Superseded Planning Milestone: v2.35 PR CI Merge Closure

**Status:** superseded before execution by `v2.36 Beads Remote Sync PR Closure`.

**Reason:** v2.35 was initialized before Beads remote sync was made operational. The same PR/CI/merge intent continues in v2.36, with an explicit remote-sync prerequisite and a refreshed Beads issue chain.

## Previously Completed Milestone: v2.34 Beads Multi-Agent Coordination Closure

**Goal:** Make Beads the authoritative coordination surface for multi-agent Videra work by locking the Docker-backed Dolt service contract, worktree redirects, issue lifecycle, and validation handoff without replacing GSD planning or widening product scope.

**Delivered outcomes:**
- documented the Docker-backed Beads service contract for the `Videra` Dolt database at `127.0.0.1:3306` with the expected project id
- added canonical multi-agent onboarding guidance for ready work, claiming, discovered follow-ups, close reasons, issue export, and Dolt sync boundaries
- documented the worktree redirect pattern so parallel phase worktrees share Beads issue truth while keeping Git branch/file ownership isolated
- recorded a real Beads lifecycle proof with create/claim/discovered-from/close behavior and Docker-backed dependency observation
- added explicit Beads coordination validation plus repository guardrail tests that keep Beads out of normal product build, CI, and release authority

## Previously Completed Milestone: v2.33 Evidence Index Release Readiness Closure

**Goal:** Align release-candidate evidence indexing, release dry-run output, and readiness docs with the repo-only Doctor visual evidence truth without turning visual evidence into a publish blocker, benchmark guarantee, or visual-regression gate.

**Delivered outcomes:**
- added optional Doctor and Performance Lab visual evidence references to the release-candidate evidence contract and generated evidence index
- added optional visual evidence context to release dry-run summaries and aligned release readiness wording
- added repository tests for visual evidence index fields, optional missing/unavailable status, and non-overclaim guardrails
- aligned Doctor, support, release cutover, and Chinese guidance around release evidence index visual evidence status
- passed milestone audit with no public package publication, release tag, feed mutation, renderer work, new Demo feature, or visual-regression gate

## Previously Completed Milestone: v2.32 Doctor Evidence Integration Closure

**Goal:** Integrate Performance Lab visual evidence into the repo-only Doctor and support evidence chain so maintainers can discover, validate, and attach the bundle without overclaiming benchmark or visual-regression guarantees.

**Delivered outcomes:**
- added passive Doctor discovery for `artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json`
- exposed structured `evidencePacket.performanceLabVisualEvidence` fields for present, missing, and unavailable states
- added focused repository tests for Doctor visual evidence state handling
- aligned support, release-readiness, quality-gate, issue-template, and Chinese troubleshooting docs around evidence-only visual evidence routing
- added repository guardrails that keep Doctor visual evidence passive, repo-only, and non-mutating
- merged PR `#93` after all CI checks passed; merge commit `bf8e43a52751bb3e1fdbb40ed2fa340f924921eb`

## Previously Completed Milestone: v2.31 Performance Lab Visual Evidence Closure

**Goal:** Turn the deterministic Performance Lab scenarios from v2.30 into screenshot-backed, repo-owned visual evidence artifacts with manifest/schema validation, support-bundle alignment, and evidence-only CI publication.

**Delivered outcomes:**
- added `tools/Videra.PerformanceLabVisualEvidence` and `scripts/Invoke-PerformanceLabVisualEvidence.ps1` as the repo-local capture contract
- produced a deterministic bundle with PNG visual evidence, manifest JSON, summary text, diagnostics text, and explicit unavailable-state truth
- added GPU-independent validation for manifest schema, artifact references, screenshot dimensions, nonblank sanity, and unavailable semantics
- added an evidence-only CI artifact job for Performance Lab visual evidence
- aligned English/Chinese docs and repository guardrails around visual evidence boundaries
- merged PR `#92` after all CI checks passed; merge commit `ae1d2ba6e8f6c12b3592841df64fe938172455f2`

## Previously Completed Milestone: v2.30 Performance Lab Dataset Proof

**Goal:** Turn the existing viewer Performance Lab and SurfaceCharts streaming path into a focused, reproducible dataset proof that demonstrates large-scene rendering, streaming chart updates, and diagnostics export without broad renderer or chart-family expansion.

**Delivered outcomes:**
- added deterministic repo-owned viewer and SurfaceCharts Performance Lab scenario contracts
- wired bounded Performance Lab scenario controls for viewer instance batches and SurfaceCharts streaming paths
- enriched Performance Lab evidence snapshots and support summaries with scenario/settings/runtime diagnostics
- added focused GPU-independent tests for scenario generation, bounded controls, and evidence text
- aligned English/Chinese docs and repository guardrails around Performance Lab proof truth
- merged PR `#91` after all CI checks passed; merge commit `5277efad033a41ac6c22c98d14d71810eb563072`

## Previously Completed Milestone: v2.28 Streaming Performance Hardening

**Goal:** Harden the large-dataset performance slice by turning SurfaceCharts columnar scatter into a diagnosable streaming/FIFO path, adding chart-local interaction/refine policy, and strengthening benchmark/docs guardrails without expanding into new chart families or broad renderer architecture.

**Delivered outcomes:**
- hardened `ScatterColumnarSeries` append/FIFO semantics with bounded retention, validation, sorted continuation checks, and streaming counters
- exposed streaming/FIFO diagnostics in chart data, rendering status, demo panels, and support summary text
- added scatter `InteractionQuality` / `InteractionQualityChanged` diagnostics with deterministic `Interactive` / `Refine` transitions
- added evidence-only streaming benchmarks for append, FIFO trim, diagnostics aggregation, and allocation reporting without changing hard thresholds
- aligned English/Chinese docs and repository guardrails around streaming/FIFO, scatter interaction-quality, and evidence-only benchmark truth
## Previously Completed Milestone: v2.27 Performance Foundation Vertical Slice

**Goal:** Prove Videra can render and inspect large static 3D datasets in Avalonia desktop apps with measurable performance truth, using narrow diagnostics, viewer instancing, benchmark evidence, a focused Performance Lab, and the first SurfaceCharts columnar/streaming data path.

**Delivered outcomes:**
- added viewer and SurfaceCharts performance diagnostics for draw calls/instance counts/upload bytes/resident bytes/pickable counts and chart tile residency truth
- added first-version viewer `InstanceBatch` contracts for same mesh/material with per-instance transform/color/id and picking identity
- added retained instance-batch runtime/picking proof plus benchmark evidence
- added a focused Performance Lab in `Videra.Demo`
- added SurfaceCharts columnar scatter data with `ReplaceRange`, `AppendRange`, optional FIFO capacity, and pickable-off-by-default high-volume behavior
- merged PR `#89` after all CI checks passed; merge commit `b8b899086d13442d81bd3990989b47a2d03b0a1a`

## Previously Completed Milestone: v2.26 Branch Protection Review-Gate Closure

**Goal:** Align `master` branch protection with the repository's solo-maintainer CI-first maintenance flow so future CI-passing PRs do not require admin review bypasses.

**Delivered outcomes:**
- removed the required approving review count that forced PR `#83` to use admin merge after CI passed
- preserved strict required status checks for `verify`, `macos-native`, `windows-native`, `linux-x11-native`, and `linux-wayland-xwayland-native`
- preserved required conversation resolution
- made no product code, release, tag, package, feed, or workflow changes

## Previously Completed Milestone: v2.24 Analyzer 10 and Dependency Hygiene

**Goal:** Upgrade maintenance quality deliberately by defining and applying the analyzer 10 rule policy, tightening dependency-update hygiene, and proving the resulting quality gates without broad architecture rewrites, compatibility layers, release publishing, or product feature breadth.

**Delivered outcomes:**
- one analyzer 10 scope lock and rule-policy pass for noisy rules such as `CA1051`, `CA1720`, `CA1822`, and `CA1859`
- one bounded analyzer upgrade implementation that keeps `TreatWarningsAsErrors` useful without forcing public API churn or god-code cleanup
- one dependency-update hygiene slice covering Dependabot grouping/ignore policy and repo-wide test tooling version consistency
- one quality-gate evidence closure that proves analyzer/package hygiene through local and CI checks

## Previously Completed Milestone: v2.23 Human-Approved Alpha Release Cutover

**Status:** shipped on `2026-04-26`; PR `#81`; follow-up dependency hygiene PR `#82`; no public packages were published and no release tags were created

**Goal:** Prepare Videra for a human-approved alpha public release by making publish workflows gated, auditable, evidence-driven, and reversible without automatically publishing packages, creating release tags, mutating feeds, or widening product architecture during planning.

**Delivered outcomes:**
- established one release-control truth model across local dry-run, GitHub Packages preview, and public `nuget.org` publication paths
- added fail-closed public release preflight evidence validation for version/source/package/docs/native/consumer/benchmark readiness
- hardened human-approved public publish workflows with explicit version/tag inputs, environment approval, package-set validation, and evidence artifacts
- closed cutover, abort, release notes, install guidance, and support-evidence docs without performing an actual release
- proved preview-feed parity and final non-mutating public-release simulation guardrails
- replaced partial Dependabot `coverlet.collector` drift with a repo-owned PR that aligned all test projects on `10.0.0`

## Recently Completed Milestone: v2.22 Alpha Release Readiness Dry Run

**Status:** shipped locally on `2026-04-26`; audit `tech_debt`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** Prove the alpha candidate can be validated from clean package artifacts through Doctor, release dry-run, consumer smoke, benchmark/native evidence, and support docs without publishing, tagging, or widening product architecture.

**Delivered outcomes:**
- added explicit clean packaged consumer smoke scenarios for viewer-only, viewer + OBJ, viewer + glTF, and SurfaceCharts paths
- added Doctor evidence packet validation that correlates repository, package, benchmark, consumer-smoke, native-validation, and skipped/unavailable states
- closed the read-only release dry-run contract around package contract truth, package validation, version/tag simulation, and evidence indexing
- aligned alpha candidate docs, changelog, and support routing around what maintainers should run and what users should attach
- completed blocker-only closeout by fixing stale public API contract truth and native validation exit-code propagation

### Structural Assessment (2026-04-25)

Recent independent review confirmed that the repository's primary structural challenge is **runtime-truth convergence** rather than feature breadth. The documented primitive-centric runtime model (`SceneDocument` → `SceneNode` / `MeshPrimitive` / `MaterialInstance`) is architecturally sound, but the renderer hot-path is still in transition from the legacy `Object3D`-centric upload/delta contract. The current milestone sequence (`v2.10` runtime bridge → `v2.11` PBR consumption → `v2.18` transparent ordering) correctly addresses this in bounded slices. The review explicitly validated deferring: second UI adapter, Wayland native embedding, OpenGL/WebGL backend, animation/skeletal/morph, lighting/shadows/post-processing, and new chart families until the primitive-first render path is fully closed.

### Productization Review (2026-04-26)

Follow-up static review of the current GitHub files concluded that the next work after `v2.19` should **not** be a broad architecture rewrite. That recommendation was executed across `v2.20` and `v2.21`: importer/package contract truth, importer-backed load DX, support-ready demo diagnostics, backend capability truth, SurfaceCharts diagnostics, repo-local Doctor validation, quality-gate truth, and release readiness guardrails. `v2.22` now turns those pieces into a dry-run alpha candidate validation loop. SurfaceCharts remains an independent package family; future work should continue to avoid merging chart semantics into `VideraView`.

## Recently Completed Milestone: v2.21 Repo Doctor and Quality Gate Closure

**Status:** shipped locally on `2026-04-26`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** Improve release and support confidence by adding a repo-only `Videra Doctor` validation surface, tightening quality-gate contract truth, and adding focused allocation evidence for the importer/demo diagnostics paths delivered in `v2.20`, without widening product architecture.

**Delivered outcomes:**
- aligned benchmark threshold repository tests, benchmark docs, and contract truth with the committed hard-gate slice
- added repo-only, non-mutating `Videra Doctor` support snapshots with JSON and text artifacts under `artifacts/doctor`
- wired Doctor opt-in validation states for package validation, benchmark thresholds, consumer smoke, native validation, and demo diagnostics while reusing existing validators
- added evidence-only diagnostics allocation benchmarks for `Videra.Demo` import reports/diagnostics bundles and SurfaceCharts support-summary paths
- aligned release readiness docs and repository guardrails around Doctor, package validation, Benchmark Gates, native validation, consumer smoke, and support artifact routing

## Recently Completed Milestone: v2.20 API/DX Productization and Diagnostics Contract Closure

**Status:** shipped locally on `2026-04-26`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** Make the alpha consumer path easier to install, wire, diagnose, and support by tightening importer/load APIs, demo diagnostics, backend capability truth, and package/docs contracts without widening the architecture.

**Delivered outcomes:**
- corrected explicit importer package/docs truth and added install-by-scenario guidance plus repository guardrails
- shipped async importer registry and structured per-file load diagnostics while preserving explicit `Videra.Import.*` package boundaries
- added support-ready `Videra.Demo` diagnostics bundles, import reports, and minimal reproduction metadata without creating editor/project persistence
- exposed backend advanced API capability flags for shader/resource-set seams through render capabilities and diagnostics
- added SurfaceCharts demo active-path `RenderingStatus` diagnostics without merging chart semantics into `VideraView`

## Recently Completed Milestone: v2.19 Runtime Truth Hardening and Product Observability

**Status:** shipped locally on `2026-04-25`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** Harden the primitive-first runtime path after `v2.18` by expanding observable quality gates, tightening SurfaceCharts residency/probe efficiency, and completing runtime-truth guardrails before broader product expansion.

**Delivered outcomes:**
- expanded benchmark hard gates from 4 to 7 thresholds covering allocation, upload drain, inspection snapshot, and existing chart residency/probe paths
- reduced SurfaceCharts probe/residency churn through bounded optimizations with benchmark evidence
- added primitive-first XML documentation and tests around runtime truth seams
- aligned README, capability matrix, and localized docs with the post-`v2.18` primitive-first runtime boundary

## Recently Completed Milestone: v2.17 Bounded Emissive/Normal-Map Renderer Consumption Baseline

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不引入 shadows、environment maps、post-processing、animation、第二条公开 UI 包线或 broader runtime/package expansion 的前提下，把已经 retained 在 runtime truth 里的 emissive / normal-map-ready material inputs 最小化消费到现有 static-scene renderer，并把 docs/support/repository truth 一起收齐。

**Delivered outcomes:**
- one explicit scope lock for emissive/normal-map renderer consumption only on the existing viewer/runtime line
- one minimum renderer/material-bake contract for emissive and normal-map-ready inputs on the shipped static-scene path
- one repo-owned proof plus explicit `10`-second desktop validation on the existing proof hosts
- one docs/support/repository-guardrail pass that keeps broader advanced-runtime breadth explicitly deferred

**Delivered outcomes:**
- locked `v2.17` to emissive/normal-map renderer consumption only on the existing static-scene seam
- consumed retained emissive and normal-map-ready inputs on the shipped renderer path without introducing a generic lighting/material framework
- proved the slice through repository-owned `ConsumerSmoke` and `WpfSmoke` paths, with every desktop proof app surviving an explicit `10`-second hold
- aligned README/docs/demo wording, Chinese mirrors, and repository guardrails with the bounded emissive/normal-map seam and its explicit non-goals
## Previously Completed Milestone: v2.16 Bounded Broader Lighting Baseline

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不引入 shadows、environment maps、post-processing、emissive/normal-map renderer consumption、animation、第二条公开 UI 包线或 broader runtime/package expansion 的前提下，沿着现有 static-scene viewer/runtime advanced-runtime 线，把 `v2.15` 的 direct-lighting baseline 收成一个更宽但仍受限的 broader-lighting baseline。

**Delivered outcomes:**
- one explicit scope lock for a bounded broader-lighting follow-on slice on the existing viewer/runtime line
- one minimum broader style-lighting contract on the shipped static-scene path
- one repo-owned broader-lighting proof plus explicit `10`-second desktop validation wiring
- one docs/support/repository-guardrail pass that keeps broader advanced-runtime breadth explicitly deferred

**Delivered outcomes:**
- locked `v2.16` to one bounded broader-lighting slice on the existing style/uniform seam
- shipped a minimal fill-light contract through `LightingParameters.FillIntensity` on the native static-scene path without introducing a new lighting framework
- proved the broader-lighting baseline through repository-owned desktop proof hosts, with every validation app surviving an explicit `10`-second hold without crashing
- aligned README/docs/Chinese mirrors/support wording and repository guardrails with the broader-lighting baseline and its explicit non-goals

## Previously Completed Milestone: v2.15 Opt-In Direct Lighting Baseline

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不引入 shadows、environment maps、post-processing、animation、第二条公开 UI 包线或 broader runtime/package expansion 的前提下，在现有 static-scene viewer/runtime 路径上交付一个 opt-in 的 direct-lighting baseline，并把 docs/support/repository truth 一起收齐。

**Delivered outcomes:**
- one explicit scope lock for the first advanced-runtime slice on the existing viewer/runtime line
- one bounded direct-lighting material/render contract on the shipped static-scene path
- one repo-owned lighting proof plus diagnostics evidence on the supported host path
- one docs/support/repository-guardrail pass that keeps broader advanced-runtime breadth explicitly deferred

**Delivered outcomes:**
- locked `v2.15` to one bounded first advanced-runtime slice and kept broader lighting/runtime/package breadth explicitly deferred
- closed the Vulkan/native-path direct-lighting contract gap on the shipped static-scene viewer path without introducing a new lighting framework
- proved the baseline through repository-owned smoke hosts, including opt-in `10`-second survival validation on the selected desktop proof apps
- aligned README/docs/Chinese mirrors/support wording and repository guardrails with the direct-lighting baseline and its explicit non-goals

## Previously Completed Milestone: v2.14 WPF Hosted Adapter Validation

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不引入第二条公开 UI 包线、不扩 backend/runtime/chart/import breadth 的前提下，把现有 `WpfSmoke` 从 repo-only 烟雾证明推进成一个更严格的第二宿主验证基线，证明当前 viewer/runtime seam 能在 `WPF on Windows` 上稳定复用。

**Delivered outcomes:**
- one explicit scope lock for second-host validation on `WPF on Windows`
- one minimal shared host-seam tightening slice for the existing viewer/runtime line
- one repo-only WPF golden-path / diagnostics parity slice
- one docs/support/repository-guardrail pass that keeps `WpfSmoke` validation-only and non-public

**Delivered outcomes:**
- locked `v2.14` to one bounded repository-only `WPF on Windows` validation slice with explicit non-goals around public UI expansion and broader runtime/chart/backend/import breadth
- tightened the shared host-sync seam so both Avalonia and `WpfSmoke` reuse the same sized native-host synchronization path
- proved one repository-only WPF golden path with a bounded rendered scene and richer `wpf-smoke-diagnostics.txt` support artifact
- aligned README/docs/native-validation wording and repository guardrails with `WpfSmoke` as validation/support evidence only on the Avalonia-first public viewer path

## Recently Completed Milestone: v2.13 Package Install Boundary Tightening

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在现有 Avalonia viewer 产品线上，收紧默认安装面，让 importer-backed model loading 成为显式包/使用选择，并让 package truth、sample truth、docs、size evidence 与 repository guardrails 讲同一套真相。

**Delivered outcomes:**
- one explicit scope lock for package/install-boundary tightening only
- one default dependency-surface slimming slice for `Videra.Avalonia`
- one explicit importer-backed packaged load-path slice
- one docs/package-validation/repository-guardrail closure slice

**Delivered outcomes:**
- `Videra.Avalonia` 不再默认携带 importer packages，viewer-only 路径更瘦
- importer-backed `LoadModelAsync(...)` / `LoadModelsAsync(...)` 现在通过显式 `Videra.Import.*` 安装和 `VideraViewOptions.ModelImporter` 接入
- packaged consumer smoke 现在诚实地走 explicit importer-enabled path
- docs、`Validate-Packages.ps1`、package-size budget 与 repository guardrails 已对齐新的安装边界

## Recently Completed Milestone: v2.11 Static glTF/PBR Renderer Consumption

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在现有 viewer/runtime 边界上，把已经导入并保存在 runtime truth 里的 static glTF/PBR metadata 真正消费到 renderer，使 imported truth 与 on-screen truth 之间的落差继续缩小，而不扩大产品范围。

**Delivered outcomes:**
- one explicit scope lock for static glTF/PBR renderer consumption with runtime/rendering/import/chart/backend/UI-adapter breadth, compatibility shims, and migration adapters out of scope
- one texture-transform and UV-set consumption slice on the shipped static-scene renderer path
- one occlusion-aware renderer-consumption slice plus bounded golden-scene evidence
- one docs/support/repository-guardrail pass that teaches consumed-vs-imported-only metadata truth honestly

**Delivered outcomes:**
- locked `v2.11` to renderer consumption of already-imported static glTF/PBR metadata with broader runtime/render/import/chart/backend/UI scope still deferred
- shipped runtime consumption of baseColor texture sampling with UV-set selection plus `KHR_texture_transform` on the existing static-scene path
- shipped occlusion texture binding/strength consumption plus bounded golden-scene evidence on the same path
- aligned README/docs/demo wording, Chinese mirrors, and repository guardrails with the new consumed-vs-retained boundary

## Recently Completed Milestone: v2.10 Imported Asset Runtime Tightening

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在现有 viewer/runtime 边界上，收紧 imported asset truth 与最终 upload/render 对象之间的过渡层，优先把 runtime hot path 从 `Object3D`-centric 向 primitive-centric 推进，并把 delta/upload 粒度与优先级做得更真实。

**Delivered outcomes:**
- one explicit scope lock for imported-asset/runtime tightening with renderer/material breadth, new chart families, extra UI adapters, backend/API breadth, compatibility shims, and migration adapters out of scope
- one primitive-level upload/render bridge that allows mixed opaque and transparent primitive participation without guarding the whole imported asset as an object-level failure case
- one finer-grained scene delta and upload queue contract with change kinds, coalescing, and visibility/interaction-aware prioritization
- one docs/diagnostics/repository truth pass that teaches the primitive-centric runtime story honestly on the existing viewer/runtime line

**Delivered outcomes:**
- locked `v2.10` to imported-asset/runtime tightening with renderer/material breadth, new chart families, extra UI adapters, backend/API breadth, compatibility shims, and migration adapters out of scope
- shipped a primitive-level runtime bridge that expands imported runtime entries into multiple internal runtime objects on the canonical hot path
- shipped typed retained-entry deltas plus coalesced attached-first upload-queue behavior while keeping backend rebind on the same queue path
- aligned docs, localized mirrors, and repository guardrails with the primitive-centric runtime story

## Recently Completed Milestone: v2.12 SurfaceCharts Residency and Probe Efficiency

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在现有 `SurfaceCharts` 家族边界上，优先降低 cache-backed / overview-first surface path 的 tile residency churn、probe latency，以及 render/overlay 之间不必要的 invalidation/allocation，使大矩阵交互更稳，而不扩大 chart/runtime/platform 产品范围。

**Delivered outcomes:**
- one explicit scope lock for `SurfaceCharts` efficiency work with new chart families, chart-kernel widening, viewer/runtime breadth, backend/UI/platform expansion, compatibility shims, and migration adapters out of scope
- one tile-residency prioritization and prefetch slice on the existing surface/cache-backed path
- one probe-latency and interaction-stability slice on the current chart-local `SurfaceChartView` path
- one invalidation/allocation/benchmark guardrail slice that keeps churn and probe regressions visible

**Delivered outcomes:**
- locked `v2.12` to one bounded SurfaceCharts efficiency slice and kept chart-family/runtime/platform widening out of scope
- tightened interactive tile residency behavior on the existing cache-backed / overview-first surface path
- reduced probe-path churn on the shipped `SurfaceChartView` overlay path without widening into a generic analytics kernel
- aligned benchmark/docs/Chinese mirrors/repository guardrails around the same post-181/182 efficiency story while keeping hard-gate benchmark names stable

## Recently Completed Milestone: v2.9 Release Candidate Final Validation

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在当前 public package/release-candidate 边界上，交付最终候选验证的 scope lock、version/tag simulation guardrails、release evidence packet，以及 abort/cutover runbook truth。

**Delivered outcomes:**
- locked `v2.9` to final release-candidate validation with publishing, tag creation, runtime/rendering/material/chart/backend/UI breadth, compatibility shims, and migration adapters out of scope
- added candidate version/tag simulation through `scripts/Test-ReleaseCandidateVersion.ps1` and release dry-run wiring
- added `eng/release-candidate-evidence.json` and release-candidate evidence index generation
- added abort/cutover runbook truth and repository guardrails for human approval boundaries

## Recently Completed Milestone: v2.8 Release Candidate Contract Closure

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不新增 runtime/rendering/chart/backend 功能、不做兼容层或迁移适配的前提下，把 Videra 当前 public package line 的 API、包资产、release workflow、release notes/docs/support truth 收成一个可验证的 release-candidate contract。

**Delivered outcomes:**
- locked `v2.8` to release-candidate contract closure with explicit non-goals around runtime breadth, renderer/material breadth, new chart families, extra UI adapters, compatibility shims, actual public publishing, and tag creation
- added deterministic public API guardrails for the shipped public package surface
- added a read-only release dry-run and package asset evidence path driven by `eng/public-api-contract.json`
- aligned README, capability matrix, package matrix, support matrix, release policy, releasing runbook, changelog, localized docs, and repository tests around the same release-candidate boundary

## Recently Completed Milestone: v2.7 Inspection Replay Diagnostics Closure

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不新增 bundle/archive 格式、fallback replay、partial restore、`VideraView` API breadth、renderer/material/backend/platform breadth 或 editor-style project persistence 的前提下，让 `VideraInspectionBundleService`、`VideraDiagnosticsSnapshotFormatter`、`Videra.InteractionSample`、consumer smoke 和 support docs 对 inspection replay/support 讲同一套真相。

**Delivered outcomes:**
- one explicit scope lock for inspection replay/support diagnostics coherence
- one bundle replay contract closure around `CanReplayScene`, `ReplayLimitation`, and no target-view mutation for non-replayable imports
- one diagnostics/support truth closure around `VideraDiagnosticsSnapshotFormatter`, `TransparentFeatureStatus`, last-frame object counts, and inspection-bundle guidance
- one guardrail and verification slice that keeps the public support truth from drifting

**Delivered outcomes:**
- locked `v2.7` to inspection replay/support diagnostics coherence with explicit non-goals
- strengthened bundle replayability evidence and sample wording around `CanReplayScene` and `ReplayLimitation`
- aligned diagnostics/support docs, issue-template boundaries, README surfaces, localization mirrors, and repository guardrails
- added focused final guardrails for diagnostics count relationships and replayability metadata

## Recently Completed Milestone: v2.5 Static glTF/PBR Breadth

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不扩大到动画、灯光/阴影、后处理、额外 UI adapter 或 `WebGL` / `OpenGL` API breadth 的前提下，基于 `v2.4` 已统一的 runtime-truth 路径，交付一个新的 bounded static-scene glTF/PBR breadth 切片。

**Delivered outcomes:**
- locked static-scene glTF/PBR breadth into one explicit milestone boundary with frozen non-goals
- preserved per-primitive non-`Blend` material participation while keeping mixed `Blend` / non-`Blend` imports guarded
- preserved occlusion texture binding/strength and texture-transform-aware bindings on imported-asset/runtime material truth
- aligned docs, support wording, demo text, Chinese mirrors, and repository guardrails on the same broadened static material story

## Recently Completed Milestone: v2.6 Render Pipeline Diagnostics Closure

**Status:** shipped locally on `2026-04-23`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在不扩大到灯光/阴影、后处理、动画、额外 UI adapter、backend API 或新 glTF/material feature breadth 的前提下，把现有 viewer/runtime render-pipeline diagnostics contract 做成可验证、可支持、可 guardrail 的闭环。

**Delivered outcomes:**
- one explicit scope-lock slice for render-pipeline diagnostics closure with frozen non-goals
- shipped backend-neutral pipeline snapshot / diagnostics object counts for existing pass and feature participation
- tightened transparent, picking, screenshot, and overlay diagnostics evidence on existing test/smoke seams
- aligned docs, support wording, samples, localization mirrors, and repository guardrails with the diagnostics boundary

## Recently Completed Milestone: v2.3 Surface Analytics Depth

**Status:** shipped locally on `2026-04-22`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在现有 surface family 边界内，把 `SurfaceCharts` 从“能展示”继续推向“更专业的 analytics surface”，优先补非均匀坐标/标量语义、分析级 probe，以及一个明确的分析视图 proof。

**Delivered outcomes:**
- one scope-lock slice for analytics-depth surface work with frozen non-goals
- one explicit/non-uniform coordinate and scalar-field contract upgrade under the existing surface family
- one analysis-grade probe workflow with interpolation and comparative readouts
- one concrete analytics proof path plus aligned truth/guardrails on the existing Avalonia chart line

**Delivered outcomes:**
- locked analytics-depth surface work to the existing family boundary and kept generic `Chart3D`, `SurfaceMesh`, a fourth family, new UI adapters, and `Wayland` / `OpenGL` out of scope
- shipped explicit/non-uniform coordinate semantics and preserved separable height/color scalar fields on the existing surface line
- shipped an analysis-grade probe workflow with interpolated reads and comparative delta readouts on the existing Avalonia path
- added one concrete analytics proof path and aligned docs, support, package, release, and repository truth around the same analytics-depth story

## Recently Completed Milestone: v2.2 Scatter Productization

**Status:** shipped locally on `2026-04-22`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Delivered outcomes:**
- one explicit scope lock for analytics-depth scatter productization inside the existing family boundary
- one line-capable presentation slice under `ScatterChartView`
- one repo-owned scatter proof path on the existing Avalonia chart line
- one docs/support/package/release/repository truth pass that teaches the productized scatter story honestly

**Delivered outcomes:**
- locked `ScatterChartView` productization to the existing family boundary and froze explicit non-goals around generic `Chart3D`, `SurfaceMesh`, a fourth family, new UI adapters, and `Wayland` / `OpenGL`
- shipped line-capable scatter presentation under the existing family boundary without creating a top-level `PointLine` family
- added one honest repo-owned scatter proof path to `Videra.SurfaceCharts.Demo`
- aligned docs, support wording, package/release truth, CI evidence, and repository guards around the same scatter-proof story

## Previously Completed Milestone: v2.1 Scatter Chart Family

**Status:** shipped locally on `2026-04-22`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Delivered outcomes:**
- one explicit scope lock for a concrete third chart family and frozen non-goals
- one minimal kernel/runtime reuse slice only for `ScatterChartView`
- one thin direct Avalonia scatter proof with chart-local interaction/render truth
- one docs/support/package/release/repository truth pass that teaches the shipped tri-family story honestly

**Delivered outcomes:**
- locked `ScatterChartView` as the third concrete chart family and froze explicit non-goals around generic `Chart3D`, `SurfaceMesh`, a fourth family, new UI adapters, and `Wayland` / `OpenGL`
- extended only the minimal chart contracts/runtime seams needed for the scatter proof
- shipped one thin direct `ScatterChartView` path on the existing Avalonia chart line
- aligned docs, support wording, package/release truth, CI evidence, and repository guards around the same `Surface + Waterfall + Scatter` chart story

## Previously Completed Milestone: v2.0 Advanced Runtime Features

**Status:** shipped locally on `2026-04-22`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Delivered outcomes:**
- one explicit transparency slice boundary and frozen non-goals before implementation starts
- one truthful transparent feature contract plus diagnostics vocabulary on the existing runtime path
- one alpha mask rendering baseline on the current runtime path
- one deterministic alpha blend ordering baseline without broadening into OIT or generic material abstractions
- one docs/support/repository truth pass that keeps the transparency story honest

**Delivered outcomes:**
- locked one concrete transparency slice and explicit non-goals around lighting, shadows, environment maps, post-processing, animation, generic material abstractions, new UI adapters, and OIT
- established the transparent contract and diagnostics truth on the existing runtime path
- shipped alpha mask rendering and deterministic alpha blend ordering baselines on the current runtime path
- aligned docs, support wording, release/package truth, and repository guardrails around the same transparency story

## Previously Completed Milestone: v1.27 Additional Chart Breadth

**Status:** shipped locally on `2026-04-22`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Goal:** 在 `v1.25` 已经把 `SurfaceCharts` public product line 和最小 chart-kernel seam 收稳、`v1.26` 又验证了仓库不会因第二 UI shell 证明而扩 scope 的前提下，选择并交付一个第二具体 chart family，让 chart kernel 的“可复用”从抽象承诺变成一条真实产品线，而不把仓库重新拉回 generic `Chart3D` 或宽泛 chart 平台。

**Delivered outcomes:**
- locked `Waterfall` as the second concrete chart family and froze explicit non-goals around generic `Chart3D`, a third family, a second UI shell, and broader runtime expansion
- proved a thin `Waterfall` chart path on the existing Avalonia line without widening kernel, runtime, or host ownership
- aligned docs, support wording, package/release truth, and repository guardrails around the shipped dual-family `Surface + Waterfall` chart story
- closed the milestone with PR-based merges and clean worktree cleanup, with no accepted milestone-local debt

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? -> Move to Out of Scope with reason
2. Requirements validated? -> Move to Validated with phase reference
3. New requirements emerged? -> Add to Active
4. Decisions to log? -> Add to Key Decisions
5. "What This Is" still accurate? -> Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check -- still the right priority?
3. Audit Out of Scope -- reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-29 after starting v2.52 milestone*


