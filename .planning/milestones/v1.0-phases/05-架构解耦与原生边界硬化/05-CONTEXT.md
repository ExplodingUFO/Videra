# Phase 5: 架构解耦与原生边界硬化 - Context

**Gathered:** 2026-04-02
**Status:** Ready for planning
**Source:** Auto-captured from architecture/performance/safety/Rust feasibility review

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 把渲染组合层、backend 解析、native host 生命周期的职责边界收紧。
2. 对 macOS / Linux 的高风险 native 路径做第一轮 C# 硬化，优先解决未检查返回值、句柄/对象所有权、回滚与重绑问题。
3. 形成书面的 Rust 决策与准入边界：默认不引入；只有在粗粒度、证据驱动的小边界上才允许继续评估。

本阶段**不**包含：
- 全仓库改写到 Rust
- 全量重写统一图形抽象
- 立即拆分 `Videra.Core` 为多个 NuGet/package
- 在无性能证据的前提下引入 Rust 软件栅格器

</domain>

<decisions>
## Implementation Decisions

### 组合层与后端解析
- **D-01:** `Videra.Core` 不应继续直接承担平台后端发现与反射装配职责；后端解析应迁移到组合层，并保留兼容适配器。
- **D-02:** Phase 5 优先引入显式 resolver/registry seam，避免一步到位重写现有 `GraphicsBackendFactory`。

### 渲染会话与 UI 边界
- **D-03:** `VideraView` 当前职责过重，Phase 5 要把 backend/session/timer/native-handle 生命周期从控件中抽离到单独的 render session/controller。
- **D-04:** native handle 的 destroy/recreate 必须被视为 backend rebind 事件，而不是简单 resize。

### 图形抽象与接口收口
- **D-05:** 先用 typed constants / enums 取代当前 binding slot 与 primitive magic numbers，再考虑更大的接口拆分。
- **D-06:** 不在本阶段做“全接口重设计”；先把 optional capability 与 always-supported contract 的边界识别清楚。

### Native Host 与平台边界
- **D-07:** `VideraView` 不应继续直接 new 各平台 native host；需要新增 `INativeHostFactory` 或等价 seam。
- **D-08:** Linux 侧继续沿用 `ISurfaceCreator` 作为可复用模式，并将其推广为更一般的 host/surface 组合边界。

### Native 安全硬化优先级
- **D-09:** macOS ObjC/Metal 路径是当前最高优先级 native 风险区，优先检查 zero-handle、`contents` 空指针、防止 `CAMetalLayer` 泄漏、收敛 `objc_msgSend` 变体。
- **D-10:** Linux Vulkan 路径第二优先，优先补齐 `MapMemory`、`BindBufferMemory`、提交/呈现相关 `Result` 检查，并收紧 X11 display/window 所有权模型。
- **D-11:** Windows D3D11 路径不作为本阶段重构中心，只做低风险修补（如 HRESULT/rollback/resize error handling）。

### 性能推进顺序
- **D-12:** 当前主要性能问题先按架构/API 使用方式处理，不以 Rust 为第一手优化手段。
- **D-13:** 优先排查 software backend 的 UI 线程整帧 copy、macOS `waitUntilCompleted`、每对象 uniform upload/rebind、线框颜色完整重写上传。
- **D-14:** 无 benchmark / profiler 证据前，不做“Rust 会更快”的推进决策。

### Rust 决策
- **D-15:** 本阶段默认结论是 **No Rust by default**。
- **D-16:** 若未来性能或安全证据要求继续评估，唯一可接受的第一批候选边界是：
  - `ModelImporter` / mesh preprocessing（首选）
  - 在确认 software fallback 为真实瓶颈后的软件栅格核心（次选）
  - 出于安全隔离目的的 macOS native boundary（仅在 C# 第一轮硬化后仍不满意时）
- **D-17:** 不接受把 Rust 用于跨平台 engine core、Avalonia host glue、或全平台 native interop 总重写。

### the agent's Discretion
- decoupling 方案的命名可以灵活，例如 `IGraphicsBackendResolver` / `RenderSession` / `ViewRendererController`，只要职责边界满足 D-01 ~ D-08。
- 研究/计划阶段可以自行决定是否把 Phase 5 切成“解耦波次”和“native hardening 波次”，前提是不弱化 D-09 ~ D-16。

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and project constraints
- `.planning/ROADMAP.md` — Phase 5 的目标、依赖、需求与成功标准
- `.planning/PROJECT.md` — 项目核心价值、约束、范围与 out-of-scope
- `.planning/REQUIREMENTS.md` — PERF / SEC / MACOS / LINUX / CLEAN 相关 requirement 定义
- `.planning/STATE.md` — 当前 TEST-03 gap、平台验证状态与历史决策
- `AGENTS.md` — 仓库级工作约束与工具/编辑规则

### Current composition and UI orchestration
- `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` — 当前 backend 发现/装配路径
- `src/Videra.Avalonia/Controls/VideraView.cs` — 当前 render loop、session ownership、native handle lifecycle
- `src/Videra.Avalonia/Controls/IVideraNativeHost.cs` — native host 当前最小契约
- `src/Videra.Avalonia/Controls/VideraNativeHost.cs` — Windows host glue
- `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs` — Linux host glue
- `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs` — macOS host glue

### Graphics contract and scene model
- `src/Videra.Core/Graphics/VideraEngine.cs` — 当前 engine orchestration 与 binding conventions
- `src/Videra.Core/Graphics/Object3D.cs` — CPU scene state 与 GPU resource ownership 融合点
- `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs` — 当前资源创建抽象
- `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs` — 当前命令执行抽象
- `src/Videra.Core/IO/ModelImporter.cs` — 当前 import 与 GPU upload 耦合点

### Native risk areas
- `src/Videra.Platform.macOS/ObjCRuntime.cs` — ObjC runtime interop surface
- `src/Videra.Platform.macOS/MetalBackend.cs` — Metal device/layer/depth state lifecycle
- `src/Videra.Platform.macOS/MetalResourceFactory.cs` — Metal buffer/library/pipeline construction
- `src/Videra.Platform.macOS/MetalBuffer.cs` — Metal buffer contents access
- `src/Videra.Platform.Linux/VulkanBackend.cs` — Vulkan lifecycle and rollback path
- `src/Videra.Platform.Linux/VulkanResourceFactory.cs` — Vulkan resource creation and bind path
- `src/Videra.Platform.Linux/VulkanBuffer.cs` — Vulkan memory map/write path
- `src/Videra.Platform.Linux/ISurfaceCreator.cs` — 现有 Linux seam 的参考模式
- `src/Videra.Platform.Linux/X11SurfaceCreator.cs` — X11 surface ownership and creation

### Performance-sensitive paths
- `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs` — 软件栅格热点
- `src/Videra.Core/Graphics/Software/SoftwareBackend.cs` — full-frame copy boundary
- `src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs` — 线框全量颜色重写路径

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ISurfaceCreator` in `src/Videra.Platform.Linux/` 已经是一个小而清晰的 seam，可作为 host/surface factory 设计参考。
- `ObjCRuntime` in `src/Videra.Platform.macOS/` 已经开始集中化 ObjC interop，可继续收口为 owned-handle / typed helper 模式。
- `tests/Tests.Common/Platform/NativeHostTestHelpers.cs` 可用于 future lifecycle/rebind/hardening tests。

### Established Patterns
- 平台差异目前主要通过 `Videra.Platform.*` 项目承载；这说明“按平台隔离 native 风险”本身是合理的，不应把风险重新拉回 Core。
- `VideraView` 当前是组合根，但它承担的职责已经超出控件边界。
- `VideraEngine` 当前通过固定 slot/index 约定与后端交互，说明 contract tightening 应先从 typed constants 入手。

### Integration Points
- backend resolver seam 将落在 `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` 与 `src/Videra.Avalonia/Controls/VideraView.cs` 之间。
- render session seam 将落在 `src/Videra.Avalonia/Controls/VideraView.cs` 与 `src/Videra.Core/Graphics/VideraEngine.cs` 之间。
- native host factory seam 将落在 `src/Videra.Avalonia/Controls/`。
- native hardening 主要落在 `src/Videra.Platform.macOS/` 与 `src/Videra.Platform.Linux/`。

</code_context>

<specifics>
## Specific Ideas

- 当前推荐推进顺序是“先解耦，再硬化，再决定 Rust”，不是“先选 Rust 再倒推改造”。
- 若最终要做 Rust spike，首选应是 `ModelImporter` / mesh preprocessing 的粗粒度 `cdylib` 边界，而不是 engine core。
- 如果未来要用 Rust 做安全隔离，macOS ObjC/Metal 比 Windows host glue 或 engine core 更像合理入口。

</specifics>

<deferred>
## Deferred Ideas

- 把 `Videra.Core` 直接拆成多个 packages / assemblies
- 全量重写统一 graphics abstraction
- 广泛 Rust 化 native host / Vulkan-X11 / Avalonia glue
- 在无 profiling 证据前引入 Rust software rasterizer

</deferred>

---

*Phase: 05-架构解耦与原生边界硬化*
*Context gathered: 2026-04-02*
