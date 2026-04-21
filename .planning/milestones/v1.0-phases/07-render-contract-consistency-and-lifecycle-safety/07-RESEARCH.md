# Phase 7: 渲染契约一致性与生命周期安全 - Research

**Researched:** 2026-04-08
**Domain:** render lifecycle ownership / software depth-state / wireframe contract
**Confidence:** HIGH

<user_constraints>
## User Constraints (from discussion and CONTEXT.md)

### Locked Decisions
- 生命周期安全优先于新的渲染能力。
- `Dispose` 之后优先 harmless `no-op` 语义。
- 允许做一次较大的结构化收口，而不只是打 guard patch。
- 第二优先级是软件后端、线框模式、风格系统之间的契约一致性。

### Deferred Ideas (OUT OF SCOPE)
- Wayland
- package/distribution follow-up
- demo UX cleanup
- typed Metal binding replacement
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RES-01 | 资源清理验证 | 当前风险集中在 `RenderSession` / `VideraEngine` 交错释放和部分 teardown 后的重入。 |
| RES-02 | 性能优化 | 当前 Phase 7 不以微优化为主，但显式状态机会减少热路径中的隐式异常路径与无意义工作。 |
| PERF-01 | 性能优化/契约收敛 | 当前线框与风格契约不一致导致额外逻辑分叉，适合在 Phase 7 收口。 |
| DEPTH-01 | 深度缓冲一致性 | 软件后端仍未实现真实深度状态切换，这是当前最明确的 contract gap。 |
</phase_requirements>

## Summary

Phase 7 的根问题不是“代码缺少更多 if 保护”，而是生命周期和渲染契约都缺单一事实来源。`RenderSession` 既持有 render-loop，又驱动 backend 初始化、bitmap 生命周期、handle 绑定和 resize；`VideraEngine` 则持有 scene/resource orchestration，但对 `Disposed` / `Suspended` / `Active` 没有显式语义。只要这两层继续通过字段组合推断状态，就会一直存在“看起来没问题、交错时才出事”的风险。

渲染契约侧的缺口同样明确：`WireframeRenderer` 调用 `SetDepthState` / `ResetDepthState`，但 `SoftwareCommandExecutor` 仍把它们实现成 no-op；`RenderStylePreset.Wireframe` 会把 `Material.WireframeMode` 写进 uniform，但 shader 并不消费这个字段，engine 也不根据它决定 solid/wireframe pass。结果就是 API 暴露了两套 wireframe 语义，但真实 rendering path 只实现了其中一套。

最佳方向不是继续 patch 这些分叉，而是：
1. 先为 `VideraEngine` 与 `RenderSession` 引入显式生命周期状态机。
2. 再让软件后端深度状态 API 变成真实语义。
3. 最后定义一个明确的 effective-wireframe contract，把 style-driven wireframe intent 和显式 wireframe override 收到同一套规则里。

## Option Analysis

### Option A: Minimal lock/guard patching
**Pros**
- 改动面小
- 易于快速落地

**Cons**
- 生命周期仍然隐式
- 后续问题仍会围绕“某个字段在某个时刻有没有被置空”反复出现

**Verdict**
- 不推荐。它更像延期，而不是 Phase 7 的 closure。

### Option B: Explicit dual-state lifecycle model
**Pros**
- 与用户批准的“较大结构收口”一致
- 便于用测试覆盖状态迁移与 no-op 语义
- 为后续软件深度状态和风格契约修正提供稳定基座

**Cons**
- 初始改动更大
- 需要先补一批更具体的 integration tests

**Verdict**
- 推荐。

### Option C: Full render orchestration rewrite
**Pros**
- 长期架构可能更干净

**Cons**
- 超出 Phase 7 必要范围
- 容易把 Phase 8 和更长远的架构目标重新混进来

**Verdict**
- 不推荐作为当前 phase 的默认路径。

## Recommended Technical Direction

### 1. `VideraEngine` lifecycle state
- 引入 internal state enum：`Uninitialized`, `Active`, `Suspended`, `Disposed`
- `Dispose` 后所有公开入口走 `no-op`
- `Suspend` 不销毁场景集合，只释放 graphics resources
- `Initialize` against `Disposed` 不复活实例

### 2. `RenderSession` lifecycle state
- 引入 internal state enum：`Detached`, `WaitingForSize`, `WaitingForHandle`, `Ready`, `Faulted`, `Disposed`
- `TryInitialize` 不再直接由字段组合决定，而由状态和前置条件决定
- render-loop tick 读取快照后再执行；若 session 在执行期间被 dispose/suspend，本帧安全放弃

### 3. Software depth-state implementation
- 在 `SoftwareCommandExecutor` 中保存当前 depth-test / depth-write state
- 所有 draw path 统一通过该状态执行 per-pixel 决策
- `ResetDepthState` 恢复默认 solid-pass 语义，而不是 no-op

### 4. Effective wireframe contract
- `WireframeRenderer.Mode` 保留为显式 pass-level override
- 若显式模式为 `None` 且 `StyleService.CurrentParameters.Material.WireframeMode == true`，则有效模式为 `WireframeOnly`
- 这样 `RenderStylePreset.Wireframe` 终于影响真实渲染行为，而不是只写 uniform

## Planning Implications

### Wave 1: lifecycle state machine and no-op contract
- 关键风险：render-loop tick / suspend / dispose / handle rebind 的交错
- 关键验证：integration tests，而不是 grep

### Wave 2: software depth-state and wireframe semantics
- 关键风险：line/triangle draw path 对深度读写规则不一致
- 关键验证：软件帧缓冲或像素级差异

### Wave 3: style-to-wireframe consistency
- 关键风险：preset、style uniform、wireframe override 继续形成双重语义
- 关键验证：preset + explicit override 的 precedence tests

## Risks and Tradeoffs

### Risk 1: Larger refactor breaks attach/resize order
**Mitigation:** 先补 `RenderSession` contract tests，再改状态迁移。

### Risk 2: `no-op after dispose` hides bugs
**Mitigation:** 测试需要验证“不会复活、不再绘制、不再创建 backend”，而不只是“不抛异常”。

### Risk 3: software depth-state fixes reveal existing test shallowness
**Mitigation:** 把 `WireframeRendererIntegrationTests` 从 `NotThrow` 升级到 framebuffer-level assertions。

## Environment Availability

| Dependency | Required By | Available | Notes |
|------------|------------|-----------|-------|
| Local .NET 8 build/test | Phase 7 planning and implementation | ✓ | sufficient for lifecycle and software-renderer work |
| Native host CI | Regression guard after implementation | ✓ | already enforced on GitHub Actions |
| Linux/macOS real host debugging | Native-path regressions if introduced | partial | still available through existing workflow/scripts |

## Sources

### Primary (HIGH confidence)
- `.planning/phases/07-render-contract-consistency-and-lifecycle-safety/07-CONTEXT.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `src/Videra.Core/Graphics/VideraEngine.cs`
- `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- `src/Videra.Core/Graphics/VideraEngine.Resources.cs`
- `src/Videra.Avalonia/Rendering/RenderSession.cs`
- `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs`
- `src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs`
- `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`
- `src/Videra.Core/Styles/Parameters/StyleUniformData.cs`
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- `src/Videra.Platform.macOS/Shaders.metal`

### Secondary (MEDIUM confidence)
- `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/WireframeRendererIntegrationTests.cs`
- `tests/Videra.Core.IntegrationTests/Styles/StyleEventIntegrationTests.cs`
- `tests/Videra.Core.Tests/Graphics/Software/SoftwareBackendTests.cs`
- `tests/Videra.Core.Tests/Graphics/Software/SoftwareRasterizerTests.cs`

## Metadata

**Confidence breakdown:**
- lifecycle refactor direction: HIGH
- software depth-state gap: HIGH
- style/wireframe contract resolution: MEDIUM-HIGH

**Research date:** 2026-04-08
**Valid until:** 2026-05-08
