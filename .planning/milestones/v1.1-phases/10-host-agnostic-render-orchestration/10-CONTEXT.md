# Phase 10: Host-Agnostic Render Orchestration - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.1 milestone continuation + direct planning path (`$gsd-plan-phase 10`)

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 把 `RenderSession` 中可独立存在的 render orchestration 提炼成不依赖 `VideraView` / `NativeControlHost` 的内部协调层。
2. 让 `VideraView` 回到 Avalonia UI shell + input/native-host surface 的角色，不再承担散落的 session attach/resize/diagnostics orchestration。
3. 用 headless-friendly tests 和 architecture guards 固定新的职责边界，为 Phase 11 的 public extensibility API 做准备。

本阶段**不**包含：
- 对外开放 `IRenderPassContributor`、`RegisterPass(`、`FrameHook` 等 public extensibility API（Phase 11）
- 新的大型渲染特性，如 PBR、阴影、后处理、shader graph
- compositor-native Wayland embedding
- higher-level macOS safer binding replacement
- 替换 Avalonia 或整体 UI 宿主栈
- 改变 `VideraEngine` 作为 frame-plan / render-pipeline owner 的事实

</domain>

<decisions>
## Implementation Decisions

### Phase scope
- **D-01:** Phase 10 只做内部 orchestration 解耦与测试闭环，不兑现新的 public extension API。
- **D-02:** 优先保护 `v1.0` 已验证的平台 truth、lifecycle contract、diagnostics truth 和 demo 行为。
- **D-03:** 若必须在“最少改动”与“长期可维护性”之间取舍，优先建立可持续的内部 ownership seam。

### Ownership and boundary rules
- **D-04:** `VideraEngine` 继续拥有 frame plan / pipeline stage sequencing；Phase 10 不把 pipeline ownership 上移到 `RenderSession` 或 `VideraView`。
- **D-05:** `VideraView` 应回到 UI shell，保留 Avalonia property surface、native host visual tree、pointer input 和 engine-facing convenience API。
- **D-06:** host-agnostic orchestration 必须能在不实例化 `VideraView`、`NativeControlHost`、`WriteableBitmap`、`DispatcherTimer` 的条件下被组装和测试。
- **D-07:** render loop driver、software bitmap presentation copy、UI invalidation 都属于 adapter 层，不应再和 backend activation / suspend / rebind state 纠缠在同一实现里。

### Concrete seam direction
- **D-08:** 规划默认命名为：
  - `RenderSessionInputs`
  - `RenderSessionSnapshot`
  - `RenderSessionOrchestrator`
  - `VideraViewSessionBridge`
- **D-09:** `RenderSessionOrchestrator` 应拥有 session state、backend lifecycle、resolution/error/display-server/pipeline snapshot truth，以及一次 frame render 的协调入口。
- **D-10:** `RenderSession` 应保留 Avalonia-specific presentation adapter 职责，例如 render loop driver、bitmap creation/copy、request-render callback，但不再持有完整 orchestration 状态机。
- **D-11:** `VideraViewSessionBridge` 应成为 `VideraView` 与 session/orchestrator 的唯一同步入口，承接 attach/resize/handle/diagnostics refresh 的桥接逻辑。

### Contract and quality
- **D-12:** Phase 7 的 harmless `no-op` 契约必须保留：`Dispose` 后公开入口不抛异常、不重建 backend。
- **D-13:** Phase 9 的 pipeline truth 必须继续可读：`RenderPipelineProfile`、`LastFrameStageNames`、`UsesSoftwarePresentationCopy` 不能因为解耦而丢失。
- **D-14:** 新核心边界必须有 headless-friendly integration coverage，并有 repository-level guards 防止 `VideraView` 再次滑回 orchestration root。

### the agent's Discretion
- 若执行时发现更合适的内部类型命名，可调整文件名/类型名，但必须保留同等职责边界，并同步更新 docs / guards / tests。
- 允许在 `src/Videra.Avalonia/Rendering/` 与 `src/Videra.Avalonia/Controls/` 之间微调新 seam 的文件落点，只要“host-agnostic orchestration”与“view glue”边界清晰。

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and milestone constraints
- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `AGENTS.md`

### Prior phase outputs
- `.planning/phases/05-架构解耦与原生边界硬化/05-SUMMARY.md`
- `.planning/phases/07-render-contract-consistency-and-lifecycle-safety/07-SUMMARY.md`
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-CONTEXT.md`
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-RESEARCH.md`
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-SUMMARY.md`

### Current render/session/view boundaries
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Rendering/RenderSessionHandle.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`
- `src/Videra.Avalonia/Controls/VideraView.Input.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraViewOptions.cs`
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineSnapshot.cs`

### Existing tests and architecture assertions
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- `ARCHITECTURE.md`
- `.planning/codebase/ARCHITECTURE.md`

### Related design references
- `docs/plans/2026-04-08-phase-7-render-lifecycle-design.md`
- `docs/plans/2026-04-08-library-usability-design.md`

</canonical_refs>

<code_context>
## Existing Code Insights

### Current `RenderSession`
- 已有显式 lifecycle state，但仍同时持有：
  - backend resolution / activation
  - handle waiting / size waiting
  - bitmap creation and software copy
  - render loop driver ownership
  - request-render callback
  - diagnostics truth storage
- 这意味着“可 headless 测试的 orchestration”和“Avalonia-specific presentation adapter”还没有被分开。

### Current `VideraView`
- 当前仍直接参与：
  - `_renderSession.Attach(...)`
  - `_renderSession.BindHandle(...)`
  - `_renderSession.Resize(...)`
  - diagnostics snapshot 拼装与 refresh
  - retry / reattach 路径调度
- 这与 Phase 5 “VideraView should remain a UI shell” 的目标已有部分漂移。

### Current strengths to preserve
- `RenderSessionIntegrationTests` 已覆盖 rebind、disabled env override、dispose no-op、pipeline snapshot capture。
- `VideraViewSceneIntegrationTests` 已覆盖 diagnostics truth、scene helper API 和 backend summary。
- Phase 9 已让 pipeline truth 有稳定 vocabulary，可作为解耦后的 session snapshot 内容继续传递。

### Maintainability pressure
- 现在最难维护的不是单个 backend，而是 host/session/view 多层之间的重复同步与散落责任。
- 如果不先拆 clean orchestration seam，Phase 11 很容易把当前内部实现细节固化成 public API。

</code_context>

<specifics>
## Specific Ideas

- 推荐把 Phase 10 拆成三波：
  1. 提炼 `RenderSessionOrchestrator`、`RenderSessionInputs`、`RenderSessionSnapshot`
  2. 提炼 `VideraViewSessionBridge`，让 `VideraView` 只保留 UI shell / native host / input 责任
  3. 用 integration tests、repository guards 和 architecture docs 锁定新边界
- `RenderSessionOrchestrator` 应避免直接依赖 `WriteableBitmap`、`DispatcherTimer`、`NativeControlHost`
- `VideraViewSessionBridge` 应成为 `VideraView` 中 attach / resize / handle / diagnostics 协调的唯一入口
- 计划中允许保留 `RenderSession` 这个现有 public/internal surface，但它应退化成 façade + adapter，而不是继续做 orchestration owner

</specifics>

<deferred>
## Deferred Ideas

- public custom render pass / frame hook API（Phase 11）
- backend / capability / diagnostics query public API（Phase 11）
- sample-driven developer onboarding and compatibility guards（Phase 12）
- platform deepening work inherited from v1.0 deferred list

</deferred>

---

*Phase: 10-host-agnostic-render-orchestration*
*Context gathered: 2026-04-08*
