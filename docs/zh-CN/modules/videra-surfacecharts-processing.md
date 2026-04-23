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
- 对 cache-backed source 复用 persistent payload session，而不是每个 tile 重新打开 sidecar
- 通过 `ISurfaceTileBatchSource` 提供 ordered batch reads
- 通过 `SurfaceTileStatistics` 保留 reduced tile 的 source-region truth、range、average 与 exact-vs-reduced 语义

## 当前限制

- 当前主要面向离线数据准备
- 还不是实时 streaming pipeline
- 还不是 UI 层的相机或交互状态管理入口
- `XWayland` compatibility 这类宿主渲染限制由 Avalonia 控制层记录，不属于 Processing 的职责

## 性能与可选 native seam

- `BenchmarkDotNet` 基准当前覆盖 viewport selection、cache batch reads 与 pyramid/statistics 路径
- SurfaceCharts 当前的硬门槛仍只落在 `SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement` 和 `SurfaceChartsProbeBenchmarks.ProbeLatency`，它们对应的是更紧的交互驻留和更低的 probe 路径抖动；allocation thresholds 仍然只是后续升级的指导，也就是 `future escalation guidance`，不是这一阶段的阻塞项
- optional native seam 只允许停留在粗粒度 reduction / cache-processing hotspot，不把交互或 renderer orchestration 拉过边界

## 相关入口

- [Videra.SurfaceCharts.Core](videra-surfacecharts-core.md)
- [Videra.SurfaceCharts.Avalonia](videra-surfacecharts-avalonia.md)
- [独立 Demo](../../../samples/Videra.SurfaceCharts.Demo/README.md)
