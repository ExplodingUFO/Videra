# Phase 16: Rendering Host Seam and GPU Main Path - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `16-CONTEXT.md` - this log preserves the alternatives considered.

**Date:** 2026-04-14
**Phase:** 16-Rendering Host Seam and GPU Main Path
**Areas discussed:** baseline seam strategy, GPU presentation model, fallback contract, incremental state scope
**Selection mode:** user accepted recommended defaults (`按推荐`)

---

## Baseline seam strategy

| Option | Description | Selected |
|--------|-------------|----------|
| A | 先建立 chart-local render-host seam，让 GPU 与 software 都挂到同一 seam 下 | ✓ |
| B | 直接在现有 `SurfaceChartView` 上叠 GPU，再回头整理 seam | |
| C | 只做接口 spike，不迁移当前 CPU painter 主路径 | |

**User's choice:** 按推荐
**Notes:** 当前 checkout 仍由 `SurfaceChartView` 同时持有 control、controller、tile cache 与 render-scene rebuild；Phase 16 的成功标准要求先把 renderer seam 做明确，而不是继续让 view 拿着主路径 ownership。

---

## GPU presentation model

| Option | Description | Selected |
|--------|-------------|----------|
| A | 定义统一 renderer/host 合同，目标主路径是“支持时直接 GPU 渲染，不支持时走 software fallback” | ✓ |
| B | 先做 GPU 结果拷回 Avalonia bitmap 的过渡方案，并把它当作主路径 | |
| C | 只做 native-surface 专用 GPU 路径，不先考虑 host capability / test fallback | |

**User's choice:** 按推荐
**Notes:** 选项 A 既保留“GPU-first”真相，也避免把 bitmap 回拷伪装成主路径；同时给 unsupported host / test 环境留下明确 fallback 合同。

---

## Fallback contract

| Option | Description | Selected |
|--------|-------------|----------|
| A | 自动 fallback 到 software，但必须显式暴露 active mode、fallback 状态和原因 | ✓ |
| B | 静默 fallback，只保证还能显示 | |
| C | 没有 GPU 就直接失败，不保留 software path | |

**User's choice:** 按推荐
**Notes:** 仓库前几轮 phase 已经把“docs / demo / tests 同一真相”定成硬约束；这里继续沿用显式 fallback，而不是 silent behavior。

---

## Incremental state scope

| Option | Description | Selected |
|--------|-------------|----------|
| A | 本阶段只先收敛 renderer state 增量边界，把 scheduler / cache / I/O 深层演进留到 Phase 17 | ✓ |
| B | 在 Phase 16 一次性把 scheduler、cache、renderer state 全部重做 | |
| C | 只做 GPU/backend 切换，不碰增量 state | |

**User's choice:** 按推荐
**Notes:** 这样可以把 Phase 16 的交付聚焦到 renderer seam、GPU main path 与 truthful fallback，不把 `REND-03` / `DATA-*` 的后续工作提前卷进来。

---

## the agent's Discretion

- 新 seam、backend、diagnostics surface 的具体类型命名
- 是否新增独立 chart rendering project，以及它与现有 `Avalonia` / `Core` 包的落点
- README / demo 文案需要最小更新到什么程度，才能忠实表达 shipped host limits

## Deferred Ideas

- 大数据 cache / residency / batch-I/O / Rust hotspot seam 留给 Phase 17
- broad chart runtime / `ViewState` / rich camera UX 收口不在本阶段顺带兑现
- 把 chart renderer 并回 `VideraView` host stack 不属于本 phase

---

*Phase: 16-rendering-host-seam-and-gpu-main-path*
*Captured: 2026-04-14*
