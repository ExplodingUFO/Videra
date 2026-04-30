# Phase 425 Plan: Analysis Workspace and Streaming Inventory

## Goal

Map current SurfaceCharts multi-chart, linked interaction, streaming,
high-density, cookbook, package-smoke, CI, and release-readiness surfaces before
implementation.

## Beads

### 425A: Chart APIs and Workspace Seams

Bead: `Videra-7tqx.1.1`

Write scope:

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425A-API-WORKSPACE-INVENTORY.md`

Validation:

```powershell
rg -n "VideraChartView|Plot\\.Add|LinkViewWith|SelectionReported|TryCreate" src tests samples docs README.md
git diff --check
```

### 425B: Demo Cookbook and Package Templates

Bead: `Videra-7tqx.1.2`

Write scope:

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425B-DEMO-COOKBOOK-TEMPLATE-INVENTORY.md`

Validation:

```powershell
rg -n "Cookbook|Recipe|ConsumerSmoke|support summary|SurfaceCharts.Demo" samples smoke docs tests README.md
git diff --check
```

### 425C: Streaming, High-Density, and Performance Truth

Bead: `Videra-7tqx.1.3`

Write scope:

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425C-STREAMING-PERFORMANCE-INVENTORY.md`

Validation:

```powershell
rg -n "DataLogger3D|Streaming|HighPerformance|Cache|Benchmark|PerformanceLab" src samples tests docs README.md
git diff --check
```

### 425D: CI, Release-Readiness, and Guardrails

Bead: `Videra-7tqx.1.4`

Write scope:

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425D-CI-RELEASE-GUARDRAIL-INVENTORY.md`

Validation:

```powershell
rg -n "ReleaseReadiness|Invoke-ReleaseReadiness|ConsumerSmoke|ROADMAP.generated|SnapshotExportScope|guardrail" .github scripts tests docs README.md
git diff --check
```

## Integration

After the four inventory branches return:

1. Merge each branch into `master`.
2. Close child beads after verifying each inventory file exists.
3. Write `425-SUMMARY.md` with Phase 426-430 dependency and safe
   parallelization guidance.
4. Write `425-VERIFICATION.md` with validation evidence and status `passed`.
5. Update `.planning/ROADMAP.md` and `.planning/STATE.md`.
6. Regenerate `docs/ROADMAP.generated.md`.
7. Close `Videra-7tqx.1` only after all child beads are closed and validation
   passes.

## Success Criteria

1. Current multi-chart, linked interaction, streaming/high-density, cookbook,
   package-smoke, CI, and release-readiness owners are mapped.
2. Candidate work is classified as native workflow API, demo/sample scenario,
   performance evidence, CI/release truth, or out-of-scope expansion.
3. Phase 426-430 write sets, dependencies, validation commands, and safe
   worktree split points are identified.
