# Phase 375: Integration and Verification — Context

## Goal

All interactivity features coexist with existing snapshot export and probe evidence contracts, with no regression to existing chart rendering.

## Requirements

- **INT-01**: Snapshot export captures clean chart output without crosshair, tooltip, or toolbar chrome
- **INT-02**: Consumer smoke validates crosshair and tooltip rendering
- **INT-03**: Existing `SurfaceChartProbeEvidence` contract works with enhanced probes
- **VER-01**: Beads state reflects milestone progress throughout
- **VER-02**: Each phase has isolated execution with clean verification
- **VER-03**: Existing chart rendering shows no regression

## Key Decisions

### 1. Snapshot Export Suppresses Interaction Chrome

When `CaptureSnapshotAsync` is called, the overlay coordinator must suppress:
- Crosshair guidelines and axis-value pills
- Hovered probe readout bubbles
- Toolbar overlay buttons

**Approach:** Add `IsSnapshotMode` property to `SurfaceChartOverlayCoordinator`. When true, the `Render()` method skips crosshair, probe (hovered only — pinned probes are intentional data), and toolbar rendering. Axis and legend render normally as they are chart content, not interaction chrome.

### 2. Consumer Smoke Validates Interactivity

The existing consumer smoke app (`Videra.SurfaceCharts.ConsumerSmoke`) already loads surface, bar, and contour series. We extend it to:
- Validate crosshair overlay options are respected
- Validate probe evidence can be created for all series types
- Report interactivity status in the support summary

### 3. Probe Evidence Contract Compatibility

`SurfaceChartProbeEvidenceFormatter.Create()` takes `SurfaceProbeInfo?` and `IReadOnlyList<SurfaceProbeInfo>`. The new probe strategies (scatter, bar, contour) produce `SurfaceProbeInfo` via `ISeriesProbeStrategy.TryResolve()`. Since `SurfaceProbeInfo` is the shared contract, the evidence formatter works unchanged — no modifications needed to the evidence contract itself.

**Verification:** Create a test that exercises `SurfaceChartProbeEvidenceFormatter.Create()` with probe info from each strategy type.

### 4. Regression Guardrail Tests

Add integration tests that verify all five chart families (surface, waterfall, scatter, bar, contour) render without error:
- Each family can be added to a plot
- Overlay coordinator can refresh with each family
- Probe evidence can be created for each family
- Snapshot can be captured (if render bridge is available)

## Architecture

### Files to Create

1. **`SurfaceChartIntegrationTests.cs`** — Integration tests for:
   - Snapshot mode suppresses interaction chrome
   - Probe evidence compatibility with all series types
   - Regression guardrail for all chart families

### Files to Modify

1. **`SurfaceChartOverlayCoordinator.cs`** — Add `IsSnapshotMode` property, suppress interaction overlays in `Render()` when snapshot mode is active
2. **`Plot3D.cs`** — Set `IsSnapshotMode` on coordinator before snapshot capture, restore after
3. **`MainWindow.axaml.cs`** (ConsumerSmoke) — Add interactivity validation to support summary
4. **`SurfaceChartsConsumerSmokeConfigurationTests.cs`** — Update assertions for new interactivity fields

### Success Criteria

1. Snapshot export captures clean chart output without crosshair, tooltip, or toolbar chrome
2. Consumer smoke validates crosshair and tooltip rendering without errors
3. Existing `SurfaceChartProbeEvidence` contract works with the enhanced probe system
4. Beads state reflects milestone progress throughout all phases
5. Existing chart rendering (surface, waterfall, scatter, bar, contour) shows no visual regression

## Requirements Traceability

- **INT-01**: IsSnapshotMode flag on coordinator suppresses interaction overlays during snapshot
- **INT-02**: Consumer smoke extended with interactivity validation
- **INT-03**: Probe evidence formatter test with all series types
- **VER-01**: Beads state updates
- **VER-02**: Phase 371-374 all have verification files (already done)
- **VER-03**: Regression guardrail tests for all chart families
