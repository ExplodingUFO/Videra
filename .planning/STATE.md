# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-21)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** 定义 `v1.21 Scene and Material Runtime v1` 的 requirements 和 roadmap

## Current Position

Milestone: `v1.21`
Phase: `Not started (defining requirements)`
Plan: `—`
Status: `Defining requirements`
Last activity: `2026-04-21` — started milestone `v1.21 Scene and Material Runtime v1`

Progress: `v1.20` is archived locally. `v1.21` milestone initialization is in progress.

## Performance Metrics

- Latest fully archived milestone: `v1.20 Viewer Product Boundary and Core Slimming`
- Archived milestone phases: `4`
- Archived milestone requirements: `9`
- Active milestone phases: `0`
- Active milestone requirements: `0`

## Accumulated Context

### Decisions

- `v1.17`: the repair line is now merged on `master`, so benchmark compile drift, SurfaceCharts analyzer debt, and Linux `XWayland` smoke no longer block the next milestone.
- `v1.18`: `SurfaceCharts` should get deeper before it gets wider; generalized data/axis/scalar contracts outrank generic `Chart3D`, public package expansion, and `OpenGL`/backend expansion.
- `v1.18`: benchmark review for analytics hotspots remains label-gated evidence rather than a hard numeric blocker.
- `v1.18` closeout: no milestone tag is retained because repository release tags remain version-aligned rather than milestone-aligned.
- `v1.19`: the next highest-value gap is default chart readability and interaction semantics, so presentation-space separation and interaction presets outrank contour/probe expansion for this milestone.
- `v1.20`: before chasing scene/material runtime breadth, Videra should first define a strict viewer/runtime `1.0` boundary and slim `Videra.Core` into a real runtime core.
- `v1.20` closeout: the package/product boundary work is complete locally, and the next milestone should move back to runtime/product depth rather than reopen package-boundary cleanup.

### Pending Todos

- Define `v1.21` requirements and roadmap, then start `Phase 107`.

### Blockers/Concerns

- Historical `.planning/phases/13-surfacechart-runtime-and-view-state-contract/13-VERIFICATION.md` and `.planning/phases/14-built-in-interaction-and-camera-workflow/14-VERIFICATION.md` still report `gaps_found`; they were acknowledged again at `v1.20` closeout and do not block archive.
- `audit-open` still reports quick-task slug `260421-gre-videra-surfacecharts-demo` as `missing` even though the fix was shipped and documented; this remains acknowledged process debt rather than a `v1.20` blocker.
- Carry-forward engineering debt from `v1.18` lives in `.planning/PROJECT.md` Current Risks and `.planning/milestones/v1.18-MILESTONE-AUDIT.md`.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260421-gre | 修复 `Videra.SurfaceCharts.Demo` 无法打开的问题 | 2026-04-21 | 998e0a5 | [260421-gre-videra-surfacecharts-demo](./quick/260421-gre-videra-surfacecharts-demo/) |

## Deferred Items

Items acknowledged and deferred at milestone close remain tracked here.

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Platform | Native compositor-hosted Wayland embedding | deferred until analytics depth stops outranking platform-scope expansion | `v1.18` |
| Platform | `OpenGL` backend evaluation | deferred unless the analytics roadmap exposes a real backend-coverage blocker | `v1.18` |
| Packages | Publishing `Videra.SurfaceCharts.*` as public consumer packages | deferred until the analytics contracts stabilize | `v1.18` |
| Product shape | Generic `Chart3D` scene abstraction | deferred until at least one more concrete 3D series exists | `v1.18` |
| Analytics | Interpolated probe, contour/wireframe, slicing, and full analysis-oriented camera presets | deferred until display-space defaults and interaction semantics land cleanly | `v1.19` |
| Series | `WaterfallSeries3D`, `PointLine/ScatterSeries3D`, and `SurfaceMeshSeries3D` | deferred until the core surface analytics contract proves reusable | `v1.18` |
| Process | Historical Phase 13 verification gap (`13-VERIFICATION.md`) | re-acknowledged at `v1.20` closeout; not blocking archive | `v1.20 closeout` |
| Process | Historical Phase 14 verification gap (`14-VERIFICATION.md`) | re-acknowledged at `v1.20` closeout; not blocking archive | `v1.20 closeout` |
| Quick task | `260421-gre-videra-surfacecharts-demo` audit-open metadata gap | acknowledged at `v1.20` closeout; shipped fix exists but artifact audit still reports the quick-task slug as `missing` | `v1.20 closeout` |

## Session Continuity

Last session: `2026-04-21 +08:00`
Stopped at: `v1.21` milestone start; next step is finalizing requirements and roadmap
Resume file: `.planning/REQUIREMENTS.md`
