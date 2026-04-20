# Milestones

## v1.16 SurfaceCharts Adoption Surface

**Shipped:** 2026-04-20  
**Phases:** 4 (`87-90`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-20` → `2026-04-20`)  
**Repo state:** local milestone completion was preserved at transition into `v1.17`; roadmap/requirements snapshots were archived, but a dedicated milestone-audit artifact had not yet been captured

### Key Accomplishments

1. Defined one canonical source-first `first chart` story and aligned `SurfaceChartView` contract language around chart-local ownership, `ViewState`, interaction, overlays, and rendering status.
2. Added a narrow source-first evaluation path so `SurfaceCharts` can be tried without reverse-engineering the broader demo application.
3. Added CI/support evidence for the chart adoption path so diagnostics and troubleshooting share the same runtime truth.
4. Aligned README, release, and support wording so `SurfaceCharts` remains explicitly source-first and distinct from `VideraView` package promises.

### Archived Materials

- `.planning/milestones/v1.16-ROADMAP.md`
- `.planning/milestones/v1.16-REQUIREMENTS.md`

### Notes

- The milestone transitioned directly into `v1.17 修` after CI surfaced benchmark compile drift, SurfaceCharts warnings-as-errors debt, and a Linux `XWayland` consumer-smoke regression
- No dedicated `.planning/milestones/v1.16-MILESTONE-AUDIT.md` existed at transition time
- Raw phase execution history remains in `.planning/phases/`

---

## v1.15 Repository Guard and Evidence Calibration

**Shipped:** 2026-04-20  
**Phases:** 3 (`84-86`)  
**Plans:** 9  
**Timeline:** 1 day (`2026-04-20` → `2026-04-20`)  
**Repo state:** local worktree closeout recorded after focused verification and milestone audit; no milestone tag was retained because release tags stay version-aligned

### Key Accomplishments

1. Replaced the token-only `OpenGL` guard with an explicit “no `OpenGL` product promise” contract across shared backend-minimum-contract docs and repository tests.
2. Expanded `quality-gate-evidence` to the outward-facing `Videra.MinimalSample` path and removed a false-red worktree baseline assumption in SurfaceCharts repository guards.
3. Aligned benchmark workflow naming, benchmark runbook wording, README wording, release guidance, and live planning around one opt-in, label-gated, non-numeric benchmark-review contract.

### Archived Materials

- `.planning/milestones/v1.15-ROADMAP.md`
- `.planning/milestones/v1.15-REQUIREMENTS.md`
- `.planning/milestones/v1.15-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `OpenGL`, compositor-native Wayland, whole-repo warnings-as-errors, and automated benchmark thresholds remain deliberately deferred
- The work landed in an isolated worktree branch so follow-up branch management remains a separate choice from milestone closeout

---

## v1.14 Compatibility and Quality Hardening

**Shipped:** 2026-04-20  
**Phases:** 4 (`80-83`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-20` → `2026-04-20`)  
**Repo state:** milestone implementation and closeout were committed locally; audit status is `tech_debt` with 3 accepted follow-up items, and no milestone tag was retained because release tags stay version-aligned

### Key Accomplishments

1. Normalized the shipped backend minimum contract across `D3D11`, `Vulkan`, and `Metal`, and made unsupported seams explicit instead of silent or backend-specific.
2. Added a stable Linux `DisplayServerCompatibility` diagnostics line and aligned runtime, smoke artifacts, and support docs around the same `X11` / `XWayland` truth.
3. Promoted packaged consumer smoke plus `Videra.ExtensibilitySample` / `Videra.InteractionSample` runtime evidence into routine pull-request coverage.
4. Tightened the alpha-ready `green` definition by validating the real packaged consumer path with warnings as errors and aligning CI/docs around the same evidence story.

### Archived Materials

- `.planning/milestones/v1.14-ROADMAP.md`
- `.planning/milestones/v1.14-REQUIREMENTS.md`
- `.planning/milestones/v1.14-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- The upstream `gsd-tools audit-open` helper was broken locally (`ReferenceError: output is not defined`), so the pre-close artifact audit was completed manually
- Accepted debt at close: strengthen the `OpenGL` repo guard semantics, decide whether to widen warnings-as-errors scope, and keep benchmark review label-gated for now

---

## v1.13 Inspection Fidelity

**Shipped:** 2026-04-19  
**Phases:** 4 (`76-79`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-19` → `2026-04-19`)  
**Repo state:** local planning closeout recorded after fresh milestone-targeted verification, inspection benchmark dry runs, and packed-package consumer smoke; no git tag or immutable release boundary was created because `.planning` remains local-only and the working tree still contains implementation changes

### Key Accomplishments

1. Replaced bounds-based inspection truth with richer mesh-accurate hit records and per-mesh acceleration while preserving object-level public selection semantics.
2. Added viewer-first measurement snap modes and made snap intent round-trip through `VideraInspectionState` without introducing editor-style tooling.
3. Added same-API inspection fast paths through cached clip-truth reuse and preferred live snapshot readback, then captured the new pressure points in `InspectionBenchmarks`.
4. Added `VideraInspectionBundleService`, taught the interaction sample and consumer smoke to emit replayable inspection bundles, and aligned docs/support truth around the same artifact.

### Archived Materials

- `.planning/milestones/v1.13-ROADMAP.md`
- `.planning/milestones/v1.13-REQUIREMENTS.md`
- `.planning/milestones/v1.13-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Explicit multi-backend epsilon harnessing, backend-specific clip preview, and single-file bundle packaging remain deferred follow-up work

---

## v1.12 Viewer-First Inspection Workflow

**Shipped:** 2026-04-18  
**Phases:** 4 (`72-75`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-18` → `2026-04-18`)  
**Repo state:** local planning closeout recorded after fresh inspection integration tests, consumer smoke, benchmark runs, and full repository verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Added viewer-first clipping planes, measurement state, inspection-state capture/restore, and snapshot export without widening `VideraEngine` extensibility boundaries.
2. Extended the diagnostics snapshot contract so clipping state, measurement counts, and snapshot-export outcomes can be copied into support workflows directly.
3. Updated the interaction sample, consumer smoke path, and public docs so inspection workflows are demonstrated through the same public `VideraView` surface.
4. Locked the inspection path with focused runtime tests, integration tests, repository guards, consumer smoke, benchmark reruns, and fresh full verification.

### Archived Materials

- `.planning/milestones/v1.12-ROADMAP.md`
- `.planning/milestones/v1.12-REQUIREMENTS.md`
- `.planning/milestones/v1.12-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Editor-style authoring tools, native compositor-hosted Wayland, public `Videra.SurfaceCharts.*` packaging, and another deep performance pass remain deferred future work

---

## v1.11 Alpha Happy-Path Stabilization and Diagnostics Productization

**Shipped:** 2026-04-18  
**Phases:** 4 (`68-71`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-18` → `2026-04-18`)  
**Repo state:** local planning closeout recorded after fresh verification, consumer smoke, and benchmark runs; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Froze the public alpha onboarding path around one canonical `Options -> LoadModel(s) -> FrameAll/ResetCamera -> BackendDiagnostics` story across root docs, Avalonia docs, and `Videra.MinimalSample`.
2. Added `VideraDiagnosticsSnapshotFormatter`, wired it into the minimal sample and consumer smoke output, and made the diagnostics snapshot the default alpha support artifact.
3. Tightened alpha feedback, troubleshooting, contributing, and issue templates around the same reproduction checklist and diagnostics contract.
4. Kept benchmark stewardship and public release validation aligned with the same consumer path by rerunning both benchmark suites, preserving trend-oriented docs, and extending publish validation with consumer smoke.

### Archived Materials

- `.planning/milestones/v1.11-ROADMAP.md`
- `.planning/milestones/v1.11-REQUIREMENTS.md`
- `.planning/milestones/v1.11-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Native Wayland compositor hosting, public `Videra.SurfaceCharts.*` packaging, and a new deep internal optimization loop remain deferred future work

---

## v1.10 Alpha Consumer Integration and Feedback Loop

**Shipped:** 2026-04-17  
**Phases:** 4 (`64-67`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Added `Videra.MinimalSample` and rewrote the default docs path so the public `VideraView` happy path is short, typed, and separate from advanced extensibility seams.
2. Added `Videra.ConsumerSmoke`, `Invoke-ConsumerSmoke.ps1`, and a host-specific workflow so package-consumer regressions are validated from install to first scene.
3. Added viewer and surface-chart benchmark workflows plus benchmark gate docs so performance evidence now survives as workflow artifacts instead of only local runs.
4. Tightened alpha feedback, issue templates, troubleshooting, and support docs around diagnostics-rich reproduction data and the truthful X11/XWayland support boundary.

### Archived Materials

- `.planning/milestones/v1.10-ROADMAP.md`
- `.planning/milestones/v1.10-REQUIREMENTS.md`
- `.planning/milestones/v1.10-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Native Wayland compositor hosting, public `Videra.SurfaceCharts.*` packaging, and another deep internal runtime rewrite remain deferred future work

---

## v1.9 Scene Performance Evidence and Coordinator Cleanup

**Shipped:** 2026-04-17  
**Phases:** 4 (`60-63`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Surfaced scene upload bytes, latency, failure counts, and resolved budgets through the stable backend-diagnostics shell instead of leaving queue behavior implicit.
2. Added a dedicated `Videra.Viewer.Benchmarks` project and explicit upload-policy tests so scene performance work now has first-class evidence.
3. Extracted `SceneRuntimeCoordinator` so `VideraViewRuntime` further retreats to shell/orchestration responsibilities while scene publication and rehydrate flow stay internal.
4. Added XML docs and repository-guarded quick-start vocabulary for the public viewer flow, keeping onboarding aligned across IDEs and docs.

### Archived Materials

- `.planning/milestones/v1.9-ROADMAP.md`
- `.planning/milestones/v1.9-REQUIREMENTS.md`
- `.planning/milestones/v1.9-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Deeper queue-policy rewrites, hardware telemetry, and native mesh preprocess remain deferred future work

---

## v1.8 Scene Residency Efficiency and Mesh Payload Optimization

**Shipped:** 2026-04-17  
**Phases:** 4 (`56-59`)  
**Plans:** 12  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Replaced cadence-driven dirty sweeps with event-driven scene residency transitions so steady-state rendering no longer requeues or reuploads already-resident scene entries.
2. Introduced shared `MeshPayload` semantics across imported assets and deferred `Object3D` materialization, cutting repeated vertex/index duplication while keeping explicit retention semantics.
3. Tightened upload coordination around queue-aware heuristic budgets and kept GPU realization inside frame-prelude cadence instead of public scene mutation paths.
4. Locked the new residency, payload, and recovery truth with focused Avalonia runtime tests, Core object/payload tests, scene integration tests, and fresh full verification.

### Archived Materials

- `.planning/milestones/v1.8-ROADMAP.md`
- `.planning/milestones/v1.8-REQUIREMENTS.md`
- `.planning/milestones/v1.8-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Hardware telemetry, native mesh preprocess, and broader multi-surface productization remain deferred future work

---

## v1.7 Scene Pipeline Closure v1

**Shipped:** 2026-04-17  
**Phases:** 9 (`47-55`)  
**Plans:** 27  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Added a dedicated `Videra.Avalonia.Tests` project and used it to lock runtime-scene invalidation, document mutation, residency, queue, and import-service behavior.
2. Hardened `SceneDocument` with stable entry identity, versioning, and ownership semantics, then converted `Items` synchronization and runtime publication onto that contract.
3. Split scene publication into explicit delta/application/residency/upload services and moved GPU realization into a budgeted frame-prelude drain path with recovery-aware dirty requeue semantics.
4. Exposed scene residency diagnostics through the existing viewer diagnostics shell and aligned the narrow Scene Pipeline Lab, docs, and repository guards around the same runtime truth.

### Archived Materials

- `.planning/milestones/v1.7-ROADMAP.md`
- `.planning/milestones/v1.7-REQUIREMENTS.md`
- `.planning/milestones/v1.7-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `.planning` closeout remains local-only and does not create a repository release boundary by itself
- Shared CPU mesh payload reduction and native mesh preprocess work were intentionally deferred until the stabilized scene pipeline produces benchmark evidence

---

## v1.6 Scene Pipeline Truth and Backend Surface Closure

**Shipped:** 2026-04-17  
**Phases:** 5 (`42-46`)  
**Plans:** 15  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Made `SceneDocument` the authoritative viewer scene owner and converted runtime scene mutations to document-first behavior.
2. Split import from upload, introduced deferred scene objects, and made batch load use bounded parallelism plus atomic replace semantics.
3. Enabled backend/surface rebind to rebuild scene resources from retained imported assets without treating `SoftwareResourceFactory` as the steady-state truth.
4. Migrated built-in backends onto direct `IGraphicsDevice` / `IRenderSurface` contracts and aligned a narrow Scene Pipeline Lab, docs, and repository guards around the same story.

### Archived Materials

- `.planning/milestones/v1.6-ROADMAP.md`
- `.planning/milestones/v1.6-REQUIREMENTS.md`
- `.planning/milestones/v1.6-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- `LegacyGraphicsBackendAdapter` remains as a compatibility seam for non-migrated or custom legacy backends, not the built-in steady-state path
- The Scene Pipeline Lab intentionally stays narrow and contract-focused rather than becoming a broader viewer showcase

---

## v1.5 VideraView Runtime Thinning and Render Orchestration

**Shipped:** 2026-04-17  
**Phases:** 7 (`35-41`)  
**Plans:** 21  
**Timeline:** 1 day (`2026-04-17` → `2026-04-17`)  
**Repo state:** local planning closeout recorded after fresh verification; no git tag or immutable release boundary was created as part of planning closeout

### Key Accomplishments

1. Thinned `VideraView` into a forwarding public shell backed by internal `VideraViewRuntime`.
2. Replaced the ready-state permanent render loop with invalidation-driven scheduling and interactive frame leases.
3. Unified scene ownership through backend-neutral assets and runtime-owned `SceneDocument` truth.
4. Moved interaction and overlay semantics into Core services, split internal graphics device/surface responsibilities, and decomposed `VideraEngine` internals without changing its public extensibility role.

### Archived Materials

- `.planning/milestones/v1.5-ROADMAP.md`
- `.planning/milestones/v1.5-REQUIREMENTS.md`
- `.planning/milestones/v1.5-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- The milestone intentionally kept `VideraEngine` as the only public extensibility root
- Native backend packages still flow through the legacy adapter; backend-specific device/surface rewrites remain future work

---

## v1.4 Open Source Consumption and Release Surfaces

**Shipped:** 2026-04-16  
**Phases:** 6 (`29-34`)  
**Plans:** 18  
**Timeline:** 1 day (`2026-04-16` → `2026-04-16`)  
**Repo state:** local planning closeout recorded on a dirty worktree; no git tag or clean release commit was created

### Key Accomplishments

1. Reworked the public README and docs entrypoints so newcomers can distinguish published packages, source-only modules, samples, demos, and support levels.
2. Split public vs preview package publishing into truthful workflows and documented `nuget.org` as the public feed with GitHub Packages reserved for preview/internal use.
3. Added icon, SourceLink, deterministic metadata, symbol-package output, and stronger package validation for all five public packages.
4. Routed GitHub Issues / Discussions / Security intake, added release-note categories and a maintainer runbook, and enabled weekly Dependabot maintenance automation.

### Archived Materials

- `.planning/milestones/v1.4-ROADMAP.md`
- `.planning/milestones/v1.4-REQUIREMENTS.md`
- `.planning/milestones/v1.4-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- The nuget.org public flow is now configured in-repo, but the next tagged release still needs `NUGET_API_KEY` to publish externally
- Git tag / immutable release point were intentionally left undone because the repository was not normalized to a clean release boundary during local closeout

---

## v1.3 Camera-True Surface Charts

**Shipped:** 2026-04-16  
**Phases:** 6 (`23-28`)  
**Plans:** 18  
**Timeline:** 1 day (`2026-04-16` → `2026-04-16`)  
**Repo state:** local planning closeout recorded on a dirty worktree; no git tag or clean release commit was created

### Key Accomplishments

1. Unified the chart-view spine around `SurfaceViewState`, `SurfaceCameraFrame`, and shared projection math across rendering, overlay, and picking.
2. Replaced viewport-linear probe behavior with true 3D picking and upgraded request planning to camera-aware projected-footprint / screen-error logic.
3. Slimmed the GPU resident path, removed unnecessary software-scene shadowing, and moved color-map churn into backend-owned remap/update logic.
4. Shipped chart-local `OverlayOptions`, professional overlay layout behavior, and aligned the independent demo, English/Chinese docs, and repository guards with that truth.

### Archived Materials

- `.planning/milestones/v1.3-ROADMAP.md`
- `.planning/milestones/v1.3-REQUIREMENTS.md`
- `.planning/milestones/v1.3-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- Processing/native hotspot follow-through remains deferred until benchmark evidence justifies pulling it into active scope
- Git tag / immutable release point were intentionally left undone because the repository was not normalized to a clean release boundary during local closeout

---

## v1.2 Professional Surface Charts

**Shipped:** 2026-04-16  
**Phases:** 8 delivered (`15-22`), with superseded historical gap slots `13-14` preserved in the archive  
**Plans:** 24 delivered  
**Timeline:** 2 days (`2026-04-14` → `2026-04-16`)  
**Repo state:** local planning closeout recorded on a dirty worktree; no git tag or clean release commit was created

### Key Accomplishments

1. Recovered the original runtime/view-state contract with `SurfaceViewState`, `SurfaceDataWindow`, `SurfaceCameraPose`, `SurfaceChartRuntime`, and host-facing `FitToData()` / `ResetCamera()` / `ZoomTo(...)`.
2. Shipped built-in orbit / pan / dolly / focus workflows plus explicit `InteractionQuality`, keeping chart interaction chart-local and independent from `VideraView`.
3. Locked the professional-chart stack through GPU-first rendering, view-aware large-dataset scheduling, persistent/batch cache paths, and live scheduler batch-read adoption.
4. Aligned the independent demo, English/Chinese docs, repository guards, summary metadata, and milestone audit truth so the shipped behavior is auditable end to end.

### Archived Materials

- `.planning/milestones/v1.2-ROADMAP.md`
- `.planning/milestones/v1.2-REQUIREMENTS.md`
- `.planning/milestones/v1.2-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- Original Phase `13` / `14` gap history is preserved in the archive, but delivery was recovered by Phases `19` / `20`
- Git tag / immutable release point were intentionally left undone because the repository was not at a clean release state during local closeout

---

## v1.1 Render Pipeline Architecture

**Shipped:** 2026-04-09  
**Phases:** 4 (`9-12`)  
**Plans:** 13  
**Timeline:** 1 day (`2026-04-08` → `2026-04-09`)  
**Repo commit span:** 16 commits (`88eb422` → `95a96c7`)  
**Measured delta:** 58 files / +4,231 net lines

### Key Accomplishments

1. Converted the implicit frame path into an explicit render-pipeline contract with stable stage vocabulary, pipeline snapshots, and diagnostics truth.
2. Extracted host-agnostic session orchestration and `VideraViewSessionBridge`, reducing coupling between engine, session, native host, and Avalonia view shell.
3. Shipped the first public extensibility surface through `RegisterPassContributor(...)`, `ReplacePassContributor(...)`, `RegisterFrameHook(...)`, and `GetRenderCapabilities()`.
4. Added a narrow developer-facing sample, English/Chinese extensibility contract docs, and repository guards that lock lifecycle, fallback, and scope-boundary truth.

### Archived Materials

- `.planning/milestones/v1.1-ROADMAP.md`
- `.planning/milestones/v1.1-REQUIREMENTS.md`
- `.planning/milestones/v1.1-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- Deferred items were kept out of `v1.1` scope rather than hidden behind overclaimed platform or feature promises

---

## v1.0 Alpha Ready

**Shipped:** 2026-04-08  
**Phases:** 8  
**Plans:** 24  
**Planned tasks:** 52  
**Timeline:** 92 days (`2026-01-07` → `2026-04-08`)  
**Repo commit span:** 168 commits  
**Measured surface:** 214 files / 25,885 lines

### Key Accomplishments

1. Established repository-wide test infrastructure, structured logging cleanup, analyzer baseline, and GitHub-hosted matching-host native validation.
2. Replaced fragile generic/native failure handling with domain-specific exceptions, safety guards, and explicit rollback behavior.
3. Closed platform truth on a realistic alpha-ready scope: Windows, macOS, Linux `X11 native`, and Linux Wayland-session `XWayland` compatibility.
4. Fixed distribution truth by separating entry/package semantics, adding package semantic validation, and gating release workflows on matching-host evidence.
5. Explicitly modeled render/session lifecycle, software depth semantics, wireframe/style contracts, and Demo user-facing truth.

### Archived Materials

- `.planning/milestones/v1.0-ROADMAP.md`
- `.planning/milestones/v1.0-REQUIREMENTS.md`
- `.planning/milestones/v1.0-MILESTONE-AUDIT.md`

### Notes

- Raw phase execution history remains in `.planning/phases/`
- Deferred items were intentionally moved out of `v1.0` scope rather than misreported as shipped

---
*Updated on 2026-04-19*
