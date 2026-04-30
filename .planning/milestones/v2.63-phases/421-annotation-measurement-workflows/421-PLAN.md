# Phase 421 Plan: Annotation and Measurement Workflows

## Goal

Add bounded native contracts that help hosts turn existing probe/selection
reports into annotation and measurement workflows without making the chart own a
general editor or persisted annotation store.

## Execution Order

### 421A: Annotation Anchor DTOs

Bead: `Videra-b5n.1`

Scope:

- Add a small immutable annotation anchor DTO under the Avalonia overlay
  contracts.
- Add factory methods from `SurfaceProbeInfo` and `SurfaceChartSelectionReport`.
- Add `VideraChartView` convenience methods that create anchors from screen
  positions by using existing probe/selection mapping.
- Cover probe and selection anchors in `SurfaceChartInteractionRecipeTests`.

Validation:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter SurfaceChartInteractionRecipe --no-restore
git diff --check
```

### 421B: Measurement Report Helpers

Blocked by 421A. Use anchor DTOs to compute point-to-point and rectangle
measurement reports. Do not include source-tile statistics unless a separate
bead owns source sampling semantics.

### 421C: Selection Report Event

Blocked by 421A. Add event delivery for immutable selection reports from
built-in gestures while leaving selected state host-owned.

## Phase Close

Phase 421 closes after all three beads have focused tests and the generated
roadmap/Beads state are synchronized.
