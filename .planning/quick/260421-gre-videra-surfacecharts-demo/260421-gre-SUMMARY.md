---
quick_id: 260421-gre
title: 修复 Videra.SurfaceCharts.Demo 无法打开的问题
status: complete
completed: 2026-04-21
---

# Quick Task 260421-gre Summary

## Outcome

`Videra.SurfaceCharts.Demo` 之前打不开的根因是 Avalonia 交互层已经开始透传 `DisplaySpace`，但 `Videra.SurfaceCharts.Core` 仍停留在旧版 `SurfaceViewState` 契约，导致 Demo 项目编译阶段直接报 `DisplaySpace` 缺失和三参构造函数不存在。

本次修复没有回退现有未提交交互改动，而是补齐了最小 core 契约，使这组改动重新闭环。

## Delivered

- 新增 `SurfaceDisplaySpace`，提供 `Raw` / `Presentation`
- 扩展 `SurfaceViewState`，增加 `DisplaySpace` 属性和三参构造函数，默认仍保持 `Raw`
- 扩展 `SurfaceViewState.CreateDefault(...)`，允许显式携带 display-space，同时不改变旧调用方默认语义
- 为 `SurfaceViewStateTests` 增加默认值与显式值断言

## Verification

- `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj -c Debug -m:1 --no-restore` -> passed
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Debug -m:1 --no-restore --filter "FullyQualifiedName~SurfaceViewStateTests"` -> passed (`8/8`)
- Demo launch smoke: `dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj -c Debug --no-build` stayed alive for 5 seconds before forced stop (`RUNNING`)

## Notes

- 这次修复只补齐了让 Demo 恢复可编译/可启动的最小 display-space 契约，并没有把完整 `v1.19` presentation-space 行为整体前移到当前分支。
