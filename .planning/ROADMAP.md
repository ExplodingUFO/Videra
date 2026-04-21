# Roadmap

## Active Milestone

- `v1.21 Scene and Material Runtime v1`
- Status: `defined locally on 2026-04-21`
- Focus: introduce backend-neutral scene/material runtime contracts without reopening package-boundary work or widening into PBR, animation, or backend expansion

### Phase 107: Scene Node and Mesh Primitive Contracts

**Goal:** introduce stable scene-graph and mesh-primitive contracts that separate node identity, hierarchy, transforms, and shared geometry ownership.
**Depends on:** `v1.20` viewer/runtime product boundary and slimmed package layering
**Plans:** 3 planned

Plans:

- [ ] 107-01: define stable `SceneNode` contracts with backend-neutral identity, parent/child relationships, and local transform semantics
- [ ] 107-02: define `MeshPrimitive` references so nodes can attach geometry without collapsing node identity, shared payload reuse, and scene hierarchy into one object
- [ ] 107-03: adapt the existing shared-geometry and residency paths to the new scene-node/mesh-primitive model without introducing backend-specific public scene objects

### Phase 108: Material, Texture, and Sampler Assets

**Goal:** introduce backend-neutral material and texture assets that are explicit runtime objects rather than importer-specific or backend-shaped data carriers.
**Depends on:** Phase 107
**Plans:** 3 planned

Plans:

- [ ] 108-01: define `MaterialInstance` as a backend-neutral runtime asset separated from mesh data and scene-node identity
- [ ] 108-02: define `Texture2D` and `Sampler` assets with explicit ownership and reuse semantics independent from importer-specific types
- [ ] 108-03: thread material, texture, and sampler assets through the current viewer/runtime composition path without widening backend-specific public APIs

### Phase 109: Render Feature Vocabulary and Runtime Bridge Integration

**Goal:** make scene/material participation explicit through one render-feature vocabulary that the runtime, passes, and host bridges can share.
**Depends on:** Phase 108
**Plans:** 3 planned

Plans:

- [ ] 109-01: define one stable render-feature vocabulary for `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`
- [ ] 109-02: align runtime/pass composition with the new feature vocabulary instead of ad hoc pass-local feature assumptions
- [ ] 109-03: keep the viewer/runtime bridge explicit and direct rather than adding fallback layers or speculative abstraction seams

### Phase 110: Docs, Samples, and Repository Truth for Scene/Material Runtime

**Goal:** make docs, samples, and repository guards teach the same scene/material runtime model that the code ships.
**Depends on:** Phase 109
**Plans:** 3 planned

Plans:

- [ ] 110-01: update README, architecture docs, and package guidance to explain the scene/material runtime model in viewer-first terms
- [ ] 110-02: align demo/sample truth with the new scene/material contracts without leaking backend-specific types into public examples
- [ ] 110-03: add or update repository guards so docs, samples, and code stay aligned on the new runtime model

## Operational Baseline

- `v1.20` fixed the `1.0` viewer/runtime product boundary and slimmed package story across `Core`, `Import`, `Backend`, `UI adapter`, and `Charts`
- the next gap is broader runtime/product depth, not another round of boundary/package clarification
- `v1.21` is intentionally limited to runtime contracts and repository truth; static glTF/PBR breadth, advanced rendering, and second-UI validation remain later milestones

## Recently Shipped

- `v1.20 Viewer Product Boundary and Core Slimming` — shipped `2026-04-21`; phases `103-106`; requirements `9/9`; audit `passed`; archive: `.planning/milestones/v1.20-ROADMAP.md`, `.planning/milestones/v1.20-REQUIREMENTS.md`, `.planning/milestones/v1.20-MILESTONE-AUDIT.md`
- `v1.18 SurfaceCharts Analytics Core` — shipped `2026-04-20`; phases `95-98`; requirements `11/11`; audit `tech_debt`; archive: `.planning/milestones/v1.18-ROADMAP.md`, `.planning/milestones/v1.18-REQUIREMENTS.md`, `.planning/milestones/v1.18-MILESTONE-AUDIT.md`
- `v1.16 SurfaceCharts Adoption Surface` — completed locally `2026-04-20`; phases `87-90`; requirements `9/9`; archive snapshot: `.planning/milestones/v1.16-ROADMAP.md`, `.planning/milestones/v1.16-REQUIREMENTS.md`; audit artifact not yet captured at transition
- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
