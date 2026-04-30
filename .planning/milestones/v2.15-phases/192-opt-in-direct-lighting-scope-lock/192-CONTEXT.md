# Phase 192 Context: Opt-In Direct Lighting Scope Lock

## Why now

`v2.14` finished the second-host validation slice. The next roadmap-aligned gap is not another host shell; it is the first bounded advanced-runtime slice on the stable static-scene viewer/runtime path.

The important discovery for `v2.15` is that Videra already carries a style-driven direct-lighting seam internally:

- `LightingParameters`
- `RenderStyleParameters.ToUniformData()`
- `StyleUniformData`
- `RenderStylePresets`
- style uniform upload through `ResourceLifetimeRegistry` / `VideraEngine`
- native shader consumption on Windows and macOS

So the milestone should not invent a new lighting framework. It should productize one small direct-lighting baseline honestly.

## Scope

- lock `v2.15` to one opt-in direct-lighting slice only
- keep lighting as a style/uniform concern on the existing static-scene viewer/runtime path
- use one repo-owned proof path and diagnostics story
- require Windows desktop proof apps used for validation to stay alive for `10` seconds without crashing

## Proposed product truth

- one directional light only
- current Blinn-Phong-style ambient/diffuse/specular math only
- no new scene-light runtime model
- no shadows, environment maps, post-processing, animation, or broader advanced-runtime systems
- no public package reshaping in this milestone

## Expected implementation seam

- smallest code gap appears to be native parity on the Vulkan path
- Windows and macOS already consume the style-driven lighting inputs
- software fallback can remain unlit unless proof/verification shows that fallback parity is required to make the bounded baseline honest

## Non-goals

- no shadow maps or light lists
- no generic lighting/material graph
- no environment maps or post-processing
- no animation/skeleton/morph/mixer work
- no new public runtime package line
- no compatibility shims or transitional contracts
