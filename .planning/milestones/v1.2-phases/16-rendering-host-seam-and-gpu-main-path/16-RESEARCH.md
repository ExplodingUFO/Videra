# Phase 16 Research: Rendering Host Seam and GPU Main Path

**Phase:** 16  
**Name:** Rendering Host Seam and GPU Main Path  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

回答本 phase 的规划问题：

> 如何在不把 chart 语义塞回 `VideraView` 主线的前提下，为 surface-chart 建立显式 render-host seam、落地真实 GPU 主路径，并保留可诊断的 software fallback，同时把当前“每次 viewport/input 变化都重建整份 scene”的路径改造成增量 render state？

## Current Code Reality

### 1. 当前 renderer ownership 还在 `SurfaceChartView`

当前 checkout 里，`SurfaceChartView` 同时持有：

- `_tileCache`
- `_cameraController`
- `_controller`
- `_renderer`
- `_renderScene`

其中 `InvalidateRenderScene()` 会直接触发 `RebuildRenderSceneIfPossible()`，而后者每次都会：

1. 读取当前 loaded tiles
2. 选定 `ColorMap`
3. 调用 `SurfaceRenderer.BuildScene(...)`
4. 重新构造整份 `SurfaceRenderScene`

这意味着当前 chart control 不是单纯的 view shell，而是直接拥有 render-scene rebuild orchestration。

### 2. 当前软件路径是真正的实现基线，但它是 full-snapshot 模式

当前软件路径分两层：

- `SurfaceRenderer`：把 `SurfaceTile` 转成 `SurfaceRenderTile` / `SurfaceRenderScene`
- `SurfaceScenePainter`：把 `SurfaceRenderScene` 投影并逐三角形 `DrawGeometry`

优点：

- truth 已经被 `SurfaceRendererInputTests`、`SurfaceScenePainterTests` 锁住
- Phase 15 的 `SurfaceChartProjection` 已经让 overlay 与 painter 共享投影

缺点：

- `SurfaceRenderScene` 是整份 immutable snapshot
- viewport / projection / loaded tile 变化时没有 resident state 增量边界
- 当前 software path 适合作为 fallback/reference，不适合作为长期 main path 的 ownership 模型

### 3. `VideraView` 侧已经证明了 host seam 的形状，但不能直接复用 ownership

`VideraView` 当前有一套成熟分层：

- `RenderSessionOrchestrator`
- `RenderSessionSnapshot`
- `VideraViewSessionBridge`
- `NativeControlHost + overlay container`

这说明以下模式在仓库里已经被验证：

- host-agnostic orchestrator + typed snapshot
- Avalonia shell 只做 native host / overlay / input glue
- fallback/diagnostics truth 必须显式可查

但 surface-chart 不能直接挂这套 ownership：

- `VideraEngine` 继续是 viewer pipeline owner
- chart 仍是 sibling module family，不是 `VideraView` mode
- chart renderer 需要 chart-specific scene/state vocabulary，而不是 viewer scene object / pass contributor vocabulary

规划含义：

- 可以借用 `RenderSessionOrchestrator` / `VideraViewSessionBridge` 的分层思路
- 不能把 `RenderSession` 或 `VideraView` 变成 chart renderer root

### 4. 当前 `SurfaceChartView` 还是 alpha thin shell 文档，但代码已经前进了一步

`src/Videra.SurfaceCharts.Avalonia/README.md` 与 demo README 仍在写：

- 交互未完成
- axis/legend 未完成
- hover probe 未完成

但 Phase 15 已经在当前 checkout 落下：

- chart-local axis / legend overlay
- hover + pinned probe
- shared `SurfaceChartProjection`

因此 Phase 16 一旦引入 GPU-first + fallback truth，这两份 README 至少需要同步 renderer/fallback/host-limit 事实，否则 planning 完成后 docs 会再次落后于代码。

### 5. 当前 scheduler / cache 限制是真问题，但不是本 phase 的主目标

`SurfaceTileScheduler` 当前仍是：

- overview first
- viewport tiles sequential await
- `UpdateViewport` / `UpdateViewSize` 时 prune detail tiles

`SurfaceCacheReader` 当前也仍有 reopen / read / copy 路径。

这些问题会直接影响大型数据体验，但 roadmap 已经把它们放到 Phase 17：

- `REND-03`
- `DATA-01..04`

所以 Phase 16 的合理边界是：

- 先把 render host seam、GPU/software backend truth、renderer-state 增量边界定下来
- 不在本 phase 一次性吃掉 scheduler / cache / batch-I/O 演进

## Design Options

### Option A: 保持现有 `SurfaceChartView` + `_renderScene` 结构，只新增一个 GPU 旁路

做法：

- 保留 `_renderScene` full rebuild
- 为 GPU 新增一个并列实现
- 由 `SurfaceChartView` 直接决定走 CPU 还是 GPU

**优点**

- 短期改动表面上较少
- 当前软件路径几乎不用拆

**缺点**

- seam 仍然没有建立，`SurfaceChartView` 继续是 renderer owner
- GPU 与 software 很容易演变成两套不同 truth
- `REND-02` 里的 incremental state 很难在这种结构里落地

**结论**

- Reject

### Option B: 新增 chart-specific rendering package，先建立 `SurfaceChartRenderHost`，再让 GPU / software backend 都挂到同一 seam

做法：

- 新增 `Videra.SurfaceCharts.Rendering`
- 把 typed inputs / snapshot / backend selection / render-state ownership 下沉到这个新包
- `SurfaceChartView` 收回到 Avalonia shell + overlay owner + input glue
- software path 先通过 backend adapter 继续复用 `SurfaceRenderer` truth
- GPU path 在同一 host seam 下接入

**优点**

- 正面满足 Phase 16 success criteria
- 为 Phase 17 incremental residency 留出清晰演进点
- 能显式表达 fallback truth，而不是隐藏在 control 细节里

**缺点**

- 需要引入新 project + test wiring
- `SurfaceChartView` 很可能要演化出 native-host shell 形态

**结论**

- Recommended

### Option C: 直接复用 `RenderSession` / `VideraView` host stack 来承载 chart GPU 渲染

做法：

- chart 直接借 `RenderSession` / `NativeControlHost`
- 尽量少造 chart-specific host vocabulary

**优点**

- 看起来能复用较多现有 viewer 基础设施

**缺点**

- 直接破坏 sibling boundary
- 很容易把 chart 变回 `VideraView` 的一种模式
- 后续 chart runtime/rendering 会受 viewer 侧 public/internal contract 牵制

**结论**

- Reject

## Recommended Architecture

### 1. 新增 `Videra.SurfaceCharts.Rendering` project

推荐在 `src/` 下新增：

- `src/Videra.SurfaceCharts.Rendering/Videra.SurfaceCharts.Rendering.csproj`

依赖方向建议：

- `Videra.SurfaceCharts.Rendering` -> `Videra.SurfaceCharts.Core`
- `Videra.SurfaceCharts.Rendering` -> `Videra.Core`
- `Videra.SurfaceCharts.Avalonia` -> `Videra.SurfaceCharts.Rendering`

这样可以：

- 让 chart renderer 拿到 `IGraphicsBackend` / `IResourceFactory` / `ICommandExecutor`
- 同时不把 Avalonia UI types 放进 renderer ownership 层

### 2. `SurfaceChartRenderHost` 应成为 chart-local renderer root

推荐新类型：

- `SurfaceChartRenderInputs`
- `SurfaceChartRenderSnapshot`
- `SurfaceChartRenderBackendKind`
- `ISurfaceChartRenderBackend`
- `SurfaceChartRenderHost`

边界应类似 Phase 10 的 session seam：

- `SurfaceChartRenderHost` 拥有 typed inputs、active backend、fallback truth、resident render state
- `SurfaceChartView` 只负责把 source / viewport / projection / view size / handle 状态同步给 host
- software 与 GPU backend 都从 `SurfaceChartRenderHost` 进出

### 3. 软件路径先退化成 backend adapter，而不是 main-path owner

当前 software path 的事实应尽量保留：

- `SurfaceRenderer.BuildTile/BuildScene` 的 sample-to-axis/value truth
- `SurfaceScenePainter` 的投影和逐三角形绘制 truth

但 ownership 要变：

- software path 不再由 `SurfaceChartView` 直接重建 `_renderScene`
- 而是由 `SurfaceChartRenderHost` 通过 `SurfaceChartSoftwareRenderBackend` 统一维护

这能让 software fallback 继续真实，同时不阻塞后续 GPU backend 并轨。

### 4. GPU backend 应复用 Videra 的 graphics abstractions，但不要复用 `VideraEngine`

仓库已经有：

- `IGraphicsBackend`
- `IResourceFactory`
- `ICommandExecutor`

这些足够支撑 chart-local GPU backend。

推荐方向：

- `SurfaceChartGpuRenderBackend` 直接基于这些 abstraction 创建 vertex/index/uniform resources
- 输入来自 chart-specific render state（tile geometry / colors / projection）
- 不引入 `VideraEngine`、scene object、pass contributor、selection overlay 等 viewer vocabulary

### 5. GPU host surface 应该是 chart-local，而不是借 `VideraView` 的 native host

如果要让 GPU main path 真实呈现，就需要真正的 host surface。

推荐做法：

- 在 `Videra.SurfaceCharts.Avalonia` 里引入 chart-local `ISurfaceChartNativeHost` / factory
- `SurfaceChartView` 为 GPU path 组织 native host + overlay 容器
- software path 继续走 `Render(DrawingContext)`

这与 `VideraView` 的 shell 形态相似，但 ownership 仍是 chart-local。

### 6. fallback truth 必须和 diagnostics / docs / tests 同步

`SurfaceChartRenderSnapshot` 至少应携带：

- `ActiveBackend`
- `IsFallback`
- `FallbackReason`
- `UsesNativeSurface`
- `ResidentTileCount`

推荐再给 `SurfaceChartView` 一个只读投影 surface，例如：

- `RenderingStatus`
- 或 `LastRenderSnapshot`

这样 integration tests、README 和 demo 才能讲同一套真相。

### 7. 增量 state 的核心不是“零重建”，而是明确 dirty boundary

本 phase 的合理目标不是把 renderer 一步做成最终形态，而是建立至少这四个 dirty bucket：

- `FullResetRequired`：source / metadata / backend replacement
- `ResidencyDirty`：tile add/remove
- `ColorDirty`：color-map changed
- `ProjectionDirty`：viewport / projection / view-size changed

对应的 truth 应该是：

- viewport / projection 改变时，不重建 resident tile geometry
- color-map 改变时，不重算 sample position
- tile residency 改变时，只处理 changed keys
- 只有 source / metadata 替换才做 full reset

## Risks To Plan Around

### Risk 1: GPU “主路径”被偷偷做成 GPU -> bitmap copy

这会让 Phase 16 看似完成，但实际没有 GPU-first truth。

**Mitigation**

- plan 中明确要求：GPU path 在支持 host 时必须直接呈现到 native surface
- software bitmap path 只能作为 fallback

### Risk 2: 新 seam 名义上存在，实际上 `SurfaceChartView` 仍持有 full-scene rebuild

这会让 `REND-02` 实际上没有进展。

**Mitigation**

- repository/integration tests 要验证 `SurfaceChartView.Rendering.cs` 不再直接 new `SurfaceRenderer()` 并 full rebuild `_renderScene`

### Risk 3: GPU backend 把 `VideraView` / `RenderSession` 语义重新拉进 chart 包

这会直接破坏 sibling boundary。

**Mitigation**

- 明确禁止 `SurfaceChartView` / `Videra.SurfaceCharts.Rendering` 引用 `VideraView`、`RenderSession`、`VideraViewSessionBridge`
- 用 repository guards 固定

### Risk 4: README / demo 继续停留在 alpha 旧说法

这样 phase 执行完后，对外文档会再次失真。

**Mitigation**

- 把最小必要 README / demo truth update 放进 Phase 16 plan 03
- 只更新 renderer/fallback/host-limit truth，不顺带夸大未实现交互

## Suggested Plan Shape

Phase 16 可以稳定拆成 roadmap 里已经给出的三份执行计划：

1. **16-01 Rendering package and render-host contract spike**
   - 新 rendering project
   - `SurfaceChartRenderHost` / typed inputs / snapshot
   - software backend adapter
   - `SurfaceChartView` 不再自己做 full-scene rebuild ownership

2. **16-02 GPU surface renderer with software fallback selection**
   - chart-local native host shell
   - `SurfaceChartGpuRenderBackend`
   - fallback selection / diagnostics truth
   - control-visible rendering status

3. **16-03 Incremental render-state/residency path and host-limit documentation**
   - resident tile / change-set / dirty-flag 模型
   - viewport/projection/color changes 的增量路径
   - README/demo/guards 同步 renderer truth

## Recommendation

按 **Option B** 规划：先建立新的 chart-local render-host seam，再把 software 路径降级为 backend adapter，随后接 GPU backend 和 fallback truth，最后用增量 render state 与最小必要文档/guards 把 Phase 16 固定下来。这样既不破坏 `VideraView` 边界，也能给 Phase 17 的大数据 residency 演进留下真正可用的 renderer foundation。
