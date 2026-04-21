# Phase 7: 渲染契约一致性与生命周期安全 - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** Approved design discussion after Phase 6 closure

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 把 `VideraEngine` 与 `RenderSession` 的生命周期收口成显式、可测试、可推理的状态模型。
2. 让软件后端真正实现深度状态 API，并让线框模式与深度语义一致。
3. 收掉 `RenderStylePreset.Wireframe` / `Material.WireframeMode` / `WireframeRenderer.Mode` 之间“API 暴露与真实渲染行为不一致”的问题。

本阶段**不**包含：
- Wayland 支持
- 新的平台分发或包模型调整
- broad render coordinator rewrite beyond the current `VideraEngine` / `RenderSession` boundary
- Demo 体验与用户反馈文案修正（Phase 8）
- macOS typed binding replacement

</domain>

<decisions>
## Implementation Decisions

### 生命周期优先级
- **D-01:** Phase 7 的第一优先级是生命周期安全，不是新的渲染能力。
- **D-02:** `Dispose` 之后的公开入口尽量采取 harmless `no-op`，不把调用者炸出来。
- **D-03:** 允许进行较大的结构化收口，只要目标是更清晰的 ownership 和 lifecycle contract，而不是 broad rewrite。

### 状态模型
- **D-04:** `VideraEngine` 应拥有显式生命周期状态，至少覆盖 `Uninitialized`, `Active`, `Suspended`, `Disposed`。
- **D-05:** `RenderSession` 应拥有显式 session 状态，至少覆盖 `Detached`, `WaitingForSize`, `WaitingForHandle`, `Ready`, `Faulted`, `Disposed`。
- **D-06:** render-loop tick、resize、handle rebind、suspend、dispose 必须通过统一状态转换路径，而不是依赖字段组合推断。

### 软件后端与线框契约
- **D-07:** `ICommandExecutor.SetDepthState` / `ResetDepthState` 在软件后端不能继续是 no-op；它们必须成为真实可测试语义。
- **D-08:** `Overlay`, `VisibleOnly`, `AllEdges`, `WireframeOnly` 四种线框模式的深度行为必须被明确实现，而不是靠注释或推断。

### 风格系统与真实渲染行为
- **D-09:** `RenderStylePreset.Wireframe` 不能继续只写 uniform 而不改变真实 rendering contract。
- **D-10:** 显式 `Wireframe.Mode` 保留为 pass-level override；如果显式模式是 `None` 且 style 参数要求 wireframe，则引擎必须推导出有效 wireframe 行为。

### the agent's Discretion
- 可自行决定生命周期 state enum 的命名与封装位置，只要状态转换和 no-op 契约清晰可测。
- 可在保持外部 API 尽量稳定的前提下调整内部 helper / guard / render-pass organization。

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and project constraints
- `.planning/ROADMAP.md` — Phase 7 的目标、依赖、需求与成功标准
- `.planning/STATE.md` — 当前项目阶段、已完成 Phase 6、以及下一阶段指向
- `.planning/PROJECT.md` — 项目边界、out-of-scope 与允许的破坏性重构
- `.planning/REQUIREMENTS.md` — `RES-01`, `RES-02`, `PERF-01`, `DEPTH-01` requirement 定义
- `AGENTS.md` — 仓库工作规则

### Current lifecycle and rendering ownership
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/VideraEngine.Resources.cs`
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Avalonia/Controls/VideraView.cs`

### Current software backend and wireframe contract
- `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`
- `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`
- `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs`
- `src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs`
- `src/Videra.Core/Graphics/Wireframe/WireframeMode.cs`

### Current style contract
- `src/Videra.Core/Styles/Services/RenderStyleService.cs`
- `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`
- `src/Videra.Core/Styles/Parameters/RenderStyleParameters.cs`
- `src/Videra.Core/Styles/Parameters/StyleUniformData.cs`
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- `src/Videra.Platform.macOS/Shaders.metal`

### Existing verification surface
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/WireframeRendererIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Styles/StyleEventIntegrationTests.cs`
- `tests/Videra.Core.Tests/Graphics/Software/SoftwareBackendTests.cs`
- `tests/Videra.Core.Tests/Graphics/Software/SoftwareRasterizerTests.cs`

</canonical_refs>

<code_context>
## Existing Code Insights

### Lifecycle risk hot spots
- `RenderSession` 目前通过 `_backend`, `_width`, `_height`, `HandleState`, `_disposed` 的组合来推断“能否初始化/能否绘制”，没有单一状态源。
- `RenderSession.RenderOnce` 与 `Dispose` / `Suspend` / `BindHandle` 可能交错操作同一套 backend/bitmap/engine 资源。
- `VideraEngine` 目前用 `_lock` 和 `_disposed` 控制访问，但 `Initialize` / `Suspend` / `Dispose` / `Draw` 没有显式状态机。

### Contract mismatches
- 软件后端 `SetDepthState` / `ResetDepthState` 仍是注释式 no-op。
- `WireframeRenderer` 假设 executor 的深度状态是真实可切换的。
- `RenderStylePreset.Wireframe` 会把 `Material.WireframeMode` 写入 uniform，但当前 solid/wireframe pass 仍只看 `WireframeRenderer.Mode`。
- Windows/macOS shader struct 定义了 `wireframeMode` 字段，但 shader 逻辑并未消费它。

### Reusable assets
- Phase 5 已把 render-session seam 抽出来，适合作为 Phase 7 的 lifecycle ownership 边界。
- 现有 integration tests 已覆盖基础 engine/session 行为，可以扩成 contract tests，而不必从零搭测试 harness。

</code_context>

<specifics>
## Specific Ideas

- 推荐把 Phase 7 拆成三波：
  1. lifecycle state machines and no-op contract
  2. software depth-state and wireframe semantics
  3. style-to-wireframe contract and Avalonia-facing consistency
- `RenderStylePreset.Wireframe` 的真实语义建议落成：
  - 显式 `Wireframe.Mode != None` 时显式模式优先
  - 否则 `Material.WireframeMode == true` 推导出 `WireframeOnly`

</specifics>

<deferred>
## Deferred Ideas

- Wayland support
- package/distribution changes
- multithreaded software renderer
- broad scene/renderer separation beyond the current phase scope
- demo UX feedback truthfulness

</deferred>

---

*Phase: 07-render-contract-consistency-and-lifecycle-safety*
*Context gathered: 2026-04-08*
