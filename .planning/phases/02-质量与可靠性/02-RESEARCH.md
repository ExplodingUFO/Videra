---
phase: 02-质量与可靠性
artifact: research
status: ready_for_planning
generated: 2026-03-29
source:
  - .planning/phases/02-质量与可靠性/02-CONTEXT.md
  - .planning/REQUIREMENTS.md
  - .planning/STATE.md
  - .planning/phases/01-基础设施与清理/01-VERIFICATION.md
---

# 02-RESEARCH: 质量与可靠性实现研究

## 1) 边界与前置结论

- Phase 2 只处理：`ERROR-*`, `QUAL-02`, `QUAL-03`, `RES-*`, `SEC-*`。
- 不扩展新平台能力，不下调 Phase 1 的 TEST-03 严格标准。
- 当前真实可执行主机仍是 Windows；Linux/macOS 改动需保持可构建、语义正确、并为后续宿主验证留入口。

## 2) 现状证据（代码级）

### 2.1 未实现方法（QUAL-02）

`NotImplementedException` 仍存在：

- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs:264`
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs:269`
- `src/Videra.Platform.Windows/D3D11CommandExecutor.cs:90`
- `src/Videra.Platform.Linux/VulkanCommandExecutor.cs:63`
- `src/Videra.Platform.Linux/VulkanResourceFactory.cs:319`
- `src/Videra.Platform.macOS/MetalResourceFactory.cs:309`
- `src/Videra.Platform.macOS/MetalResourceFactory.cs:314`
- `src/Videra.Platform.macOS/MetalResourceFactory.cs:319`

这些属于“接口表面承诺 > 实际能力”的直接风险点，应改为域语义明确异常或实现。

### 2.2 泛化异常与弱诊断（ERROR-01/02/03）

`throw new Exception(...)` 仍大量存在，核心集中在：

- Windows D3D11 初始化/资源创建路径：
  - `src/Videra.Platform.Windows/D3D11Backend.cs`（含 HRESULT）
  - `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
  - `src/Videra.Platform.Windows/D3D11Buffer.cs`
- Linux Vulkan 初始化/资源路径：
  - `src/Videra.Platform.Linux/VulkanBackend.cs`
  - `src/Videra.Platform.Linux/VulkanResourceFactory.cs`
- macOS Metal 初始化路径：
  - `src/Videra.Platform.macOS/MetalBackend.cs`
- 原生宿主前置条件：
  - `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`
  - `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`

问题并非“无错误信息”，而是缺少统一异常层次与结构化字段，调用方无法稳定识别错误类型。

### 2.3 安全边界（SEC-01/SEC-02, QUAL-03）

- `ModelImporter.Load` 当前未做路径边界验证：`src/Videra.Core/IO/ModelImporter.cs:15-30`。
- 平台原生句柄/库入口已有零散检查，但规则不统一，且异常类型不统一：
  - 例如 `VulkanBackend.Initialize(IntPtr windowHandle, ...)` 直接 generic exception。

### 2.4 回滚与清理（RES-01）

- `Object3D.Initialize` 已有 try/catch 清理，是可复用模式：`src/Videra.Core/Graphics/Object3D.cs:49-115`。
- 平台 backend 多步骤初始化仍有半失败风险（尤其 Vulkan / D3D11）。
- Dispose 幂等性在不同后端实现风格不一致，需要统一验证。

## 3) 实现方向（按决策约束收敛）

## 3.1 异常体系最小可执行设计

建议在 `src/Videra.Core` 新增统一领域异常基础层（命名可在实现时微调）：

- `VideraException`（基础）
- `GraphicsInitializationException`
- `ResourceCreationException`
- `PipelineCreationException`
- `PlatformDependencyException`
- `InvalidModelInputException`
- `UnsupportedOperationException`（用于替换不应继续 `NotImplementedException` 的路径）

结构化诊断字段（不依赖字符串解析）：

- `ErrorCode`（平台码，如 HRESULT/VkResult）
- `Platform`（Windows/Linux/macOS）
- `Operation`（Initialize/CreateBuffer/LoadModel 等）
- `Context`（可选键值对）

## 3.2 NotImplementedException 处理策略（QUAL-02）

- 对“短期不会实现但可被调用”的方法：改抛 `UnsupportedOperationException`（带平台+方法+建议）。
- 对“本阶段可补齐且影响 ERROR/RES/SEC 关键路径”的方法：优先实现最小能力。
- 不保留 `NotImplementedException` 作为对外语义。

## 3.3 安全边界策略（SEC-01/SEC-02, QUAL-03）

只在系统边界加验证：

1. `ModelImporter.Load`：
   - 空/空白路径
   - 非允许扩展名（`.gltf/.glb/.obj`）
   - 文件不存在
   - 目录路径误传
   - 路径规范化后再判断（防 traversal）
2. 平台 backend `Initialize` 入口：
   - `IntPtr.Zero` / 无效句柄直接失败，抛平台语义异常
3. 原生库与函数入口：
   - 明确“缺依赖/缺符号”异常，避免静默失败

## 3.4 回滚与清理策略（RES-01/02/03）

- 多步骤初始化统一“失败即回滚”：用 try/finally + 分阶段释放。
- `Dispose` 必须可重复调用、可部分初始化状态调用。
- 热路径（渲染循环）不新增 Console I/O 或昂贵字符串拼接。

## 4) 影响面与文件候选

核心优先文件：

- `src/Videra.Core/IO/ModelImporter.cs`（SEC-01、ERROR-03）
- `src/Videra.Platform.Windows/D3D11Backend.cs`（ERROR-01/02、RES-01）
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`（ERROR-01/02、QUAL-02）
- `src/Videra.Platform.Windows/D3D11Buffer.cs`（ERROR-02）
- `src/Videra.Platform.Linux/VulkanBackend.cs`（ERROR-01/02、RES-01、SEC-02）
- `src/Videra.Platform.Linux/VulkanResourceFactory.cs`（ERROR-01/02、QUAL-02）
- `src/Videra.Platform.Linux/VulkanCommandExecutor.cs`（QUAL-02）
- `src/Videra.Platform.macOS/MetalBackend.cs`（ERROR-01/02、SEC-02）
- `src/Videra.Platform.macOS/MetalResourceFactory.cs`（QUAL-02、ERROR-01）
- `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`（ERROR-03）
- `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`（ERROR-03）

以及新增（建议）：

- `src/Videra.Core/Exceptions/*.cs`（统一异常层）

## 5) 测试与验证策略

Windows 本机可执行验证：

- `dotnet build F:/CodeProjects/DotnetCore/Videra/Videra.slnx -c Debug`
- `dotnet test .../tests/Videra.Core.Tests/Videra.Core.Tests.csproj`
- `dotnet test .../tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj`
- `dotnet test .../tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj`

新增测试重点：

- `ModelImporter` 路径非法输入矩阵（空、目录、不存在、扩展名错误、traversal）
- 各 backend 初始化失败注入场景下的异常类型和结构化字段断言
- Dispose 幂等 + 半初始化清理断言
- NotImplementedException 清零断言（repo grep 级）

Linux/macOS：

- 本阶段以代码正确性与可构建为目标；真实宿主执行延续 Phase 1 严格 gap closure，不伪造完成。

## 6) 风险与缓解

- 风险：异常替换面广，易引入测试脆弱（依赖 message 全文）。
  - 缓解：测试断言“异常类型 + 结构化字段”，避免 message 全匹配。
- 风险：平台后端回滚改动可能影响现有生命周期。
  - 缓解：先补失败路径测试，再实施回滚。
- 风险：Phase 1 状态被误更新为完成。
  - 缓解：Phase 2 文档明确依赖但不覆盖 Phase 1 blocker 事实。

## 7) 结论

Phase 2 的最优执行顺序应是：

1. 先建立异常骨架与统一抛出策略；
2. 并行收敛 `ModelImporter` 和平台 backend 边界验证；
3. 对多步骤初始化路径做失败回滚与 Dispose 幂等加固；
4. 用测试锁住 ERROR/SEC/RES/QUAL 目标，并保持 Phase 1 严格 gap 状态不被稀释。
