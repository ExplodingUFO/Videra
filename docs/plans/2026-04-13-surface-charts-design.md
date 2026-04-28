# Surface Charts Design

**Date:** 2026-04-13

## Context

`Videra` already ships a cross-platform 3D viewer stack for Avalonia with native backends, a software fallback path, public render extensibility, and a controlled interaction surface.

The requested feature is not another `VideraView` mode. It is a separate product surface: a high-performance Avalonia control for offline inspection of very large time-frequency and surface-style datasets.

The control must:

- stay decoupled from `VideraView`
- avoid god-code growth in existing viewer files
- prioritize performance and memory efficiency over MVVM purity
- support overview-first exploration of very large datasets
- remain maintainable enough to evolve into a reusable `Videra` family module

## Goals

- Add a dedicated high-performance surface-chart control for Avalonia
- Target offline analysis of very large `time x frequency -> value` matrices
- Support a primary `3D surface + color map` presentation for spectrogram-like data
- Support both in-memory matrices and preprocessed cache-backed data sources
- Keep the chart module separate from `VideraView` and the current viewer interaction surface
- Make low-resolution overview plus zoomed-in detail a first-class product behavior
- Ship an independent demo application for the chart library
- Keep code comments in English and extend README coverage with explicit boundary guidance

## Non-Goals

- No attempt to turn `VideraView` into a chart host
- No first-release real-time streaming waterfall pipeline
- No first-release general-purpose 3D chart suite with every chart type
- No requirement that the UI layer strictly follow MVVM for high-frequency state
- No first-release dependency on host-owned `Object3D` collections or existing viewer selection contracts

## Product Direction

The recommended product shape is a `Videra` family sibling module:

- `Videra.SurfaceCharts.Core`
- `Videra.SurfaceCharts.Avalonia`
- optional `Videra.SurfaceCharts.Processing`
- independent demo: `Videra.SurfaceCharts.Demo`

This should feel like a separate charting product in the same repository, not like a feature flag on the existing viewer.

## Why Not Extend `VideraView`

Three broad approaches were considered:

1. Add a chart mode directly to `VideraView`
2. Build an independent chart control that still reuses `VideraView` semantics and scene-object contracts
3. Build an independent chart control with its own product API and only limited reuse of low-level rendering infrastructure

The third approach is preferred.

The first approach creates the highest long-term coupling. It would force time-frequency browsing, tile loading, color maps, chart axes, and large-matrix LOD concerns into a viewer shell that is intentionally centered on scene viewing.

The second approach looks cheaper at first, but it still drags chart semantics through `Object3D`, `VideraView`, and viewer-oriented interaction boundaries. That would make maintenance worse without giving a clean product boundary.

The third approach keeps public semantics clean while still allowing the repository to reuse underlying cross-platform rendering infrastructure where that reuse is truly generic.

## Recommended Architecture

The recommended architecture is a four-layer split:

1. `Videra.SurfaceCharts.Core` for chart-domain models, LOD policies, tile contracts, color maps, and viewport logic
2. `Videra.SurfaceCharts.Processing` for pyramid building, offline cache generation, and future native acceleration
3. `Videra.SurfaceCharts.Avalonia` for the dedicated `VideraChartView` control and Avalonia-specific overlay/input behavior
4. Independent demo and docs that show the supported chart workflow without using `VideraView`

## Architectural Boundaries

### 1. `Videra.SurfaceCharts.Core`

This layer should be framework-agnostic and must not depend on Avalonia types.

Responsibilities:

- surface metadata and axis semantics
- viewport and zoom domain models
- tile addressing and tile payload contracts
- LOD policy
- color-map contracts
- picking result contracts
- chart-facing renderer input models

This layer should not know about:

- Avalonia controls
- pointer events
- native handles
- visual tree lifecycle
- view-model patterns

### 2. `Videra.SurfaceCharts.Processing`

This layer should own expensive data preparation work.

Responsibilities:

- build a multi-resolution surface pyramid from a matrix source
- write and read cache-backed tile data
- support very large offline datasets without forcing full in-memory expansion
- provide an isolation boundary for future Rust-backed acceleration

This layer should not know about:

- control lifecycle
- camera state
- Avalonia bindings
- overlay UI

Rust or another native implementation is acceptable here if it improves throughput while preserving a safe boundary. Any such acceleration should remain behind a stable .NET contract.

### 3. `Videra.SurfaceCharts.Avalonia`

This layer should ship the public control surface.

Responsibilities:

- `VideraChartView`
- control lifecycle and host integration
- input interpretation
- camera interaction
- tooltip and overlay presentation
- tile request orchestration from the current viewport

This layer should remain thin. It must not become the home of tile decoding, heavy preprocessing, or cache generation.

### 4. Shared Rendering Infrastructure

The chart modules may reuse low-level cross-platform rendering substrate already proven in the repository, but only where the abstraction is truly generic.

Allowed reuse:

- backend abstractions
- cross-platform buffer and command interfaces
- generic native-host plumbing where applicable
- software fallback support if it can be kept generic

Disallowed coupling:

- direct dependency on `VideraView`
- chart code that assumes viewer selection or annotation semantics
- chart code that depends on `RenderSession`, `VideraViewSessionBridge`, or other viewer-specific orchestration seams

## Data Model

The public input model should use dual entry points with one internal read contract.

### Public Input Shapes

- in-memory matrix source
- cache-backed pyramid source

### Internal Read Contract

Recommended internal abstraction:

- `ISurfaceTileSource`

Recommended supporting models:

- `SurfaceMetadata`
- `SurfaceViewport`
- `SurfaceTileKey`
- `SurfaceTile`
- `SurfaceValueRange`
- `SurfaceAxisDescriptor`

This keeps the control logic independent from whether data originates in RAM or from prebuilt cache files.

## Overview And LOD Strategy

Very large offline data makes a full-resolution single-mesh strategy unacceptable.

The first release should treat multi-resolution browsing as a first-class requirement:

- low-resolution overview loads first
- zooming refines visible detail by level
- only visible tiles plus a small neighborhood remain resident
- in-memory data may build a temporary pyramid internally
- cache-backed data reads only the levels and tiles needed for the viewport

This is not an optimization to add later. It is the architectural center of the chart product.

## Rendering Design

The first release should prioritize a dedicated surface rendering path, not a scene-object path.

Primary display:

- `3D surface + color map`

Deferred or later display modes:

- waterfall / curtain mode
- alternate top-down analysis views

Each tile should render as a regular surface patch. The design should avoid turning each data sample or each tile into a generic `Object3D`.

Recommended rendering characteristics:

- consistent patch topology per tile
- shared index patterns where practical
- value-driven height field
- color-map driven vertex or shader color application
- independent visibility and replacement of low/high LOD tiles

The preferred first-release bias is implementation simplicity with good performance, not maximal rendering sophistication.

## Interaction Model

The chart control should have its own interaction surface.

Recommended first-release interactions:

- orbit / inspect camera
- zoom
- pan
- value probe / tooltip
- optional crosshair or hovered data readout

The chart control should not reuse viewer selection and annotation semantics. If annotations are later required for charts, they should come through chart-specific contracts rather than backflowing through `VideraView`.

## Performance And Memory Direction

The main performance constraints are:

- very large offline matrix sizes
- predictable memory use
- avoiding needless object churn
- limiting GPU uploads to the current working set

The design should therefore prefer:

- dedicated tile data structures over per-sample objects
- tight cache ownership boundaries
- explicit eviction policy
- overview-first rendering
- preprocessed cache formats for large datasets
- optional native acceleration in preprocessing, not in UI orchestration

The control does not need strict MVVM ownership for high-frequency render state. It is acceptable for camera state, requested tiles, and render snapshots to live in dedicated controller/services rather than in a general-purpose bound view model.

## Maintainability Rules

The following constraints are part of the design, not optional implementation taste:

- `VideraChartView` must remain a UI shell, not a god class
- input interpretation, tile scheduling, rendering, cache access, and overlay presentation must be separated
- code comments must be in English
- public APIs should carry XML docs
- README and sample docs must explain that this module is independent from `VideraView`
- simple implementation tasks should be preferred for `gpt-5.4-mini`
- higher-coupling or boundary-defining tasks should be reserved for stronger coding subagents

## Demo And Documentation

The chart library should ship with an independent demo application.

Recommended demo responsibilities:

- load an in-memory matrix example
- load a cache-backed example
- demonstrate overview plus zoomed detail
- expose color-map switching
- show the intended interaction model
- make the chart product boundary obvious

Documentation expectations:

- new module README files
- repository README updates
- demo README
- explicit public boundary notes describing why this is not a `VideraView` mode

## Testing Strategy

### Core Unit Tests

Add headless tests for:

- tile key and viewport math
- LOD selection
- color-map evaluation
- metadata and axis contracts
- picking result translation

### Processing Tests

Add tests for:

- pyramid generation from in-memory matrices
- cache read/write correctness
- overview-level generation
- tile consistency across levels

### Avalonia Integration Tests

Add tests for:

- control initialization
- source switching
- viewport changes requesting tiles
- overview-first load behavior
- overlay/readout behavior

### Repository Guards

Add repository tests that lock:

- README terminology
- module boundaries
- independent demo presence
- documentation consistency

## Risks

- Reusing viewer-specific orchestration classes would quietly erase the separation goal
- Modeling tiles as generic scene objects would degrade memory behavior and future maintainability
- Delaying pyramid/cache design would force a later rewrite for real data sizes
- Over-centralizing camera, tile scheduling, and overlay logic in one control file would create the next god class

## Expected Outcome

If implemented this way, Videra gains a maintainable chart sibling product that:

- supports offline exploration of very large spectrogram-like datasets
- stays decoupled from `VideraView`
- keeps public semantics aligned with charting rather than scene viewing
- can scale from in-memory prototypes to cache-backed large-data workflows
- leaves room for Rust-backed preprocessing without contaminating the UI layer
