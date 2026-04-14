# Videra.SurfaceCharts.Processing - 曲面图预处理与缓存模块

[English](../../../src/Videra.SurfaceCharts.Processing/README.md) | [中文](videra-surfacecharts-processing.md)

`Videra.SurfaceCharts.Processing` 负责 surface-chart 模块家族里的离线预处理、pyramid 构建和 cache 文件路径。

> 中文镜像用于快速查阅，英文版为准。

## 模块边界

- 负责大矩阵到多级 tile 数据的准备过程
- 负责 cache manifest 与 payload sidecar 的读写
- 不负责 Avalonia 控件生命周期
- 不负责鼠标交互、坐标轴或 overlay UI

## 当前能力

- 从 `SurfaceMatrix` 构建 overview-first pyramid
- 读写 `*.surfacecache.json` + `*.surfacecache.json.bin`
- 先读取 manifest 元数据，再按 viewport 需要懒加载 tile payload

## 当前限制

- 当前主要面向离线数据准备
- 还不是实时 streaming pipeline
- 还不是 UI 层的相机或交互状态管理入口

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Avalonia](videra-surfacecharts-avalonia.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)
