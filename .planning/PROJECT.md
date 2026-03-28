# Videra 开源准备项目

## What This Is

将 Videra 3D 渲染引擎打造成高质量的开源项目。Videra 是一个基于 .NET 8 的跨平台 3D 渲染引擎，支持 Windows (D3D11)、Linux (Vulkan)、macOS (Metal) 三个平台，提供软件渲染回退、线框渲染系统和 Avalonia UI 集成。

## Core Value

**跨平台 3D 渲染引擎的可靠性** — 在三个主流平台上稳定运行，具备完整的测试覆盖和文档。

## Requirements

### Validated

<!-- 从现有代码推断的功能 -->

- ✓ **跨平台图形后端** — Windows D3D11、Linux Vulkan、macOS Metal 实现 — existing
- ✓ **软件渲染回退** — CPU 光栅化渲染器 — existing
- ✓ **3D 场景管理** — 对象管理、场景图、相机控制 — existing
- ✓ **线框渲染系统** — 多种线框显示模式（全边、可见边、覆盖、仅线框）— existing
- ✓ **模型加载** — glTF/GLB/OBJ 格式支持 — existing
- ✓ **渲染风格预设** — 可配置的材质和光照参数 — existing
- ✓ **Avalonia UI 集成** — WPF-like 控件封装 — existing

### Active

<!-- 当前范围：开源准备 -->

- [ ] **TEST-01**: 完整的测试覆盖（目标 80%+）
  - 核心功能的单元测试
  - 渲染管线的集成测试
  - 平台特定的测试
  - 边界情况和错误处理测试
- [ ] **TEST-02**: 测试基础设施
  - 测试项目结构
  - 测试运行配置
  - 覆盖率报告
- [ ] **LOG-01**: 结构化日志系统
  - 移除所有 Console.WriteLine 调试代码
  - 实现 ILogger/ILoggerFactory 抽象
  - 可配置的日志级别
- [ ] **LOG-02**: 调试计数器清理
  - 移除 macOS 后端的静态调试计数器
  - 移除帧日志的模运算操作
- [ ] **API-01**: 修复未实现的方法
  - 移除或实现 NotImplementedException 方法
  - 清理接口中不支持的 API
  - 为每个平台明确支持的方法添加文档
- [ ] **ERROR-01**: 结构化错误处理
  - 定义领域特定异常类型
  - 包含相关诊断信息（HRESULT、Vulkan 错误码）
  - 用户友好的错误消息
- [ ] **MACOS-01**: 重构 Metal 互操作
  - 迁移到更安全的绑定方式
  - 移除手动的 Objective-C runtime 调用
  - 使用类型化绑定替代字符串选择器
- [ ] **LINUX-01**: Wayland 支持
  - 添加 Wayland 显示后端
  - 动态库加载（dlopen）
  - 从 X11 回退到软件渲染
- [ ] **LINUX-02**: 改进 X11 处理
  - 验证库句柄
  - 实现回退机制
- [ ] **CLEAN-01**: 资源清理验证
  - RAII 模式实现
  - 部分初始化失败时的回滚
  - 资源泄漏检测
- [ ] **PERF-01**: 性能优化
  - 移除热路径中的 Console I/O
  - 优化线框颜色更新（避免完整数组拷贝）
  - 实现资源缓存（pipeline、shader、buffer）
- [ ] **PERF-02**: 可扩展性改进
  - GPU 资源池化
  - 实例渲染支持
  - 动态 uniform buffer（ring buffer 模式）
- [ ] **SEC-01**: 安全加固
  - 添加指针操作的边界检查
  - 文件路径验证（防止路径遍历）
  - 验证库句柄和函数指针
- [ ] **DOCS-01**: API 文档
  - XML 注释覆盖所有公开 API
  - 生成 API 文档网站
- [ ] **DOCS-02**: 用户文档
  - README 更新（架构、快速开始）
  - 贡献指南
  - 故障排查指南
  - 示例代码
- [ ] **DOCS-03**: 架构文档
  - 系统架构说明
  - 渲染管线文档
  - 平台特定注意事项
- [ ] **BUILD-01**: 跨平台构建验证
  - Windows 编译测试
  - Linux 编译测试
  - macOS 编译测试
- [ ] **CLEAN-02**: 临时文件清理
  - 删除所有 tmpclaude-* 文件
  - 添加工具清理步骤
- [ ] **SHADER-01**: 着色器统一
  - 评估跨平台着色器方案
  - 考虑使用 SPIRV-Cross 等工具
- [ ] **DEPTH-01**: 深度缓冲一致性
  - 统一各平台的深度缓冲管理
  - 确保正确的清理和调整大小行为

### Out of Scope

- CI/CD 自动化（使用本地构建验证）
- NuGet 包发布（准备但不立即发布）
- 向后兼容性（可以破坏现有 API 进行重构）
- 多线程软件渲染器
- LOD（细节级别）支持

## Context

**现有代码库状态：**

这是一个功能完整但需要质量提升的项目。根据代码库分析：

**优点：**
- 清晰的分层架构（核心层、抽象层、后端实现层）
- 事件驱动的渲染风格系统
- 完整的平台抽象接口
- 支持三种原生图形 API

**需要解决的问题：**
- 零测试覆盖
- 大量调试代码混在生产代码中
- 未实现的方法（NotImplementedException）
- 脆弱的平台特定代码（尤其是 macOS Metal 互操作）
- 缺少结构化错误处理
- 性能瓶颈（热路径中的 Console I/O）
- 缺少文档

**技术环境：**
- .NET 8.0
- Silk.NET 2.21.0（图形 API 绑定）
- Avalonia 11.3.9（UI 框架）
- SharpGLTF.Toolkit 1.0.6（模型加载）
- System.Numerics（向量数学）

## Constraints

- **平台**: 必须支持 Windows、Linux、macOS 三个平台，同等优先级
- **测试**: 目标 80%+ 代码覆盖率
- **文档**: 完整的 API 和用户文档
- **质量**: 实用模式 — 专注功能正确性，允许灵活性
- **兼容性**: 可以破坏现有 API 进行重构
- **时间**: 不紧急，全面细致的方法

## Key Decisions

| 决策 | 原因 | 结果 |
|------|------|------|
| 严格测试覆盖（80%+） | 开源项目需要可靠性保障 | — Pending |
| 完整文档 | 降低贡献门槛，提升可用性 | — Pending |
| 三个平台同等优先级 | 不偏向任何平台 | — Pending |
| 可以破坏现有 API | 这是清理的最佳时机 | — Pending |
| 并行推进 | 加速进度，独立区域可同时工作 | — Pending |
| 仅本地构建 | 暂不需要 CI/CD | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-28 after initialization*
