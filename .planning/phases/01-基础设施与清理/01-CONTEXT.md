# Phase 1: 基础设施与清理 - Context

**Gathered:** 2026-03-28
**Status:** Ready for planning

<domain>
## Phase Boundary

建立可靠的基础设施 — 包括测试框架、结构化日志系统、静态分析工具和代码库清理。此阶段不添加新功能，专注于为后续开发建立坚实的质量保障基础。

**交付物：**
- 可运行的测试套件（单元测试 + 集成测试）
- 代码覆盖率报告（目标 80%+）
- 结构化日志系统替换所有 Console.WriteLine
- 静态代码分析配置
- 清理后的代码库（无调试代码、无临时文件）

</domain>

<decisions>
## Implementation Decisions

### 测试项目结构
- **按测试类型分层组织**：单元测试项目（Videra.Core.Tests）、集成测试项目（Videra.Core.IntegrationTests）
- **平台特定测试**：每个平台后端有自己的测试项目（Videra.Platform.Windows.Tests、Videra.Platform.Linux.Tests、Videra.Platform.macOS.Tests）
- **测试项目命名约定**：`{SourceProject}.Tests` 用于单元测试，`{SourceProject}.IntegrationTests` 用于集成测试
- **测试项目位置**：放在 `tests/` 目录下，与 `src/` 平级
- **测试项目依赖**：单元测试项目只引用被测试的项目；集成测试项目可以引用多个项目

### 测试框架选择
- **xUnit 2.9.x** 作为主测试框架（与 .NET 生态最佳实践对齐）
- **Moq 4.20.x** 用于模拟对象（轻量、API 稳定）
- **FluentAssertions 7.0.x** 用于断言（可读性高、丰富的断言库）
- **AutoFixture** (后续添加) 用于测试数据生成（Phase 2 考虑）
- **Coverlet 6.0.x** 用于代码覆盖率（与 xUnit 集成良好）

### 测试组织策略
- **单元测试**：测试单个类/方法的行为，使用 mock 隔离依赖
- **集成测试**：测试组件间交互，不 mock 内部实现
- **测试夹具**：共享的测试辅助类放在 `Tests/Common/` 目录
- **测试数据**：小型测试数据内联在测试方法中；大型测试数据文件放在 `Tests/Data/` 目录
- **命名约定**：测试方法名使用 `MethodName_ExpectedBehavior_State` 格式

### 日志系统设计
- **Serilog 4.2.x** 作为结构化日志框架
- **日志级别**：默认 Information，开发环境 Debug，生产环境 Warning
- **日志输出**：Console（开发）、File（生产）、Debug（调试时）
- **日志格式**：使用 Serilog 的输出模板，包含时间戳、级别、来源、消息、上下文属性
- **日志集成**：通过 Microsoft.Extensions.Logging 集成到 DI 容器
- **日志作用域**：使用 `LogContext` 推送属性（如 RequestId、Platform、BackendType）

### 日志清理原则
- **移除所有 Console.WriteLine**：包括生产代码和热路径中的调试输出
- **移除调试计数器**：删除 macOS 后端的 `_frameDebugCount`、`_setBufferCallCount` 等静态变量
- **移除条件调试输出**：删除 `if (_frameDebugCount % 100 == 0)` 类似的模运算日志
- **保留条件编译**：如果需要调试输出，使用 `#if DEBUG` 包裹，确保不进入生产代码

### 静态分析配置
- **警告视为错误**：在 Debug 构建中将警告视为错误（`TreatWarningsAsErrors = true`）
- **启用所有 NetAnalyzers**：Microsoft.CodeAnalysis.NetAnalyzers 全部启用
- **启用 SonarAnalyzer.CSharp**：额外的代码质量规则
- **规则严格度**：
  - CA1062: 验证公共 API 的 XML 文档注释
  - CA1303: 字符串字面量应传递给文化感知的 API
  - CA2007: 考虑使用 await 而不是 Task.Wait()
- **自定义规则集**：在 `.editorconfig` 中配置项目特定的规则

### 临时文件清理策略
- **删除所有 tmpclaude-* 文件**：这些是工具运行产生的临时文件，应全部删除
- **添加 .gitignore 规则**：`tmpclaude*` 防止未来累积
- **添加清理脚本**：创建 `clean.sh` / `clean.ps1` 用于手动清理临时文件
- **配置工具清理**：在相关工具配置中添加清理步骤（如果工具支持）

### 项目结构规范
```
Videra.sln
├── src/
│   ├── Videra.Core/
│   ├── Videra.Avalonia/
│   ├── Videra.Platform.Windows/
│   ├── Videra.Platform.Linux/
│   └── Videra.Platform.macOS/
├── tests/
│   ├── Videra.Core.Tests/
│   ├── Videra.Core.IntegrationTests/
│   ├── Videra.Platform.Windows.Tests/
│   ├── Videra.Platform.Linux.Tests/
│   ├── Videra.Platform.macOS.Tests/
│   └── Tests.Common/          # 共享测试辅助类
├── samples/
│   └── Videra.Demo/
└── docs/
```

### Claude's Discretion
- 测试方法的具体实现细节（由规划者/执行者决定）
- 日志模板的具体格式（符合 Serilog 最佳实践即可）
- 静态分析规则的细微调整（在保持严格度前提下）
- 清理脚本的具体实现方式（shell/PowerShell 均可）

</decisions>

<specifics>
## Specific Ideas

- 参考 .NET 生态的最佳实践（dotnet/runtime、dotnet/aspnetcore 的测试组织）
- 日志输出应便于在开发时调试，在生产环境中诊断问题
- 测试应该快速运行（单元测试）且稳定（集成测试）
- 代码分析规则应该在编译时立即反馈，不等待 CI

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 代码库分析
- `.planning/codebase/STACK.md` — 现有技术栈和依赖
- `.planning/codebase/ARCHITECTURE.md` — 系统架构和组件边界
- `.planning/codebase/CONCERNS.md` — 已知问题和需要修复的地方
- `.planning/codebase/CONVENTIONS.md` — 代码约定和模式
- `.planning/codebase/TESTING.md` — 当前测试状态（零覆盖）

### 项目定义
- `.planning/PROJECT.md` — 项目目标、约束、关键决策
- `.planning/REQUIREMENTS.md` — v1 需求规范
- `.planning/ROADMAP.md` — 路线图和阶段定义

### 研究输出
- `.planning/research/STACK.md` — 推荐的技术栈和版本
- `.planning/research/FEATURES.md` — 功能类别和标准要求
- `.planning/research/PITFALLS.md` — 需要避免的陷阱

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **Videra.Core** 核心抽象层（IBuffer, IPipeline, ICommandExecutor, IResourceFactory）是测试的主要目标
- **现有的解决方案结构**：使用 `Videra.slnx`（新型解决方案格式）
- **Microsoft.Extensions.DependencyInjection** 已在项目中使用，可用于日志 DI 集成

### Established Patterns
- **抽象工厂模式**：GraphicsBackendFactory 用于后端选择，测试时可利用
- **事件驱动模式**：StyleChanged 事件模式可用于测试响应验证
- **平台分离**：每个平台是独立项目，测试也应独立组织

### Integration Points
- **测试项目与源项目**：通过项目引用连接
- **日志与 DI 容器**：通过 Microsoft.Extensions.Logging.Integration 集成
- **静态分析与构建**：通过 .csproj 中的 AnalyzerConfigReference 集成
- **覆盖率与测试**：通过 Coverlet 收集器集成

</code_context>

<deferred>
## Deferred Ideas

- **性能基准测试**（BenchmarkDotNet）— Phase 5 考虑
- **模糊测试**（不安全代码的边界测试）— Phase 2 安全加固时考虑
- **测试可视化**（Allure/SpecFlow 报告）— 非当前阶段优先级
- **实时测试监控**— CI/CD 相关，当前使用本地构建

</deferred>

---
*Phase: 01-基础设施与清理*
*Context gathered: 2026-03-28*
