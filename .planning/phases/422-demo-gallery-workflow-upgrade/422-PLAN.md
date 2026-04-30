# Phase 422 Plan: Demo Gallery Workflow Upgrade

## Goal

Upgrade the SurfaceCharts demo into a descriptor-driven, recipe-first gallery
while keeping the work bounded to demo/catalog/support responsibilities.

## Execution Order

### 422A: Demo Scenario Descriptors

Bead: `Videra-j3z.1`

Scope:

- Add a demo-owned scenario descriptor table for visible source paths.
- Drive source selection and cookbook recipe routing by scenario id instead of
  duplicated integer indexes.
- Cover descriptor/catalog alignment in sample tests.

Validation:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests
git diff --check
```

### 422B: MainWindow Responsibility Split

Blocked by 422A. Reduce demo code-behind ownership without changing visible
workflow semantics.

### 422C: Scatter Selector Affordance

Blocked by 422A. Make scatter scenario selection visibly scoped to scatter.

### 422D: Support Summary Snapshot State

Blocked by 422A. Clarify snapshot state in support-summary workflow while
keeping PNG-only export boundaries.

### 422E: Demo Coverage Map

Blocked by 422B and 422D. Document visible demo path coverage against recipes
and tests.

### 422F: Consumer Smoke Truth Alignment

Blocked by 422A. Keep packaged smoke truth separate from demo UX and decide
whether supplemental feature status belongs in the smoke artifacts.

## Phase Close

Phase 422 closes after all child beads are complete, focused tests pass, Beads
state is exported, and generated roadmap/planning state are synchronized.
