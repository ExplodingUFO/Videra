---
quick_id: 260421-gre
title: 修复 Videra.SurfaceCharts.Demo 无法打开的问题
status: complete
created: 2026-04-21
---

# Quick Task 260421-gre

修复 `Videra.SurfaceCharts.Demo` 因 `SurfaceViewState` / `DisplaySpace` 契约半接入而无法编译启动的问题。

## Must Haves

- 补齐最小 `DisplaySpace` core 契约，让现有 Avalonia 交互层改动可以编译
- 保持默认行为兼容，避免额外扩大 `v1.19` 的实现范围
- 恢复 `Videra.SurfaceCharts.Demo` 的构建与启动
- 增加一条最小核心测试，锁住 `SurfaceViewState` 的默认/显式 display-space 语义

## Plan

1. 新增 `SurfaceDisplaySpace` 枚举，定义 `Raw` / `Presentation`
2. 扩展 `SurfaceViewState`，增加 `DisplaySpace` 属性与三参构造/默认参数
3. 保持 `CreateDefault(...)` 支持 display-space，但默认仍为 `Raw`
4. 为 `SurfaceViewStateTests` 补两条 display-space 断言
5. 验证 Demo 构建、Demo 启动 smoke、以及相关 Core tests
