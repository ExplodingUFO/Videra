# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-20)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** repair the post-`v1.16` CI red line across benchmark compile, SurfaceCharts quality gate, and Linux `XWayland` consumer smoke

## Current Position

Milestone: `v1.17 修`
Phase: `93 Linux XWayland Consumer Smoke Stabilization`
Plan: `re-run the repaired XWayland smoke command on a real Linux XWayland environment and validate the runtime artifact set`
Status: `Phases 91-92 are complete locally; Phase 93 has isolated the workflow/artifact gaps and now needs real Linux XWayland runtime confirmation`
Last activity: `2026-04-20` — repaired the XWayland smoke workflow contract and added fallback failure artifacts to the smoke wrapper

Progress: `[██████□□] 6/8 requirements complete; phase 93 runtime confirmation under real XWayland is next`

## Performance Metrics

- Latest completed milestone: `v1.16 SurfaceCharts Adoption Surface` (local transition snapshot)
- Phases shipped in latest milestone: `4`
- Requirements satisfied in latest milestone: `9/9`
- Active milestone phases: `4`
- Active milestone requirements: `8`

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
- `v1.17`: the current top blocker is no longer product-surface ambiguity but the broken green line created by benchmark compile drift, SurfaceCharts analyzer debt, and the Linux `XWayland` smoke regression.
- `v1.17`: benchmark compile drift is now closed through compile-backed benchmark fixes and solution-level verification, not through brittle source-text guards.
- `v1.17`: the SurfaceCharts warnings-as-errors blocker no longer reproduces on the current workspace; the active analyzer policy and quality-gate evidence path are already green.
- `v1.17`: the XWayland consumer-smoke root cause split into a workflow/session-contract gap and an artifact-persistence gap; the remaining unknown is runtime success under a real Linux XWayland host.

### Pending Todos

- Run the repaired Linux `XWayland` consumer smoke command on a real Linux host or CI runner.
- Preserve the local `v1.16` archive snapshot while `v1.17` repairs the remaining red baseline.
- Re-run the previously failing CI-equivalent commands after each repair phase.

### Blockers/Concerns

- Most of `.planning` still remains gitignored/local-only, while explicitly archived milestone closeout files can be force-added when a durable release boundary is needed.
- This Windows workstation does not currently have a prepared Linux/XWayland runtime stack, so `SMOKE-02` still needs CI or a Linux host for final proof.
- Compositor-native Wayland hosting and any future `OpenGL` evaluation remain explicitly deferred beyond this milestone.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Platform | Native compositor-hosted Wayland embedding | deferred until inspection-fidelity alpha feedback proves it is a higher-value blocker than hit truth and support reproducibility | `v1.13` |
| Packages | Publishing `Videra.SurfaceCharts.*` as public consumer packages | deferred unless strategy shifts from viewer-depth to product-width expansion | `v1.13` |
| Public surface | Plugin/package discovery or broader API-surface expansion | deferred while `VideraEngine` remains the only public extensibility root | `v1.13` |
| Editor tooling | Gizmos, transform handles, or general scene authoring | deferred to keep inspection workflows viewer-first | `v1.13` |
| Runtime perf | Another deep performance milestone beyond same-API fast paths and inspection benchmarks | deferred until the current green-line repair is complete and new evidence identifies the next dominant bottleneck | `v1.13` |
| Analyzer policy | Whole-repo warnings-as-errors expansion beyond the active repair scope | deferred until `v1.17` closes the current targeted SurfaceCharts debt | `v1.17` |

## Session Continuity

Last session: `2026-04-20 +08:00`
Stopped at: `Phases 91-92 complete; Phase 93 XWayland smoke stabilization is next`
Resume file: `.planning/ROADMAP.md`
