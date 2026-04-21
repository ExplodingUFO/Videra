# Requirements: Videra v1.21 Scene and Material Runtime v1

**Defined:** 2026-04-21
**Core Value:** 跨平台 3D 渲染引擎的可靠性

## v1 Requirements

Requirements committed for the `v1.21` milestone. Each requirement will map to exactly one roadmap phase.

### Scene Graph and Geometry

- [ ] **SCN-01**: Maintainer can represent scene graph hierarchy through stable `SceneNode` contracts with backend-neutral identity, parent/child relationships, and local transform semantics.
- [ ] **SCN-02**: Maintainer can attach one or more `MeshPrimitive` references to scene nodes without conflating node identity, geometry ownership, and instance reuse.
- [ ] **SCN-03**: Existing shared-geometry and residency paths can adapt to the new scene-node/mesh-primitive contracts without leaking backend-specific scene objects into the public API.

### Material and Texture Assets

- [ ] **MAT-01**: Maintainer can represent `MaterialInstance` as a backend-neutral runtime asset separated from mesh data and scene-node identity.
- [ ] **MAT-02**: Maintainer can represent `Texture2D` and `Sampler` runtime assets with explicit ownership and reuse semantics independent from importer-specific types.
- [ ] **MAT-03**: The current viewer/runtime composition path can bind material, texture, and sampler assets through explicit contracts without widening backend-specific public APIs.

### Render Feature Vocabulary and Truth

- [ ] **FEAT-01**: Renderer/runtime can classify scene/material participation through stable `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot` feature vocabulary.
- [ ] **TRUTH-01**: Docs, samples, and repository tests describe and validate the new scene/material runtime model consistently.

## Future Requirements

Deferred after `v1.21`. These remain visible but are not part of the current roadmap.

### Static glTF and PBR

- **GLTF-01**: Static glTF scenes render with UVs, textures, tangents, metallic-roughness, normal/emissive/alpha handling, and correct sRGB/linear behavior.
- **GLTF-02**: Material/texture upload caching and material-instance reuse are productized for static glTF scenes.

### Viewer and Productization

- **INSP-01**: Inspection workflows compose directly on top of the new scene/material runtime contracts without special-case scene bridges.
- **UIAD-01**: A second UI adapter proves the new scene/material host seams outside Avalonia.
- **PERF-01**: CI fails on agreed performance regressions and package-size budgets once the runtime model stabilizes.

### Advanced Runtime Features

- **ADV-01**: Lighting, shadows, environment maps, transparent sorting, post-processing, and animation remain an opt-in `2.0` line rather than a `1.x` runtime-core requirement.

## Out of Scope

Explicitly excluded from `v1.21` to keep the milestone focused on scene/material runtime contracts.

| Feature | Reason |
|---------|--------|
| Full static glTF/PBR parity | This milestone should define the runtime contracts first; the richer glTF/PBR surface comes after those contracts stabilize |
| Animation, skeletons, morph targets, or mixers | These belong to a later runtime/features line, not the first scene/material contract milestone |
| New backend work such as `OpenGL` / `WebGL` | Backend expansion still ranks below deepening the native viewer/runtime product |
| A second UI adapter | The host seams should improve here, but cross-UI validation comes later |
| Generic `Chart3D` extraction or new chart product lines | `v1.21` is about viewer/runtime scene/material contracts, not widening the chart surface |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SCN-01 | Phase 107 | Planned |
| SCN-02 | Phase 107 | Planned |
| SCN-03 | Phase 107 | Planned |
| MAT-01 | Phase 108 | Planned |
| MAT-02 | Phase 108 | Planned |
| MAT-03 | Phase 108 | Planned |
| FEAT-01 | Phase 109 | Planned |
| TRUTH-01 | Phase 110 | Planned |

**Coverage:**
- v1 requirements: `8` total
- Mapped to phases: `8`
- Unmapped: `0`

---
*Requirements defined: 2026-04-21*
