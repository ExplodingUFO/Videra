# Retrospective

## Milestone: v1.20 — Viewer Product Boundary and Core Slimming

**Shipped:** 2026-04-21  
**Phases:** 4  
**Plans:** 12

### What Was Built

- A canonical `1.0` capability/package-layer boundary that now defines Videra as a native desktop viewer/runtime plus inspection and source-first charts
- A slimmer `Videra.Core` with importer and concrete logging-provider dependencies extracted into explicit packages and composition seams
- Canonical hosting-boundary docs plus repository guards that keep `Core`, `Import`, `Backend`, `UI adapter`, and `Charts` independently explainable
- End-to-end package truth across English/Chinese docs, consumer smoke, package validation, CI, and public-release workflows

### What Worked

- Doing the boundary work before broader runtime feature work prevented another milestone from landing on a blurry core/package story.
- Treating docs, smoke paths, package validation, and publish workflows as product-boundary code kept the milestone grounded in shipped truth instead of repo folklore.
- The retroactive verification pass was straightforward once the phase scope was kept small and tied to concrete tests/scripts rather than invented new abstractions.

### What Was Inefficient

- Phase `104` landed in code without its `.planning/phases/104-*` directory, so closeout needed a reconstruction pass before audit could run cleanly.
- `audit-open` still surfaces historical verification debt and one quick-task metadata gap during unrelated milestone closeouts, so process debt remains noisier than it should be.
- Because `.planning` is local-first, isolated worktrees need an explicit `.planning` snapshot copy before closeout work can proceed.

### Patterns Established

- Product/package-boundary milestones are highest leverage when they bind docs, tests, smoke paths, and workflows to the same story instead of treating those as downstream polish.
- If a package split is the feature, the verification surface should be consumer smoke plus repository/package guards, not just project-file diffs.
- Retroactive verification is tractable when phase work stayed narrow and evidence was captured through stable tests/scripts.

### Key Lessons

- Videra's next leverage point is not more boundary cleanup; it is deeper runtime/product capability built on the now-stable viewer-first package line.
- Missing phase artifacts are cheaper to repair immediately than to rediscover at milestone-close time.
- Keeping milestone tags version-aligned with real release tags avoids creating a second, confusing version line for local planning closeouts.

### Cost Observations

- Model profile: balanced
- Sessions: one closeout-focused pass with retroactive verification backfill, milestone audit, and archive normalization
- Notable: the milestone created more value by making the existing product boundary honest and enforceable than by adding another outward-facing runtime feature

## Milestone: v1.13 — Inspection Fidelity

**Shipped:** 2026-04-19  
**Phases:** 4  
**Plans:** 12

### What Was Built

- Mesh-accurate inspection hit truth with richer hit records, per-mesh acceleration, and measurement anchors that now start from surface truth instead of bounds-only truth
- Viewer-first measurement snap modes with persisted snap intent through `VideraInspectionState` and sample-visible switching
- Same-API inspection performance work through cached clip-truth reuse, preferred live snapshot readback, and dedicated `InspectionBenchmarks`
- Replayable inspection bundles plus aligned docs/support/sample/consumer-smoke truth around one support artifact

### What Worked

- Sequencing the milestone as `mesh hit truth -> snap semantics -> same-API fast paths -> replayable bundle/docs closure` kept the work narrow and avoided slipping into another architecture rewrite.
- Reusing the existing public inspection seams let fidelity improve materially without widening `VideraView` or reopening the extensibility story.
- Consumer smoke, benchmark dry runs, and repository guards made it practical to prove supportability and outward contract alignment in the same loop.

### What Was Inefficient

- `.planning` still needed an explicit reconciliation pass after the code landed; implementation finished before milestone-close artifacts were normalized.
- The upstream `gsd-tools audit-open` command is currently broken in this environment, so the milestone audit needed a manual synthesis path.
- Explicit multi-backend epsilon proof is still lighter than the requirement wording because no dedicated comparison harness exists yet.

### Patterns Established

- When a viewer workflow already exists, the highest-value follow-up is often to deepen its truth and reproducibility before adding adjacent feature surface.
- Support quality improves fastest when the same public workflow also generates the reproduction artifact consumers/support actually exchange.
- Same-API performance work is a good fit for alpha hardening when it preserves deterministic fallback and benchmark evidence stays visible.

### Key Lessons

- Improving internal inspection truth can deliver more product value than adding another outward-facing control when the gap is trust, not discoverability.
- A dedicated support artifact like the inspection bundle reduces ambiguity immediately, but it also forces runtime, sample, docs, and smoke paths to stay aligned.
- Local-only planning means milestone closeout and repository release state remain intentionally separate; that boundary needs to be stated explicitly every time.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop plus a manual milestone-audit/archive reconciliation pass
- Notable: the highest leverage came from deepening inspection truth without widening the public surface

## Milestone: v1.12 — Viewer-First Inspection Workflow

**Shipped:** 2026-04-18  
**Phases:** 4  
**Plans:** 12

### What Was Built

- Typed clipping, measurement, inspection-state capture/restore, and snapshot export on the public `VideraView` inspection surface
- Runtime/overlay/interaction wiring that keeps clipping and measurement truth consistent across scene rendering, overlays, diagnostics, and exported snapshots
- Inspection-aware diagnostics snapshot output plus interaction sample, consumer smoke, and public docs that all demonstrate the same workflow
- Focused inspection integration tests and repository guards that keep the new user-visible workflow from drifting

### What Worked

- Sequencing the milestone as `clipping -> measurement -> inspection-state/export -> docs/sample/validation closure` kept the work viewer-first and stopped it from turning into another architecture milestone.
- Reusing the existing scene/runtime seams let the new features pressure real boundaries without widening the public extensibility surface.
- Consumer smoke and repository guards caught final documentation drift quickly, including the need to keep public plan docs free of internal agent instructions.

### What Was Inefficient

- `.planning` was still at milestone-init state after the code landed, so local closeout needed an explicit reconciliation pass at the end.
- `verify.ps1` remains broad relative to the narrow inspection surface touched in this milestone.
- Public plan docs under `docs/plans/` can still accidentally pick up internal execution phrasing unless they are reviewed as part of repository guards.

### Patterns Established

- Once the public alpha path is stable, the highest-leverage next step is a viewer-first workflow that creates real user value and exercises existing seams.
- Inspection features are a good pressure test for scene/runtime/overlay boundaries because they are visible to users without dragging the product toward editor semantics.
- Support quality improves when diagnostics, exported artifacts, sample UI, and smoke validation all ask for the same reproduction truth.

### Key Lessons

- Clipping and measurement can stay viewer-first as long as the public contract is host-owned state, not engine-level editing hooks.
- Snapshot export is most useful when it shares the same truth as on-screen overlays; anything less creates support ambiguity immediately.
- Documentation guardrails need to catch internal agent/playbook language in user-visible engineering notes, not just product-facing README files.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the milestone produced user-visible workflow value without reopening large runtime architecture work

## Milestone: v1.11 — Alpha Happy-Path Stabilization and Diagnostics Productization

**Shipped:** 2026-04-18  
**Phases:** 4  
**Plans:** 12

### What Was Built

- A single canonical alpha onboarding path across root docs, Avalonia docs, and the minimal sample
- `VideraDiagnosticsSnapshotFormatter` plus minimal-sample copy/export flow and consumer-smoke snapshot artifact generation
- Support surfaces that ask for the same reproduction anchors and diagnostics snapshot instead of freeform environment summaries
- Benchmark and release guidance tied to the same consumer smoke path and trend-oriented evidence story

### What Worked

- Sequencing the work as `happy path -> diagnostics snapshot -> support closure -> benchmark/release alignment` kept the milestone on external adoption friction instead of drifting back into deeper runtime work.
- Reusing `BackendDiagnostics` as the canonical runtime truth made the diagnostics snapshot cheap to add without widening the public API.
- Repository guards caught drift quickly, including the missing `LoadModelsAsync(...)` atomic-replace wording that would otherwise have left docs inconsistent with the new support story.

### What Was Inefficient

- The milestone started from a fresh `v1.11` init while the local phase artifact history still reflected the earlier `64-67` consumer-integration work, so local planning closeout needed an explicit reconciliation pass.
- `verify.ps1` is still broad relative to the narrow code/docs surface touched in this milestone.
- `.planning` remains local-only, so the closeout still lives outside immutable repository history.

### Patterns Established

- After the core runtime is healthy, the next leverage point is not more engine work; it is reducing ambiguity in the public happy path and support contract.
- Diagnostics become much more useful once they are a copy-paste artifact instead of an instruction to manually inspect multiple runtime fields.
- Benchmark stewardship is most valuable when it stays visibly connected to release and smoke validation, not when it becomes an isolated performance ritual.

### Key Lessons

- Compatibility entrypoints can stay supported, but they should not appear in the first-screen public narrative for an alpha package.
- Support docs, issue templates, sample UI, and consumer smoke should all ask for the same artifact; otherwise early external reports fragment immediately.
- Trend-oriented benchmark language is the right alpha posture until there is enough history to justify hard thresholds.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the highest leverage came from tightening adoption and support surfaces rather than adding any new core rendering feature

## Milestone: v1.10 — Alpha Consumer Integration and Feedback Loop

**Shipped:** 2026-04-17  
**Phases:** 4  
**Plans:** 12

### What Was Built

- `Videra.MinimalSample` and a tightened happy-path `VideraView` onboarding story across root docs and Avalonia docs
- A package-only `Videra.ConsumerSmoke` app plus local/script/workflow validation from install to first scene
- Workflow-visible viewer and surface-chart benchmark gates with retained artifacts and documented interpretation
- Alpha-facing feedback/support surfaces that ask for diagnostics, install-path, and truthful host-boundary information

### What Worked

- Sequencing the milestone as `happy path -> consumer smoke -> benchmark gates -> feedback surfaces` kept the work focused on external adoption friction instead of slipping back into another internal optimization loop.
- The new repository guards made it practical to keep minimal sample, package smoke, benchmark docs, and support templates telling the same story.
- Reusing the existing runtime diagnostics shell meant consumer smoke and feedback surfaces could ask for concrete evidence instead of inventing a second reporting path.

### What Was Inefficient

- The currently published `0.1.0-alpha.1` public packages lag the latest happy-path API surface, so smoke validation had to pack current public packages locally instead of validating against the stale published alpha build.
- `.planning` closeout is still local-only, so milestone archival remains separate from repository history.
- `verify.ps1` still proves the whole repository even when milestone work is concentrated in docs, smoke, and workflow surfaces.

### Patterns Established

- Once a runtime is technically healthy enough, the next leverage point is consumer-path clarity, not one more internal cleanup pass.
- A minimal sample is far more valuable than another broad demo when the public goal is time-to-first-scene.
- Benchmark evidence only becomes durable once it has a script, a workflow, published artifacts, and a short interpretation guide.

### Key Lessons

- Default onboarding and advanced extensibility need to be intentionally separated; otherwise the public package story always reads like an internal-engine story.
- Consumer smoke should validate the actual package-consumer boundary, even if that means packing current packages locally while the published alpha trail catches up.
- Feedback surfaces are most useful when they ask for diagnostics and reproduction anchors that already exist elsewhere in the repository.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the highest leverage came from reducing external integration ambiguity rather than adding new core runtime capability

## Milestone: v1.9 — Scene Performance Evidence and Coordinator Cleanup

**Shipped:** 2026-04-17  
**Phases:** 4  
**Plans:** 12

### What Was Built

- Richer scene upload telemetry projected through `BackendDiagnostics`, including bytes, failures, latency, and resolved budgets
- A dedicated `Videra.Viewer.Benchmarks` harness for import, residency apply, upload drain, and load/rebind evidence
- `SceneRuntimeCoordinator` as the new internal home of scene publication, enqueue, rehydrate, and diagnostics flow
- XML docs plus repository-guarded quick-start vocabulary for the public viewer scene/load/camera flow

### What Worked

- Sequencing the milestone as `telemetry -> benchmarks -> coordinator extraction -> onboarding docs` kept the work evidence-driven and prevented another inference-based performance rewrite.
- The existing `Videra.Avalonia.Tests` and scene integration suites were already strong enough to validate the coordinator extraction without widening the public API.
- Using repository guards for both benchmark presence and doc vocabulary kept the new evidence/onboarding surfaces honest.

### What Was Inefficient

- `.planning` closeout is still local-only, so milestone archival remains separate from repository history.
- `verify.ps1` is still the broad final proof point even when the milestone work is concentrated in viewer scene/runtime seams.
- Tests still keep a small amount of compatibility coupling to runtime internals because the coordinator extraction was intentionally incremental rather than a full public/internal seam rewrite.

### Patterns Established

- Once scene residency and upload policy become explicit, the next leverage point is observability, not another blind optimization pass.
- Benchmark projects should be first-class repository citizens when a subsystem becomes performance-sensitive enough to guide future scope.
- Coordinator extraction is easier and safer after telemetry and tests already explain the subsystem's behavior.

### Key Lessons

- No-op frames can silently erase the last useful upload telemetry unless diagnostics deliberately preserve the last meaningful result.
- A benchmark harness is most valuable when paired with explicit contract tests that say what queue/budget behavior is supposed to mean.
- XML docs and shared onboarding vocabulary are worth doing after architecture stabilizes; doing them earlier would have documented moving targets.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the highest leverage came from making performance behavior measurable before choosing any deeper optimization direction

## Milestone: v1.8 — Scene Residency Efficiency and Mesh Payload Optimization

**Shipped:** 2026-04-17  
**Phases:** 4  
**Plans:** 12

### What Was Built

- Event-driven scene residency transitions so render cadence no longer globally dirties or requeues upload candidates each frame
- Queue-aware heuristic upload budgeting that tightens interactive drain and expands steady-state drain only when backlog pressure justifies it
- Shared `MeshPayload` semantics across imported assets and deferred objects, plus explicit `Object3D` retention metadata
- Focused residency, queue, payload, and recovery tests layered onto the existing runtime/integration verification stack

### What Worked

- Sequencing the work as `dirty semantics -> payload sharing -> budget heuristics -> validation` kept the optimization scope small and prevented performance changes from reopening public-surface or scene-truth questions.
- The `Videra.Avalonia.Tests` harness from `v1.7` paid off again by making residency and upload-queue behavior cheap to assert without expanding the demo.
- Reusing the existing scene delta/residency/upload architecture meant the milestone mostly tightened semantics instead of inventing another orchestration layer.

### What Was Inefficient

- `verify.ps1` remains the broad final proof point even when the real work is concentrated in a small set of runtime and payload seams.
- `.planning` closeout is still local-only, so milestone archive truth remains separate from repository history.
- `Object3D` now shares payload instead of duplicating arrays, but more aggressive CPU-mesh release or native preprocess work still needs a later benchmark-driven milestone.

### Patterns Established

- Once a runtime already has explicit residency and upload services, the next performance wins often come from tightening state transitions rather than adding more orchestration layers.
- Queue-aware budgeting is a good first step; hardware telemetry should stay deferred until simple heuristics stop being enough.
- Payload sharing belongs in the asset/object seam, not inside the render/session seam.

### Key Lessons

- The biggest regression risk in upload optimization was not throughput, but accidentally letting steady-state render cadence retrigger work that should have remained resident.
- Shared CPU payload semantics are much easier to land after scene truth, residency, and rehydration are already explicit.
- Focused runtime tests are enough to safely evolve internal performance contracts without dragging broader demos or public APIs into the milestone.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the milestone produced clear performance/correctness wins without expanding the public viewer surface

## Milestone: v1.7 — Scene Pipeline Closure v1

**Shipped:** 2026-04-17  
**Phases:** 9  
**Plans:** 27

### What Was Built

- A dedicated Avalonia/runtime test harness for scene publication, residency, upload queue, and import-service behavior
- Stable `SceneDocument` identity/version/ownership semantics plus incremental `Items` mutation
- Explicit scene delta planning, engine application, residency tracking, budgeted upload queue draining, and backend dirty-reupload recovery
- A narrow Scene Pipeline Lab, diagnostics shell, docs, and repository guards that all describe the same internal scene-pipeline truth

### What Worked

- Sequencing the milestone as `tests -> asset metadata -> document contract -> incremental mutation -> delta/apply -> upload queue -> recovery -> import service -> lab/docs truth` prevented later phases from reopening scene ownership questions.
- The new `Videra.Avalonia.Tests` project paid off immediately by making runtime-scene changes cheap to validate without bloating demos or only depending on broad integration suites.
- Keeping upload work on the render/device cadence instead of public API paths made the runtime architecture cleaner and the recovery story more obviously correct.

### What Was Inefficient

- `.planning` closeout still lives outside normal git history, so milestone archival continues to diverge from repository release boundaries.
- `verify.ps1` is still the final proof point even when the main work is concentrated in runtime-scene seams and dedicated targeted suites.
- Shared CPU mesh payload / copy-reduction work was intentionally left out, so some imported-scene memory inefficiency remains visible after the contract work landed.

### Patterns Established

- Scene pipelines stay maintainable only when document truth, delta planning, residency state, and upload cadence are explicit separate concerns.
- A queue-driven frame-prelude upload path is easier to reason about than API-path synchronous upload once the runtime already has invalidation-driven render scheduling.
- Narrow labs plus repository guards are a better proof surface than broad demos when the real goal is to validate a contract change.

### Key Lessons

- The dedicated Avalonia/runtime test harness was worth adding before deeper scene refactors; waiting longer would have forced more integration-only debugging.
- Recovery correctness depends on treating old GPU buffers as stale after device changes, even when those buffers are still non-null on the object.
- Import and upload need to stay separate responsibilities all the way down; otherwise the runtime quickly regresses into mixed policy and device work in one path.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the highest leverage came from making scene realization explicit and queue-driven rather than adding more viewer surface area

## Milestone: v1.6 — Scene Pipeline Truth and Backend Surface Closure

**Shipped:** 2026-04-17  
**Phases:** 5  
**Plans:** 15

### What Was Built

- An authoritative `SceneDocument` scene contract with document-first viewer mutations and retained imported-asset truth
- A backend-neutral import pipeline with deferred upload, bounded-parallel batch import, and atomic replace semantics
- Runtime rehydration from retained scene assets after backend or surface rebind, without defaulting to software upload as the steady-state story
- Direct built-in backend support for `IGraphicsDevice` / `IRenderSurface`, plus a narrow Scene Pipeline Lab and aligned repository/docs truth

### What Worked

- Sequencing the milestone as `scene owner -> import/upload split -> rehydration -> backend migration -> lab/docs truth` kept the contract coherent and prevented later phases from reopening ownership questions.
- Existing viewer integration tests were strong enough to drive the scene-pipeline closure without guessing about runtime behavior.
- Treating the demo/docs work as contract proof instead of UI expansion kept the last phase small and high-signal.

### What Was Inefficient

- `.planning` closeout remains local-only, so milestone archival still diverges from normal git history.
- Full `verify.ps1` is still the final proof point even when the milestone mainly changes viewer/runtime/backend seams.
- The migration still needed compatibility handling for custom or non-migrated legacy backends, so the adapter cannot disappear entirely yet.

### Patterns Established

- If a runtime keeps both document truth and engine truth, the document has to win decisively before rebind or backend-migration work becomes trustworthy.
- Import should produce CPU-side truth first; upload should be an explicit runtime/backend step, not a hidden side effect of parsing a file.
- Narrow validation labs are more valuable than broad demos when the real task is to prove a new architectural contract.

### Key Lessons

- Authoritative scene ownership made the backend-v2 and rehydration work simpler; trying to finish the backend seam first would have locked in more ambiguity.
- Built-in backend migration can stay incremental as long as the repository clearly distinguishes direct device/surface implementations from compatibility-only adapters.
- Demo/docs truth belongs in the same milestone as internal contract changes whenever the new behavior would otherwise be hard to discover or easy to overclaim.

### Cost Observations

- Model profile: balanced
- Sessions: one autonomous implementation/verification/closeout loop after milestone initialization
- Notable: the highest leverage came from closing the last transition seams, not from adding more viewer surface area

## Milestone: v1.5 — VideraView Runtime Thinning and Render Orchestration

**Shipped:** 2026-04-17  
**Phases:** 7  
**Plans:** 21

### What Was Built

- `VideraViewRuntime` as the internal coordinator behind the public `VideraView` shell
- Invalidation-driven render scheduling with interactive frame leases instead of a permanent ready-state loop
- Runtime-owned `SceneDocument` truth plus backend-neutral import/upload separation
- Core interaction, overlay, graphics device/surface, and engine-subsystem seams that keep the public extensibility contract unchanged

### What Worked

- Sequencing the milestone as `shell -> scheduling -> scene -> interaction -> overlay -> graphics -> engine` kept later refactors from reopening shell thickness or public-surface questions
- Existing integration and repository tests provided enough behavioral truth to refactor internals aggressively without guessing
- Using a compatibility adapter for graphics v2 established a future-facing seam without dragging all native backends into the same PR-sized change

### What Was Inefficient

- The milestone still required local planning closeout because `.planning` remains outside the normal git history path
- Full `verify.ps1` remained the final proof point even when the milestone mainly touched viewer/runtime architecture rather than broader product scope
- A few repository guards still assumed older shell/bridge placement and had to be updated as part of the final truth alignment

### Patterns Established

- Thin-shell viewer work should keep public API fixed and move orchestration inward before touching deeper engine/backend seams
- Invalidation-driven scheduling is easiest to land once there is a dedicated runtime coordinator instead of a thick control shell
- Internal abstraction upgrades are cheaper when they ship behind compatibility adapters and repository guards that keep docs/tests synchronized

### Key Lessons

- `VideraEngine` can stay the only public extensibility root even while its internals split substantially, as long as repository docs/tests enforce that boundary
- Scene ownership and overlay math both benefit from moving toward Core services once runtime coordination is centralized
- Repository truth matters for architectural work; docs and guards must move with the code or the milestone is not actually complete

### Cost Observations

- Model profile: balanced
- Sessions: one dense autonomous implementation/verification/closeout loop
- Notable: the highest leverage came from preserving public compatibility while reducing internal coupling

## Milestone: v1.4 — Open Source Consumption and Release Surfaces

**Shipped:** 2026-04-16  
**Phases:** 6  
**Plans:** 18

### What Was Built

- A newcomer-facing README plus package/support matrix docs that separate public packages, source-only modules, samples, and demos
- Truthful public-vs-preview package publishing workflows and a release-policy / releasing-doc pair
- Complete public-package metadata through icon, SourceLink, deterministic settings, and symbol-package output
- GitHub issue/discussion/security routing, release-note categories, and weekly Dependabot maintenance automation

### What Worked

- Driving the work through repository guards first kept docs, workflows, and GitHub metadata from drifting independently
- The milestone stayed tightly scoped because it treated public-entry truth as product work instead of as miscellaneous cleanup
- Package validation script changes paid off immediately by catching incomplete pack evidence during local verification

### What Was Inefficient

- Full `verify.ps1` was still the final proof point even though only repo/docs/workflow/package surfaces changed
- Closeout remains a local planning operation because `.planning` is outside normal git history
- The repo can configure the public nuget.org path, but actual external publication still depends on secrets and the next tagged release

### Patterns Established

- For open-source standardization work, repository tests are the fastest way to keep README, docs, workflows, and GitHub metadata aligned
- Feed truth should be expressed as one coherent system: docs, workflows, package metadata, and release policy together
- Source-only modules should stay explicit source-only until a later milestone deliberately expands the public support contract

### Key Lessons

- README structure matters more than directory cosmetics when the main gap is newcomer comprehension
- Package metadata and release assets are part of the consumer surface, not optional polish
- Collaboration routing and maintenance automation belong in the same milestone as release truth because users experience them as one public interface

### Cost Observations

- Model profile: balanced
- Sessions: one dense implementation/verification/closeout loop completed in a single day
- Notable: the highest leverage came from aligning public truth surfaces, not from touching renderer or chart internals

## Milestone: v1.3 — Camera-True Surface Charts

**Shipped:** 2026-04-16  
**Phases:** 6  
**Plans:** 18

### What Was Built

- A unified chart-view spine through `SurfaceViewState`, `SurfaceCameraFrame`, shared projection math, and true 3D picking
- Camera-aware LOD/request planning, slimmer GPU/software resident contracts, and backend-owned color remap behavior
- Professional overlay behavior through `OverlayOptions`, grid-plane and axis-side selection, minor ticks, legend/axis formatter customization, and dense-label culling
- A consistent demo/docs/repository-guard story for the full camera-true surface-chart stack

### What Worked

- Sequencing the milestone as `camera/projection -> picking -> LOD -> GPU slimming -> color remap -> overlays` kept the math spine coherent and prevented rework
- Test-first work on picking, LOD, and overlay behavior forced public contracts to stay honest instead of becoming architecture-only abstractions
- Repository guards continued to be the fastest way to keep code, demo, English docs, and Chinese docs aligned

### What Was Inefficient

- Closeout is still a local planning operation because `.planning` remains outside normal git history, so milestone archive state and repository release state still diverge
- Some verification cost repeated across phases because the repo does not yet have a narrower reusable surface-chart milestone verification entrypoint
- Hosted CI evidence still points at the last pushed `master`, while the local milestone closeout carries newer unpushed planning/code state

### Patterns Established

- When a product slice depends on geometric truth, unify the shared math spine before optimizing GPU/backend details
- Keep chart-specific customization in chart-local option surfaces instead of leaking it back into shared viewer abstractions
- Treat demo panels and repository text guards as part of the shipped contract, not after-the-fact documentation

### Key Lessons

- Correctness-first sequencing produced more leverage than early performance work; the later GPU/color-map changes were simpler after the camera/picking/LOD spine was stable
- A professional overlay baseline needs both layout behavior and host-facing customization to be credible
- Future processing/native work should stay evidence-gated; after `v1.3`, the biggest unknown is hotspot value, not architectural direction

### Cost Observations

- Model profile: balanced
- Sessions: one milestone completed in a dense single-day execution and closeout loop
- Notable: the highest leverage again came from making runtime, rendering, overlay, demo, docs, and planning artifacts tell the same story

## Milestone: v1.2 — Professional Surface Charts

**Shipped:** 2026-04-16  
**Phases:** 8 delivered (`15-22`)  
**Plans:** 24

### What Was Built

- Adaptive axes / legend / probe overlays, GPU-first chart rendering, and large-dataset scheduler / cache evolution for the professional chart baseline
- Recovered `SurfaceViewState` / `SurfaceChartRuntime` and built-in interaction / camera workflow contracts that the original milestone definition expected
- Post-audit cleanup on summary metadata, superseded verification wording, and live scheduler consumption of ordered batch tile reads
- A consistent demo / docs / repository-guard story for the shipped chart boundary and behavior

### What Worked

- Treating the milestone audit as a real engineering gate exposed false completion state early enough to recover it without rewriting shipped truth
- Repository guards continued to be the fastest way to keep code, demo, English docs, and Chinese docs aligned
- Running Phase 22 test-first prevented the batch seam from remaining a “paper capability” that the live scheduler never actually used

### What Was Inefficient

- Original Phase `13` / `14` completion state was wrong and forced recovery phases `19` / `20`
- Closeout tooling was incomplete enough that milestone normalization and archival had to be done manually
- The repository never reached a clean tagged release point during closeout, so planning completion and git-release completion diverged

### Patterns Established

- Preserve superseded audit history instead of rewriting it away; recovery work should be explicit and traceable
- Use cleanup-only phases when requirement scope is already closed but auditability or operational debt is still real
- Treat optional batch or native seams as unfinished until the live path actually consumes them under tests

### Key Lessons

- A passed audit needs delivered-behavior evidence, not just the presence of phase artifacts
- Summary metadata is part of the milestone contract because it feeds future traceability and audit automation
- Milestone closeout and repository release hygiene are related but distinct concerns; both need explicit truth

### Cost Observations

- Model profile: balanced
- Sessions: multi-session recovery, cleanup, re-audit, and archival cycle completed across two calendar days
- Notable: the biggest leverage again came from correcting truth and ownership boundaries, not from adding unrelated feature surface

## Milestone: v1.1 — Render Pipeline Architecture

**Shipped:** 2026-04-09  
**Phases:** 4  
**Plans:** 13

### What Was Built

- Explicit render-pipeline stage vocabulary, frame-plan modeling, and pipeline snapshots
- Host-agnostic `RenderSessionOrchestrator` and `VideraViewSessionBridge` boundaries
- Public extensibility APIs for render-pass contributors, frame hooks, and runtime capability queries
- `Videra.ExtensibilitySample` plus English/Chinese extensibility contract docs and repository guards

### What Worked

- Sequencing the work as `pipeline truth -> orchestration seam -> public API -> sample/docs` kept scope disciplined
- Repository guards continued to be the most effective way to keep docs, code, and localization aligned
- The sample forced the public API design to stay honest instead of remaining architecture-only

### What Was Inefficient

- The archive CLI inferred milestone stats from whole-repo phase totals and needed manual correction for `v1.1`
- Because `.planning` is intentionally ignored from Git, milestone completion remained a local planning operation rather than a versioned repository commit
- Nyquist validation stayed disabled, so the audit depended on phase verification artifacts and manual cross-checking

### Patterns Established

- Ship public extensibility only after internal contract and orchestration seams are already stable
- Treat developer sample code as a first-class contract surface, not as disposable demo code
- Use bilingual repository tests to guard public docs rather than relying on one-time manual proofreading

### Key Lessons

- Another pure decoupling milestone would likely have diminishing returns; the new contract now needs feature-driven pressure testing
- If planning artifacts are kept local-only, archive tooling should be treated as a scaffold that may need manual normalization
- A narrow but real sample is the fastest way to expose missing public-surface truth

### Cost Observations

- Model profile: balanced
- Sessions: concentrated multi-session milestone completed in roughly one day of implementation and closure work
- Notable: the highest leverage again came from contract clarification and guardrails, not from adding broad feature surface

## Milestone: v1.0 — Alpha Ready

**Shipped:** 2026-04-08  
**Phases:** 8  
**Plans:** 24

### What Was Built

- Cross-platform test infrastructure and required hosted native validation
- Structured exception handling, safety guards, and resource cleanup behavior
- Truthful Linux/macOS platform boundary closure for the current Avalonia stack
- Distribution/package/install/release truth with workflow gating
- Lifecycle/render/depth/wireframe contract closure
- Demo truthfulness across UI, commands, diagnostics, and docs

### What Worked

- Tight phase-by-phase execution with repository tests as guardrails
- Converting platform uncertainty into explicit CI evidence instead of vague local assumptions
- Treating documentation and Demo behavior as first-class contracts rather than post-hoc polish

### What Was Inefficient

- Several later phases completed in code before their phase-level planning artifacts were normalized
- Requirement registry drift accumulated because new requirement namespaces were introduced before the root registry was updated
- Nyquist validation was disabled, so validation artifacts had to be reconstructed manually at the end

### Patterns Established

- Use repository tests to lock workflow/doc/package truth
- Prefer truthful scope reduction over shipping false platform claims
- Treat Demo state and docs as product contracts, not sample-only details

### Key Lessons

- Hosted matching-host validation is the right closure mechanism for multi-platform native claims
- Platform support must be phrased in terms of the actual UI/framework stack, not ideal future architecture
- Milestone audit artifacts need to be kept current as phases complete, not reconstructed only at the end

### Cost Observations

- Model profile: balanced
- Sessions: multi-session milestone with dense late-stage closure work on CI, platform truth, and planning normalization
- Notable: the highest leverage came from contract clarification, not from adding more subsystems

## Cross-Milestone Trends

- `v1.20` showed that package/product-boundary work is worth doing before broader runtime expansion when the repo already has strong internals but a blurry outward story.
- `v1.13` showed that viewer-depth follow-through is most valuable when it improves inspection truth, reproducibility, and support artifacts without widening the public API.
- `v1.2` proved milestone audit and cleanup phases are worth treating as first-class engineering work when earlier completion state was wrong.
- `v1.0` proved platform truth and release truth matter more than optimistic support claims.
- `v1.1` proved the next leverage point after platform closure is contract clarity plus sample-backed public APIs.
- Across all three milestones, the best outcomes came from making code, tests, docs, diagnostics, and planning artifacts tell the same story.
