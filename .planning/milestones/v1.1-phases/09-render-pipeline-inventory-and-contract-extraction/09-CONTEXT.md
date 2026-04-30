# Phase 9: Render Pipeline Inventory and Contract Extraction - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.1 milestone initialization + direct planning path (`$gsd-plan-phase 9`)

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 盘点当前 end-to-end frame path，把它从“隐式 helper 调用顺序”提炼成显式、可描述的 render pipeline contract。
2. 在 `VideraEngine` 内部建立稳定的 stage / pass vocabulary，让 frame orchestration 不再完全依赖单个 partial file 的隐式流程。
3. 让这套 contract 能被 tests、diagnostics 和文档消费，为后续的 host-agnostic orchestration 与 public extensibility API 打基础。

本阶段**不**包含：
- 完整的 host-agnostic render orchestration 提取（Phase 10）
- 对外开放完整 custom render pass / frame hook API（Phase 11）
- 新的大型渲染特性，如 PBR、阴影、后处理、shader graph
- compositor-native Wayland embedding
- higher-level macOS safer binding replacement
- 替换 Avalonia 或整体 UI 宿主栈

</domain>

<decisions>
## Implementation Decisions

### Phase scope
- **D-01:** Phase 9 先做 contract extraction，不直接做完整 plugin system。
- **D-02:** 优先保留 `v1.0` 已验证的对外行为和平台真相，不在本阶段引入新的平台能力或渲染特性。
- **D-03:** 若必须在“内部优雅”与“后续可扩展”之间取舍，优先建立对后续 `Phase 10/11` 友好的前向兼容 seam。

### Render pipeline contract
- **D-04:** 对外可讲清的一帧阶段顺序应采用稳定 vocabulary，而不是直接暴露当前私有 helper 名称。
- **D-05:** 推荐的显式 stage 名称为：
  - `PrepareFrame`
  - `BindSharedFrameState`
  - `GridPass`
  - `SolidGeometryPass`
  - `WireframePass`
  - `AxisPass`
  - `PresentFrame`
- **D-06:** `ResolveEffectiveWireframeMode`、`EnsureWireframeResources` 这类准备逻辑仍可存在，但不作为对外 stage vocabulary 的一部分。

### Boundary rules
- **D-07:** `VideraEngine` 应拥有单一 frame-plan / pipeline-plan 生成与执行路径，而不是继续把阶段顺序散落在 `RenderFrame()` 内的 ad hoc helper 调用里。
- **D-08:** `RenderSession` / `VideraView` 在本阶段可以消费 pipeline snapshot / summary 进行诊断，但不应在本阶段成为新的 pipeline owner。
- **D-09:** `IGraphicsBackend` / `ICommandExecutor` 的核心 contract 在本阶段尽量保持稳定；如需扩展，优先通过 engine-local contract types 先收口。

### Diagnostics and documentation
- **D-10:** 这套 pipeline contract 必须至少能被 integration tests、backend diagnostics、以及 architecture/module docs 同时引用。
- **D-11:** 任何新增诊断字段都应表达“当前 frame contract 是什么”，而不是直接暴露不稳定的内部实现对象。

### the agent's Discretion
- 可决定 stage contract 使用 enum、record、snapshot object、plan object 的具体命名和文件布局，只要 vocabulary 和执行边界稳定。
- 可决定 diagnostics 以 `string` / snapshot type / read-only list 何种方式暴露，只要测试、文档和实现使用同一套真相。

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

### Current render path ownership
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/VideraEngine.Resources.cs`
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`

### Current backend and diagnostics seams
- `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
- `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`
- `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`

### Existing tests and architecture assertions
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`
- `.planning/codebase/ARCHITECTURE.md`

### Related prior design work
- `docs/plans/2026-04-08-library-usability-design.md`
- `docs/plans/2026-04-08-library-usability-implementation.md`
- `docs/plans/2026-04-08-phase-7-render-lifecycle-design.md`
- `docs/plans/2026-04-08-phase-7-render-lifecycle-implementation.md`

</canonical_refs>

<code_context>
## Existing Code Insights

### Current frame path
- `VideraEngine.Draw()` 目前通过 `_lock` 串行化调用，然后在 `RenderFrame()` 内按顺序执行：
  - 计算 effective wireframe mode
  - ensure wireframe resources
  - begin frame
  - bind shared frame state
  - grid pass
  - solid object pass
  - overlay/wireframe/axis pass
  - backend end frame
- 这条顺序目前只存在于 `VideraEngine.Rendering.cs` 的 helper 调用顺序里，没有单独的 pipeline contract object。

### Current ownership split
- `RenderSession` 同时负责 backend resolution、native-handle waiting、bitmap copy、render loop 和 frame tick。
- `VideraView` 同时负责 property-to-engine sync、native host lifecycle、backend diagnostics 拼装和 render session attach/resize 调度。
- `VideraEngine` 既持有 scene/camera/style state，也直接拥有一帧的 orchestration 细节。

### Current maintainability pressure
- `VideraEngine` 已通过 partial files 分出 `Rendering` / `Resources`，但“frame stage sequencing”仍然是隐式流程，不是显式 contract。
- `VideraBackendDiagnostics` 目前描述的是 backend/fallback/host truth，还没有表达 pipeline truth。
- `GraphicsBackendFactory`、`RenderSession`、`VideraView` 已经出现了对 typed diagnostics 和 higher-level usability 的基础设施，适合作为 pipeline snapshot 的消费端。

### Useful precedent
- Phase 7 已经证明：先把 lifecycle / wireframe contract 显式化，再继续向上开放 API，比直接暴露内部细节更稳。
- 2026-04-08 的 library-usability 设计已经明确指出：当前最大 friction 之一是 host app 需要依赖低层对象和隐式行为；这和本阶段目标一致，但本阶段先做 contract extraction，不直接做完整 facade。

</code_context>

<specifics>
## Specific Ideas

- 推荐把 Phase 9 拆成三波：
  1. 建立显式 pipeline vocabulary 和 engine-level frame plan/snapshot contract
  2. 让 session/view diagnostics 消费这套 snapshot
  3. 用 architecture/module docs 和 repository guards 固定对外真相
- 对外稳定 stage vocabulary 推荐固定为：
  - `PrepareFrame`
  - `BindSharedFrameState`
  - `GridPass`
  - `SolidGeometryPass`
  - `WireframePass`
  - `AxisPass`
  - `PresentFrame`
- 本阶段不建议直接在 `IGraphicsBackend` 上新增 plugin-style hooks；后续 public extensibility API 应建立在已经提炼出的 contract 之上。

</specifics>

<deferred>
## Deferred Ideas

- host-agnostic orchestration extraction to its own composition root (Phase 10)
- public custom pass / frame hook API (Phase 11)
- sample-driven external API adoption and compatibility guards (Phase 12)
- platform deepening work inherited from v1.0 deferred list

</deferred>

---

*Phase: 09-render-pipeline-inventory-and-contract-extraction*
*Context gathered: 2026-04-08*
