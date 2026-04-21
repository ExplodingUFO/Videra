# Videra 开源准备项目

## What This Is

Videra 是一个基于 `.NET 8` 的跨平台 3D 渲染引擎，提供 Windows (`D3D11`)、Linux (`Vulkan`)、macOS (`Metal`) 三个平台后端，包含软件渲染回退、viewer 宿主桥接和可扩展 render pipeline。

仓库同时维护与 `VideraView` 独立的 surface-chart 模块家族，用于离线大矩阵、曲面图和时频图一类可视化场景。viewer 主线在 `v1.5-v1.13` 已经完成壳层瘦身、scene truth、backend rehydration 基线、scene realisation / residency / upload closure、event-driven dirty、shared mesh payload、queue-aware upload budgeting、scene telemetry / benchmark evidence / coordinator cleanup、alpha consumer happy path / smoke / feedback surfaces / diagnostics snapshot productization、viewer-first inspection workflow，以及 inspection fidelity 深化；`v1.16` 把 `SurfaceCharts` 收成了 source-first adoption surface，而随后 `v1.17-v1.18` 已把 repair baseline、analytics-core contracts、render fast paths 和 benchmark hotspot evidence 合并回 `master`。接下来的主线不是追 Three.js 式通用 engine parity，而是先把 Videra 做成边界清楚、包结构清楚、viewer/runtime 定位清楚的 native 桌面 viewer + inspection + surface-chart 产品。

## Core Value

**跨平台 3D 渲染引擎的可靠性** — 在三个主流平台上稳定运行，并让代码、CI、文档、Demo、包发布边界、scene runtime 合同和可观测性讲同一套真相。

## Current State

- 最新完整归档 milestone：`v1.20 Viewer Product Boundary and Core Slimming`
- 当前 active milestone：`—`
- 当前 focus：从 `v1.20` 的 boundary/package closeout 转回下一阶段的 runtime/product depth，优先候选仍是 `v1.21 Scene and Material Runtime v1`

## Next Milestone Candidates

- `v1.21 Scene and Material Runtime v1`：补齐 `SceneNode` / `MeshPrimitive` / `MaterialInstance` / `Texture2D` / `Sampler` 等 scene/material runtime 核心模型
- `v1.22 Static glTF/PBR`：把静态 glTF 和 PBR 基线打穿到 UV、texture、metallic-roughness、normal/emissive/alpha、tangent、sRGB/linear
- `v1.23 Inspection Productization`：把 load → inspect → measure → annotate → export/replay 主线收成更明确的产品流
- `v1.24 Performance and Size Gates`：把 benchmark 和 package size 从证据提升到 threshold gate
- `v1.25 SurfaceCharts Productization and Chart Kernel`：先把 `Videra.Charts.Surface` 产品化，再评估抽共享 chart kernel
- `v1.26 Second UI Adapter Validation`：在 Avalonia 之外验证第二 UI 适配层和 host seams
- `v2.0 Advanced Runtime Features`：灯光、阴影、环境贴图、透明排序、post-processing、动画等高级 runtime 能力

## Latest Completed Milestone: v1.20 Viewer Product Boundary and Core Slimming

**Goal:** 先把 `Videra` 从“已有正确骨架的 native viewer/runtime”收成一个边界清楚的 `1.0` 产品：明确 viewer/runtime 能力矩阵，收紧 `Videra.Core`，把 importer / logging / hosting seams 从隐式依赖变成显式 package 和 contract。

**Target features:**
- a clear viewer/runtime `1.0` vs engine `2.0` capability matrix instead of broad engine-adjacent language
- a slimmed `Videra.Core` that no longer carries concrete importer or Serilog provider dependencies
- dedicated `Import` / `Logging` / `Hosting` package seams that keep Avalonia and backend integrations thin
- docs, package matrix, consumer smoke, and repository guards that all describe the same viewer-first product boundary

**Status:** shipped locally on `2026-04-21`; audit `passed`; archived locally without a milestone tag because repository release tags stay version-aligned

**Delivered outcomes:**
- defined one explicit `1.0` boundary document and capability/package-layer matrix so Videra now reads as a native desktop viewer/runtime plus inspection and source-first charts instead of a blurry “engine-ish” runtime
- extracted concrete importer and logging-provider dependencies out of `Videra.Core`, leaving a slimmer runtime kernel and explicit `Videra.Import.*` package composition
- documented and guarded the host/package boundary so `Core`, `Import`, `Backend`, `UI adapter`, and `Charts` remain independently explainable without widening public abstractions
- aligned package docs, support/release docs, Chinese onboarding docs, consumer smoke, package validation, and publish workflows around the same canonical public viewer stack

## Previously Completed Milestone: v1.18 SurfaceCharts Analytics Core

**Goal:** 把 `SurfaceCharts` 从 source-first surface control 推进成专业 surface analytics core：优先升级 geometry/axis/scalar contracts，再落 shader/LUT recolor、proper normals、以及 low-copy residency 这类高杠杆实现基础，并以 label-gated 热点证据替代硬性阈值作为 milestone 收口条件。

**Target features:**
- generalized surface geometry contracts through `RegularGrid` / `ExplicitGrid` while keeping `SurfaceMatrix` as a convenience type
- axis-scale and scalar-field contracts that support non-uniform coordinates, `DateTime`/`TimeSpan`-style axes, independent `HeightField` / `ColorField`, and first-class missing-data semantics
- render fast paths for shader/LUT recolor, seam-safe derived normals, and lower-copy tile residency
- analytics benchmark evidence for `recolor` / `orbit` / `probe` / `churn` / `cache lookup-miss` / `resize-rebind` hotspots without widening into generic `Chart3D`

**Status:** shipped locally on `2026-04-20`

**Delivered outcomes:**
- generalized the chart-core data model through `SurfaceGeometryGrid`, richer axis-scale semantics, and a preserved `SurfaceMatrix` convenience path
- separated `HeightField` / `ColorField` and promoted masks / holes / `NaN` regions into first-class data semantics that flow through render/probe behavior
- moved recolor, normals, and resident scalar handling onto higher-value GPU/LUT and low-copy fast paths
- split the SurfaceCharts benchmark suite into focused hotspot classes, added benchmark smoke tests, aligned docs/repository truth, and merged the work after green CI

## Previously Completed Milestone: v1.16 SurfaceCharts Adoption Surface

**Goal:** 把 `SurfaceCharts` 收成一个清晰的 source-first adoption surface：有 canonical `first chart` 路径、有稳定 public contract、有 consumer-facing evidence，也有和 viewer package 真相分开的 release/support 叙述。

**Status:** shipped locally on `2026-04-20`

**Delivered outcomes:**
- froze one canonical source-first `first chart` story and aligned `SurfaceChartView` contract language around `ViewState`, interaction, overlays, rendering status, and chart-local ownership
- added a narrow source-first evaluation path so `SurfaceCharts` can be tried without reverse-engineering the full demo application
- proved the chart adoption path in CI/support-facing artifacts so diagnostics and troubleshooting share the same runtime truth
- aligned README / support / release truth so `SurfaceCharts` stays explicitly source-first and does not inherit `VideraView` package promises by accident

## Previously Completed Milestone: v1.15 Repository Guard and Evidence Calibration

**Goal:** 把 `v1.14` 审计里最明确的 follow-up 收口成可验证、低风险、可审计的 cleanup：repository guard 语义更准确，quality-gate scope 更真实，benchmark stewardship 说法更一致。

**Status:** shipped locally on `2026-04-20`

**Delivered outcomes:**
- converted the `OpenGL` repository guard from token presence into an explicit no-product-promise contract
- widened `quality-gate-evidence` to the outward-facing `Videra.MinimalSample` path and removed a false-red repository-test assumption for fresh worktrees
- aligned benchmark workflow naming, benchmark runbook guidance, README wording, release guidance, and planning language around an opt-in label-gated review model

## Previously Completed Milestone: v1.14 Compatibility and Quality Hardening

**Goal:** 在不增加 `OpenGL`、不扩 public API、也不追求 compositor-native Wayland 的前提下，把 alpha 当前最真实的兼容性与质量风险收成更清晰、更可验证、更少误导的 contract。

**Status:** shipped locally on `2026-04-20`

**Delivered outcomes:**
- normalized the built-in backend minimum contract across `D3D11`, `Vulkan`, and `Metal`, making unsupported seams explicit instead of backend-specific folklore
- aligned Linux host/display-server truth across diagnostics, smoke output, and public docs around the same `X11` / `XWayland` compatibility story
- promoted packaged consumer smoke plus advanced public sample evidence into routine merge-time validation
- tightened the alpha-ready `green` definition around packaged consumer and curated-Core quality evidence without pretending benchmark review is already a hard numeric blocker

## Previously Completed Milestone: v1.13 Inspection Fidelity

**Goal:** 把 inspection workflow 从 viewer-first 能用提升到 inspection truth 更可信：命中结果 mesh-accurate、measurement snapping 明确、same-API fast path 有证据、support 可以交付可重放的 inspection bundle。

**Status:** shipped locally on `2026-04-19`

**Delivered outcomes:**
- richer mesh-accurate hit results built on per-mesh acceleration structures while preserving object-level public selection semantics
- viewer-first snap modes `Free` / `Vertex` / `EdgeMidpoint` / `Face` / `AxisLocked` for measurements without introducing editor tooling
- same-API fast paths through cached clip-truth reuse and preferred live snapshot readback, backed by dedicated inspection benchmarks
- exportable/restorable inspection bundles plus doc/architecture/support truth alignment around the fidelity story

**Observed outcomes:**
- `Measure`、annotation anchors 和 future probe 默认依赖更精确的 hit truth，而 host-facing selection 仍然保持 object-level public 语义。
- inspection state restore 现在稳定保留 snap mode、clipping、measurements、annotations 和 camera truth；显式 multi-backend epsilon harness 仍是后续可选增强。
- `ClippingPlanes` 和 `ExportSnapshotAsync(...)` 保持当前 public story，但内部已经获得 capability-aware fast path 与 truthful fallback。
- sample / demo / support workflow 可以交付一份可重放的 inspection bundle，而不是让用户手工拼凑 bug 描述。

## Previously Completed Milestone: v1.12 Viewer-First Inspection Workflow

**Goal:** 把 `VideraView` 的下一条价值主线收成真实 inspection workflow：用户能做 section / clipping、轻量 measurement / probe、保存和恢复 view state，并导出可共享的 inspection snapshot，而不用走 editor 或深 extensibility 路径。

**Delivered outcomes:**
- `VideraView` 现在公开 `ClippingPlanes`、`Measurements`、`CaptureInspectionState()`、`ApplyInspectionState(...)` 和 `ExportSnapshotAsync(...)`，把 inspection workflow 收成稳定 viewer 合同。
- Interaction/overlay/runtime seams 现在支持 clipping 和 measurement 这类 viewer-first inspection truth，而没有把产品面拖向 editor/gizmo。
- Diagnostics snapshot、consumer smoke、minimal/interaction samples、README、Avalonia README 和 alpha feedback 文档现在都能围绕 inspection workflow 讲同一套故事。
- Focused tests、consumer smoke 和 full verify 已证明 clipping、measurement、inspection-state restore 和 snapshot export 都能通过当前 alpha 支撑面运行。

## Previously Completed Milestone: v1.11 Alpha Happy-Path Stabilization and Diagnostics Productization

**Goal:** 把 `Videra` 从“技术上已经可消费的 alpha”推进到“外部用户愿意试、试了能反馈、反馈后能快速定位”的 alpha：默认 happy path 只有一条，diagnostics 能一键导出，反馈和支持面围绕同一 reproduction contract，benchmark 继续作为趋势型回归门而不是一次性证据。

**Delivered outcomes:**
- 冻结唯一 public happy path：`Options -> LoadModel(s) -> FrameAll/ResetCamera -> BackendDiagnostics`
- 增加 `VideraDiagnosticsSnapshotFormatter`，并让 minimal sample 与 consumer smoke 输出同一 diagnostics snapshot 合同
- 对齐 alpha feedback / issue templates / troubleshooting / support docs，使外部反馈围绕同一 diagnostics snapshot 与 reproduction checklist
- 把 benchmark docs、release docs、consumer smoke 与 publish workflow 收成同一条 alpha consumer story

## Previously Completed Milestone: v1.10 Alpha Consumer Integration and Feedback Loop

**Goal:** 把 Videra 从“内部引擎感很强的 alpha”收口成更容易接入、验证、反馈的 embeddable viewer 组件：默认 happy path 更短，consumer smoke 真正覆盖 package 安装到 first scene，benchmark evidence 进入 workflow，公开反馈入口和支持边界更清楚。

**Delivered outcomes:**
- 增加 `Videra.MinimalSample`，把默认 first-scene story 收成 `Options`、`LoadModel(s)`、`FrameAll/ResetCamera`、`BackendDiagnostics`
- 增加 package-only `Videra.ConsumerSmoke`、`Invoke-ConsumerSmoke.ps1` 和 multi-host workflow，验证公共包安装到 first scene 的接入链
- 增加 viewer / surface charts benchmark runner、workflow gate 和 benchmark docs，把性能证据变成可见 artifact
- 对齐 alpha feedback / issue templates / troubleshooting / support docs，使外部反馈围绕同一 canonical consumer path 和 truthful Linux X11/XWayland 边界

## Previously Completed Milestone: v1.9 Scene Performance Evidence and Coordinator Cleanup

**Goal:** 先把 scene pipeline 的性能和预算行为做成“可测量、可解释、可回归”的合同，再基于这些证据把剩余偏厚的 scene runtime 协调层收薄，并在最后补 public onboarding 的可发现性。

**Delivered outcomes:**
- scene upload telemetry 现在暴露 queued/uploaded bytes、upload latency、resolved budgets 和 failure counts，并通过 `BackendDiagnostics` 进入稳定 viewer 诊断壳层
- dedicated `Videra.Viewer.Benchmarks` 项目已覆盖 import、batch import、residency apply、upload drain 和 load/rebind 场景
- `SceneRuntimeCoordinator` 现在拥有 scene publish/apply/enqueue/rehydrate/diagnostics 的内部主线，`VideraViewRuntime` 进一步退回 shell/orchestration 角色
- `VideraView` public scene/load/camera 入口和 model-load result types 现在带 XML docs，root/Avalonia/extensibility docs 也已对齐 canonical quick-start

## Previously Completed Milestone: v1.7 Scene Pipeline Closure v1

**Goal:** 把 `SceneDocument → import → upload → engine sync → backend recovery` 这条链收成更稳定、更 native、也更可维护的内部 runtime contract：document 负责真相，delta 负责变化，residency 负责 GPU 状态，upload queue 负责预算式上传，frame prelude 负责在 render/device cadence 中落地。

**Delivered outcomes:**
- 新增 dedicated `Videra.Avalonia.Tests`，为 runtime-scene / upload-queue / import-service work 提供低层测试地基
- `ImportedSceneAsset` / `Object3D` 现在持有预算、deferred readiness 和 recovery 所需元信息
- `SceneDocument` 现在具备稳定 internal identity、version 和 ownership semantics；`Items` 同步改成增量 mutation
- scene publish 拆成 `SceneDocumentStore`、`SceneDeltaPlanner`、`SceneEngineApplicator`、`SceneResidencyRegistry`、`SceneUploadQueue` 和 `RuntimeFramePrelude`
- backend/surface recovery 现在基于 retained scene truth 做 dirty reupload，并通过 `BackendDiagnostics` 暴露 pending/resident/dirty/failed scene counts
- `SceneImportService` 统一了 single/batch import orchestration；`Videra.Demo` 只新增窄 `Scene Pipeline Lab`，README / docs / repository guards 也已对齐

## Previously Completed Milestone: v1.6 Scene Pipeline Truth and Backend Surface Closure

**Goal:** 把 `SceneDocument` / `ImportedSceneAsset` 提升为 viewer runtime 的真实 contract，彻底分离导入与上传，进一步摆脱 `LegacyGraphicsBackendAdapter` 的过渡形态，并用一个窄验证场景证明 scene pipeline 与 backend rebind 的真相。

**Delivered outcomes:**
- `SceneDocument` 现在持有 document entries，并成为 `VideraViewRuntime` 的权威 scene owner；viewer mutations 先改 document，再同步到 engine
- `LoadModelAsync()` / `LoadModelsAsync()` 现在先产生 backend-neutral imported assets，再在 active resource factory 可用时上传；批量导入改成有界并行和原子 replace
- backend / surface recreation 可以基于 retained imported assets 重建 scene 资源，不再把 `SoftwareResourceFactory` 当 steady-state fallback 真相
- built-in software / D3D11 / Vulkan / Metal backends 现在直接实现 `IGraphicsDevice` / `IRenderSurface`，orchestrator 只在非迁移 legacy backend 上使用 compatibility adapter
- `Videra.Demo` 新增窄 `Scene Pipeline Lab`，README / docs / repository guards 全面对齐新 contract

## Previously Completed Milestone: v1.5 VideraView Runtime Thinning and Render Orchestration

**Goal:** 把 `VideraView` 收成真正的 public shell，并把 runtime coordination、frame scheduling、scene ownership、interaction semantics、overlay projection、graphics abstraction 和 `VideraEngine` 内部分层收口到更稳定的内部架构上。

**Delivered outcomes:**
- `VideraViewRuntime` 内部化 `session / bridge / overlay / native host / input` 协调
- invalidation-driven frame scheduling，替代 ready 后常驻 `16ms` render loop
- backend-neutral `SceneDocument / ImportedSceneAsset` 路径和单一 scene owner baseline
- Core-level camera manipulation / picking / selection services
- Core-level overlay projection / selection outline / annotation layout services
- graphics abstraction v2：分离 device 与 surface，并通过 adapter 保持兼容
- `VideraEngine` 内部拆分为 `RenderWorld / PassRegistry / ResourceLifetimeRegistry / SharedFrameState`

## Requirements

### Validated

- ✓ Cross-platform test infrastructure, matching-host native validation, and truthful platform scope for Windows / Linux / macOS — `v1.0`
- ✓ Distribution/install/release truth across English and Chinese entrypoints — `v1.0`
- ✓ Explicit render-pipeline contract, host-agnostic orchestration, and public extensibility APIs with guarded sample/docs truth — `v1.1`
- ✓ Surface-chart module family ships as a sibling product area instead of a `VideraView` mode — `v1.1`
- ✓ Professional surface-chart baseline and camera-true chart spine through runtime/view-state recovery, built-in interaction, true 3D picking, camera-aware LOD, GPU slimming, backend-owned color mapping, and professional overlays — `v1.2-v1.3`
- ✓ Open-source consumption and release surfaces through README/docs/public-feed truth, NuGet metadata completeness, collaboration routing, release-page configuration, and dependabot automation — `v1.4`
- ✓ `VideraView` shell thinning, invalidation-driven scheduling, Core interaction/overlay extraction, graphics abstraction v2 seam introduction, and `VideraEngine` internal decomposition — `v1.5`
- ✓ SceneDocument single-source truth, import/upload separation, scene rehydration after backend/surface recreation, platform-side backend-v2 migration, and narrow lab/docs truth — `v1.6`
- ✓ Scene pipeline closure through dedicated runtime tests, asset metrics, hardened `SceneDocument`, incremental item mutation, delta/application services, budgeted upload queue, recovery diagnostics, unified import service, and narrow lab/docs truth — `v1.7`
- ✓ Event-driven scene residency, shared mesh payload, queue-aware upload budgeting, and focused residency/recovery correctness coverage — `v1.8`
- ✓ Scene upload telemetry, viewer-scene benchmark evidence, scene runtime coordinator extraction, and public onboarding discoverability — `v1.9`
- ✓ Happy-path viewer onboarding, consumer package smoke, benchmark workflow gates, and alpha feedback/support truth — `v1.10`
- ✓ Viewer-first inspection workflows through clipping, measurement, inspection-state persistence, snapshot export, and aligned diagnostics/sample/docs closure — `v1.12`
- ✓ Inspection fidelity through mesh-accurate hit truth, viewer-first snap modes, same-API fast paths, inspection benchmarks, and replayable inspection bundles — `v1.13`
- ✓ Compatibility truth, packaged consumer/sample evidence, backend minimum-contract normalization, and actionable alpha-ready quality-gate semantics — `v1.14`
- ✓ `SurfaceCharts` source-first adoption surface through canonical `first chart` docs, chart-local contract language, CI/support evidence, and release-boundary truth — `v1.16`
- ✓ Generalized surface geometry/scalar contracts, chart-local render fast paths, and label-gated analytics hotspot benchmark evidence — `v1.18`

### Active

- (None yet — define the next milestone before starting new phase work.)

### Out of Scope

- Promoting new viewer/runtime orchestration APIs to public surface — `v1.18` deepens chart-local analytics contracts instead of widening `VideraEngine`/`VideraView`
- Generic `Chart3DView` / scene-wide multi-series abstraction before a second concrete analytics series exists — first make `SurfaceCharts` deep, then make 3D charts broad
- Native Wayland compositor-hosted embedding or explicit `OpenGL` backend work — current priority is analytics depth on already-supported backends, not a fourth graphics API
- Publishing `Videra.SurfaceCharts.*` as stable public consumer packages in `v1.18` — source-first adoption proof landed in `v1.16`; package expansion comes after contract stabilization
- Interpolated probe, contour/wireframe, slicing, waterfall/scatter/mesh 3D series, or automated benchmark thresholds — these are deliberately deferred follow-up threads after the core contract and hot-path baseline land
- Editor-style authoring tools, transform handles, or gizmos — `Videra` stays viewer-first
- Another viewer-scene runtime rewrite — the active optimization work is chart-local and data-model-driven, not a new engine rewrite

## Context

### Current Strengths

- `VideraView` is now a thin shell over runtime-local coordination with invalidation-driven scheduling
- `SceneDocument` is authoritative viewer scene truth, and scene publication already flows through explicit delta / residency / upload services
- built-in backends directly satisfy `IGraphicsDevice` / `IRenderSurface`, while recovery and diagnostics operate against retained scene truth
- selection / annotation / clipping / measurement / inspection-state seams now sit on richer mesh hit truth, explicit measurement snap modes, and replayable inspection bundles
- `SurfaceCharts.Core` / `Processing` / `Avalonia` 已经有清晰分层，`SurfaceChartView` 还是 chart-local thin shell，现有 `ViewState`、内建 orbit/pan/dolly/focus、overlay、hover/pinned probe、lazy cache-backed tile scheduling 和独立 benchmark suite 都已经存在
- viewer package install, diagnostics shell, benchmark workflows, minimal sample, consumer smoke, and the new source-first chart adoption path all exist, so analytics work can target real outward-facing seams instead of hypothetical ones

### Current Risks

- surface-cache manifest v1 still cannot represent explicit-grid or non-linear-axis metadata, so richer cache DTO work remains a follow-up once the analytics contracts settle.
- low-copy residency now assumes `SurfaceTile` instances are immutable snapshots; callers must publish a new tile object instead of mutating height/color memory in place.
- Vulkan still carries a finite scalar-descriptor cache budget under high residency churn; the current milestone reserves headroom but does not claim the story is closed for heavier future analytics scenes.
- Current chart benchmark coverage now exists for recolor/orbit/probe/churn/cache lookup-miss/resize-rebind hotspots, but the render-host slice is intentionally scoped to benchmark-local contract-path cost rather than real driver/swapchain overhead.

## Constraints

- **Public API**: keep `VideraEngine` as the only public extensibility root and do not widen `VideraView` surface — preserves the viewer/runtime boundary established in `v1.5-v1.8`
- **Inspection truth**: picking, clipping, measurement, and snapshot results must agree across hardware backends and software fallback within explicit epsilon budgets — keeps support evidence defensible
- **Viewer-first workflow**: snap modes may guide inspection, but no editor gizmos, transform handles, or authoring semantics are allowed — keeps product direction aligned
- **Upload cadence**: GPU resource creation must stay in render/device cadence, not public scene mutation APIs — avoids UI-path stalls
- **Scene truth**: retained scene truth continues to live in `SceneDocument` — rehydration and residency orbit document truth, not engine state
- **Consumer path first**: default docs, smoke, and samples should optimize time-to-first-scene through one canonical options/load/camera/diagnostics flow
- **Published-package validation**: consumer smoke and release docs must prove the public package install path, not just repo-local project references
- **Support truth**: Linux support language must stay honest about X11-hosted and XWayland-compatible paths; native Wayland embedding remains deferred
- **Diagnostics portability**: alpha issue reporting should prefer one copy-paste diagnostics snapshot or one inspection bundle over freeform manual environment summaries
- **Chart-local depth first**: keep `SurfaceCharts` as a dedicated chart stack in `v1.18`; do not introduce a generic multi-series `Chart3D` scene abstraction until a second concrete 3D series justifies it
- **Contract migration safety**: `SurfaceMatrix` stays as the regular-grid convenience type even if deeper contracts move to generalized grid/scalar abstractions
- **Analytics semantics before visuals**: contour, slicing, and richer probe workflows stay deferred until the underlying grid/axis/scalar contracts and benchmark evidence are stable

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep `v1.8` fully inside internal scene/runtime seams | The public extensibility story was already stable; the highest leverage was runtime efficiency, not API growth | completed in `v1.8` |
| Replace global per-frame dirty sweeps with event-driven residency transitions | Dirty work should be caused by document/epoch changes, not render cadence itself | completed in `v1.8` |
| Tackle shared mesh payload now that `v1.7` stabilized residency/upload semantics | Copy-reduction was the next highest-leverage memory win | completed in `v1.8` |
| Keep upload budgeting heuristic-driven rather than telemetry-driven in this round | Runtime-mode and queue-aware budgets were implementable now without platform-specific complexity | completed in `v1.8` |
| Make telemetry/benchmarks the first step of `v1.9` | The next performance move should be data-driven rather than another inference-based rewrite | completed in `v1.9` |
| Delay deeper mesh/native optimization until evidence exists | Shared payload and queue budgeting already changed the baseline; more change needs proof | completed in `v1.9` |
| Shift the next milestone toward consumption and feedback, not another internal optimization loop | The public alpha now has enough internal stability that adoption friction is the highest-value gap | completed in `v1.10` |
| Keep Wayland compositor-native work below public-consumer improvements | Current documented X11/XWayland truth is sufficient for alpha positioning; default integration path and feedback loops are more urgent | completed in `v1.10` |
| Freeze a single public happy path before expanding alpha surface area | Default onboarding should not force new users to choose between multiple configuration stories | completed in `v1.11` |
| Prefer diagnostics snapshot/export and support closure over another deep performance pass | External alpha reports are currently more leverage than one more internal optimization cycle | completed in `v1.11` |
| Use viewer-first inspection workflows to pressure stabilized runtime seams before another architecture/performance milestone | Clipping, measurement, inspection-state, and snapshot export are user-visible features that validate the current alpha viewer as a working inspection component | completed in `v1.12` |
| Use inspection fidelity as the `v1.13` direction instead of widening product surface | The biggest remaining gap is trust in hit truth, reproducibility, and supportability, not another package or plugin seam | completed in `v1.13` |
| Start `v1.14` with compatibility and quality hardening instead of an `OpenGL` line | The highest current alpha risk is truthful compatibility/evidence and backend contract drift, not the absence of a fourth graphics API | completed in `v1.14` |
| Start `v1.15` as a small cleanup milestone instead of reopening platform expansion | The accepted debt is now concrete enough to close directly, while `OpenGL` and native Wayland still lack evidence to outrank guard/evidence hardening | completed in `v1.15` |
| Keep benchmark review opt-in and label-gated in `v1.15` | The current workflow uploads comparable artifacts but has no reliable threshold/baseline machinery for automatic numeric blocking | completed in `v1.15` |
| Start `v1.16` with `SurfaceCharts` adoption instead of another viewer-only cleanup loop | The highest-value unanswered question is now whether the chart stack can become a credible source-first product surface for external consumers | completed in `v1.16` |
| Keep `v1.16` SurfaceCharts release truth on demo/docs/CI/support-summary evidence instead of package assets | The milestone goal is adoption proof and supportability, not an accidental public package promise | completed in `v1.16` |
| Start `v1.17` as a repair milestone instead of opening another product-surface thread immediately | The latest CI evidence said the trust gap was benchmark compile drift, SurfaceCharts analyzer debt, and Linux `XWayland` smoke stability, so restoring the green line outranked another new feature push | merged on `master`; archive pending |
| Start `v1.18` with data/coordinate/scalar contract depth instead of `OpenGL`, public package expansion, generic `Chart3D`, or immediate feature sprawl | The highest-value next step is turning `SurfaceCharts` into a professional analytics core; generalized contracts unlock contour, probe, slice, and future series work without premature scene abstraction | completed in `v1.18` |
| Start `v1.19` with presentation-space and interaction-default work instead of jumping straight to contour/probe or second-series expansion | The current highest-value gap is not deeper analytics math but that the default chart does not yet present those semantics clearly; display-space separation also unlocks later log-axis and preset work | active in `v1.19` |
| Start `v1.20` with viewer product boundary and core slimming instead of jumping straight to scene/material runtime, static PBR, or chart productization | The next highest-leverage move is to make Videra's `1.0` product boundary and package layering explicit before broadening runtime features; otherwise later milestones will keep landing on a blurry core | completed in `v1.20` |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

## References

- Archive roadmap: `.planning/milestones/v1.0-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.1-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.2-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.3-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.4-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.5-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.6-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.7-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.8-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.9-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.10-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.11-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.12-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.13-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.14-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.15-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.16-ROADMAP.md`
- Archive roadmap: `.planning/milestones/v1.18-ROADMAP.md`
- Milestone index: `.planning/MILESTONES.md`
- Retrospective: `.planning/RETROSPECTIVE.md`

---
*Last updated: 2026-04-21 after archiving the v1.20 Viewer Product Boundary and Core Slimming milestone*
