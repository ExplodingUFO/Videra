# Phase 17: Large-Dataset Residency, Cache Evolution, and Optional Rust Spike - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-04-14
**Phase:** 17-Large-Dataset Residency, Cache Evolution, and Optional Rust Spike
**Areas discussed:** Scheduler and residency policy, Cache I/O evolution, Pyramid reducers and statistics, Profiling and optional Rust seam, Verification budgets

---

## Scheduler and residency policy

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal sequential cleanup | Keep the current overview-first pipeline and only polish failure handling around the existing sequential detail walk. | |
| View-aware bounded scheduler | Keep overview availability, but prioritize detail work by the active viewport and LOD, cancel stale work, and cap parallel requests. | ✓ |
| Aggressive persistent residency | Retain large amounts of prior detail data and prefetch broadly across viewport changes. | |

**User's choice:** `[auto]` View-aware bounded scheduler
**Notes:** `[auto]` Recommended default selected. Evidence: `SurfaceTileScheduler` currently awaits detail tiles sequentially after the overview request, and `SurfaceChartController` plus `SurfaceTileCache` currently prune stale detail tiles on viewport and size changes.

---

## Cache I/O evolution

| Option | Description | Selected |
|--------|-------------|----------|
| Keep current reopen-per-tile path | Leave manifest and payload access unchanged and accept reopen, allocate, and copy work on each tile read. | |
| Persistent reader plus additive batch reads | Preserve the current format first, but add persistent payload access and additive batch-read seams behind existing contracts. | ✓ |
| Full cache-format rewrite first | Redesign the manifest and payload layout before addressing request behavior. | |

**User's choice:** `[auto]` Persistent reader plus additive batch reads
**Notes:** `[auto]` Recommended default selected. Evidence: `SurfaceCacheReader.LoadTileAsync` opens the payload file, allocates a byte buffer, and copies tile data for every request, while `SurfaceCacheWriter` already defines a stable manifest plus sidecar format worth preserving initially.

---

## Pyramid reducers and statistics

| Option | Description | Selected |
|--------|-------------|----------|
| Average only | Keep the existing reduction logic and continue deriving truth from averaged tile values. | |
| Pluggable reducers plus tile statistics | Keep average as the default, but add richer reducers and per-tile statistics such as min, max, and range or peak-preserving summaries. | ✓ |
| Push truth handling into UI only | Leave cache and pyramid data unchanged and compensate in overlays or probes later. | |

**User's choice:** `[auto]` Pluggable reducers plus tile statistics
**Notes:** `[auto]` Recommended default selected. Evidence: `SurfacePyramidBuilder` currently computes block averages only, while `SurfaceProbeService` and legend overlay paths depend on the loaded-tile truth surface.

---

## Profiling and optional Rust seam

| Option | Description | Selected |
|--------|-------------|----------|
| No Rust seam regardless of results | Keep all work in C# even if profiling shows a persistent lower-level hotspot. | |
| Measurement-gated coarse seam | Profile C# first and introduce a narrow, coarse-grained Rust helper only if a lower-level hotspot clearly justifies it. | ✓ |
| Early Rust rewrite | Start moving major processing or runtime paths into Rust during this phase. | |

**User's choice:** `[auto]` Measurement-gated coarse seam
**Notes:** `[auto]` Recommended default selected. Evidence: `.planning/PROJECT.md` already states that Rust is optional, coarse-grained, and limited to lower-level hotspots rather than UI or renderer orchestration.

---

## Verification budgets

| Option | Description | Selected |
|--------|-------------|----------|
| Demo-only proof | Rely on manual responsiveness checks and visual inspection. | |
| Deterministic tests plus fixture measurements | Add bounded-concurrency, stale-work, cache-read, and reducer-statistics verification with generated fixtures and stable measurements. | ✓ |
| Benchmark later with no contract tests | Defer proof until after implementation and avoid new regression coverage. | |

**User's choice:** `[auto]` Deterministic tests plus fixture measurements
**Notes:** `[auto]` Recommended default selected. Evidence: the existing suite already validates scheduling, cache, and pyramid behavior through xUnit plus headless Avalonia tests, so Phase 17 should extend that proof surface instead of replacing it with demo-only claims.

---

## the agent's Discretion

- Exact scheduler priority formula and queue implementation.
- Additive API names for persistent or batch cache access.
- Whether a small manifest extension is necessary after the implementation path is tested.
- The profiling harness shape and whether optional Rust work becomes a concrete subplan or a documented no-op.

## Deferred Ideas

- Compositor-native Wayland embedding or broader Linux hosting claims.
- Moving surface charts into `VideraView` or a shared render-session abstraction.
- A broad Rust rewrite of runtime, UI, scheduler, or renderer orchestration.
- New chart interaction or presentation capabilities beyond the current roadmap boundary.
- Giant checked-in benchmark datasets or demo polish as the primary proof surface.
