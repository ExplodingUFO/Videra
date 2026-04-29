# Requirements: v2.54 Chart Interactivity

## v1 Requirements

### Crosshair

- [x] **XHAIR-01**: Crosshair overlay follows mouse position showing projected ground-plane guidelines through the probe point
- [x] **XHAIR-02**: Crosshair displays axis coordinate values at the guideline endpoints (axis-value pills)
- [x] **XHAIR-03**: Crosshair visibility is configurable (on/off per chart instance)
- [x] **XHAIR-04**: Crosshair uses lightweight overlay path — bypasses full overlay rebuild on pointer move

### Tooltips

- [x] **TOOL-01**: Enhanced tooltip shows detailed data values when hovering over chart elements (series name, coordinates, value)
- [x] **TOOL-02**: Tooltip supports multi-series awareness — shows values from all series at the same X/Z position
- [x] **TOOL-03**: Tooltip positions itself to avoid clipping at chart edges
- [x] **TOOL-04**: Tooltip follows pointer with configurable offset

### Series Probe

- [x] **PROBE-01**: Scatter series supports mouse-driven probe via nearest-point lookup
- [x] **PROBE-02**: Bar series supports mouse-driven probe showing bar value and category
- [x] **PROBE-03**: Contour series supports mouse-driven probe showing iso-line value at cursor
- [x] **PROBE-04**: Probe resolution uses `ISeriesProbeStrategy` interface for extensibility per series kind

### Keyboard & Controls

- [x] **KB-01**: Keyboard shortcuts for zoom in/out (+/-), reset view (Home), fit to data (F)
- [x] **KB-02**: Arrow keys for pan (left/right/up/down)
- [x] **KB-03**: Zoom/pan toolbar buttons rendered as overlay controls
- [x] **KB-04**: Cursor feedback — changes cursor shape during hover, drag, zoom operations

### Integration

- [x] **INT-01**: All interactivity features work with existing snapshot export (no interference)
- [x] **INT-02**: Consumer smoke validates crosshair and tooltip rendering
- [x] **INT-03**: Existing probe evidence contract (`SurfaceChartProbeEvidence`) works with enhanced probes

### Verification

- [x] **VER-01**: Beads state reflects milestone progress throughout
- [x] **VER-02**: Each phase has isolated execution with clean verification
- [x] **VER-03**: Existing chart rendering shows no regression

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
| XHAIR-01 | Phase 371 | Complete |
| XHAIR-02 | Phase 371 | Complete |
| XHAIR-03 | Phase 371 | Complete |
| XHAIR-04 | Phase 371 | Complete |
| TOOL-01 | Phase 372 | Complete |
| TOOL-02 | Phase 372 | Complete |
| TOOL-03 | Phase 372 | Complete |
| TOOL-04 | Phase 372 | Complete |
| PROBE-01 | Phase 373 | Complete |
| PROBE-02 | Phase 373 | Complete |
| PROBE-03 | Phase 373 | Complete |
| PROBE-04 | Phase 373 | Complete |
| KB-01 | Phase 374 | Complete |
| KB-02 | Phase 374 | Complete |
| KB-03 | Phase 374 | Complete |
| KB-04 | Phase 374 | Complete |
| INT-01 | Phase 375 | Complete |
| INT-02 | Phase 375 | Complete |
| INT-03 | Phase 375 | Complete |
| VER-01 | Phase 375 | Complete |
| VER-02 | Phase 375 | Complete |
| VER-03 | Phase 375 | Complete |
