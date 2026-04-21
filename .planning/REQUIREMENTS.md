# Requirements: Videra v1.20 Viewer Product Boundary and Core Slimming

**Defined:** 2026-04-21
**Core Value:** 跨平台 3D 渲染引擎的可靠性

## v1 Requirements

Requirements committed for the `v1.20` milestone. Each requirement maps to exactly one roadmap phase.

### Product Boundary

- [ ] **BNDR-01**: Consumer can tell from public docs and package guidance that `Videra 1.0` is a native desktop viewer / inspection / surface-chart stack, not a general-purpose game engine or Three.js-style runtime.
- [ ] **BNDR-02**: Consumer can read a capability matrix that separates shipped viewer/runtime capabilities from explicitly deferred engine-`2.0` features.
- [ ] **BNDR-03**: Consumer can read one package-layer matrix that consistently distinguishes `Core`, `Import`, `Backend`, `UI adapter`, and `Charts`.

### Core and Package Structure

- [ ] **CORE-01**: `Videra.Core` no longer depends on concrete importer packages.
- [ ] **CORE-02**: `Videra.Core` no longer depends on concrete logging provider packages.
- [ ] **PKG-01**: glTF and OBJ import functionality ship through dedicated import packages that compose with `Videra.Core`.
- [ ] **PKG-02**: Serilog-based logging ships through a dedicated adapter package instead of being an implicit `Videra.Core` dependency.

### Hosting and Validation

- [ ] **HOST-01**: Host composition seams are explicit enough that runtime core, importers, backends, UI adapters, and charts can be described and consumed independently without hand-waving.
- [ ] **VALD-01**: Consumer-facing docs, smoke paths, and repository guards validate the canonical slimmed package stack and fail when the new layering regresses.

## v2 Requirements

Deferred after `v1.20`. These remain visible but are not part of the current roadmap.

### Scene and Material Runtime

- **RUNT-01**: User can compose scenes through stable `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler` runtime contracts.
- **RUNT-02**: The renderer exposes a stable pass vocabulary for `Opaque`, `Transparent`, `Overlay`, `Picking`, and screenshot-oriented paths.

### glTF and PBR

- **GLTF-01**: Static glTF scenes render with UVs, textures, tangents, metallic-roughness, normal/emissive/alpha handling, and correct sRGB/linear behavior.
- **GLTF-02**: Material/texture upload caching and material-instance reuse are productized for static glTF scenes.

### Performance and Productization

- **PERF-01**: CI fails on agreed performance regressions for first load, first frame, steady-state orbit, interaction latency, allocation budgets, and package size budgets.
- **CHRT-01**: `SurfaceCharts` ships as a first-class public product line with stable package/documentation/smoke truth.
- **CHRT-02**: A shared chart kernel is extracted only after `SurfaceCharts` proves the right reusable contracts.
- **UIAD-01**: A second UI adapter proves the new host seams outside Avalonia.
- **ENGN-01**: Advanced runtime features such as lights, shadows, environment maps, post-processing, and animation remain an opt-in `2.0` line rather than a `1.0` requirement.

## Out of Scope

Explicitly excluded from `v1.20` to keep the milestone focused on product boundary and core/package structure.

| Feature | Reason |
|---------|--------|
| Full Three.js parity | This milestone is about viewer/runtime product definition, not web-engine feature matching |
| `OpenGL` / `WebGL` backend work | Backend expansion still ranks below clarifying the native desktop viewer/runtime boundary |
| Physics, ECS, scripting, editor tools, or plugin marketplace | These would drag Videra toward engine/editor scope before the `1.0` viewer product is even clearly defined |
| Lights, shadows, environment maps, post-processing, transparency sorting, and animation | These belong to later scene/material/runtime milestones after the core/package boundary is stable |
| Generic `Chart3D` kernel or many new 3D chart types | The current priority is to clarify the viewer/runtime core and package stack, not widen chart scope early |
| A second UI adapter in this milestone | Host seams should be defined here, but cross-UI validation comes later |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| BNDR-01 | Phase 103 | Pending |
| BNDR-02 | Phase 103 | Pending |
| BNDR-03 | Phase 103 | Pending |
| CORE-01 | Phase 104 | Pending |
| CORE-02 | Phase 104 | Pending |
| PKG-02 | Phase 104 | Pending |
| PKG-01 | Phase 105 | Pending |
| HOST-01 | Phase 105 | Pending |
| VALD-01 | Phase 106 | Pending |

**Coverage:**
- v1 requirements: `9` total
- Mapped to phases: `9`
- Unmapped: `0` ✓

---
*Requirements defined: 2026-04-21*
*Last updated: 2026-04-21 after initializing the v1.20 milestone*
