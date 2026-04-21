---
phase: 02-质量与可靠性
plan: 01
type: execute
wave: 6
depends_on: ["01-07"]
files_modified:
  - src/Videra.Core/IO/ModelImporter.cs
  - src/Videra.Core/Graphics/Object3D.cs
  - src/Videra.Core/Exceptions/VideraException.cs
  - src/Videra.Core/Exceptions/GraphicsInitializationException.cs
  - src/Videra.Core/Exceptions/ResourceCreationException.cs
  - src/Videra.Core/Exceptions/PipelineCreationException.cs
  - src/Videra.Core/Exceptions/PlatformDependencyException.cs
  - src/Videra.Core/Exceptions/InvalidModelInputException.cs
  - src/Videra.Core/Exceptions/UnsupportedOperationException.cs
  - src/Videra.Platform.Windows/D3D11Backend.cs
  - src/Videra.Platform.Windows/D3D11ResourceFactory.cs
  - src/Videra.Platform.Windows/D3D11Buffer.cs
  - src/Videra.Platform.Windows/D3D11CommandExecutor.cs
  - src/Videra.Platform.Linux/VulkanBackend.cs
  - src/Videra.Platform.Linux/VulkanResourceFactory.cs
  - src/Videra.Platform.Linux/VulkanCommandExecutor.cs
  - src/Videra.Platform.macOS/MetalBackend.cs
  - src/Videra.Platform.macOS/MetalResourceFactory.cs
  - src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs
  - src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs
  - tests/Videra.Core.Tests/IO/ModelImporterTests.cs
  - tests/Videra.Core.Tests/Graphics/Object3DTests.cs
  - tests/Videra.Platform.Windows.Tests/Backend/D3D11BackendSmokeTests.cs
  - tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendSmokeTests.cs
  - tests/Videra.Platform.macOS.Tests/Backend/MetalBackendSmokeTests.cs
requirements: [ERROR-01, ERROR-02, ERROR-03, QUAL-02, QUAL-03, RES-01, RES-02, RES-03, SEC-01, SEC-02]
autonomous: true
---

<objective>
在不扩功能、不降标准的前提下，完成 Phase 2 的质量与可靠性目标：建立统一领域异常体系、补齐边界安全验证、清除 NotImplementedException 对外语义、并确保多步骤初始化失败回滚与 Dispose 幂等。
</objective>

<execution_context>
@F:/CodeProjects/DotnetCore/Videra/.planning/phases/02-质量与可靠性/02-CONTEXT.md
@F:/CodeProjects/DotnetCore/Videra/.planning/phases/02-质量与可靠性/02-RESEARCH.md
@F:/CodeProjects/DotnetCore/Videra/.planning/REQUIREMENTS.md
@F:/CodeProjects/DotnetCore/Videra/.planning/STATE.md
@F:/CodeProjects/DotnetCore/Videra/.planning/phases/01-基础设施与清理/01-VERIFICATION.md
</execution_context>

<guardrails>
- 不新增平台能力，不修改 Phase 1 TEST-03 的严格完成定义。
- 仅在系统边界做验证（文件路径、native handle、P/Invoke/native 入口、资源生命周期入口）。
- 不以字符串拼接充当结构化诊断；异常类型与字段必须可断言。
- Windows 为本地主验证平台；Linux/macOS 保持代码正确与可构建，不伪造“已在宿主验证完成”。
</guardrails>

<tasks>

<task id="02-01" type="auto" tdd="true">
  <name>建立 Phase 2 领域异常体系并替换泛化异常入口</name>
  <files>
    src/Videra.Core/Exceptions/*.cs
    src/Videra.Platform.Windows/D3D11Backend.cs
    src/Videra.Platform.Windows/D3D11ResourceFactory.cs
    src/Videra.Platform.Windows/D3D11Buffer.cs
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.Linux/VulkanResourceFactory.cs
    src/Videra.Platform.macOS/MetalBackend.cs
    src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs
    src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs
  </files>
  <action>
    1. 在 `src/Videra.Core/Exceptions/` 创建统一异常层：`VideraException` + 初始化/资源/管线/平台依赖/非法输入/不支持操作等子类。
    2. 用领域异常替换关键路径 `throw new Exception(...)`：至少覆盖 backend initialize 与 resource creation 失败点。
    3. 为平台相关异常注入结构化字段（如 `ErrorCode`, `Platform`, `Operation`），同时保留可读 message。
    4. 保证 Demo/UI 可直接消费 message，不要求解析底层原始字符串。
  </action>
  <acceptance_criteria>
    - 核心失败路径不再直接抛 `Exception` 作为对外语义
    - Windows HRESULT / Vulkan 错误码/Metal失败上下文可从异常对象稳定获取
    - 对外 message 同时具备可诊断性与可展示性
  </acceptance_criteria>
</task>

<task id="02-02" type="auto" tdd="true">
  <name>收敛边界安全验证（路径、句柄、库入口）</name>
  <files>
    src/Videra.Core/IO/ModelImporter.cs
    src/Videra.Platform.Windows/D3D11Backend.cs
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.macOS/MetalBackend.cs
    src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs
    src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs
    tests/Videra.Core.Tests/IO/ModelImporterTests.cs
  </files>
  <action>
    1. 在 `ModelImporter.Load` 增加路径边界校验：空路径、非法扩展名、文件不存在、目录误传、规范化路径检查。
    2. 在 backend/native-host 生命周期入口统一 `IntPtr` 非法句柄失败策略，替换为平台语义异常。
    3. 对缺依赖/缺库句柄/缺函数入口统一显式报错，禁止静默失败。
  </action>
  <acceptance_criteria>
    - `SEC-01` 场景均有确定性失败行为与异常类型
    - `SEC-02` 缺依赖/句柄无效路径给出明确平台语义错误
    - 边界验证只出现在系统边界，不污染纯内部调用链
  </acceptance_criteria>
</task>

<task id="02-03" type="auto" tdd="true">
  <name>移除 NotImplementedException 对外语义并统一不支持能力处理</name>
  <files>
    src/Videra.Platform.Windows/D3D11ResourceFactory.cs
    src/Videra.Platform.Windows/D3D11CommandExecutor.cs
    src/Videra.Platform.Linux/VulkanResourceFactory.cs
    src/Videra.Platform.Linux/VulkanCommandExecutor.cs
    src/Videra.Platform.macOS/MetalResourceFactory.cs
  </files>
  <action>
    1. 将现存 `NotImplementedException` 改为明确领域异常（优先 `UnsupportedOperationException`）。
    2. message 必须包含平台、方法、当前不支持原因与调用建议（若有）。
    3. 如某方法可在本阶段低成本补齐且影响 ERROR/RES 关键路径，可实现最小能力；否则禁止继续保留 `NotImplementedException`。
  </action>
  <acceptance_criteria>
    - `src/**/*.cs` 中对外可达路径不再包含 `throw new NotImplementedException(`
    - 调用方可基于异常类型区分“未支持”与“运行失败”
  </acceptance_criteria>
</task>

<task id="02-04" type="auto" tdd="true">
  <name>加固多步骤初始化回滚与 Dispose 幂等</name>
  <files>
    src/Videra.Platform.Windows/D3D11Backend.cs
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.macOS/MetalBackend.cs
    src/Videra.Core/Graphics/Object3D.cs
    tests/Videra.Platform.Windows.Tests/Backend/D3D11BackendSmokeTests.cs
    tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendSmokeTests.cs
    tests/Videra.Platform.macOS.Tests/Backend/MetalBackendSmokeTests.cs
    tests/Videra.Core.Tests/Graphics/Object3DTests.cs
  </files>
  <action>
    1. 对 backend initialize/resize/resource factory 相关多步骤路径应用失败即回滚（try/finally 或等价 cleanup）。
    2. 统一 Dispose 语义：可重复调用、可在部分初始化状态调用且不泄漏。
    3. 为失败路径补测试：验证回滚后对象状态一致、重复 Dispose 安全。
  </action>
  <acceptance_criteria>
    - `RES-01`：半失败场景无悬挂资源（按对象状态与测试可观测项验证）
    - `RES-02`：不在热路径引入 Console I/O 或重字符串格式化
    - `RES-03`：保持线框颜色更新路径改进需求的兼容执行空间（不引入回退）
  </acceptance_criteria>
</task>

<task id="02-05" type="auto">
  <name>执行 Phase 2 验证矩阵并更新阶段产物</name>
  <files>
    .planning/phases/02-质量与可靠性/02-SUMMARY.md
    .planning/phases/02-质量与可靠性/02-VERIFICATION.md
    .planning/STATE.md
  </files>
  <action>
    1. 运行构建与测试矩阵（Windows 本机 + 跨平台可构建性）。
    2. 以 requirement ID 回填证据：ERROR/QUAL/RES/SEC。
    3. 更新 `02-SUMMARY.md` 与 `02-VERIFICATION.md`，并同步 `STATE.md` 进度。
    4. 保持 Phase 1 严格 gap 的事实陈述，不因 Phase 2 推进而改写。
  </action>
  <acceptance_criteria>
    - 每个 requirement 有可追踪证据（文件/命令/行为）
    - 状态文件与验证文件一致，无“阶段漂移”
  </acceptance_criteria>
</task>

</tasks>

<verification>
- `dotnet build F:/CodeProjects/DotnetCore/Videra/Videra.slnx -c Debug`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Core.Tests/Videra.Core.Tests.csproj --no-restore`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj --no-restore`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj --no-restore`
- grep 校验：
  - `src/**/*.cs` 不再存在对外路径 `throw new NotImplementedException(`
  - 关键 backend 路径异常类型已迁移至领域异常
</verification>

<success_criteria>
1. ERROR-01/02/03：错误语义统一为领域异常，包含结构化诊断，且 UI/Demo 可读。
2. QUAL-02：NotImplementedException 从对外可达路径清零。
3. QUAL-03 + SEC-01/02：边界验证覆盖路径/句柄/库入口，失败显式、可诊断。
4. RES-01/02/03：多步骤失败回滚与 Dispose 幂等可测，且不引入热路径日志回退。
5. 规划与状态文件保持真实一致，不掩盖 Phase 1 严格 blocker。
</success_criteria>
