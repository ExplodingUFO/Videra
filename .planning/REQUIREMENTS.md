# Videra 开源准备 - v1 需求

**生成日期：** 2026-03-28
**来源：** 代码库分析、领域研究、用户确认

---

## v1 需求

### 测试基础设施 (TEST)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| TEST-01 | 测试框架 | 集成 xUnit 2.9.x 作为测试框架，Moq 4.20.x 用于模拟，FluentAssertions 7.0.x 用于断言 |
| TEST-02 | 核心单元测试 | 为所有核心抽象（IBuffer、IPipeline、ICommandExecutor、IResourceFactory）编写单元测试 |
| TEST-03 | 集成测试 | 为平台特定后端编写集成测试，验证渲染管线端到端功能 |
| TEST-04 | 覆盖率报告 | 配置 Coverlet 6.0.x 生成代码覆盖率报告，目标 80%+ |

### 日志系统 (LOG)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| LOG-01 | Serilog 集成 | 集成 Serilog 4.2.x 结构化日志，通过 Microsoft.Extensions.Logging 集成到 DI 容器 |
| LOG-02 | 调试代码清理 | 移除所有生产代码中的 Console.WriteLine 调试语句（约 40+ 处） |
| LOG-03 | 调试计数器移除 | 移除 macOS MetalCommandExecutor 中的静态调试计数器（_frameDebugCount、_setBufferCallCount 等） |

### 错误处理 (ERROR)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| ERROR-01 | 域特定异常 | 定义域特定异常类型：GraphicsInitializationException、ShaderCompilationException、ResourceCreationException、PipelineStateException |
| ERROR-02 | 结构化错误信息 | 异常包含相关诊断信息（Windows HRESULT、Vulkan 错误码、Metal 错误描述） |
| ERROR-03 | 用户错误消息 | 提供用户友好的错误消息，用于 UI 显示和日志记录 |

### 代码质量 (QUALITY)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| QUAL-01 | 静态分析 | 集成 Microsoft.CodeAnalysis.NetAnalyzers 9.x 和 SonarAnalyzer.CSharp 10.x |
| QUAL-02 | 未实现方法修复 | 移除或实现所有 NotImplementedException 方法（约 10+ 处），或从接口中移除不支持的方法 |
| QUAL-03 | 安全加固 | 为所有不安全指针操作添加边界检查，考虑使用 Span<T> 替代原始指针 |

### 跨平台支持 (PLATFORM)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| PLAT-01 | Wayland 支持 | 添加 Linux Wayland 显示后端支持，不仅限于 X11 |
| PLAT-02 | 动态库加载 | 使用 dlopen/dlsym 动态加载库，替代硬编码库路径 |
| PLAT-03 | 构建验证 | 验证在 Windows、Linux、macOS 三个平台上都能成功编译和运行 |

### macOS 后端重构 (MACOS)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| MAC-01 | Metal 互操作重构 | 重构 macOS Metal 后端，使用更安全的绑定替代手动的 Objective-C runtime 调用（objc_msgSend） |
| MAC-02 | 深度缓冲一致性 | 统一各平台的深度缓冲管理，确保正确的创建、清理和调整大小行为 |

### 资源管理 (RESOURCE)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| RES-01 | 资源清理验证 | 实现 RAII 模式，部分初始化失败时正确回滚，添加资源泄漏检测 |
| RES-02 | 性能优化 | 移除渲染循环热路径中的所有 Console I/O 和字符串格式化操作 |
| RES-03 | 线框颜色优化 | 优化 Object3D.UpdateWireframeColor，避免每次颜色更改时完整拷贝顶点数组 |

### 文档 (DOCS)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| DOC-01 | API 文档 | 为所有公开 API 添加 XML 注释，使用 DocFX 2.76.x 生成 API 文档站点 |
| DOC-02 | 用户文档 | 编写 README（项目概述、快速开始）、示例代码、基本使用教程 |
| DOC-03 | 完整文档 | 编写架构说明（系统设计、渲染管线）、贡献指南（PR 流程、编码标准）、故障排查指南（平台特定问题） |

### 安全 (SECURITY)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| SEC-01 | 路径验证 | 在 ModelImporter.Load 中添加文件路径验证，防止路径遍历攻击 |
| SEC-02 | 库句柄验证 | 验证所有平台库句柄和函数指针，实现缺失库的回退机制 |

### 清理 (CLEAN)

| REQ-ID | 需求 | 描述 |
|--------|------|------|
| CLEAN-01 | 临时文件清理 | 删除所有 tmpclaude-* 临时文件（约 90 个），添加工具清理步骤 |

---

## 需求跟踪表

下表将每个需求映射到其来源和优先级：

| REQ-ID | 来源 | 优先级 | 复杂性 | 依赖 |
|--------|------|--------|--------|------|
| TEST-01 | 研究 | 高 | 低 | 无 |
| TEST-02 | 研究/代码库 | 高 | 高 | TEST-01 |
| TEST-03 | 研究 | 高 | 高 | TEST-01 |
| TEST-04 | 研究 | 高 | 中 | TEST-02 |
| LOG-01 | 研究/代码库 | 高 | 中 | 无 |
| LOG-02 | 代码库 CONCERNS | 高 | 低 | 无 |
| LOG-03 | 代码库 CONCERNS | 中 | 低 | 无 |
| ERROR-01 | 研究/代码库 | 高 | 中 | 无 |
| ERROR-02 | 代码库 CONCERNS | 高 | 中 | ERROR-01 |
| ERROR-03 | 代码库 | 中 | 低 | ERROR-01 |
| QUAL-01 | 研究 | 中 | 低 | 无 |
| QUAL-02 | 代码库 CONCERNS | 高 | 中 | 无 |
| QUAL-03 | 代码库 CONCERNS | 高 | 高 | 无 |
| PLAT-01 | 代码库 CONCERNS | 高 | 高 | 无 |
| PLAT-02 | 代码库 CONCERNS | 高 | 中 | 无 |
| PLAT-03 | 用户 | 高 | 中 | 无 |
| MAC-01 | 代码库 CONCERNS | 高 | 高 | 无 |
| MAC-02 | 代码库 CONCERNS | 中 | 中 | 无 |
| RES-01 | 代码库 CONCERNS | 高 | 中 | 无 |
| RES-02 | 代码库 CONCERNS | 高 | 低 | 无 |
| RES-03 | 代码库 CONCERNS | 中 | 低 | 无 |
| DOC-01 | 研究/用户 | 高 | 中 | 无 |
| DOC-02 | 用户 | 高 | 中 | 无 |
| DOC-03 | 用户 | 高 | 高 | DOC-02 |
| SEC-01 | 代码库 CONCERNS | 高 | 低 | 无 |
| SEC-02 | 代码库 CONCERNS | 高 | 中 | 无 |
| CLEAN-01 | 代码库 CONCERNS | 低 | 低 | 无 |

**总计：** 24 个需求

**复杂性分布：**
- 低：9 个
- 中：11 个
- 高：4 个

**优先级分布：**
- 高：21 个
- 中：3 个

---

## 需求类别摘要

| 类别 | 需求数 | 关键需求 |
|------|--------|----------|
| 测试基础设施 | 4 | TEST-01, TEST-02, TEST-04 |
| 日志系统 | 3 | LOG-01, LOG-02 |
| 错误处理 | 3 | ERROR-01, ERROR-02 |
| 代码质量 | 3 | QUAL-02, QUAL-03 |
| 跨平台支持 | 3 | PLAT-01, PLAT-03 |
| macOS 重构 | 2 | MAC-01 |
| 资源管理 | 3 | RES-01, RES-02 |
| 文档 | 3 | DOC-01, DOC-03 |
| 安全 | 2 | SEC-01, SEC-02 |
| 清理 | 1 | CLEAN-01 |

---

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| TEST-01 | Phase 1 | Complete |
| TEST-02 | Phase 1 | Pending |
| TEST-03 | Phase 1 | Complete (01-06) |
| TEST-04 | Phase 1 | Pending |
| LOG-01 | Phase 1 | Complete (01-02) |
| LOG-02 | Phase 1 | Pending |
| LOG-03 | Phase 1 | Pending |
| QUAL-01 | Phase 1 | Complete |
| CLEAN-01 | Phase 1 | Complete |
| ERROR-01 | Phase 2 | Pending |
| ERROR-02 | Phase 2 | Pending |
| ERROR-03 | Phase 2 | Pending |
| QUAL-02 | Phase 2 | Pending |
| QUAL-03 | Phase 2 | Pending |
| RES-01 | Phase 2 | Pending |
| RES-02 | Phase 2 | Pending |
| RES-03 | Phase 2 | Pending |
| SEC-01 | Phase 2 | Pending |
| SEC-02 | Phase 2 | Pending |
| MAC-01 | Phase 3 | Pending |
| MAC-02 | Phase 3 | Pending |
| PLAT-01 | Phase 3 | Pending |
| PLAT-02 | Phase 3 | Pending |
| PLAT-03 | Phase 3 | Pending |
| DOC-01 | Phase 4 | Pending |
| DOC-02 | Phase 4 | Pending |
| DOC-03 | Phase 4 | Pending |

---

## 反模式（明确不做的事）

- **CI/CD 自动化** — 不使用 GitHub Actions，仅本地构建验证
- **NuGet 包发布** — 准备但不立即发布
- **向后兼容性** — 可以破坏现有 API 进行重构
- **多线程软件渲染器** — 单线程 CPU 渲染器足够
- **LOD 支持** — 不实现细节级别系统
- **GUI 属性编辑器** — Videra 是引擎，不是编辑器
- **资产业务线工具** — 使用现有 Blender/glTF 工具
- **物理模拟** — 超出范围
- **多人游戏** — 超出范围
- **VR/AR 支持** — 需要平台特定 SDK
- **移动平台** — iOS/Android 超出范围

---
*需求定义完成：2026-03-28*
