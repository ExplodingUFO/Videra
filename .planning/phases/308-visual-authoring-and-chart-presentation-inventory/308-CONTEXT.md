# Phase 308 Context: Visual Authoring and Chart Presentation Inventory

## Bead

`Videra-5f3`

## Goal

Inventory the existing scene-authoring, instance-batch, diagnostics, SurfaceCharts overlay, formatter, color-map, and precision seams before implementing v2.42 product work.

## Decision

Keep v2.42 bounded to a static-scene and chart-presentation vertical slice:

- No animation, skeletons, shadows, post-processing, material graph, ECS, WebGL/OpenGL, generic chart engine, compatibility layer, or hidden fallback/downshift work.
- Scene authoring should stay data-oriented and build on `SceneDocument`, `MeshPrimitive`, `MaterialInstance`, and `InstanceBatchDescriptor`.
- Chart presentation should stay chart-local under SurfaceCharts and must not push chart semantics into `VideraView`.
