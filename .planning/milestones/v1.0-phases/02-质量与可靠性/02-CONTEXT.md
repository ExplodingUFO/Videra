# Phase 2: 质量与可靠性 - Context

**Gathered:** 2026-03-28
**Status:** Ready for planning

<domain>
## Phase Boundary

建立结构化错误处理和安全加固，确保资源管理正确。该阶段只处理质量与可靠性：定义领域异常体系、为平台后端与核心组件补上结构化错误信息、为不安全路径补边界保护、收紧文件路径与库句柄验证，并把资源创建失败时的回滚与清理行为做实。

本阶段不扩展新功能，不改变 Phase 1 的严格平台验证标准，不引入新平台能力，也不做文档站点或 UI 设计类工作。

</domain>

<decisions>
## Implementation Decisions

### 异常体系
- **D-01:** 引入明确的领域异常体系，而不是继续使用通用 `Exception`/`NotImplementedException` 作为对外语义。
- **D-02:** 异常层次优先围绕图形生命周期组织，建议至少包含：图形初始化、资源创建、着色器/管线创建、平台依赖/宿主前置条件、无效输入。
- **D-03:** Windows HRESULT、Vulkan 返回码、Metal/Objective-C 失败上下文必须进入异常对象或异常消息中的结构化字段，而不是只保留字符串拼接。
- **D-04:** 对外错误消息要同时兼顾“开发者可诊断”和“Demo/UI 可展示”，不要只保留底层 API 报错原文。

### 安全与边界验证
- **D-05:** 只在系统边界做验证：文件路径、平台原生句柄、P/Invoke 入口、native resource 生命周期入口；不要给纯内部调用加过度防御。
- **D-06:** `ModelImporter.Load` 必须增加路径合法性检查，至少覆盖空路径、非法扩展名、文件不存在、目录路径误传。
- **D-07:** 对 `IntPtr` / native handle 的验证要形成统一规则：非法句柄尽早失败，并给出平台语义明确的错误。
- **D-08:** 继续允许平台差异，但不允许“静默失败”。对缺依赖、缺宿主、缺库句柄，一律显式报错。

### 资源回滚与清理
- **D-09:** 所有多步骤初始化路径都应具备“失败即回滚”的语义，优先使用 try/finally 或显式 cleanup helper，不接受部分成功后悬挂资源。
- **D-10:** Phase 2 要把“Dispose 可以安全重复/部分初始化调用”作为默认要求，尤其是 Linux/macOS/Windows 原生后端。
- **D-11:** 回滚逻辑要优先覆盖 backend initialize / resize / resource factory 创建这些最可能半失败的路径。

### 计划执行策略
- **D-12:** Windows 仍然是当前唯一真实可验证宿主，因此 Phase 2 的本地验证以 Windows 为主；Linux/macOS 相关改动要保持代码正确、可构建，并为未来宿主验证留好明确入口。
- **D-13:** 优先修核心与平台层的质量问题，不为暂时无法在本机完全验证的 Linux/macOS 场景降低标准。

### Claude's Discretion
- 异常类命名的具体细粒度层次
- 结构化异常字段是属性还是 message 约定
- 安全验证代码放在现有类中还是抽成少量专用 helper
- 哪些 NotImplementedException 应改为显式平台不支持异常，哪些应真正实现

</decisions>

<specifics>
## Specific Ideas

- 错误处理要优先让“库使用者 / Demo 使用者”能看懂发生了什么，而不只是开发时能猜出来。
- 资源失败回滚优先级高于“优雅抽象”；先确保行为正确，再考虑后续抽象。
- Linux/macOS 在当前环境无法完全运行，不意味着它们可以继续保留模糊错误或脆弱清理逻辑。

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 项目约束与阶段定义
- `.planning/PROJECT.md` — 项目级目标、约束、优先级
- `.planning/REQUIREMENTS.md` — Phase 2 requirement IDs: ERROR-01, ERROR-02, ERROR-03, QUAL-02, QUAL-03, RES-01, RES-02, RES-03, SEC-01, SEC-02
- `.planning/ROADMAP.md` — Phase 2 goal 与 success criteria
- `.planning/STATE.md` — 当前真实进度与 Phase 1 残余 gap 背景

### 已有验证与问题来源
- `.planning/phases/01-基础设施与清理/01-VERIFICATION.md` — 当前严格验证结论，说明不要为了流程推进而降低标准
- `.planning/codebase/CONCERNS.md` — 当前已识别的异常处理、安全、回滚、P/Invoke、路径验证、资源管理问题
- `.planning/codebase/ARCHITECTURE.md` — 生命周期、边界、线程模型
- `.planning/codebase/TESTING.md` — 当前测试基础设施现状，可作为 Phase 2 验证手段基础

### 重点代码区域
- `src/Videra.Platform.Windows/` — HRESULT 与原生资源生命周期问题
- `src/Videra.Platform.Linux/` — Vulkan 初始化、X11、Dispose/cleanup 风险点
- `src/Videra.Platform.macOS/` — Metal/objc runtime 路径与资源清理
- `src/Videra.Core/IO/ModelImporter.cs` — 路径验证与导入边界
- `src/Videra.Core/Graphics/` — 核心资源与对象生命周期

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tests/Tests.Common/` 已建立通用测试基础，可继续承载 Phase 2 的测试夹具
- `tests/Videra.Core.Tests/` 已有核心单元测试基础，适合补异常/边界/回滚测试
- `tests/Videra.Platform.*.Tests/` 已存在平台测试项目，可用于补平台错误路径与 cleanup 测试

### Established Patterns
- Phase 1 已建立按项目分测试工程的结构，Phase 2 应复用，不再重建新测试组织方式
- Windows 已有真实 HWND fixture；Linux/macOS 仍以代码正确性和 future-host readiness 为目标
- Demo 现在会暴露状态消息，因此 Phase 2 的错误语义会直接影响 Demo 用户体验

### Integration Points
- 平台后端异常会穿过 `GraphicsBackendFactory` / `VideraView` / Demo 初始化链
- `ModelImporter` 的路径验证会直接影响 Demo 导入流程
- backend dispose / cleanup 正确性会直接影响后续原生宿主验证稳定性

</code_context>

<deferred>
## Deferred Ideas

- Linux/macOS 真实 native-host lifecycle/render-path 验证执行本身仍属于 Phase 1 严格 gap closure，不并入 Phase 2 范围
- 文档站点、贡献指南、用户文档归属于 Phase 4
- Wayland 支持、深度缓冲一致性、Metal 类型安全绑定仍属于 Phase 3

</deferred>

---
*Phase: 02-质量与可靠性*
*Context gathered: 2026-03-28*
