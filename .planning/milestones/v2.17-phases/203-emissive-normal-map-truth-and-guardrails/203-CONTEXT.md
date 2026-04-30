# Phase 203 Context

## Why This Phase Exists

`v2.17` Phase 201 and Phase 202 already changed the shipped static-scene path:

- emissive now contributes to retained vertex-color inputs on the shipped path
- normal-texture input now perturbs retained vertex normals when tangent data is present
- the bounded repo-owned desktop proof hosts now exercise that slice and keep the explicit `10`-second survival rule

The remaining gap is repository truth. Multiple docs and guardrail tests still describe emissive and normal-map-ready inputs as retained-only runtime truth, which is now stale.

## Scope

Update the narrow wording layer only:

- root product docs and module READMEs
- Chinese mirrors that restate the same shipped-path truth
- repository guardrail tests and demo-contract tests that lock the old wording

## Non-Goals

- no renderer/runtime/product code changes
- no native-validation workflow rewiring
- no broader lighting, shader, backend, or material-system claims
- no package/install-surface changes

## Required Truth

The shipped static-scene viewer/runtime path now consumes:

- baseColor texture sampling
- occlusion texture binding/strength
- `KHR_texture_transform` and texture-coordinate override where requested
- emissive inputs on the bounded shipped path
- normal-map-ready inputs on the bounded shipped path

This still does **not** imply:

- a broader lighting system
- general shader/backend normal mapping claims
- shadows, environment maps, post-processing, animation, or broader engine/runtime breadth
