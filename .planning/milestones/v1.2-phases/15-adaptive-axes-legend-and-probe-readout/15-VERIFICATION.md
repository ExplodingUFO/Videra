# Phase 15 Verification

**Phase:** 15  
**Name:** adaptive-axes-legend-and-probe-readout  
**Date:** 2026-04-14  
**Status:** passed

## Verification Scope

Verified against the shipped branch reality, not the stale later-branch plan assumptions:

- chart camera/projection seam: `Viewport` plus internal `UpdateProjectionSettings(...)`
- overlay ownership seam: chart-local `SurfaceChartView` partials and `Videra.SurfaceCharts.Avalonia` presenters/services
- pinned probe interaction seam: hover plus `Shift + LeftClick` on `SurfaceChartView`

## Goal Verdict

**Phase goal achieved.**

The branch now delivers:

- adaptive chart-local X/Y/Z axes
- truthful legend range handling
- chart-local hover and pinned probe readouts
- deterministic core/integration/repository regression coverage
- no leakage of Phase-15 overlay behavior into `VideraView`

## Requirement Evidence

### AXIS-01 - adaptive X/Y/Z axes

**Verdict:** satisfied for the shipped Phase-15 scope.

Code evidence:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs`
  - `Create(...)` builds a shared camera-aware projection and fitted screen transform from scene geometry plus chart-bound anchor points.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs`
  - painter calls `SurfaceChartProjection.Create(...)`, so overlays and triangles share the same projection math.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs`
  - control render path also creates/stores `SurfaceChartProjection` with chart-bounds anchors and camera projection settings.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs`
  - builds explicit X/Y/Z axis state from metadata
  - formats X/Z titles with units when present
  - uses a nice-step sequence `{1, 2, 2.5, 5} * 10^n` for adaptive tick spacing
  - selects the displayed chart edge from projected front-corner candidates, so edge placement changes with camera yaw/pitch
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
  - computes and renders axis state inside `SurfaceChartView`, without any `VideraView` overlay dependency

Test evidence:

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`
  - `AxisOverlay_UsesMetadataLabelsAndLegendRange`
  - `AxisOverlay_SwitchesDisplayedEdges_WhenCameraYawChanges`
  - `AxisOverlay_TicksStayMonotonicAndLegendTracksColorMapRange_AfterViewChanges`

Notes:

- `REQUIREMENTS.md` says "adaptive major/minor ticks", but the shipped Phase-15 implementation and tests lock adaptive tick generation through a single explicit tick set. That is not blocking the stated phase goal, but it is broader wording than the implemented contract.

### AXIS-02 - truthful legend range handling

**Verdict:** satisfied for the shipped branch truth.

Code evidence:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`
  - uses `colorMap.Range` when an explicit `ColorMap` is active
  - falls back to `metadata.ValueRange` only for the fallback color map path
  - maps swatches through `colorMap.Map(...)`, so labels and colors share the same value-range truth
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
  - passes `ColorMap is not null` into legend-state creation, preserving the explicit-vs-fallback distinction

Test evidence:

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`
  - `AxisOverlay_UsesMetadataLabelsAndLegendRange` checks legend min/max text against the active color-map range
  - `AxisOverlay_TicksStayMonotonicAndLegendTracksColorMapRange_AfterViewChanges` re-checks legend range after viewport/projection changes

Notes:

- The current branch does not expose separate clipping or value-transform concepts on `SurfaceColorMap`, so Phase 15 truth here is the active range contract rather than a richer transform pipeline.

### PROBE-01 - chart-local hover probe with truthful X/Y/Z plus Approx/Exact

**Verdict:** satisfied.

Code evidence:

- `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs`
  - carries `SampleX`, `SampleY`, `AxisX`, `AxisY`, `Value`, and `IsApproximate`
  - `FromResolvedSample(...)` maps sample coordinates into axis space from metadata width/height and marks coarse tiles as approximate when tile bounds span more samples than the tile grid
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs`
  - clamps viewport and screen-space input to valid sample-space coordinates
  - resolves the most detailed loaded tile covering the probe point
  - routes final mapping through the core `SurfaceProbeInfo` helper
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`
  - renders hover readout text with axis-space X/Y, sample-space X/Y, value, and literal `Approx` / `Exact`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
  - probe state is computed chart-locally through `SurfaceProbeService`

Test evidence:

- `tests/Videra.SurfaceCharts.Core.Tests/Picking/SurfaceProbeInfoTests.cs`
  - min/mid/max sample-to-axis mapping coverage
  - coarse-vs-exact approximation coverage
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs`
  - `ProbeOverlay_ConvertsSampleCoordinatesToAxisValues`
  - `ProbeOverlay_ClampsViewportAndReportsAxisTruth`
  - `ProbeUpdate_ViewportFocusChange_RecomputesAxisValues`
  - `ProbeOverlay_FlagsApproximateWhenTileDensityIsCoarse`

### PROBE-02 - pinned/highlight probe workflow

**Verdict:** satisfied.

Code evidence:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs`
  - tracks `Shift + LeftClick` with a `4` pixel travel threshold for pin toggling
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs`
  - routes pointer move/press/release through the chart-local interaction seam
  - toggles the currently hovered probe into or out of the pinned collection
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
  - stores pinned probes as chart-local sample-space requests on `SurfaceChartView`
  - recomputes pinned probe truth through the shared probe service
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`
  - renders pinned markers and pinned readout bubbles with `Approx` / `Exact` state

Test evidence:

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs`
  - `PinnedProbe_ShiftClick_TogglesProbeBubble`
  - `PinnedProbe_SurvivesViewportAndProjectionChanges_WithStableAxisTruth`

### Sibling Boundary / No Leakage Into VideraView

**Verdict:** satisfied.

Code evidence:

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`
  - Phase-15 overlay state stays inside `SurfaceChartView`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`
  - `SurfaceChartAxisProbeOverlay_ShouldStayOutOfVideraView` asserts that `VideraView.cs` and `VideraView.Overlay.cs` do not contain `SurfaceAxisOverlayPresenter`, `SurfaceLegendOverlayPresenter`, `SurfaceProbeService`, `SurfaceProbeInfo`, or `Videra.SurfaceCharts`
- fresh repository grep on `src/Videra.Avalonia/Controls/VideraView.cs` and `src/Videra.Avalonia/Controls/VideraView.Overlay.cs` returned no matches for those symbols

## Fresh Verification Commands

Executed during this verification pass:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceProbeInfoTests"
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceAxisOverlayTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests"
rg -n "SurfaceAxisOverlayPresenter|SurfaceLegendOverlayPresenter|SurfaceProbeService|SurfaceProbeInfo|Videra\\.SurfaceCharts" src/Videra.Avalonia/Controls/VideraView.cs src/Videra.Avalonia/Controls/VideraView.Overlay.cs
```

Results:

- core probe tests: `4/4` passed
- Avalonia integration tests: `13/13` passed
- repository architecture tests: `4/4` passed
- `rg` leakage check: no matches

## Human Verification

No blocking human verification is required for the stated phase goal.

Optional non-blocking follow-up:

- visual UX spot-check in the demo or a headful harness for label crowding/readability under extreme camera angles and small viewports

## Residual Risks / Gaps

No blocking functional gaps were found against the stated phase goal.

Residual notes:

1. `REQUIREMENTS.md` still says "adaptive major/minor ticks", while the shipped implementation/test matrix locks adaptive tick generation through a single tick tier.
2. `AXIS-02` wording mentions clipping/value transform, but this branch's `SurfaceColorMap` model exposes only range + palette. Phase 15 is truthful within that shipped contract.

