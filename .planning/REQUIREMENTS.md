# Requirements: v2.54 Chart Interactivity

## v1 Requirements

### Crosshair

- [ ] **XHAIR-01**: Crosshair overlay follows mouse position showing projected ground-plane guidelines through the probe point
- [ ] **XHAIR-02**: Crosshair displays axis coordinate values at the guideline endpoints (axis-value pills)
- [ ] **XHAIR-03**: Crosshair visibility is configurable (on/off per chart instance)
- [ ] **XHAIR-04**: Crosshair uses lightweight overlay path — bypasses full overlay rebuild on pointer move

### Tooltips

- [ ] **TOOL-01**: Enhanced tooltip shows detailed data values when hovering over chart elements (series name, coordinates, value)
- [ ] **TOOL-02**: Tooltip supports multi-series awareness — shows values from all series at the same X/Z position
- [ ] **TOOL-03**: Tooltip positions itself to avoid clipping at chart edges
- [ ] **TOOL-04**: Tooltip follows pointer with configurable offset

### Series Probe

- [ ] **PROBE-01**: Scatter series supports mouse-driven probe via nearest-point lookup
- [ ] **PROBE-02**: Bar series supports mouse-driven probe showing bar value and category
- [ ] **PROBE-03**: Contour series supports mouse-driven probe showing iso-line value at cursor
- [ ] **PROBE-04**: Probe resolution uses `ISeriesProbeStrategy` interface for extensibility per series kind

### Keyboard & Controls

- [ ] **KB-01**: Keyboard shortcuts for zoom in/out (+/-), reset view (Home), fit to data (F)
- [ ] **KB-02**: Arrow keys for pan (left/right/up/down)
- [ ] **KB-03**: Zoom/pan toolbar buttons rendered as overlay controls
- [ ] **KB-04**: Cursor feedback — changes cursor shape during hover, drag, zoom operations

### Integration

- [ ] **INT-01**: All interactivity features work with existing snapshot export (no interference)
- [ ] **INT-02**: Consumer smoke validates crosshair and tooltip rendering
- [ ] **INT-03**: Existing probe evidence contract (`SurfaceChartProbeEvidence`) works with enhanced probes

### Verification

- [ ] **VER-01**: Beads state reflects milestone progress throughout
- [ ] **VER-02**: Each phase has isolated execution with clean verification
- [ ] **VER-03**: Existing chart rendering shows no regression

## Future Requirements

- Configurable keyboard shortcuts
- Touch gesture support (pinch zoom, two-finger pan)
- Selection box (rubber band) for zoom-to-region
- Animated zoom/pan transitions
- Probe history / comparison mode

## Out of Scope

| Exclusion | Reason |
|-----------|--------|
| Touch gestures | Desktop-first; touch deferred to future milestone |
| Animated transitions | Performance-critical; defer to profiling |
| Selection box | Complexity vs value; defer |
| New chart types | Covered in v2.53 |
| Backend changes | All features are presentation-layer overlays |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| XHAIR-01 | Phase 371 | Pending |
| XHAIR-02 | Phase 371 | Pending |
| XHAIR-03 | Phase 371 | Pending |
| XHAIR-04 | Phase 371 | Pending |
| TOOL-01 | Phase 372 | Pending |
| TOOL-02 | Phase 372 | Pending |
| TOOL-03 | Phase 372 | Pending |
| TOOL-04 | Phase 372 | Pending |
| PROBE-01 | Phase 373 | Pending |
| PROBE-02 | Phase 373 | Pending |
| PROBE-03 | Phase 373 | Pending |
| PROBE-04 | Phase 373 | Pending |
| KB-01 | Phase 374 | Pending |
| KB-02 | Phase 374 | Pending |
| KB-03 | Phase 374 | Pending |
| KB-04 | Phase 374 | Pending |
| INT-01 | Phase 375 | Pending |
| INT-02 | Phase 375 | Pending |
| INT-03 | Phase 375 | Pending |
| VER-01 | Phase 375 | Pending |
| VER-02 | Phase 375 | Pending |
| VER-03 | Phase 375 | Pending |
