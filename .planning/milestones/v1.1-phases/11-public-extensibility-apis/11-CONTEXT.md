# Phase 11: Public Extensibility APIs - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.1 milestone continuation + direct planning path (`$gsd-plan-phase 11`)

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 在 Phase 9 的 pipeline vocabulary 和 Phase 10 的 orchestration seam 之上，向库使用者开放稳定的 public extensibility contract。
2. 让开发者可以在不修改 `VideraEngine` 源码的前提下，注册或替换 render pass contributor，并在稳定的 frame lifecycle point 插入自定义逻辑。
3. 把 pipeline / backend / capability / diagnostics truth 组织成可查询的 public API，而不是继续依赖 internal snapshot 或反射访问。

本阶段**不**包含：
- sample-driven developer onboarding、reference sample、完整使用文档收口（Phase 12）
- package/plugin discovery model，或跨包自动发现扩展
- 新的大型渲染特性，如 PBR、阴影、后处理、shader graph
- compositor-native Wayland embedding
- higher-level macOS safer binding replacement
- 替换 Avalonia 或整体 UI 宿主栈
- 把 frame-plan / pipeline ownership 从 `VideraEngine` 移走

</domain>

<decisions>
## Implementation Decisions

### Phase scope
- **D-01:** Phase 11 只开放 C#-first public extensibility API，不做 plugin marketplace / package discovery。
- **D-02:** 扩展能力必须建立在已经显式化的 pipeline contract 之上，不能重新回到隐式 helper 或 internal field 拼接。
- **D-03:** 若必须在“短期方便暴露内部对象”与“长期低耦合可维护”之间取舍，优先后者。

### Public extension model
- **D-04:** `VideraEngine` 继续作为 render pipeline owner；public contributors 和 hooks 由 engine 拥有和调度，而不是由 `VideraView`、`RenderSession` 或 `RenderSessionOrchestrator` 拥有。
- **D-05:** public custom-pass contract 推荐建立在新的 typed context 上，而不是直接把 `VideraEngine` private state 暴露给调用者。
- **D-06:** public hook point 不应直接复用 `RenderPipelineStage`；推荐单独定义稳定的 lifecycle vocabulary，例如 `FrameBegin`、`SceneSubmit`、`FrameEnd`，避免把 pass vocabulary 和 hook vocabulary 混在一起。
- **D-07:** contributor 的 register/replace 语义应绑定到稳定 slot 或 stage identity，而不是依赖反射、partial method 或“按当前 helper 名称覆写”。

### Public query model
- **D-08:** `EXT-03` 中的 capability 指的是库/runtime capability 与 active backend truth，不是完整 GPU feature probing 系统。
- **D-09:** pipeline truth 继续来源于 `RenderPipelineSnapshot`；backend/session truth 继续来源于 Avalonia/session 层；Phase 11 应把两者组织成 public 可查询入口，但不公开 `RenderSessionSnapshot` 本身。
- **D-10:** 已有 `VideraView.BackendDiagnostics` 可以继续作为查询入口之一，但若需要扩展 capability/query 字段，只能建立在 session/orchestrator truth 之上。

### Quality and compatibility
- **D-11:** Phase 7 的 harmless `no-op` / dispose safety 契约不得回退；新 API 不能靠“disposed 后抛异常”来实现存在感。
- **D-12:** Phase 10 的 boundary 仍然成立：`RenderSessionOrchestrator`、`RenderSessionSnapshot`、`VideraViewSessionBridge` 仍是内部 seam，不应在本阶段被提升为公共扩展根。
- **D-13:** Phase 11 允许最小必要的 architecture/readme/guard 更新来描述 shipped public API，但完整 sample/reference onboarding 留给 Phase 12。

### the agent's Discretion
- 可为 public extensibility contract 选择更合适的具体类型名，只要职责和边界等价，并在文档/guards 中同步。
- 可决定查询模型是新的 typed snapshot，还是扩展现有 public diagnostics surface，但不得让 host app 依赖 internal runtime objects。

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
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-CONTEXT.md`
- `.planning/phases/09-render-pipeline-inventory-and-contract-extraction/09-SUMMARY.md`
- `.planning/phases/10-host-agnostic-render-orchestration/10-CONTEXT.md`
- `.planning/phases/10-host-agnostic-render-orchestration/10-RESEARCH.md`
- `.planning/phases/10-host-agnostic-render-orchestration/10-SUMMARY.md`
- `.planning/phases/10-host-agnostic-render-orchestration/10-VERIFICATION.md`

### Current render pipeline and orchestration seams
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderFramePlan.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineStage.cs`
- `src/Videra.Core/Graphics/RenderPipeline/RenderPipelineSnapshot.cs`
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Rendering/RenderSessionSnapshot.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`
- `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`

### Existing tests and architecture assertions
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEnginePipelineContractTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionOrchestrationIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSessionBridgeIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- `ARCHITECTURE.md`
- `src/Videra.Core/README.md`
- `src/Videra.Avalonia/README.md`

### Related design references
- `docs/plans/2026-04-08-library-usability-design.md`
- `docs/plans/2026-04-08-phase-7-render-lifecycle-design.md`
- `docs/plans/2026-04-08-phase-7-render-lifecycle-implementation.md`

</canonical_refs>

<code_context>
## Existing Code Insights

### Current strengths
- `VideraEngine` 已经拥有显式的 `RenderFramePlan` / `RenderPipelineSnapshot` 以及稳定的 stage vocabulary。
- `RenderSessionOrchestrator` / `RenderSession` / `VideraViewSessionBridge` 已经把 host/session/view boundary 切开，便于在正确层级开放 public API。
- `VideraView.BackendDiagnostics` 已经是 public read-only truth，覆盖 backend、display server、pipeline summary 和 software presentation copy。

### Current gaps
- `VideraEngine.Rendering.cs` 的 pass 执行仍然是硬编码方法调用，没有 register/replace contributor contract。
- 当前没有稳定的 frame hook vocabulary；host app 若想在 frame begin/end 或 scene submit 介入，只能改内部实现。
- `RenderSessionSnapshot` 承载了很多运行时 truth，但它是 internal，host app 不应依赖它。
- `IGraphicsBackend` 目前没有 capability snapshot surface；仓库文档里的 capability truth 仍主要停留在 prose。
- repository guards 当前仍在防止 `RegisterPass(` / `FrameHook` / `IRenderPassContributor` 出现在 docs 中，这意味着 Phase 11 必须显式翻转 guard 方向。

### Maintainability pressure
- 如果直接把 internal runtime objects 暴露给用户，Phase 10 刚建立的 boundary 会被立刻破坏。
- 如果 contributor 和 hook 模型不基于 Phase 9 的显式 vocabulary，后续很容易再次和真实 pipeline 漂移。
- 如果 query API 继续分散在 `LastPipelineSnapshot`、`BackendDiagnostics`、internal snapshot 之间，外部使用体验会继续模糊。

</code_context>

<specifics>
## Specific Ideas

- 推荐把 Phase 11 拆成三波：
  1. 在 `Videra.Core` 建立 public contributor / hook / capability-query contract，并先由 `VideraEngine` 实现执行和注册语义
  2. 在 Avalonia 集成层补齐 host-app 可用的 query / projection surface，但不暴露 internal session objects
  3. 用 minimal docs、repo guards 和 regression matrix 固定 shipped public API names 与 boundary truth
- public extensibility contract 推荐只暴露：
  - 既有 public abstractions（如 `ICommandExecutor`, `IResourceFactory`）
  - typed frame/hook context
  - read-only pipeline/runtime snapshots
  - read-only scene/camera/style information
- 不推荐让 `VideraView` 成为 extensibility root；更合理的 ownership 是 `VideraEngine` 负责 contributors/hooks，`VideraView` 负责 diagnostics/runtime projection。

</specifics>

<deferred>
## Deferred Ideas

- sample/reference usage and developer onboarding (Phase 12)
- bilingual module docs that teach external consumers how to adopt the new API (Phase 12)
- compatibility guards for unsupported / disposed / unavailable semantics across all new public APIs (Phase 12)
- package/plugin discovery model
- platform deepening work inherited from v1.0 deferred list

</deferred>

---

*Phase: 11-public-extensibility-apis*
*Context gathered: 2026-04-08*
