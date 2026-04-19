# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-20)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** define the next milestone after closing `v1.14 Compatibility and Quality Hardening`

## Current Position

Milestone: `None active`
Phase: `Awaiting next milestone definition`
Plan: `v1.14 is archived locally; next entrypoint is $gsd-new-milestone`
Status: `v1.14 shipped locally with accepted tech debt; roadmap collapsed and requirements archived`
Last activity: `2026-04-20` — archived v1.14 planning materials, updated project state, and prepared the workspace for the next milestone

Progress: `[████████] v1.14 complete; waiting for next milestone`

## Performance Metrics

- Latest completed milestone: `v1.14 Compatibility and Quality Hardening`
- Phases shipped in latest milestone: `4`
- Requirements satisfied in latest milestone: `12/12`
- Active milestone phases: `0`
- Active milestone requirements: `0`

## Accumulated Context

### Decisions

- `v1.7`: `SceneDocument` remains the authoritative viewer scene truth; engine sync and upload now hang off explicit delta/apply/residency services.
- `v1.7`: GPU upload only happens from frame prelude under budget, not from public scene mutation APIs.
- `v1.8`: scene residency dirtying now only moves on scene deltas, resource-epoch changes, or explicit rehydrate events; render cadence itself no longer dirties residency.
- `v1.8`: imported assets now expose shared mesh payloads, `Object3D` retains explicit payload/retention metadata, and upload budgets adapt to runtime mode plus queue pressure.
- `v1.9`: upload telemetry now survives no-op frames, viewer-scene benchmarks exist as a first-class project, and `VideraViewRuntime` delegates scene orchestration through `SceneRuntimeCoordinator`.
- `v1.10`: public alpha consumption now has a narrow minimal sample, a package-based consumer smoke path, benchmark workflow gates, and aligned feedback/support surfaces.
- `v1.11`: the public alpha story is now frozen around one canonical happy path, one diagnostics snapshot contract, and one support/release evidence path.
- `v1.12`: the next user-facing pressure should come from inspection workflows, not from another large architecture or performance cycle.
- `v1.13`: deepen inspection fidelity before widening product scope by improving hit truth, snap semantics, same-API fast paths, and support reproducibility.
- `v1.13`: measurement snap mode lives on `VideraInteractionOptions`, but persists through `VideraInspectionState` so interaction intent and saved inspection truth stay aligned.
- `v1.13`: same-API performance work landed first as cached clip-truth reuse plus preferred live snapshot readback, not as a new GPU-preview API.
- `v1.13`: replayable support artifacts live in `VideraInspectionBundleService` rather than widening `VideraView` into a larger project/session surface.
- `v1.14`: compatibility hardening beat `OpenGL` expansion; the immediate alpha risk was support truth, package-consumer evidence, backend contract drift, and quality-gate ambiguity.
- `v1.14`: packaged consumer quality gates must validate the real local-feed package path, not a raw smoke-project build against stale assumptions.

### Pending Todos

- Start the next milestone with `$gsd-new-milestone`.
- Decide whether the accepted `BACK-03` repo-guard weakness should become an immediate cleanup task or stay deferred.

### Blockers/Concerns

- Most of `.planning` still remains gitignored/local-only, while explicitly archived milestone closeout files can be force-added when a durable release boundary is needed.
- Linux compatibility risk is now better bounded, but future consumer evidence may still raise native Wayland or `OpenGL` pressure later.
- The accepted v1.14 tech debt includes a semantically weak repository guard around the “no `OpenGL` promise” contract.
- Benchmark review is now an actionable merge signal, but it is still label-gated rather than a hard numeric blocker.
- Compositor-native Wayland hosting and any future `OpenGL` evaluation remain explicitly deferred beyond this milestone.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Platform | Native compositor-hosted Wayland embedding | deferred until inspection-fidelity alpha feedback proves it is a higher-value blocker than hit truth and support reproducibility | `v1.13` |
| Packages | Publishing `Videra.SurfaceCharts.*` as public consumer packages | deferred unless strategy shifts from viewer-depth to product-width expansion | `v1.13` |
| Public surface | Plugin/package discovery or broader API-surface expansion | deferred while `VideraEngine` remains the only public extensibility root | `v1.13` |
| Editor tooling | Gizmos, transform handles, or general scene authoring | deferred to keep inspection workflows viewer-first | `v1.13` |
| Runtime perf | Another deep performance milestone beyond same-API fast paths and inspection benchmarks | deferred until new evidence identifies the next dominant bottleneck | `v1.13` |

## Session Continuity

Last session: `2026-04-20 +08:00`
Stopped at: `Milestone v1.14 archived locally; next step is to define the next milestone`
Resume file: `None`
