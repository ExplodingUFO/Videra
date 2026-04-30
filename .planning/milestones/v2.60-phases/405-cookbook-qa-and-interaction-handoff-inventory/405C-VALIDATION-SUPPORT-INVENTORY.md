---
phase: 405
title: "Validation Support and Scope Guardrail Inventory"
bead: Videra-0ji
status: inventory
created_at: 2026-04-30
scope: "Phase 405 validation/support/scope-guardrail inventory only"
---

# Phase 405C Validation Support Inventory

This file inventories validation and support evidence for v2.60 Phases 406,
407, and 408. It is evidence only: no product code, test, Beads export,
roadmap, or generated-doc changes are approved here.

## Current Evidence for Phase 406/407/408

### v2.60 planning and Beads state

- `.planning/ROADMAP.md` defines Phase 406 as cookbook QA hardening, Phase 407
  as interaction handoff polish, and Phase 408 as final verification after 406
  and 407.
- `.planning/REQUIREMENTS.md` maps Phase 406 to `QA-02`, `VERIFY-01`, and
  `SCOPE-02`; Phase 407 to `HANDOFF-02`, `VERIFY-01`, and `SCOPE-02`; and
  Phase 408 to `VERIFY-01` and `VERIFY-02`.
- `.beads/issues.jsonl` has `Videra-1h1` for Phase 406, `Videra-xq1` for Phase
  407, and `Videra-448` for Phase 408. Phase 406 and 407 both block on Phase
  405 inventory (`Videra-b53`); Phase 408 blocks on both Phase 406 and Phase
  407.
- `docs/ROADMAP.generated.md` currently exposes `Videra-tqz` and
  `Videra-b53` as ready, and lists Phase 406/407/408 as blocked. It is
  generated output from `.beads/issues.jsonl`, not a hand-edited tracker.

### Archived v2.59 verification evidence

- `.planning/milestones/v2.59-phases/401-interaction-and-cookbook-experience-inventory/401C-VALIDATION-INVENTORY.md`
  already names the focused interaction, cookbook, snapshot, Beads, and
  generated-roadmap validation surfaces that remain relevant for v2.60.
- `.planning/milestones/v2.59-phases/402-interaction-and-code-experience-polish/402-VERIFICATION.md`
  recorded focused Plot API and keyboard toolbar integration tests passing
  after interaction/code-experience polish.
- `.planning/milestones/v2.59-phases/403-cookbook-demo-conversion/403-VERIFICATION.md`
  recorded cookbook configuration tests and the SurfaceCharts demo build
  passing; it also records a prior PDB contention risk when demo build and
  focused tests ran concurrently in the same worktree.
- `.planning/milestones/v2.59-phases/404-interaction-cookbook-final-verification/404-VERIFICATION.md`
  recorded the final v2.59 gate: focused interaction tests, focused cookbook
  tests, demo build, snapshot scope guardrail, Beads export, generated roadmap,
  `BeadsPublicRoadmapTests`, and `git diff --check`.

### Relevant existing tests

- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`
  asserts cookbook README/demo entry points, recipe groups, public API snippets,
  Bar/Contour/Scatter source wiring, and the explicit "not a compatibility or
  parity layer" language.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs`
  covers demo source switching, viewport reset, waterfall behavior, cache-source
  failure behavior, and active source/data-window state.
- `tests/Videra.Core.Tests/Samples/InteractionSampleConfigurationTests.cs` and
  `tests/Videra.Core.Tests/Samples/DemoInteractionContractTests.cs` cover public
  interaction sample/demo contract surfaces.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`
  covers Plot API authoring, axis rules, typed Bar/Contour/Scatter behavior,
  PNG snapshot/save paths, and interaction-quality evidence.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewKeyboardToolbarTests.cs`
  covers toolbar state, keyboard commands, disabled interaction profile behavior,
  and `EnabledCommands` discovery.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionTests.cs`
  covers chart-local orbit, pan, wheel/dolly, and routed pointer interactions.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionRecipeTests.cs`
  covers probe resolution, selection reports, draggable overlay recipes, and
  deterministic interaction support evidence formatting.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/RegressionGuardrailTests.cs`
  remains the SurfaceCharts integration guardrail bucket for regression-focused
  interaction/rendering assertions.
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` and
  `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs` cover the
  chart-local PNG snapshot contract and explicit failure diagnostics.
- `tests/Videra.SurfaceCharts.Core.Tests/ScatterDataLogger3DTests.cs` covers
  live scatter/data-logger behavior used by cookbook and handoff examples.
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` verifies that
  `docs/ROADMAP.generated.md` is deterministic from `.beads/issues.jsonl` and
  discoverable through docs.

### Relevant existing scripts and support docs

- `scripts/Test-SnapshotExportScope.ps1` checks for old chart controls, direct
  public `Source` APIs, PDF/vector export, chart-local coupling to
  `VideraSnapshotExportService`, and hidden snapshot fallback patterns.
- `scripts/Export-BeadsRoadmap.ps1` regenerates `docs/ROADMAP.generated.md`
  from `.beads/issues.jsonl`.
- `docs/beads-coordination.md` defines the Docker-backed Dolt Beads service
  contract, worktree redirect expectations, Beads export flow, and the rule that
  generated roadmap output is read-only.
- `scripts/Test-BeadsCoordination.ps1` is the explicit Beads service health
  check when coordination infrastructure itself is in scope.
- `scripts/verify.ps1` and `scripts/verify.sh` remain broad validation entry
  points, but Phases 406/407 should prefer narrow, changed-surface gates first.

## Gate Commands

Run restores once per fresh worktree before `--no-restore` gates:

```powershell
dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj
dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj
dotnet restore tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj
dotnet restore samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj
```

### Phase 406 cookbook QA gate

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests|InteractionSampleConfigurationTests|DemoInteractionContractTests" --no-restore
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short
```

Run the focused Core tests and demo build sequentially in the same worktree to
avoid the PDB contention already observed in Phase 403.

### Phase 407 interaction handoff gate

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~RegressionGuardrailTests" --no-restore
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~ScatterDataLogger3DTests|FullyQualifiedName~PlotSnapshotContractTests|FullyQualifiedName~PlotSnapshotCaptureTests" --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short
```

If Phase 407 touches the broader `Videra.Avalonia` interaction controller or
support-evidence formatting, add:

```powershell
dotnet restore tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj
dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj --filter "VideraInteractionControllerTests|VideraInteractionEvidenceFormatterTests|VideraDiagnosticsSnapshotFormatterTests" --no-restore
```

### Phase 408 final verification gate

Phase 408 should compose the Phase 406 and Phase 407 gates, then synchronize
Beads and generated roadmap from the phase that owns those shared files:

```powershell
bd context --json
bd show Videra-1h1 --json
bd show Videra-xq1 --json
bd show Videra-448 --json
bd export -o .beads\issues.jsonl
pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
git status --short
bd ready --json
```

For explicit Beads service health or multi-agent assignment checks:

```powershell
bd context --json
bd doctor
bd worktree list
pwsh -NoProfile -File scripts\Test-BeadsCoordination.ps1
```

`bd dolt push` and `git push` are final-session sync steps, but this Phase 405C
task is explicitly no-push.

## Scope Guardrails

- No compatibility layer: keep SurfaceCharts as Videra APIs with
  ScottPlot5-inspired ergonomics only. Do not claim API parity, migration
  support, wrappers, adapters, or compatibility with ScottPlot.
- No old chart controls: `VideraChartView` remains the single shipped chart
  control. Do not reintroduce `SurfaceChartView`, `WaterfallChartView`, or
  `ScatterChartView`.
- No direct public `Source` API: keep data loading through `Plot.Add.Surface`,
  `Plot.Add.Waterfall`, `Plot.Add.Scatter`, `Plot.Add.Bar`, and
  `Plot.Add.Contour` style authoring paths.
- No fallback/downshift: unsupported output, missing render host, and invalid
  snapshot requests must remain explicit diagnostic failures. Do not silently
  switch renderer, backend, export format, or interaction mode.
- No backend expansion: v2.60 cookbook/support hardening must not add or widen
  native backend support, OpenGL support, platform promises, or viewer-runtime
  renderer scope.
- No generic workbench: keep demos and support artifacts sample-first and
  bounded. Do not turn the SurfaceCharts demo into a plugin editor, generic
  plotting engine, command framework, mouse-remapping framework, or god-code
  workbench.
- No broadened export: snapshot/export scope stays chart-local PNG/bitmap. Do
  not add PDF/vector export or route chart-local capture through the viewer-level
  `VideraSnapshotExportService`.
- Shared generated files are owned by the synchronizing phase only:
  `.beads/issues.jsonl` and `docs/ROADMAP.generated.md` should not be changed by
  parallel Phase 406/407 implementation agents unless explicitly assigned.

## Residual Risks and Human-Needed Checks

- Phase 405 is split across parallel worktrees. Phase 406/407 slice selection
  should wait for the cookbook and interaction inventory files from the sibling
  agents before final bead decomposition.
- The generated roadmap can lag live Beads state until `bd export` and
  `Export-BeadsRoadmap.ps1` run. Treat `docs/ROADMAP.generated.md` as snapshot
  evidence, not live truth.
- Beads/Dolt service health is environment-sensitive. Use
  `Test-BeadsCoordination.ps1` only when coordination health is intentionally in
  scope; normal cookbook/interaction validation should not require the Docker
  Beads service.
- Fresh worktrees may lack restored assets. Run restore before `--no-restore`
  gates.
- Existing analyzer warnings may appear during focused SurfaceCharts/demo test
  builds. Treat them as blockers only if newly introduced or tied to the phase
  change.
- Human-needed check: before Phase 408 closes v2.60, confirm whether the final
  handoff should include only local commit evidence or also the mandatory
  project-level push/Dolt push sequence from `AGENTS.md`. This current Phase
  405C task explicitly stops before push.

## Candidate Minimal Phase 408 Support-Hardening Slices

These are candidates only; create or select beads after the Phase 405 cookbook
and interaction inventories are merged.

1. **Final support evidence gate bead**
   - Scope: run the Phase 406 and Phase 407 focused gates plus
     `Test-SnapshotExportScope.ps1`, then record a compact verification table.
   - Depends on: Phase 406 and Phase 407 complete.
   - Non-goals: no product code, no new demo features, no backend/export scope.

2. **Beads/generated-roadmap synchronization bead**
   - Scope: close/update Phase 406/407 beads, export `.beads/issues.jsonl`,
     regenerate `docs/ROADMAP.generated.md`, and run `BeadsPublicRoadmapTests`.
   - Depends on: Phase 406 and Phase 407 complete; live Beads service available.
   - Non-goals: no hand edits to generated roadmap and no parallel tracker.

3. **Scope guardrail closure bead**
   - Scope: run and record `scripts/Test-SnapshotExportScope.ps1`; spot-check
     the final diff for compatibility language, hidden fallback/downshift,
     backend expansion, generic workbench scope, and broadened export claims.
   - Depends on: final Phase 406/407 diffs available.
   - Non-goals: no new guardrail script unless a concrete drift gap is found.

4. **Human handoff/readiness bead**
   - Scope: produce the final v2.60 handoff with changed files, verification
     status, Beads/roadmap state, residual risks, and explicit push/Dolt-push
     decision.
   - Depends on: Phase 408 verification and sync beads complete.
   - Human-needed: confirm final push expectations if they conflict with a
     task-specific "do not push" instruction.

## Blockers

- No technical blocker for this validation/support inventory.
- Phase 406/407 implementation selection is intentionally blocked on the full
  Phase 405 inventory set, including the sibling cookbook QA and interaction
  handoff inventory files.
