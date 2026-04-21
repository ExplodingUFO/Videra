# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-21)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** `v1.20` 已完成 Phase `104`，下一步进入 Phase `105`，把 import-package composition 与 host seams 讲清并收紧

## Current Position

Milestone: `v1.20`
Phase: `Phase 105 ready`
Plan: `—`
Status: `Phase 104 is complete locally; Videra.Core is slimmed and the public package path now includes explicit import packages`
Last activity: `2026-04-21` — completed Phase `104` by extracting importers out of `Videra.Core`, removing concrete logging-provider baggage, and closing the packaged-consumer/release truth around the new import packages

Progress: `Phase 103-104 are complete locally; Phase 105 is the next executable step.`

## Performance Metrics

- Latest fully archived milestone: `v1.18 SurfaceCharts Analytics Core`
- Archived milestone phases: `4`
- Archived milestone requirements: `11`
- Active milestone phases: `4`
- Active milestone requirements: `9`

## Accumulated Context

### Decisions

- `v1.17`: the repair line is now merged on `master`, so benchmark compile drift, SurfaceCharts analyzer debt, and Linux `XWayland` smoke no longer block the next milestone.
- `v1.18`: `SurfaceCharts` should get deeper before it gets wider; generalized data/axis/scalar contracts outrank generic `Chart3D`, public package expansion, and `OpenGL`/backend expansion.
- `v1.18`: benchmark review for analytics hotspots remains label-gated evidence rather than a hard numeric blocker.
- `v1.18` closeout: no milestone tag is retained because repository release tags remain version-aligned rather than milestone-aligned.
- `v1.19`: the next highest-value gap is default chart readability and interaction semantics, so presentation-space separation and interaction presets outrank contour/probe expansion for this milestone.
- `v1.20`: before chasing scene/material runtime breadth, Videra should first define a strict viewer/runtime `1.0` boundary and slim `Videra.Core` into a real runtime core.

### Pending Todos

- Start Phase `105`: stabilize import-package composition guidance and tighten host seams without widening backend-specific API.

### Blockers/Concerns

- Historical `.planning/phases/13-surfacechart-runtime-and-view-state-contract/13-VERIFICATION.md` and `.planning/phases/14-built-in-interaction-and-camera-workflow/14-VERIFICATION.md` still report `gaps_found`; they were acknowledged at `v1.18` closeout and do not block the archive.
- Carry-forward engineering debt from `v1.18` lives in `.planning/PROJECT.md` Current Risks and `.planning/milestones/v1.18-MILESTONE-AUDIT.md`.
- Import extraction now crosses the public package line, so future host-seam work has to preserve the explicit `Core` / `Import` / `Backend` / `UI adapter` split instead of hiding it back inside `Videra.Avalonia`.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260421-gre | 修复 `Videra.SurfaceCharts.Demo` 无法打开的问题 | 2026-04-21 | 998e0a5 | [260421-gre-videra-surfacecharts-demo](./quick/260421-gre-videra-surfacecharts-demo/) |

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Platform | Native compositor-hosted Wayland embedding | deferred until analytics depth stops outranking platform-scope expansion | `v1.18` |
| Platform | `OpenGL` backend evaluation | deferred unless the analytics roadmap exposes a real backend-coverage blocker | `v1.18` |
| Packages | Publishing `Videra.SurfaceCharts.*` as public consumer packages | deferred until the analytics contracts stabilize | `v1.18` |
| Product shape | Generic `Chart3D` scene abstraction | deferred until at least one more concrete 3D series exists | `v1.18` |
| Analytics | Interpolated probe, contour/wireframe, slicing, and full analysis-oriented camera presets | deferred until display-space defaults and interaction semantics land cleanly | `v1.19` |
| Series | `WaterfallSeries3D`, `PointLine/ScatterSeries3D`, and `SurfaceMeshSeries3D` | deferred until the core surface analytics contract proves reusable | `v1.18` |
| Process | Historical Phase 13 verification gap (`13-VERIFICATION.md`) | acknowledged at `v1.18` closeout; not blocking archive | `v1.18 closeout` |
| Process | Historical Phase 14 verification gap (`14-VERIFICATION.md`) | acknowledged at `v1.18` closeout; not blocking archive | `v1.18 closeout` |

## Session Continuity

Last session: `2026-04-20 +08:00`
Stopped at: `Phase 104 complete locally; Phase 105 is ready to start`
Resume file: `.planning/ROADMAP.md`
