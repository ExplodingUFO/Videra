---
phase: 401
title: "Validation Inventory for v2.59 Interaction and Cookbook Guardrails"
bead: Videra-2ly
status: inventory
created_at: 2026-04-30
scope: "Phase 401/402/403 validation commands, generated-roadmap checks, and scope guardrails only"
---

# Phase 401C Validation Inventory

This file is evidence and command inventory for v2.59 Phase 401/402/403. It does
not approve product-code changes, broaden the milestone scope, or replace Beads
as the task authority.

## Current Evidence Surfaces

### Prior phase verification docs

- `.planning/phases/383-scottplot-5-interaction-inventory-and-beads-coordination/383-VERIFICATION.md`
  recorded the previous interaction/cookbook inventory path and Beads export
  closure.
- `.planning/phases/384-plot-lifecycle-and-code-experience-polish/384-VERIFICATION.md`
  recorded `VideraChartViewPlotApiTests` as the focused Plot API/code-experience
  validation surface.
- `.planning/phases/385-interaction-profile-and-command-surface/385-VERIFICATION.md`
  recorded `SurfaceChartInteractionTests` and
  `VideraChartViewKeyboardToolbarTests` as focused interaction/profile
  validation.
- `.planning/phases/386-selection-probe-and-draggable-overlay-recipes/386-02-VERIFICATION.md`
  recorded the probe, pinned-probe, interaction, and draggable-overlay recipe
  validation surface.
- `.planning/phases/387-axis-rules-linked-views-and-live-view-management/387-02-VERIFICATION.md`
  recorded `DataLogger3D`, `Axis`, and `PlotApi` validation for live/linked-axis
  behavior.
- `.planning/phases/388-cookbook-demo-gallery-and-docs/388-02-VERIFICATION.md`
  recorded the SurfaceCharts demo configuration test and demo build command.
- `.planning/phases/389-integration-guardrails-and-milestone-evidence/389-02-VERIFICATION.md`
  recorded the combined v2.56 closure suite, snapshot guardrail, Beads export,
  generated-roadmap regeneration, and `bd ready --json`.
- `.planning/phases/400-final-cutover-verification-and-handoff/400-VERIFICATION.md`
  recorded `BeadsPublicRoadmapTests` as the generated public roadmap alignment
  test.

### Relevant existing tests

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionTests.cs`
  covers chart-local pointer interactions such as orbit, pan, and wheel/dolly.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionRecipeTests.cs`
  covers probe, selection report, and draggable-overlay recipe behavior.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/RegressionGuardrailTests.cs`
  is the SurfaceCharts integration guardrail bucket.
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` and
  `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs` cover the
  Plot snapshot contract.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`
  asserts cookbook README/demo entry points, recipe groups, public API snippets,
  and demo shell strings.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs`
  covers demo viewport behavior.
- `tests/Videra.Core.Tests/Samples/InteractionSampleConfigurationTests.cs` and
  `tests/Videra.Core.Tests/Samples/DemoInteractionContractTests.cs` cover public
  interaction sample/demo contract surfaces.
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` verifies that
  `docs/ROADMAP.generated.md` is deterministic from `.beads/issues.jsonl` and
  linked from docs.
- `tests/Videra.Avalonia.Tests/Interaction/VideraInteractionControllerTests.cs`
  covers the broader `VideraView` interaction controller if Phase 402 touches
  shared Avalonia interaction behavior.

### Relevant existing scripts

- `scripts/Test-SnapshotExportScope.ps1` is the scope guardrail for old chart
  controls, direct public `Source`, PDF/vector export, viewer-level export
  coupling from chart-local code, and hidden snapshot fallback patterns.
- `scripts/Export-BeadsRoadmap.ps1` regenerates `docs/ROADMAP.generated.md`
  from `.beads/issues.jsonl`.
- `scripts/Test-BeadsCoordination.ps1` validates the Docker-backed Dolt Beads
  service only when Beads service health is intentionally in scope.
- `scripts/verify.ps1` and `scripts/verify.sh` remain broader local validation
  entry points, but Phase 402/403 should prefer focused commands first.

## Phase 402 Interaction Validation Commands

Run restore once in a fresh worktree before `--no-restore` tests:

```powershell
dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj
dotnet restore tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj
```

Focused interaction/code-experience suite:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "VideraChartViewPlotApiTests|SurfaceChartInteractionTests|VideraChartViewKeyboardToolbarTests|SurfaceChartInteractionRecipeTests|Probe|PinnedProbe|Interaction|Axis|PlotApi" --no-restore
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter DataLogger3D --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
```

If Phase 402 changes shared `Videra.Avalonia` interaction controller or evidence
formatting, also run:

```powershell
dotnet restore tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj
dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj --filter "VideraInteractionControllerTests|VideraInteractionEvidenceFormatterTests" --no-restore
```

Phase 402 should keep validation focused on changed interaction/API seams. Do
not use a green broad build as a substitute for the relevant interaction tests.

## Phase 403 Cookbook Validation Commands

Run restore once in a fresh worktree before `--no-restore` test/build commands:

```powershell
dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj
dotnet restore samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj
```

Focused cookbook/docs/demo suite:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests|InteractionSampleConfigurationTests|DemoInteractionContractTests" --no-restore
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
git diff --check
```

If cookbook changes touch `samples/Videra.InteractionSample`, also run:

```powershell
dotnet restore samples\Videra.InteractionSample\Videra.InteractionSample.csproj
dotnet build samples\Videra.InteractionSample\Videra.InteractionSample.csproj --no-restore
```

Avoid running the demo build concurrently with
`SurfaceChartsDemoConfigurationTests` in the same worktree. Prior verification
recorded PDB contention when those commands ran at the same time.

## Beads Export and Generated Roadmap Verification

Use Beads for task state and keep generated roadmap changes deterministic:

```powershell
bd context --json
bd show Videra-v259.1 --json
bd show Videra-v259.2 --json
bd show Videra-v259.3 --json
bd export -o .beads\issues.jsonl
pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore
bd ready --json
```

`docs/ROADMAP.generated.md` is read-only output from `.beads/issues.jsonl`.
Do not edit it by hand. If the generated roadmap changes, commit it only from
the phase that owns Beads export/roadmap synchronization.

For Beads service health or multi-agent assignment checks, use the explicit
coordination path from `docs/beads-coordination.md`:

```powershell
bd context --json
bd doctor
bd worktree list
pwsh -NoProfile -File scripts\Test-BeadsCoordination.ps1
```

`bd dolt push` is separate from Git push. Use it only after the Beads/Dolt
remote path is known to be configured for this repo.

## Scope Guardrails and Non-Goals

- Keep SurfaceCharts as Videra 3D chart APIs inspired by ScottPlot 5 recipe
  ergonomics, not a ScottPlot compatibility, parity, adapter, or migration
  layer.
- Keep `VideraChartView` as the shipped chart control. Do not reintroduce
  `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Keep data loading through `Plot.Add.Surface`, `Plot.Add.Waterfall`, and
  `Plot.Add.Scatter`. Do not add a direct public `Source` API.
- Keep snapshot/export scope PNG/bitmap and chart-local. Do not add PDF/vector
  export or route chart-local snapshot behavior through the viewer-level export
  service.
- Keep unsupported output explicit through diagnostics. Do not add hidden
  fallback/downshift behavior.
- Keep demos bounded and sample-first. Do not turn the SurfaceCharts demo into a
  generic plotting engine, plugin workbench, or god-code editor.
- Keep Phase 402 and Phase 403 write sets disjoint if they run in parallel:
  Phase 402 owns focused interaction/API code and tests; Phase 403 owns
  cookbook docs/demo/sample surfaces.

## Risks and Blockers

- Phase 401 planning directories were not present before this inventory file;
  downstream agents should create only their owned phase artifacts.
- Fresh worktrees may lack `project.assets.json`; run focused `dotnet restore`
  before `--no-restore` validation.
- Concurrent test/build commands against `samples/Videra.SurfaceCharts.Demo`
  can contend on build outputs; run demo configuration tests and demo build
  sequentially in the same worktree.
- Existing analyzer warnings may appear in SurfaceCharts/demo outputs; treat
  them as blockers only when newly introduced or tied to the phase change.
- Generated Beads/roadmap files are shared surfaces and conflict-prone across
  agents. Coordinate ownership before committing `.beads/issues.jsonl` or
  `docs/ROADMAP.generated.md`.
- Broad `scripts/verify.ps1` can be slower than the focused phase suite. Use it
  for final integration confidence, not as the first feedback loop for narrow
  Phase 402/403 changes.
