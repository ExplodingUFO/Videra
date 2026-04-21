# Phase 28 Context

## Goal

在已统一的 camera/projection spine 上，把 axis / grid / legend / overlay 从当前“可用且真实”推进到“专业且可配置”。

## Boundary

- 只改 `Videra.SurfaceCharts.Avalonia` 内的 overlay/layout/options 合同与 tests/docs。
- 不把 chart-specific overlay 选项推回 `VideraView` 公共抽象。
- 不回退已经锁定的 sibling boundary、probe truth、render-host ownership 和 backend-owned color-map truth。

## Current Truth

- `SurfaceAxisOverlayPresenter` 当前直接生成 `X/Y/Z` 三条轴、major ticks 和 labels。
- tick 数量主要按投影长度粗略夹在 `2..7`，formatter 固定 `0.###`。
- legend 仍是固定 title/formatter/swatches 布局。
- 没有 minor ticks、grid-plane selection、label collision culling、axis-side override 或 host-facing overlay options。

## Decision

Phase 28 分三波：

1. axis/grid contracts：overlay options、grid-plane truth、visible-edge selection/main tick generation。
2. layout/customization：minor ticks、formatter、title/unit override、axis-side switching、label collision handling。
3. truth updates：demo/docs/repository guards 对新的专业 overlay 行为讲同一套现实。

## Verification Focus

- 常见视角下 labels 不明显重叠，axis edge switching 自然。
- formatter / title / unit / grid-plane 只通过 chart-local options 暴露。
- demo/docs/tests 和实际 overlay 行为保持一致。
