# Phase 196 Context: Bounded Broader Lighting Scope Lock

## Why now

`v2.15` already shipped one bounded style-driven direct-lighting baseline on the native static-scene viewer/runtime path. The next roadmap-aligned gap is still on that advanced-runtime line, but it should not jump to shadows, environment maps, or post-processing.

The important discovery for `v2.16` is that the current renderer already has one shared lighting seam:

- `LightingParameters`
- `RenderStyleParameters.ToUniformData()`
- `StyleUniformData`
- `RenderStylePresets`
- native shader consumption on Windows, Linux, and macOS

That means the narrowest honest next slice is to broaden lighting through the existing style/uniform path rather than introduce new texture, resource-set, render-target, or fullscreen-pass abstractions.

## Scope

- lock `v2.16` to one bounded broader-lighting slice only
- keep lighting as a style/uniform concern on the existing static-scene viewer/runtime path
- use repository-owned viewer proof hosts only
- require every desktop program used in milestone validation to stay alive for `10` seconds without crashing

## Proposed product truth

- one modest lighting expansion on top of the current direct-lighting baseline
- no new scene-lighting runtime model
- no shadow maps, environment maps, fullscreen post-processing, or new sampled-environment abstractions
- no emissive or normal-map renderer consumption in this milestone
- no public package reshaping in this milestone

## Expected implementation seam

- the smallest code seam is still the style/uniform path plus native fragment shaders
- proof should stay on `ConsumerSmoke` viewer mode and `WpfSmoke`, because those are the existing desktop proof hosts that already carry the hold seam
- if repository validation entrypoints use those hosts, they should request the `10`-second hold explicitly rather than rely on ad hoc manual proof runs

## Non-goals

- no shadow maps or depth-texture pipeline
- no environment-map sampling
- no post-processing or fullscreen pass framework
- no emissive or normal-map renderer consumption
- no animation / skeleton / morph / mixer work
- no new public runtime package line
- no compatibility shims or transitional contracts
