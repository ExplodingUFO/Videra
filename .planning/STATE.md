---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: in_progress
last_updated: "2026-03-28T12:55:00.000Z"
progress:
  total_phases: 4
  completed_phases: 0
  total_plans: 7
  completed_plans: 6
---

# Videra 开源准备 - 项目状态

**项目目标：** 跨平台 3D 渲染引擎的可靠性 — 在三个主流平台上稳定运行，具备完整的测试覆盖和文档。

---

## 项目参考

**核心价值：** 跨平台 3D 渲染引擎的可靠性

**当前范围：** 开源准备 — 将 Videra 3D 渲染引擎打造成高质量的开源项目

**技术栈：**

- .NET 8.0
- Silk.NET 2.21.0（图形 API 绑定）
- Avalonia 11.3.9（UI 框架）
- SharpGLTF.Toolkit 1.0.6（模型加载）

---

## 当前位置

**当前阶段：** Phase 1 - 基础设施与清理

**当前计划：** 01-07 (Windows 已推进，Linux/macOS 待对应环境执行)

**状态：** Phase 1 in progress

**进度：** 0/4 阶段完成 (Phase 1 存在严格 gap)

```
[░░░░░░░░░░] 0%
```

---

## 性能指标

**需求覆盖：** 24/24 已映射；TEST-03 严格验证仍未完全关闭

**阶段数：** 4

**粒度：** Coarse

---

## 累积上下文

### 已记录决策

| 决策 | 原因 | 结果 |
|------|------|------|
| 严格测试覆盖（80%+） | 开源项目需要可靠性保障 | Pending |
| 完整文档 | 降低贡献门槛，提升可用性 | Pending |
| 三个平台同等优先级 | 不偏向任何平台 | Pending |
| 可以破坏现有 API | 这是清理的最佳时机 | Pending |
| 并行推进 | 加速进度，独立区域可同时工作 | Pending |
| 仅本地构建 | 暂不需要 CI/CD | Pending |
| Microsoft.NET.Test.Sdk 18.3.0 + 直接引用 | SDK 10.0.201 不兼容 17.12.0 的传递引用 | Done |
| TreatWarningsAsErrors 配合 NoWarn 抑制 | 预先存在的分析器警告不应阻塞新基础设施的建立 | Done |
| 平台后端 smoke tests 使用独立测试项目 + OS guards | 作为过渡性结构补齐，不等于严格 TEST-03 完成 | Done |
| VulkanBackend.Dispose 必须容忍未完成初始化的清理 | Linux smoke test 覆盖了失败前置条件路径 | Done |
| Windows 先完成真实 HWND 宿主验证，Linux/macOS 保持环境阻塞 | 当前只有 Windows 环境，但不降低 TEST-03 标准 | Done |

### 关键约束

- **平台：** 必须支持 Windows、Linux、macOS 三个平台，同等优先级
- **测试：** 目标 80%+ 代码覆盖率
- **文档：** 完整的 API 和用户文档
- **质量：** 实用模式 — 专注功能正确性，允许灵活性
- **兼容性：** 可以破坏现有 API 进行重构
- **时间：** 不紧急，全面细致的方法

### 已知问题

从代码库分析中发现的问题：

**需要解决：**

- 未实现的方法（10+ 处 NotImplementedException）
- 脆弱的平台特定代码（尤其是 macOS Metal 互操作）
- 缺少结构化错误处理
- 缺少文档

### 阻塞问题

- **TEST-03 严格 gap 仍未关闭**
  - Windows：已具备真实 HWND-backed D3D11 lifecycle / draw-path 验证基础
  - Linux：缺真实 X11 host fixture 与对应宿主执行环境
  - macOS：缺真实 NSView host fixture 与对应宿主执行环境
- **Phase 1 不能标记完成**，直到 Linux/macOS 的真实 native-host lifecycle/render-path 测试在对应环境执行完成

---

## 会话连续性

**上次活动：** Windows 真实宿主验证增强 + Demo 第一轮完善（2026-03-28）

**下一步：**
1. 保持 01-07 为严格 gap closure plan
2. 继续在 Windows 上验证 Demo 与 D3D11 路径
3. 为 Linux/macOS 形成可执行的环境阻塞说明与后续执行包

**相关文件：**

- ROADMAP.md - 项目路线图
- REQUIREMENTS.md - 需求定义
- PROJECT.md - 项目上下文
- config.json - 配置设置
- .planning/phases/01-基础设施与清理/01-VERIFICATION.md - 当前权威验证结论
- .planning/phases/01-基础设施与清理/01-07-PLAN.md - 严格 gap closure plan

---

*状态初始化：2026-03-28*
*最近修订：2026-03-28 Windows validation + demo pass*
