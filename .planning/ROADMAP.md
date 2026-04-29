# Roadmap

## v2.54 Chart Interactivity — In Progress

**Goal:** Add interactive capabilities to SurfaceCharts — crosshair, tooltips, mouse-driven probe, zoom/pan controls.

**Phases:** 5 (371–375)
**Requirements:** 22 (XHAIR-01..04, TOOL-01..04, PROBE-01..04, KB-01..04, INT-01..03, VER-01..03)

## Phases

- [ ] **Phase 371: Crosshair Overlay** — Projected ground-plane guidelines with axis-value readout following mouse position
- [ ] **Phase 372: Enhanced Tooltips** — Multi-series-aware rich tooltips with edge-avoidance positioning
- [ ] **Phase 373: Series Probe Strategies** — Mouse-driven probe for scatter, bar, and contour series via extensible strategy interface
- [ ] **Phase 374: Keyboard & Toolbar Controls** — Keyboard shortcuts, zoom/pan toolbar buttons, and cursor feedback
- [ ] **Phase 375: Integration and Verification** — Snapshot compatibility, consumer smoke, probe evidence, and regression validation

## Phase Details

### Phase 371: Crosshair Overlay
**Goal:** Users see projected ground-plane guidelines with axis coordinate values at the probe position, following mouse movement with minimal overlay overhead
**Depends on**: Nothing (existing infrastructure)
**Requirements**: XHAIR-01, XHAIR-02, XHAIR-03, XHAIR-04
**Success Criteria** (what must be TRUE):
  1. User sees two projected guidelines (X + Z) on the ground plane that follow the mouse cursor in real time
  2. Axis coordinate values are displayed as pills at the guideline endpoints (chart edges)
  3. User can toggle crosshair visibility on/off per chart instance via `SurfaceChartOverlayOptions`
  4. Crosshair updates on pointer move without triggering a full overlay coordinator rebuild
**Plans**: TBD

### Phase 372: Enhanced Tooltips
**Goal:** Users see detailed, multi-series-aware data tooltips when hovering over chart elements, positioned to avoid chart-edge clipping
**Depends on**: Nothing (existing infrastructure)
**Requirements**: TOOL-01, TOOL-02, TOOL-03, TOOL-04
**Success Criteria** (what must be TRUE):
  1. User sees a tooltip with series name, world coordinates, and data value when hovering over chart elements
  2. Tooltip shows values from all series at the same X/Z position (multi-series awareness)
  3. Tooltip repositions itself automatically to stay within chart bounds (no edge clipping)
  4. Tooltip follows the pointer with a configurable offset from the cursor
**Plans**: TBD

### Phase 373: Series Probe Strategies
**Goal:** Each chart series type (scatter, bar, contour) supports mouse-driven probe with type-appropriate resolution via an extensible strategy interface
**Depends on**: Nothing (existing infrastructure)
**Requirements**: PROBE-01, PROBE-02, PROBE-03, PROBE-04
**Success Criteria** (what must be TRUE):
  1. Hovering over a scatter chart shows the nearest data point via snap-to-nearest probe
  2. Hovering over a bar chart shows the bar value and category
  3. Hovering over a contour chart shows the iso-line value at the cursor position
  4. Probe resolution dispatches through `ISeriesProbeStrategy` so new series kinds can add custom probe behavior
**Plans**: TBD

### Phase 374: Keyboard & Toolbar Controls
**Goal:** Users can control zoom, pan, and camera reset via keyboard shortcuts and on-chart toolbar buttons, with cursor feedback during interactions
**Depends on**: Nothing (existing infrastructure)
**Requirements**: KB-01, KB-02, KB-03, KB-04
**Success Criteria** (what must be TRUE):
  1. User can zoom in/out with +/- keys, reset camera with Home, and fit to data with F
  2. User can pan the chart view using arrow keys (left/right/up/down)
  3. Zoom/pan toolbar buttons are rendered as overlay controls on the chart
  4. Cursor shape changes during hover, drag, and zoom operations to provide interaction feedback
**Plans**: TBD

### Phase 375: Integration and Verification
**Goal:** All interactivity features coexist with existing snapshot export and probe evidence contracts, with no regression to existing chart rendering
**Depends on**: Phase 371, Phase 372, Phase 373, Phase 374
**Requirements**: INT-01, INT-02, INT-03, VER-01, VER-02, VER-03
**Success Criteria** (what must be TRUE):
  1. Snapshot export captures clean chart output without crosshair, tooltip, or toolbar chrome
  2. Consumer smoke validates crosshair and tooltip rendering without errors
  3. Existing `SurfaceChartProbeEvidence` contract works with the enhanced probe system
  4. Beads state reflects milestone progress throughout all phases
  5. Existing chart rendering (surface, waterfall, scatter, bar, contour) shows no visual regression
**Plans**: TBD

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 371. Crosshair Overlay | 1/1 | Complete | 2026-04-29 |
| 372. Enhanced Tooltips | 0/TBD | Not started | - |
| 373. Series Probe Strategies | 1/1 | Complete | 2026-04-29 |
| 374. Keyboard & Toolbar Controls | 0/TBD | Not started | - |
| 375. Integration and Verification | 0/TBD | Not started | - |

---

<details>
<summary>✅ v2.53 Chart Type Expansion and Axis Semantics — Complete (2026-04-29)</summary>

Shipped Bar/Contour chart types, Log/DateTime axes, custom formatters, multi-series legend. 5 phases (366–370), 25 requirements. Archived: `.planning/milestones/v2.53-ROADMAP.md`

</details>

<details>
<summary>✅ v2.52 Professional Chart Snapshot Export — Complete (2026-04-29)</summary>

Shipped chart-local PNG/bitmap snapshot export. 5 phases, 23 requirements. Archived: `.planning/milestones/v2.52-ROADMAP.md`

</details>
