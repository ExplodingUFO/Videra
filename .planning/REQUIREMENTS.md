# Requirements: Videra v1.19 SurfaceCharts Presentation Space and Interaction Defaults

**Defined:** 2026-04-20
**Core Value:** 跨平台 3D 渲染引擎的可靠性

## v1 Requirements

Requirements committed for the `v1.19` milestone. Each requirement maps to exactly one roadmap phase.

### Display Space

- [ ] **DISP-01**: Consumer can apply a display-space transform that changes rendered horizontal/depth/value proportions without mutating underlying metadata, scalar fields, probe values, or support-summary truth.
- [ ] **DISP-02**: Consumer can choose a built-in presentation-oriented display preset that reduces “long strip” aspect ratios and increases perceived vertical relief for wide/shallow datasets.
- [ ] **DISP-03**: `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` preserve the active display-space transform instead of resetting the chart back to raw-axis proportions.

### Overlay and Axis Semantics

- [ ] **OVRL-01**: Consumer can choose chart-local overlay defaults that make the value axis visually prominent through grid-plane and axis-side behavior.
- [ ] **OVRL-02**: Chart-facing overlay formatting can address axes by semantic role (`Horizontal`, `Value`, `Depth`, `Legend`) instead of only raw `"X"`, `"Y"`, `"Z"` keys.
- [ ] **OVRL-03**: Auto grid-plane behavior can prefer value-emphasizing or camera-aware placement instead of always collapsing to the `XZ` floor plane.

### Interaction Defaults

- [ ] **INTR-01**: Consumer can configure orbit and pan direction inversion through a public interaction-options contract instead of hard-coded controller signs.
- [ ] **INTR-02**: The default interaction preset adopts the current reversed X/Y orbit and pan behavior while preserving a legacy preset for callers that want the previous semantics.

### Demo and Adoption Truth

- [ ] **DEMO-01**: `SurfaceCharts.Demo` opens in a presentation-oriented first-chart view that makes the value axis, dataset proportions, and built-in interaction story legible without manual tweaking.
- [ ] **DEMO-02**: README, demo text, and support-summary truth describe the new display and interaction defaults without over-claiming deeper analytics work that is still deferred.

## v2 Requirements

Deferred after `v1.19`. These remain visible but are not part of the current roadmap.

### Analytics

- **ANLY-01**: User can inspect interpolated probe values with explicit confidence semantics.
- **ANLY-02**: User can enable contour and wireframe overlays on top of the surface.
- **ANLY-03**: User can inspect axis-aligned slices and arbitrary cross-section profiles.

### Series Expansion

- **SERI-01**: User can visualize waterfall-style 3D strip series on top of the shared surface analytics foundation.

## Out of Scope

Explicitly excluded from `v1.19` to keep the milestone focused on presentation defaults and interaction semantics.

| Feature | Reason |
|---------|--------|
| `OpenGL` backend evaluation | Backend expansion still ranks below chart presentation and interaction truth |
| Public `Videra.SurfaceCharts.*` packages | Contracts remain source-first alpha and are still stabilizing |
| Generic `Chart3D` scene abstraction | Still premature without a second concrete 3D series |
| Contour, wireframe, slicing, and multi-probe analytics | Valuable, but should build on a cleaner display-space and interaction baseline first |
| Hard numeric benchmark thresholds | Benchmark review remains label-gated evidence rather than an automatic blocker |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| DISP-01 | Phase 99 | Pending |
| DISP-02 | Phase 99 | Pending |
| DISP-03 | Phase 99 | Pending |
| OVRL-01 | Phase 100 | Pending |
| OVRL-02 | Phase 100 | Pending |
| OVRL-03 | Phase 100 | Pending |
| INTR-01 | Phase 101 | Pending |
| INTR-02 | Phase 101 | Pending |
| DEMO-01 | Phase 102 | Pending |
| DEMO-02 | Phase 102 | Pending |

**Coverage:**
- v1 requirements: `10` total
- Mapped to phases: `10`
- Unmapped: `0` ✓

---
*Requirements defined: 2026-04-20*
*Last updated: 2026-04-20 after initializing the v1.19 milestone*
