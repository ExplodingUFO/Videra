# Roadmap

## Active Milestone

- `v1.20 Viewer Product Boundary and Core Slimming`
- Status: `Phase 103-105 complete locally on 2026-04-21; Phase 106 is next`
- Focus: keep the viewer/runtime `1.0` boundary fixed while extracting concrete importer and logging dependencies out of `Videra.Core`

### Phase 103: Viewer Product Boundary and Capability Matrix

**Goal:** freeze `Videra 1.0` as a native desktop viewer / inspection / surface-chart product and make the deferred engine-`2.0` line explicit.
**Depends on:** current viewer/runtime foundation from `v1.18` and the existing Avalonia-first product surface
**Plans:** 3 planned

Plans:

- [x] 103-01: rewrite product positioning and package guidance so `Videra 1.0` is clearly framed as a native desktop viewer/runtime rather than a generic engine
- [x] 103-02: publish a capability matrix that separates shipped viewer/runtime capabilities from explicitly deferred engine-`2.0` features
- [x] 103-03: define one package-layer matrix for `Core`, `Import`, `Backend`, `UI adapter`, and `Charts`

Success criteria:

1. A new consumer can tell from docs/package guidance that Videra is not trying to be a Three.js-style general runtime in `1.0`.
2. Shipped vs deferred capabilities are spelled out in one explicit matrix instead of being inferred from scattered docs.
3. The layer vocabulary for `Core` / `Import` / `Backend` / `UI adapter` / `Charts` is consistent across docs and planning.

Notes:

- `docs/capability-matrix.md` is now the canonical `1.0` boundary document and is linked from the repository README, docs index, package matrix, architecture doc, and `Videra.Core` README.
- The repository truth now explicitly states that the `1.0` line is a native desktop viewer/runtime plus inspection and source-first `SurfaceCharts`, not a general engine/runtime parity effort.
- A focused repository-truth test now guards the capability/layer matrix and the corresponding entry-doc links.

### Phase 104: Core Dependency Extraction and Logging Split

**Goal:** slim `Videra.Core` into a true runtime core by removing concrete importer and logging provider dependencies.
**Depends on:** Phase 103
**Plans:** 3 planned

Plans:

- [x] 104-01: remove concrete importer package references from `Videra.Core` without breaking runtime composition
- [x] 104-02: remove concrete Serilog provider dependencies from `Videra.Core` and keep runtime composition on logging abstractions only
- [x] 104-03: prove the slimmed core still builds and composes through abstractions on the existing viewer and package paths

Success criteria:

1. `Videra.Core.csproj` no longer directly references `SharpGLTF.Toolkit`.
2. `Videra.Core.csproj` no longer directly references concrete `Serilog` provider packages.
3. Existing viewer/runtime and packaged consumer paths still compile and run through explicit abstraction-driven composition.

Notes:

- `Videra.Core` no longer directly references `SharpGLTF.Toolkit` or concrete `Serilog` provider packages.
- glTF and OBJ parsing now live in `Videra.Import.Gltf` and `Videra.Import.Obj`, and `Videra.Avalonia` consumes them transitively for the default scene-loading APIs.
- The public package/publish truth, consumer-smoke pack list, and package validation flow now include the dedicated import packages, so the extracted boundary is shippable instead of only source-local.

### Phase 105: Import Package and Hosting Abstractions

**Goal:** stabilize import-package composition and host seams so runtime core, backends, UI adapters, and charts remain independently explainable.
**Depends on:** Phase 104
**Plans:** 3 planned

Plans:

- [x] 105-01: define the canonical import-package composition story now that glTF and OBJ import live outside `Videra.Core`
- [x] 105-02: tighten the hosting-boundary docs and public API guards so runtime core, backends, UI adapters, and charts can be described independently without new abstraction layers
- [x] 105-03: keep the Avalonia path thin while proving the new seams do not widen backend-specific public API

Success criteria:

1. glTF and OBJ import packages are part of the canonical viewer composition story instead of reading like hidden implementation baggage.
2. Hosting seams are concrete enough that `Core`, `Import`, `Backend`, `UI adapter`, and `Charts` can each be explained as separate layers.
3. The Avalonia shell remains thin and does not become the place where new product coupling leaks back in.

Notes:

- `docs/hosting-boundary.md` is now the canonical seam-ownership document for the shipped viewer stack and is linked from the repository entry docs.
- Reflection- and project-graph-based repository guards now prove that `Videra.Avalonia` stays a thin public shell and that `Videra.Import.*` packages remain `Core`-based.
- The chart-shell guard now scans all public `VideraView` partials instead of only `VideraView.cs`, closing a real repository-test blind spot.

### Phase 106: Package Truth and Layering Validation

**Goal:** lock the new product boundary into consumer truth and automated validation.
**Depends on:** Phase 105
**Plans:** 3 planned

Plans:

- [ ] 106-01: update README, architecture docs, and package guidance to teach the canonical slimmed package stack
- [ ] 106-02: add or upgrade consumer smoke, repository guards, and package-matrix checks for the new layering
- [ ] 106-03: close the milestone with support/release truth that matches the shipped viewer-first package boundary

Success criteria:

1. Consumer-facing docs describe the same canonical package stack that the code actually ships.
2. Repository guards or smoke tests fail when the new layering regresses.
3. Support/release truth no longer relies on users reverse-engineering `Videra.Core` internals to understand what belongs where.

## Operational Baseline

- the current repository already has native backends, software fallback, scene truth, upload/residency services, inspection workflow seams, and a source-first `SurfaceCharts` stack; the next gap is product/package clarity, not another engine rewrite
- the highest-leverage next move is to define `1.0` viewer/runtime boundaries before widening into scene/material runtime breadth, static PBR, or second-UI validation

## Recently Shipped

- `v1.18 SurfaceCharts Analytics Core` — shipped `2026-04-20`; phases `95-98`; requirements `11/11`; audit `tech_debt`; archive: `.planning/milestones/v1.18-ROADMAP.md`, `.planning/milestones/v1.18-REQUIREMENTS.md`, `.planning/milestones/v1.18-MILESTONE-AUDIT.md`
- `v1.16 SurfaceCharts Adoption Surface` — completed locally `2026-04-20`; phases `87-90`; requirements `9/9`; archive snapshot: `.planning/milestones/v1.16-ROADMAP.md`, `.planning/milestones/v1.16-REQUIREMENTS.md`; audit artifact not yet captured at transition
- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
