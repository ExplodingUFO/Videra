# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-20)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** `v1.19` 正在定义中，目标是提升 `SurfaceCharts` 默认显示空间、value 轴表达和交互预设，而不是直接继续扩更深的 analytics 功能面

## Current Position

Milestone: `v1.19`
Phase: `Not started (defining requirements)`
Plan: `—`
Status: `Defining requirements and roadmap for SurfaceCharts presentation-space and interaction defaults`
Last activity: `2026-04-20` — started `v1.19`, scoped the milestone around display-space transforms, overlay/value-axis defaults, interaction presets, and demo truth`

Progress: `Requirements and roadmap initialized for the next SurfaceCharts milestone.`

## Performance Metrics

- Latest fully archived milestone: `v1.18 SurfaceCharts Analytics Core`
- Archived milestone phases: `4`
- Archived milestone requirements: `11`
- Active milestone phases: `4`
- Active milestone requirements: `10`

## Accumulated Context

### Decisions

- `v1.17`: the repair line is now merged on `master`, so benchmark compile drift, SurfaceCharts analyzer debt, and Linux `XWayland` smoke no longer block the next milestone.
- `v1.18`: `SurfaceCharts` should get deeper before it gets wider; generalized data/axis/scalar contracts outrank generic `Chart3D`, public package expansion, and `OpenGL`/backend expansion.
- `v1.18`: benchmark review for analytics hotspots remains label-gated evidence rather than a hard numeric blocker.
- `v1.18` closeout: no milestone tag is retained because repository release tags remain version-aligned rather than milestone-aligned.
- `v1.19`: the next highest-value gap is default chart readability and interaction semantics, so presentation-space separation and interaction presets outrank contour/probe expansion for this milestone.

### Pending Todos

- Start Phase `99`: define and thread display-space transforms through `ViewState`, camera fit, projection, and render-state compatibility paths.

### Blockers/Concerns

- Historical `.planning/phases/13-surfacechart-runtime-and-view-state-contract/13-VERIFICATION.md` and `.planning/phases/14-built-in-interaction-and-camera-workflow/14-VERIFICATION.md` still report `gaps_found`; they were acknowledged at `v1.18` closeout and do not block the archive.
- Carry-forward engineering debt from `v1.18` lives in `.planning/PROJECT.md` Current Risks and `.planning/milestones/v1.18-MILESTONE-AUDIT.md`.

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
Stopped at: `v1.19 initialized; requirements and roadmap are ready for phase discussion/planning`
Resume file: `.planning/ROADMAP.md`
