# Requirements: v2.53 Chart Type Expansion and Axis Semantics

## v1 Requirements

### Axis Semantics

- [ ] **AXIS-01**: Plot supports log scale Y axis via `SurfaceAxisScaleKind.Log` — unblocks the existing enum value and implements log tick generation
- [ ] **AXIS-02**: Plot supports DateTime X axis via `SurfaceAxisScaleKind.DateTime` — auto-formats ticks as time labels
- [ ] **AXIS-03**: User can set custom tick formatters per axis — `Func<double, string>` delegate for custom label formatting
- [ ] **AXIS-04**: Log scale validates minimum > 0 and returns explicit diagnostic for zero/negative values
- [ ] **AXIS-05**: DateTime axis stores values as UTC seconds (long) to avoid double precision loss

### Chart Legend

- [x] **LEG-01**: Chart displays a legend overlay showing all series with labels and kind-specific indicators (rectangles for surface/waterfall, dots for scatter, bars for bar chart, lines for contour)
- [x] **LEG-02**: Legend position is configurable (top-left, top-right, bottom-left, bottom-right)
- [x] **LEG-03**: Legend respects series visibility — hidden series are excluded from legend

### Bar Chart

- [ ] **BAR-01**: `Plot.Add.Bar(double[] values)` adds a vertical bar chart series to the plot
- [ ] **BAR-02**: Bar chart renders as 3D rectangular prisms in scene space with configurable color
- [ ] **BAR-03**: Bar chart supports grouped bars (multiple series side by side)
- [ ] **BAR-04**: Bar chart supports stacked bars (multiple series stacked vertically)
- [ ] **BAR-05**: Bar chart validates data (rejects empty arrays, NaN values)

### Contour Plot

- [ ] **CTR-01**: `Plot.Add.Contour(double[,] values)` adds a contour plot series from 2D scalar field data
- [ ] **CTR-02**: Contour plot extracts iso-lines using marching squares algorithm
- [ ] **CTR-03**: Contour lines render as 3D line geometry in scene space
- [ ] **CTR-04**: Contour plot supports configurable iso-level count (number of contour lines)
- [ ] **CTR-05**: Contour extraction caches results and only re-extracts when data changes

### Integration

- [ ] **INT-01**: All new chart types produce output evidence via existing `Plot3DOutputEvidence` contract
- [ ] **INT-02**: All new chart types produce dataset evidence via existing `Plot3DDatasetEvidence` contract
- [ ] **INT-03**: Demo exposes sample data and UI for each new chart type
- [ ] **INT-04**: Consumer smoke validates new chart types render without error

### Verification

- [ ] **VER-01**: Beads state reflects milestone progress throughout
- [ ] **VER-02**: Each phase has isolated execution with clean verification
- [ ] **VER-03**: Guardrails prevent regression of existing surface/waterfall/scatter behavior

## Future Requirements

- Real-time data streaming (DataLogger/DataStreamer pattern)
- Multi-chart layout (subplots)
- Crosshair and tooltip interactivity
- Performance optimization for large datasets
- Theme system and dark mode
- SVG/vector export
- Additional chart types (Radar, Polar, Box, Error Bars)

## Out of Scope

| Exclusion | Reason |
|-----------|--------|
| 2D-only chart rendering | Videra is a 3D engine; charts render in 3D scene space |
| WebGL/OpenGL backend | Stays on existing D3D11/Vulkan/Metal backends |
| Compatibility wrappers | New chart types use Plot.Add.* pattern, no legacy APIs |
| Hidden fallback/downshift | Unsupported features return explicit diagnostics |
| God-code aggregation | Each chart type is its own series kind, not a monolithic renderer |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| AXIS-01 | Phase 366 | Pending |
| AXIS-02 | Phase 366 | Pending |
| AXIS-03 | Phase 366 | Pending |
| AXIS-04 | Phase 366 | Pending |
| AXIS-05 | Phase 366 | Pending |
| LEG-01 | Phase 367 | Complete |
| LEG-02 | Phase 367 | Complete |
| LEG-03 | Phase 367 | Complete |
| BAR-01 | Phase 368 | Pending |
| BAR-02 | Phase 368 | Pending |
| BAR-03 | Phase 368 | Pending |
| BAR-04 | Phase 368 | Pending |
| BAR-05 | Phase 368 | Pending |
| CTR-01 | Phase 369 | Pending |
| CTR-02 | Phase 369 | Pending |
| CTR-03 | Phase 369 | Pending |
| CTR-04 | Phase 369 | Pending |
| CTR-05 | Phase 369 | Pending |
| INT-01 | Phase 370 | Pending |
| INT-02 | Phase 370 | Pending |
| INT-03 | Phase 370 | Pending |
| INT-04 | Phase 370 | Pending |
| VER-01 | Phase 370 | Pending |
| VER-02 | Phase 370 | Pending |
| VER-03 | Phase 370 | Pending |
