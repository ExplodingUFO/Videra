---
phase: 375
plan: 375-PLAN
type: auto
autonomous: true
wave: 5
depends_on: [371, 372, 373, 374]
---

# Phase 375: Integration and Verification

## Objective

Wire all interactivity features (crosshair, tooltips, probe strategies, keyboard/toolbar) into the snapshot export path, consumer smoke, and probe evidence contracts. Verify no regression to existing chart rendering across all five chart families.

## Context

- Phases 371-374 are complete: crosshair overlay, enhanced tooltips, series probe strategies, keyboard/toolbar controls
- Snapshot export via `Plot3D.CaptureSnapshotAsync` renders offscreen — must suppress interaction chrome
- `SurfaceChartProbeEvidenceFormatter` creates probe evidence from `SurfaceProbeInfo` — must work with new strategies
- Consumer smoke app loads surface, bar, and contour series — must validate interactivity
- Existing tests: 6 crosshair, 11 tooltip, 12 probe strategy, 12 keyboard/toolbar = 41 new tests across phases

## Tasks

### Task 1: Add IsSnapshotMode to SurfaceChartOverlayCoordinator

**Type:** auto
**Description:** Add `IsSnapshotMode` property to `SurfaceChartOverlayCoordinator`. When true, `Render()` skips crosshair, hovered probe, and toolbar rendering. Axis and legend render normally (they are chart content, not interaction chrome).

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs`

**Implementation:**
```csharp
public bool IsSnapshotMode { get; set; }

public void Render(DrawingContext context, SurfaceChartProjection? chartProjection)
{
    ArgumentNullException.ThrowIfNull(context);

    SurfaceAxisOverlayPresenter.Render(context, AxisState);
    SurfaceLegendOverlayPresenter.Render(context, LegendState);

    if (!IsSnapshotMode)
    {
        SurfaceProbeOverlayPresenter.Render(context, ProbeState, _viewSize, chartProjection);
        SurfaceCrosshairOverlayPresenter.Render(context, CrosshairState);
        SurfaceChartToolbarOverlayPresenter.Render(context, ToolbarState, _pointerScreenPosition);
    }
    else
    {
        // In snapshot mode, render only pinned probes (intentional data markers)
        // Skip hovered probe, crosshair, and toolbar (interaction chrome)
        SurfaceProbeOverlayPresenter.RenderPinnedOnly(context, ProbeState, _viewSize, chartProjection);
    }
}
```

**Verification:** Build passes, existing overlay tests pass.

**Commit:** `feat(375-PLAN): add IsSnapshotMode to suppress interaction chrome in snapshot export`

---

### Task 2: Add RenderPinnedOnly to SurfaceProbeOverlayPresenter

**Type:** auto
**Description:** Add `RenderPinnedOnly()` method to `SurfaceProbeOverlayPresenter` that renders only pinned probe readouts, skipping the hovered probe bubble. This allows snapshot exports to include intentional data markers while excluding transient interaction chrome.

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`

**Verification:** Build passes, existing probe overlay tests pass.

**Commit:** `feat(375-PLAN): add RenderPinnedOnly for snapshot-mode probe rendering`

---

### Task 3: Wire IsSnapshotMode into Plot3D.CaptureSnapshotAsync

**Type:** auto
**Description:** In `Plot3D.CaptureSnapshotAsync`, set `IsSnapshotMode = true` on the overlay coordinator before rendering offscreen, and restore it after. This ensures snapshot exports don't capture interaction chrome.

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`

**Implementation approach:**
- `Plot3D` needs access to the overlay coordinator (via `VideraChartView`)
- Add internal method `SetSnapshotMode(bool)` on `VideraChartView` that delegates to coordinator
- Call `SetSnapshotMode(true)` before render, `SetSnapshotMode(false)` after

**Verification:** Build passes, snapshot tests pass.

**Commit:** `feat(375-PLAN): wire IsSnapshotMode into snapshot capture path`

---

### Task 4: Add Probe Evidence Compatibility Test

**Type:** auto
**Description:** Create integration test that exercises `SurfaceChartProbeEvidenceFormatter.Create()` with `SurfaceProbeInfo` from each probe strategy type (surface, scatter, bar, contour). Verifies the evidence contract works unchanged with enhanced probes.

**Files to create:**
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeEvidenceCompatibilityTests.cs`

**Test cases:**
1. Surface probe info creates valid evidence
2. Scatter probe info creates valid evidence
3. Bar probe info creates valid evidence
4. Contour probe info creates valid evidence
5. Mixed probe types (hovered + pinned from different strategies) create valid evidence
6. Evidence format output is deterministic

**Verification:** All 6 tests pass.

**Commit:** `test(375-PLAN): add probe evidence compatibility tests for all series types`

---

### Task 5: Add Regression Guardrail Tests

**Type:** auto
**Description:** Create integration tests that verify all five chart families can be added to a plot, refresh the overlay coordinator, and produce probe evidence without errors. This is the regression guardrail for the v2.54 milestone.

**Files to create:**
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRegressionGuardrailTests.cs`

**Test cases:**
1. Surface series overlay refresh succeeds
2. Waterfall series overlay refresh succeeds
3. Scatter series overlay refresh succeeds
4. Bar series overlay refresh succeeds
5. Contour series overlay refresh succeeds
6. Multi-series (surface + bar + contour) overlay refresh succeeds
7. Crosshair state is empty in snapshot mode
8. Toolbar state is empty in snapshot mode

**Verification:** All 8 tests pass.

**Commit:** `test(375-PLAN): add regression guardrail tests for all chart families`

---

### Task 6: Update Consumer Smoke with Interactivity Validation

**Type:** auto
**Description:** Extend the consumer smoke app's support summary to include interactivity validation fields. Add crosshair/tooltip/probe status reporting.

**Files to modify:**
- `smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs`

**Fields to add to support summary:**
- `InteractivityCrosshairEnabled: true`
- `InteractivityTooltipOffset: (12, -12)`
- `InteractivityProbeStrategies: Surface, Bar, Contour`
- `InteractivityKeyboardShortcuts: enabled`
- `InteractivityToolbarButtons: enabled`

**Files to modify for test assertions:**
- `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs`

**Verification:** Consumer smoke configuration test passes with new fields.

**Commit:** `feat(375-PLAN): add interactivity validation to consumer smoke support summary`

---

### Task 7: Update Requirements Tracking

**Type:** auto
**Description:** Mark INT-01, INT-02, INT-03, VER-01, VER-02, VER-03 as complete in REQUIREMENTS.md. Update traceability table.

**Files to modify:**
- `.planning/REQUIREMENTS.md`

**Verification:** Requirements table shows all 22 requirements complete.

**Commit:** `docs(375-PLAN): mark integration and verification requirements complete`

---

### Task 8: Run Full Test Suite

**Type:** auto
**Description:** Run all SurfaceCharts integration tests to verify zero regressions. Run `dotnet test` on the integration test project.

**Verification:** All tests pass (existing + new). Zero regressions.

**Commit:** None (verification only).

---

## Verification Criteria

1. `IsSnapshotMode` flag exists on coordinator and suppresses interaction chrome
2. `RenderPinnedOnly()` method exists on probe presenter
3. Snapshot capture sets and restores snapshot mode
4. 6 probe evidence compatibility tests pass
5. 8 regression guardrail tests pass
6. Consumer smoke includes interactivity fields
7. All 22 requirements marked complete
8. Full test suite passes with zero regressions

## Success Criteria

1. Snapshot export captures clean chart output without crosshair, tooltip, or toolbar chrome
2. Consumer smoke validates crosshair and tooltip rendering without errors
3. Existing `SurfaceChartProbeEvidence` contract works with the enhanced probe system
4. Beads state reflects milestone progress throughout all phases
5. Existing chart rendering (surface, waterfall, scatter, bar, contour) shows no visual regression
