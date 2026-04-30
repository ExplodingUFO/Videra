# Phase 16: Rendering Host Seam and GPU Main Path - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning
**Source:** manual discuss-phase capture; user accepted recommended defaults (`按推荐`)

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 为 surface-chart 建立 chart-local 的 rendering host seam，让 `SurfaceChartView` 不再直接同时承担 view、scene rebuild、CPU painter orchestration 三种职责。
2. 在不把 chart 语义回灌进 `VideraView` 主线的前提下，落地 GPU-first 主渲染路径，并保留真实可用的 software fallback。
3. 先把渲染态和 tile residency 的增量更新边界做明确，避免 viewport / input 变化时每次都重建整份 `SurfaceRenderScene`。

本阶段**不**包含：
- 把 surface-chart 重新并回 `VideraView` / `RenderSession` / `NativeControlHost` 主线
- 把 Phase 17 的大数据 cache / pyramid / batch-I/O 演进提前打包进来
- 借 Phase 16 顺手兑现完整 chart runtime / `ViewState` / orbit-pan-dolly UX 承诺
- broad demo polish 或营销式文档扩写；只允许补齐 renderer host limit / fallback truth

</domain>

<decisions>
## Implementation Decisions

### Phase scope and architectural guardrails
- **D-01:** Phase 16 必须先建立 chart-local rendering seam，再接 GPU / software backend；不能直接把 GPU 叠到当前 `SurfaceChartView` + `SurfaceScenePainter` 结构上后再回头整理。
- **D-02:** surface-chart 继续保持与 `VideraView` 的 sibling boundary；`RenderSession` / `VideraViewSessionBridge` 只能作为 seam 参考，不是 chart 的 orchestration root。
- **D-03:** 如果为了清晰边界需要新增 chart-specific rendering package / project，可以新增；但不得把 chart-specific host contract 倒灌回 viewer 公共契约。

### Baseline seam strategy
- **D-04:** 优先把当前 `SurfaceChartView -> renderer` 关系重构成显式 chart-local render-host contract，让 GPU path 和 software path 都挂在同一 seam 下。
- **D-05:** `SurfaceChartView` 在本阶段应朝“control adapter + overlay owner + input glue”收敛，而不是继续作为 full render-scene rebuild owner。
- **D-06:** 现有 CPU painter path 可以继续作为 fallback / reference implementation，但不再被视为长期主路径的 ownership 形态。

### GPU presentation model
- **D-07:** 主路径必须是“真实 GPU 渲染”，不能把“先 GPU 再拷回 Avalonia bitmap”包装成主路径真相。
- **D-08:** backend 选择允许由 host capability 决定，但 renderer contract 必须能表达“支持 GPU 时直接走 GPU path，不支持时退回 software path”。
- **D-09:** overlay / axis / legend / probe 继续保持 chart-local；如果某些 native-surface host 形态会限制 Avalonia overlay 叠放，这个限制必须显式体现在 docs / diagnostics / tests 中，不能依赖隐式行为。

### Fallback and diagnostics truth
- **D-10:** software fallback 是 shipped contract，不只是测试替身。
- **D-11:** backend/fallback 选择必须对 host 可见；至少要能知道 active mode、是否 fallback、以及 fallback reason。
- **D-12:** demo、README、tests 必须讲同一套 renderer/fallback/host-limit 真相，不能 silent fallback。

### Incremental state scope
- **D-13:** Phase 16 的增量更新重点是 renderer state，而不是一次性吞下 scheduler / cache / I/O 全部演进。
- **D-14:** viewport / input 变化不应再默认触发整份 `SurfaceRenderScene` rebuild；需要拆出可独立失效的 projection、geometry/color upload、tile residency 等边界。
- **D-15:** `SurfaceTileScheduler` 的策略升级、cache reopen/copy 问题、100M 级数据路径和 Rust hotspot seam 继续留在 Phase 17，除非只是为 Phase 16 renderer seam 提供最小必要支撑。

### the agent's Discretion
- 具体类型名、文件名、project 名可由后续 research / planning 决定，只要职责边界与本 context 等价。
- diagnostics surface 可以是新的 chart-local snapshot / status API，也可以是扩展现有 control-visible surface，但必须保持只读、可测试、可文档化。
- Phase 16 内允许最小必要的 README / demo 文案更新，用来反映实际 shipped renderer host limits；不要求一次补齐完整 chart UX 宣传口径。

</decisions>

<specifics>
## Specific Ideas

- 用户没有给额外产品参考或视觉参照，直接接受了默认建议组合：`1A,2A,3A,4A`。
- 本阶段的核心取舍是“先把 seam 和真相做对”，再让 GPU 主路径落在正确的 ownership 上。
- 规划与研究都应以当前 checkout 为准，而不是假设 later-branch 里的 chart runtime / interaction 已经存在。

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and milestone constraints
- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `AGENTS.md`

### Prior phase outputs and boundary decisions
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-CONTEXT.md`
- `.planning/phases/10-host-agnostic-render-orchestration/10-CONTEXT.md`
- `.planning/phases/11-public-extensibility-apis/11-CONTEXT.md`
- `.planning/phases/15-adaptive-axes-legend-and-probe-readout/15-RESEARCH.md`
- `.planning/phases/15-adaptive-axes-legend-and-probe-readout/15-01-SUMMARY.md`
- `.planning/phases/15-adaptive-axes-legend-and-probe-readout/15-02-SUMMARY.md`
- `.planning/phases/15-adaptive-axes-legend-and-probe-readout/15-03-SUMMARY.md`
- `.planning/phases/15-adaptive-axes-legend-and-probe-readout/15-VERIFICATION.md`

### Current surface-chart rendering and control path
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderer.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderScene.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/SurfacePatchGeometryBuilder.cs`
- `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs`

### Viewer-side seam references that may inform, but not own, chart rendering
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Rendering/RenderSessionOrchestrator.cs`
- `src/Videra.Avalonia/Rendering/RenderSessionSnapshot.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`
- `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/IVideraNativeHost.cs`

### Existing tests and docs that define current truth
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceRendererInputTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceScenePainterTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewLifecycleTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`
- `src/Videra.SurfaceCharts.Avalonia/README.md`
- `samples/Videra.SurfaceCharts.Demo/README.md`

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SurfaceRenderer` + `SurfacePatchGeometryBuilder`: 当前已经把 tile values 转成 patch geometry / color，可作为 software fallback 或 GPU upload staging 的事实基线。
- `SurfaceChartProjection` + overlay presenters + `SurfaceProbeService`: Phase 15 已经把 axis / legend / probe truth 做成 chart-local，可继续复用且不应绑死到某个 backend。
- `SurfaceTileScheduler` + `SurfaceChartController`: 已有 overview-first 请求和 viewport 变化入口，可作为 renderer residency invalidation 的上游事件源。

### Established Patterns
- surface-chart 必须保持 sibling module boundary，仓库 guards 已明确防止 chart overlay / probe 逻辑泄漏回 `VideraView`。
- shipped truth 需要可被 docs / demo / tests 一致表达；fallback 和 host limits 不能靠隐藏行为维持。
- `VideraView` 侧已经证明 “orchestrator + bridge + diagnostics snapshot” 这种分层是可行的，但 chart 路径要做的是对应的 chart-local 版本，而不是直接复用 viewer ownership。

### Integration Points
- `SurfaceChartView` 是当前 control adapter，Phase 16 的 render-host seam 很可能从这里切入。
- `SurfaceRenderScene` 是当前“全量 scene snapshot”边界；若要做增量 state，这里很可能要被拆薄、替代或包裹。
- `SurfaceScenePainter` 是当前 CPU fallback/reference path；若 Phase 16 设计得当，它应退到 backend implementation，而不是继续做 view-owned main path。
- `src/Videra.SurfaceCharts.Avalonia/README.md` 与 `samples/Videra.SurfaceCharts.Demo/README.md` 现在仍带有早期 alpha 文案，后续若 renderer/fallback 合同发生变化，需要同步 truth。

</code_context>

<deferred>
## Deferred Ideas

- 大数据 residency / cache / pyramid / batch-I/O 演进，以及相关 Rust hotspot seam，留给 Phase 17。
- broad chart runtime / `ViewState` / rich camera interaction 兑现，不在 Phase 16 中顺带扩大范围。
- 任何试图把 chart renderer 重新并入 `VideraView` host stack 的方案，均视为越界，不在本阶段讨论。

</deferred>

---

*Phase: 16-rendering-host-seam-and-gpu-main-path*
*Context gathered: 2026-04-14*
