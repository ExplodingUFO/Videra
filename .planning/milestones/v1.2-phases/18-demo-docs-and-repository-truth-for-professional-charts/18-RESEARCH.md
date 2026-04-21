# Phase 18 Research: Demo, Docs, and Repository Truth for Professional Charts

**Phase:** 18  
**Name:** Demo, Docs, and Repository Truth for Professional Charts  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> How do we make the professional surface-chart slice externally truthful across the independent demo, English and Chinese docs, and repository guards, without re-opening the chart architecture or overclaiming unfinished interaction work?

## Current Code Reality

### 1. The independent demo exists and proves the data path, but it still under-explains shipped chart behavior

`samples/Videra.SurfaceCharts.Demo` is already the right public entrypoint for the chart family:

- it is independent from `VideraView`
- it switches between in-memory and cache-backed sources
- it exposes overview/detail viewport presets
- it hosts `SurfaceChartView` directly

What it does not yet do clearly enough is explain the chart behaviors that are now already shipped in the control:

- axis and legend overlays
- hover readout and pinned probes
- `Shift + LeftClick` pinning
- control-visible renderer and fallback truth via `RenderingStatus` / `RenderStatusChanged`
- the current residency/cache-backed data-path story

So the missing work is not "build a chart demo from scratch." It is "make the current demo legible as the public truth surface for the chart slice."

### 2. English docs already contain much of the truth, but the chart story is spread unevenly

The module/demo READMEs are ahead of the root entrypoint in several important ways:

- `src/Videra.SurfaceCharts.Avalonia/README.md` already documents the chart-local renderer seam, `GPU-first` behavior, explicit fallback truth, and `RenderingStatus`
- `samples/Videra.SurfaceCharts.Demo/README.md` already positions the sample as an independent chart demo instead of a `VideraView` mode
- `src/Videra.SurfaceCharts.Processing/README.md` already carries the Phase 17 data-path truth around cache sessions, ordered batch reads, statistics, and the optional native seam

The root README still acts as the most likely public entrypoint, so if its summary language lags behind the module pages, the repository ends up presenting two different chart stories.

### 3. Chinese mirrors still lag behind the shipped overlay, probe, and data-path reality

`docs/zh-CN/README.md` and the Chinese module pages still carry older surface-chart language in places. The main drift areas are:

- older wording that implies axes and legend are still unfinished
- missing explicit `RenderingStatus` / `RenderStatusChanged` truth
- missing hover and pinned probe guidance
- missing or incomplete Phase 17 data-path wording for persistent payload sessions, ordered batch reads, and `SurfaceTileStatistics`

The project already treats English as authoritative, but the Chinese mirrors are still expected to preserve the same public boundary and shipped capability truth. Phase 18 is the place to close that gap.

### 4. Repository guards already freeze part of the chart story, but not the whole outward-facing contract

The repo already has the right verification pattern:

- `SurfaceChartsRepositoryArchitectureTests` freezes boundary and renderer-truth language
- sample tests freeze that the independent demo exists and stays separate from `VideraView`
- processing/readme guards already protect some Phase 17 truth

What is still missing is a single repository-level guard surface that ties together:

1. the root README summary
2. the Chinese routing page and module mirrors
3. the demo UX text that exposes renderer/probe/overlay truth

That gap is why stale phrases can survive even when the code has already moved on.

### 5. The public chart boundary is still Core/Avalonia/Processing/Demo, not Rendering as a new module family

Phase 16 introduced `Videra.SurfaceCharts.Rendering` as a chart-local implementation layer, but the public onboarding boundary did not change. The public chart story is still:

- `Videra.SurfaceCharts.Core`
- `Videra.SurfaceCharts.Avalonia`
- `Videra.SurfaceCharts.Processing`
- `Videra.SurfaceCharts.Demo`

Phase 18 should explain renderer truth through Avalonia and the demo instead of promoting `Rendering` into a new top-level public onboarding target.

### 6. This phase is truth closure, not feature expansion

The repository already shipped the key architectural and behavioral changes in Phases 14-17. Phase 18 should therefore avoid turning into a stealth feature phase.

This phase should not:

- add a new renderer architecture
- redesign cache or scheduler internals
- claim finished orbit / pan / dolly interaction
- claim compositor-native Wayland support
- broaden localization beyond the chart entrypoints that need truthful parity

## Design Options

### Demo strategy

#### Option A: Leave the current sample UI mostly unchanged and rely on README text

This is the smallest editing path, but it keeps the most user-visible proof surface under-explaining the shipped chart behaviors.

**Conclusion:** Reject

#### Option B: Keep the current independent sample, but add lightweight product-facing truth panels

This means preserving the existing source/viewport flow and extending the sample UI with concise, visible chart truth:

- renderer/fallback status
- probe workflow guidance
- axis/legend availability
- current host-driven limitation wording

This matches the phase goal and reuses existing demo behavior tests.

**Conclusion:** Recommended

#### Option C: Turn the sample into a full diagnostics cockpit

This could surface every backend field and every data-path metric, but it is too large and too debug-oriented for the public onboarding role this sample actually plays.

**Conclusion:** Defer

### Documentation layering

#### Option A: Duplicate the full chart contract in every README and entrypoint

This maximizes visibility but also maximizes drift risk.

**Conclusion:** Reject

#### Option B: Keep detailed truth in module/demo READMEs and make root/Chinese entrypoints summarize and route

This matches the current repo pattern and keeps the detailed contract nearest to the actual shipping artifacts.

**Conclusion:** Recommended

#### Option C: Move all chart truth to the root README alone

This would simplify the entrypoint story, but it would weaken artifact-level documentation and make the root page too heavy.

**Conclusion:** Reject

### Verification strategy

#### Option A: Rely on manual doc review

That is too weak for wording-sensitive truth work.

**Conclusion:** Reject

#### Option B: Use exact guards for wording plus headless sample behavior tests

This matches the repository's existing verification pattern and keeps the tests deterministic.

**Conclusion:** Recommended

#### Option C: Add whole-file snapshot tests for all chart docs

This would be noisy and brittle for a phase whose goal is precise sentence-level truth, not full-document immutability.

**Conclusion:** Reject

## Recommended Architecture

### 1. Treat the demo as the public behavior surface for the chart slice

The demo should stay independent and continue showing source switching plus viewport presets, but it should also project the shipped chart behaviors that already exist in `SurfaceChartView`:

- renderer/fallback truth
- probe workflow
- axis/legend availability
- current host-driven limits

### 2. Keep module/demo READMEs authoritative and use root/Chinese entrypoints as routing layers

Detailed chart truth belongs in:

- `samples/Videra.SurfaceCharts.Demo/README.md`
- `src/Videra.SurfaceCharts.Avalonia/README.md`
- `src/Videra.SurfaceCharts.Core/README.md`
- `src/Videra.SurfaceCharts.Processing/README.md`

The root README and `docs/zh-CN/README.md` should summarize the chart story and route readers to those pages.

### 3. Keep rendering truth behind the Avalonia public story

Renderer internals matter, but the public explanation should still hang from the control that hosts them. That preserves the Phase 16 architecture boundary and avoids onboarding readers into an implementation-detail package.

### 4. Freeze the final story through deterministic repository guards

Phase 18 is fundamentally a truth-freeze phase. That means:

- exact sentence guards for root/demo/module pages
- exact term guards for Chinese parity
- headless demo behavior tests for visible UI truth
- no screenshot-only proof and no whole-file snapshots

### 5. Split the work into two content slices plus one guard slice

The smallest coherent plan shape is:

1. demo UX truth plus focused behavior tests
2. English and Chinese doc alignment
3. repository guards that freeze the final story

## Risks

### 1. Overclaiming interaction maturity

Mitigation: keep limitation wording explicit that the demo currently uses host-driven overview/detail presets and does not ship a finished orbit/pan/dolly workflow.

### 2. Spreading truth across too many entrypoints

Mitigation: keep detailed contract pages small in number and route to them instead of duplicating full prose everywhere.

### 3. Accidentally promoting rendering internals into a public module family

Mitigation: document renderer truth through Avalonia and the demo, and keep repository guards aligned to the existing public boundary.

### 4. Writing weak or brittle guards

Mitigation: guard exact high-signal phrases and user-visible demo behavior, not whole files or speculative future wording.

## Recommendation

Phase 18 should be planned as three tightly scoped slices:

1. make the independent demo visibly truthful about renderer/probe/overlay behavior
2. align English and Chinese chart docs to the shipped boundary and limitation language
3. lock that story through deterministic repository guards and sample tests

That closes the outward-facing chart truth without reopening the architecture work that earlier phases already finished.
