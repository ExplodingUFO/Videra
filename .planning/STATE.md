# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-21)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** `v1.20` 已完成 Phase `106`，下一步进入 milestone audit / closeout，确认 viewer-first package boundary 可以直接归档

## Current Position

Milestone: `v1.20`
Phase: `Phase work complete`
Plan: `—`
Status: `Phase 106 is complete locally; package/support/release truth and repository validation now point at the same canonical public viewer stack`
Last activity: `2026-04-21` — completed Phase `106` by aligning package/support/release docs, extending repository release-readiness guards across docs and automation, and updating the Chinese onboarding mirrors to reflect the same package truth

Progress: `Phase 103-106 are complete locally; v1.20 is ready for milestone audit and closeout.`

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

- Run `v1.20` milestone audit and closeout now that all roadmap phases are complete locally.

### Blockers/Concerns

- Historical `.planning/phases/13-surfacechart-runtime-and-view-state-contract/13-VERIFICATION.md` and `.planning/phases/14-built-in-interaction-and-camera-workflow/14-VERIFICATION.md` still report `gaps_found`; they were acknowledged at `v1.18` closeout and do not block the archive.
- Carry-forward engineering debt from `v1.18` lives in `.planning/PROJECT.md` Current Risks and `.planning/milestones/v1.18-MILESTONE-AUDIT.md`.
- The phase work is done; the remaining risk is milestone closeout drift if audit artifacts are not generated before starting `v1.21`.

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
Stopped at: `Phase 106 complete locally; v1.20 is ready for milestone audit and closeout`
Resume file: `.planning/ROADMAP.md`
