---
phase: 409
title: "Phase 409 CI and Anti-Fake Validation Inventory"
bead: Videra-3b6
status: inventory
created_at: 2026-04-30
scope: "Phase 409 CI/validation/anti-fake inventory only"
---

# Phase 409C CI and Validation Inventory

This file inventories the current CI, validation, Beads export, generated
roadmap, scope guardrail, and anti-fake evidence surfaces for v2.61. It is
evidence only: no product code, demo code, tests, Beads export, generated
roadmap, or other phase files are changed here.

## Current Focused CI and Test Gates

### Cookbook docs and demo surface alignment

- `.github/workflows/ci.yml` has a `sample-contract-evidence` job on
  `windows-latest` that runs focused `Videra.Core.Tests` filters for
  `SurfaceChartsDemoConfigurationTests` and
  `SurfaceChartsDemoViewportBehaviorTests`. This is the current CI gate for
  visible SurfaceCharts sample/demo evidence.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`
  pins the independent `samples/Videra.SurfaceCharts.Demo` entry point, root
  README and demo README tokens, visible recipe groups, Bar/Contour/Scatter
  wiring, PNG snapshot route text, support-summary text, and the "not a
  compatibility or parity layer" boundary.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs`
  pins the current cookbook handoff matrix across `README.md`,
  `docs/index.md`, `samples/Videra.SurfaceCharts.Demo/README.md`,
  `docs/surfacecharts-release-cutover.md`, demo XAML, and demo code-behind.
- `scripts/verify.ps1` builds `samples/Videra.SurfaceCharts.Demo` as a broad
  repository verification step. `scripts/verify.sh` also builds the Surface
  Charts demo, but does not build `samples/Videra.MinimalSample`.

### Demo runtime/build and interaction handoff

- `.github/workflows/ci.yml` runs focused `Videra.Core.IntegrationTests`
  sample runtime evidence, then focused
  `Videra.SurfaceCharts.Avalonia.IntegrationTests` filters for
  `VideraChartViewStateTests`, `SurfaceChartInteractionTests`,
  `VideraChartViewGpuFallbackTests`,
  `VideraChartViewWaterfallIntegrationTests`, and
  `VideraChartViewPlotApiTests`.
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs`
  is the headless demo-runtime evidence for source switching, built-in
  interaction text, view-state buttons, rendering diagnostics, cache load
  failure visibility, support-summary copy behavior, waterfall/scatter proof
  paths, and anti-stale async source switching.
- `tests/Videra.Core.Tests/Samples/DemoInteractionContractTests.cs` pins the
  main demo command readiness contract for importer and viewport actions.
- `tests/Videra.Core.Tests/Samples/DemoStatusContractTests.cs` pins honest
  ready/degraded/failed backend status messages for the main demo.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/RegressionGuardrailTests.cs`
  covers chart-family dataset/output evidence, snapshot-mode chrome suppression,
  and multi-series overlay refresh behavior.

### Support evidence and diagnostics truth

- `.github/workflows/ci.yml` has `quality-gate-evidence` steps for packaged
  consumer smoke and packaged SurfaceCharts consumer smoke with
  `-TreatWarningsAsErrors`, plus package validation and warning-as-error builds
  for core tests, core integration tests, and the minimal sample.
- `.github/workflows/consumer-smoke.yml` runs Windows, Windows SurfaceCharts,
  Linux X11, Linux XWayland, and macOS consumer smoke workflows and uploads
  their artifacts. The SurfaceCharts scenario is explicitly available as
  `windows-surfacecharts`.
- `tests/Videra.Core.Tests/Repository/VideraDoctorRepositoryTests.cs` parses
  `artifacts/consumer-smoke/surfacecharts-support-summary.txt` and verifies
  structured support fields such as `EvidenceOnly`, chart control, runtime,
  backend/display environment, output diagnostics, dataset evidence, rendering
  status presence, and missing-field behavior.
- `tests/Videra.Avalonia.Tests/Controls/VideraDiagnosticsSnapshotFormatterTests.cs`
  verifies that fallback and display-server compatibility are explicit in
  diagnostics text, and that missing frame metrics remain `Unavailable`
  instead of being invented.

### Beads export and generated public roadmap

- `docs/beads-coordination.md` defines Beads as the task tracker and states
  that `.beads/issues.jsonl` is the tracked export while live issue state is in
  the Docker-backed Dolt service. It also says normal product builds, package
  validation, release dry runs, and CI workflows do not start or require the
  Beads Docker service.
- `scripts/Export-BeadsRoadmap.ps1` reads `.beads/issues.jsonl` and writes
  `docs/ROADMAP.generated.md` with Active, Ready, Blocked, Backlog, and
  Recently Closed sections. It fails if the Beads export is missing.
- `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` regenerates
  `docs/ROADMAP.generated.md`, asserts the output is byte-stable from the
  checked-in Beads export, checks docs discoverability, and verifies ready,
  blocked, and recently closed issue IDs/titles from the export are present.
- `docs/ROADMAP.generated.md` currently shows v2.61 epic `Videra-uqv` and Phase
  409 `Videra-63e` as ready, with Phases 410, 411, 412, and 413 blocked.
- `.beads/issues.jsonl` currently has `Videra-3b6` in progress for this
  inventory and `Videra-79n` open for Phase 412 CI truth hardening.

### Scope guardrails and snapshot/export truth

- `scripts/Test-SnapshotExportScope.ps1` is the focused snapshot/export scope
  guardrail. It searches `src/` for old public chart view class declarations,
  direct public `Source` property APIs, PDF/vector export enablement patterns,
  chart-local coupling to `VideraSnapshotExportService`, and hidden fallback
  patterns in snapshot paths.
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` verifies
  request/result/manifest/diagnostic validation and pins output evidence kind
  `plot-3d-output` plus dataset evidence kind `Plot3DDatasetEvidence`.
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs` verifies
  explicit failed diagnostics for empty plots, unsupported formats, and missing
  render hosts. It also pins `ImageExport` supported while `PdfExport` and
  `VectorExport` remain unsupported.
- `AGENTS.md` snapshot export boundaries remain relevant for v2.61: chart-local
  PNG/bitmap snapshot export exists, old chart controls/direct `Source`
  API/PDF/vector export/backend expansion/compatibility wrappers/hidden
  fallback/generic plotting engine remain blocked.

### Broad CI and platform evidence

- `.github/workflows/ci.yml` broad `verify` runs
  `pwsh -File ./scripts/verify.ps1 -Configuration Release` on Windows. That
  script builds `Videra.slnx`, tests `Videra.slnx`, builds the main demo,
  minimal sample, and SurfaceCharts demo, then optionally runs native platform
  validation when switches are supplied.
- `.github/workflows/native-validation.yml` runs platform-native validation on
  Linux X11, Linux Wayland/XWayland, macOS, and Windows. Linux jobs install
  native packages explicitly, and Windows calls
  `scripts/run-native-validation.ps1`.
- `.github/workflows/benchmark-gates.yml` runs viewer and SurfaceCharts
  benchmark evidence plus `Test-BenchmarkThresholds.ps1` on pull requests and
  manual dispatch. Treat these as performance evidence gates, not as cookbook
  correctness or stable benchmark guarantee text.

## Current Validation Commands and What They Prove

Run restore once in a fresh worktree before using `--no-restore` gates:

```powershell
dotnet restore tests\Videra.Core.Tests\Videra.Core.Tests.csproj
dotnet restore tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj
dotnet restore tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj
dotnet restore tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj
dotnet restore samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj
```

Focused cookbook/docs/demo alignment:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests" --no-restore
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore
```

Proves docs/demo/snippet alignment, visible cookbook recipe coverage, demo
entry-point buildability, support-summary text presence, cache failure
visibility, and visible runtime path text. It does not prove real GPU rendering
quality or benchmark performance.

Focused interaction handoff/runtime evidence:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests|FullyQualifiedName~RegressionGuardrailTests" --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~DemoInteractionContractTests|FullyQualifiedName~DemoStatusContractTests" --no-restore
```

Proves chart-local interaction, recipe support evidence, keyboard/toolbar
contract, demo command readiness, status truth, and regression buckets. These
are focused gates and should be run sequentially in one worktree to avoid build
artifact contention.

Snapshot/export scope and contract:

```powershell
pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~PlotSnapshotContractTests|FullyQualifiedName~PlotSnapshotCaptureTests" --no-restore
```

Proves no obvious reintroduction of blocked snapshot/export scope, plus explicit
diagnostic behavior for unsupported or unavailable snapshot capture. The script
is pattern-based, so final review still needs a human/agent diff pass for
misleading prose claims.

Support evidence and diagnostics:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~VideraDoctorRepositoryTests" --no-restore
dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests" --no-restore
```

Proves Doctor parsing of support/evidence artifacts and explicit diagnostic
formatting for fallback, display-server compatibility, unavailable metrics, and
snapshot status.

Beads export and generated roadmap:

```powershell
bd context --json
bd show Videra-3b6 --json
bd export -o .beads\issues.jsonl
pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore
```

Proves the local worktree is using the expected Beads context, the task bead is
visible, the checked-in export is regenerated, the public roadmap is derived
deterministically, and no hand-edited generated-roadmap drift is present. This
belongs to the synchronizing phase or agent that is allowed to edit Beads export
and generated roadmap files, not this inventory-only slice.

Broad repository verification:

```powershell
pwsh -NoProfile -File scripts\verify.ps1 -Configuration Release
bash ./scripts/verify.sh --configuration Release
```

Proves broad build/test/demo build coverage. Use these late in Phase 412/413 or
when touched surfaces are broad; they are too heavy for every small docs/test
iteration and can obscure focused failures.

## CI Design Risks

- **Nondeterminism**: Headless Avalonia/demo runtime tests wait on UI state and
  async data-path transitions. Keep focused filters deterministic and avoid
  relying on timing-sensitive broad gates as the only proof.
- **PDB/build artifact contention**: v2.59/v2.60 evidence recorded fresh
  worktree restore needs and prior PDB contention when focused tests and demo
  build ran concurrently. Run focused .NET build/test gates sequentially within
  one worktree unless each job has isolated outputs.
- **Long-running broad gates**: `scripts/verify.ps1`, native validation,
  consumer smoke, and benchmark gates are valuable final evidence but too broad
  for rapid cookbook/docs iterations. Phase 412 should compose focused gates
  first, then schedule broad gates for final confidence.
- **Missing restore**: Recent phase verification had to run `dotnet restore`
  first because fresh worktrees lacked `project.assets.json`. Candidate commands
  should either restore explicitly or avoid `--no-restore`.
- **Fake green via skipped/weakened checks**: Phase 412 must not mark success by
  deleting assertions, broadening filters until tests do not run, changing
  generated roadmap by hand, replacing real smoke evidence with static text, or
  treating `skip`/`unavailable` Doctor statuses as pass evidence.
- **Environment-sensitive Beads and native tests**: Beads coordination checks
  depend on the Docker-backed Dolt service; native validation depends on host
  display/GPU prerequisites. Keep those gates explicit and do not hide
  environment failures behind fallback claims.
- **Pattern guardrail blind spots**: `Test-SnapshotExportScope.ps1` catches
  known blocked symbols/patterns, but it cannot prove that prose and examples do
  not imply unsupported backend promises, benchmark guarantees, or compatibility
  scope. Add focused text assertions when Phase 410/411 change cookbook prose.

## Anti-Fake Rules

- No synthetic support data can be presented as runtime truth. Demo/support
  summaries must identify evidence as evidence-only and expose runtime,
  assembly, backend/display environment, active series, rendering status, and
  failure details when available.
- No fake benchmark claims. Benchmark artifacts and performance-lab visual
  evidence can show measured evidence and thresholds, but cookbook text must
  not promise stable performance guarantees from synthetic or skipped runs.
- No hidden fallback or downshift. Unsupported output, missing render hosts,
  invalid snapshot formats, backend/display incompatibility, and cache-load
  failure must surface as diagnostics or visible degraded status.
- No unsupported backend promises. v2.61 cookbook/CI truth work must not imply
  OpenGL/WebGL, new native backend support, compositor-native Wayland embedding,
  or PDF/vector export unless a real shipped implementation and validation gate
  exists.
- No ScottPlot compatibility claims. ScottPlot 5 remains an ergonomics
  reference only; Videra examples must stay native to `VideraChartView`,
  `Plot.Add.*`, chart-local interactions, host-owned state, and PNG-only
  chart-local snapshots.
- No generated-roadmap hand edits. `docs/ROADMAP.generated.md` must remain
  output from `.beads/issues.jsonl` via `Export-BeadsRoadmap.ps1`.

## Candidate Minimal Phase 412 Slices

1. **Focused cookbook/demo CI gate**
   - Scope: add or align a deterministic focused gate that runs the cookbook
     docs/demo tests and SurfaceCharts demo build after restore.
   - Validation:
     `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests" --no-restore`
     and
     `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore`.
   - Non-goals: no product/demo behavior change unless a prior Phase 410/411
     change requires it.

2. **Support evidence anti-fake gate**
   - Scope: ensure CI/test coverage catches missing `EvidenceOnly`, runtime,
     backend/display, output diagnostics, dataset evidence, rendering status,
     and explicit unavailable/failure fields.
   - Validation:
     `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~VideraDoctorRepositoryTests" --no-restore`
     and
     `dotnet test tests\Videra.Avalonia.Tests\Videra.Avalonia.Tests.csproj --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests" --no-restore`.
   - Non-goals: no synthetic support artifact generation as proof.

3. **Scope and generated-roadmap truth gate**
   - Scope: compose `Test-SnapshotExportScope.ps1`,
     `Export-BeadsRoadmap.ps1`, and `BeadsPublicRoadmapTests` into the final
     Phase 412/413 validation checklist owned by the sync agent.
   - Validation:
     `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`,
     `pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1`, and
     `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore`.
   - Non-goals: no hand edits to `.beads/issues.jsonl` or
     `docs/ROADMAP.generated.md`.

4. **CI contention cleanup**
   - Scope: document or enforce sequential local verification for demo build
     and focused tests, and avoid CI output collisions where multiple jobs share
     the same build artifacts in a single runner workspace.
   - Validation: rerun the selected focused Phase 412 commands in the intended
     order on a clean worktree after restore.
   - Non-goals: no broad rearchitecture of build output unless a real
     contention failure is reproduced.

## Candidate Minimal Phase 413 Slices

1. **Composed final validation record**
   - Scope: run Phase 410, 411, and 412 focused gates plus
     `scripts/Test-SnapshotExportScope.ps1`, then record exact pass/fail output
     in Phase 413 verification.
   - Validation: use the final command list selected by Phase 412, plus
     `git diff --check`.

2. **Beads and generated-roadmap sync**
   - Scope: close/update v2.61 beads, run `bd export -o .beads\issues.jsonl`,
     regenerate `docs/ROADMAP.generated.md`, and run
     `BeadsPublicRoadmapTests`.
   - Validation:
     `bd context --json`,
     `bd ready --json`,
     `pwsh -NoProfile -File scripts\Export-BeadsRoadmap.ps1`,
     and
     `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore`.

3. **Archive and handoff closure**
   - Scope: move completed v2.61 phase artifacts into the milestone archive,
     verify branch/worktree state, and produce handoff with blockers and push
     status.
   - Validation:
     `git status --short`,
     `git log --oneline -5`,
     `bd vc status` or `bd dolt status` if available in this bd version, then
     the project-required final `bd dolt push`/`git push` sequence when not
     overridden by a task-specific "do not push" instruction.

## Non-Goals

- No product, demo, test, README, generated roadmap, Beads export, or roadmap
  state edits in this Phase 409C inventory.
- No CI workflow edits in this evidence-only slice.
- No compatibility layer, old chart controls, direct public `Source` API,
  backend expansion, hidden fallback/downshift, PDF/vector export, generic
  plotting workbench, fake benchmark guarantee, or synthetic runtime evidence.
- No broad repository verification run is required for this inventory-only
  file; the required local verification is markdown diff hygiene and git
  status.

## Final Sync Expectations

- This inventory file should be committed alone on branch `v2.61-phase409-ci`.
- This task explicitly stops before push.
- The synchronizing Phase 413 agent should reconcile Beads live state,
  `.beads/issues.jsonl`, `docs/ROADMAP.generated.md`, phase archive state,
  branch/worktree cleanup, Dolt push, and Git push unless a later instruction
  narrows that closeout.
- Parallel Phase 409 inventory agents own disjoint files. Do not revert,
  overwrite, or merge their work from this worktree.

## Blockers

None for this inventory. Phase 412 implementation should wait for the Phase 410
and Phase 411 outputs because the roadmap says Phase 412 is blocked by both.
