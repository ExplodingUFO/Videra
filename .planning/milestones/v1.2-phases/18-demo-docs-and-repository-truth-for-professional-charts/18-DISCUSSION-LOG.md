# Phase 18: Demo, Docs, and Repository Truth for Professional Charts - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-14
**Phase:** 18-Demo, Docs, and Repository Truth for Professional Charts
**Areas discussed:** Demo story surface, Limitation wording, Documentation layering, Public module boundary, Repository guard strategy

---

## Demo story surface

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal data-path demo | Keep source/viewport switching exactly as-is and rely on README text for probe/rendering/overlay behavior. | |
| Balanced product demo | Keep the independent sample and current data-path controls, but add lightweight sample UI visibility for overlays, probe gestures, renderer/fallback truth, and current-path guidance. | ✓ |
| Full diagnostics lab | Add a larger backend/debug dashboard and try to expose every renderer/cache detail in the demo itself. | |

**User's choice:** `[auto] Balanced product demo`
**Notes:** Recommended because the roadmap asks the demo to show the professional chart story, while prior phases already expose the needed control/runtime truth. The current sample shell can absorb small status/help panels without turning into a diagnostics-only app.

---

## Limitation wording

| Option | Description | Selected |
|--------|-------------|----------|
| Market intended roadmap behavior | Describe the chart as if full orbit/pan/dolly and other planned interaction work are already shipped. | |
| Describe current checkout reality | Preserve shipped claims for overlays/probe/rendering/data path, but state current interaction and Wayland limits exactly as implemented today. | ✓ |
| Strip limitation wording entirely | Focus only on positive capability claims and avoid discussing unfinished behavior. | |

**User's choice:** `[auto] Describe current checkout reality`
**Notes:** Recommended because earlier demo-truth work in Phase 8 established that user-facing status must match branch reality. Current code and READMEs still contain stale or contradictory alpha wording that Phase 18 should reconcile, not hide.

---

## Documentation layering

| Option | Description | Selected |
|--------|-------------|----------|
| Duplicate the full chart contract everywhere | Put detailed chart capability/limitation text in root README, module READMEs, demo README, and Chinese mirrors. | |
| Route from top-level docs into artifact-level READMEs | Keep detailed truth in the English module/demo READMEs, with the root README and Chinese entrypoints acting as concise summary/routing pages. | ✓ |
| Collapse everything into the root README | Make the root README the only real chart contract and reduce module pages to placeholders. | |

**User's choice:** `[auto] Route from top-level docs into artifact-level READMEs`
**Notes:** Recommended because it matches the repo's existing README-driven onboarding pattern and reduces drift. It also aligns with earlier public-doc decisions around narrow authoritative entrypoints plus exact sentence guards.

---

## Public module boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Add `Videra.SurfaceCharts.Rendering` as a public onboarding module | Promote the rendering package into the chart family’s public doc/navigation surface. | |
| Keep rendering documented through the control layer | Treat `Videra.SurfaceCharts.Rendering` as an implementation detail and describe renderer truth through `Videra.SurfaceCharts.Avalonia` plus the demo. | ✓ |
| Hide renderer truth from docs | Avoid discussing the chart-local renderer seam outside source code and tests. | |

**User's choice:** `[auto] Keep rendering documented through the control layer`
**Notes:** Recommended because the public chart family was introduced as Core/Avalonia/Processing/Demo, and the rendering project exists to support that control path without becoming a new onboarding boundary.

---

## Repository guard strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Link/file existence only | Limit repository checks to doc links, file presence, and project layout. | |
| Exact sentence/link guards plus sample behavior tests | Keep precise documentation-term guards and extend headless demo behavior checks where the sample must prove public truth. | ✓ |
| Whole-file snapshots | Snapshot large docs or XAML files to detect any wording/layout changes. | |

**User's choice:** `[auto] Exact sentence/link guards plus sample behavior tests`
**Notes:** Recommended because the repo already uses exact sentence guards effectively for chart boundary and renderer truth, and the demo already has headless behavior tests that can carry richer public-truth assertions without brittle snapshots.

---

## the agent's Discretion

- Exact wording and placement of demo-side renderer/probe/help text
- Exact split of chart truth between English module pages and top-level README summaries
- Exact repository-test additions needed to freeze Phase 18 claims

## Deferred Ideas

- Large backend diagnostics workbench for the surface-chart demo
- New camera/runtime behavior work beyond current shipped chart reality
- Promoting `Videra.SurfaceCharts.Rendering` into a first-class public module page
- Broad non-chart localization work outside this phase
