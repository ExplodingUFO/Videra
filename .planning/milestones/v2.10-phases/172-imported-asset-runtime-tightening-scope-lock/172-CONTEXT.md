# Phase 172 Context: Imported Asset Runtime Tightening Scope Lock

## Milestone

`v2.10 Imported Asset Runtime Tightening`

## Goal

Lock `v2.10` to imported-asset/runtime tightening before implementation starts.

## Boundary

This milestone narrows the existing viewer/runtime hot path:

- imported asset truth to runtime upload/render truth
- primitive-level opaque and transparent participation
- scene delta change vocabulary
- upload queue coalescing and prioritization
- primitive-centric docs, diagnostics, and repository guardrails

## Decisions

- The milestone must stay inside the current viewer/runtime product boundary.
- The milestone must not add animation, lighting, shadows, environment maps, post-processing, extra UI adapters, new chart families, or backend/API breadth.
- The milestone must not introduce compatibility shims, migration adapters, fallback runtime paths, or transitional contracts.
- Primitive-level participation should tighten the existing runtime path instead of widening into a new transparency system.
- Scene delta and upload queue work should become more granular without forking backend-rebind and normal upload behavior.
- Later renderer consumption of broader static glTF/PBR metadata remains out of scope for `v2.10`.

## Existing Evidence

- `v2.4` already unified scene/runtime truth around `SceneDocument`, imported assets, upload, render, and backend rebind.
- `v2.5` already shipped per-primitive non-`Blend` material participation plus broader static imported-material truth.
- `SceneObjectFactory` still guards mixed `Blend` and non-`Blend` primitives at the object level.
- `SceneDeltaPlanner` still uses one coarse reupload signal.
- `SceneUploadQueue` still uses object-centric queue truth.

## Deferred

- Broader renderer/material consumption of imported static glTF/PBR metadata remains deferred to a future milestone.
- SurfaceCharts performance and interaction tuning remains deferred to a future milestone.
- Package slimming and importer installation-surface reshaping remain deferred to a future milestone.
