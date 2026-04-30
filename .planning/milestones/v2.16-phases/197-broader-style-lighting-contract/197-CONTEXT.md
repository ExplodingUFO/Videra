# Phase 197 Context: Broader Style-Lighting Contract

## Why this phase exists

`Phase 196` locked `v2.16` to the narrowest follow-on advanced-runtime slice after direct lighting: broaden lighting through the existing style/uniform seam, not through new runtime abstractions.

The key existing seam is already shared across native backends:

- `src/Videra.Core/Styles/Parameters/LightingParameters.cs`
- `src/Videra.Core/Styles/Parameters/StyleUniformData.cs`
- `src/Videra.Core/Styles/Parameters/RenderStyleParameters.cs`
- `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- `src/Videra.Platform.Linux/VulkanResourceFactory.cs`
- `src/Videra.Platform.macOS/Shaders.metal`

The current gap is not “can the repo carry lighting inputs?” It can. The gap is “can the repo ship one slightly richer lighting baseline without inventing shadows, environment sampling, or fullscreen effects?”

The narrowest concrete contract is a one-scalar fill/wrap extension:

- add `LightingParameters.FillIntensity` as one `float`
- map it into `StyleUniformData` at offset `28`
- use it to soften the existing diffuse term instead of introducing a second light direction or a hemisphere model

## Planned scope

- one bounded broader-lighting contract on the existing static-scene path
- shader/uniform only across Windows, Linux, and macOS
- no new texture/resource-set/render-target abstraction
- no emissive or normal-map renderer consumption in this phase

## Planned proof boundary

- proof hosts stay repository-owned
- desktop-host `10`-second survival wiring belongs to the next proof phase, not this contract phase

## Risks to avoid

- silently turning the change into a generic scene-light model
- overcommitting to a two-light or hemisphere contract when one scalar can carry the fill baseline
- widening backend contracts to support shadows/environment maps before the repo is ready
- mixing material-texture shading work into a uniform-only milestone
