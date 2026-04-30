# Phase 420 Plan: Native 3D Feature Convenience APIs

## Goal

Add focused native convenience APIs for two high-value 3D chart workflows first:
bar category labels and explicit contour levels. Keep the implementation direct,
immutable, test-backed, and scoped to the existing `VideraChartView.Plot` model.

## Success Criteria

1. Bar chart data can carry category labels and expose them through native Plot
   authoring/evidence without a broader categorical chart framework.
2. Contour chart data can carry explicit finite levels and extraction uses those
   exact values.
3. Focused Core and Avalonia tests pass for the changed families.
4. No compatibility, old-code, fallback, downshift, or fake-evidence path is
   introduced.

## Execution Split

### 420A: Bar Category Labels

Bead: `Videra-kyy.1`

Worktree/branch: `agents/v263-phase420-bar-labels`

Responsibility boundary:

- Own bar category label data model, convenience overload, and bar evidence.
- Do not edit contour contracts.
- Do not change demo code in this bead.

Expected write scope:

- `src/Videra.SurfaceCharts.Core/BarChartData.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/BarRendererTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`

Validation:

```powershell
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter Bar --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Bar --no-restore
git diff --check
```

### 420B: Contour Explicit Levels

Bead: `Videra-kyy.2`

Worktree/branch: `agents/v263-phase420-contour-levels`

Responsibility boundary:

- Own explicit contour level data model, extraction, convenience overload, and
  contour evidence.
- Do not edit bar contracts.
- Do not add contour labels or annotation rendering.

Expected write scope:

- `src/Videra.SurfaceCharts.Core/ContourChartData.cs`
- `src/Videra.SurfaceCharts.Core/ContourExtractor.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/ContourChartDataTests.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/ContourExtractorTests.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewContourIntegrationTests.cs`

Validation:

```powershell
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter Contour --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Contour --no-restore
git diff --check
```

### 420C: Minimal Series Style Handles

Bead: `Videra-kyy.3`

Defer until 420A/420B integrate. The selected first family must be explicit in
the follow-up handoff so this does not become a broad style system.

## Integration Plan

1. Create two isolated worktrees from current `master`.
2. Run 420A and 420B in parallel workers.
3. Integrate 420A first, then 420B, resolving only the expected shared
   `Plot3DAddApi` and `Plot3DDatasetEvidence` edits.
4. Run combined bar and contour test filters after integration.
5. Close `Videra-kyy.1` and `Videra-kyy.2`; keep `Videra-kyy.3` open for the
   follow-up style handle pass.

## Verification

Phase 420 cannot close until these pass from the main workspace:

```powershell
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter "Bar|Contour" --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "Bar|Contour" --no-restore
git diff --check
bd dep cycles --json
```
