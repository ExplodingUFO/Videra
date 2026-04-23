# Package Matrix

This matrix is the public truth for what Videra publishes, what stays repository-only, and how the current install stories split between the viewer stack and the SurfaceCharts stack.

For the explicit `1.0` product boundary and deferred capability split, use [Videra 1.0 Capability Matrix](capability-matrix.md). For the canonical shipped viewer composition and internal seam ownership, use [Hosting Boundary](hosting-boundary.md).

## Canonical install stories

The canonical public viewer stack is:

1. `Videra.Avalonia`
2. exactly one matching `Videra.Platform.*` package
3. optional `Videra.Import.Gltf` / `Videra.Import.Obj` plus `VideraViewOptions.ModelImporter` when you need importer-backed file loading

The canonical public SurfaceCharts stack is:

1. `Videra.SurfaceCharts.Avalonia`
2. `Videra.SurfaceCharts.Processing` for the surface/cache-backed path
3. optional direct `Videra.SurfaceCharts.Core` consumption when you build chart contracts or custom tile sources without the Avalonia shell

`Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. `Videra.SurfaceCharts.Rendering` is a real public package because the current chart assembly split depends on it, but most consumers should not install it first.

## Package-layer matrix

| Layer | Current mapping | Public status | Notes |
| --- | --- | --- | --- |
| `Core` | `Videra.Core` | Public package | Viewer/runtime kernel |
| `Import` | `Videra.Import.Gltf`, `Videra.Import.Obj` | Public packages | Dedicated file-format ingestion layered on top of `Videra.Core` |
| `Backend` | `Videra.Platform.Windows`, `Videra.Platform.Linux`, `Videra.Platform.macOS` | Public packages | Native graphics implementations |
| `UI adapter` | `Videra.Avalonia` | Public package | Public host-framework shell |
| `Charts` | `Videra.SurfaceCharts.Core`, `Videra.SurfaceCharts.Rendering`, `Videra.SurfaceCharts.Processing`, `Videra.SurfaceCharts.Avalonia` | Public packages | Dedicated chart product line with `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView`, independent from `VideraView` |
| `Chart demo` | `Videra.SurfaceCharts.Demo` | Repository-only | Support-ready chart reference app with `Start here`, `Explore next`, and `Try next` paths, not an installable package |

## Published packages

| Package | Published | Official feed | Preview feed | Audience | Support level | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| `Videra.Core` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-only consumers and backend integrators | `alpha` | Core scene/runtime abstractions and software fallback |
| `Videra.Import.Gltf` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-first consumers that need `.gltf` / `.glb` ingestion | `alpha` | Dedicated glTF / GLB import package layered on `Videra.Core` |
| `Videra.Import.Obj` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-first consumers that need `.obj` ingestion | `alpha` | Dedicated OBJ import package layered on `Videra.Core` |
| `Videra.Avalonia` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Avalonia desktop applications | `alpha` | Main public UI entry package; importer-backed file loading requires explicit `Videra.Import.*` installation plus `VideraViewOptions.ModelImporter` |
| `Videra.Platform.Windows` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Windows hosts | `alpha` | Install with `Videra.Avalonia` on Windows |
| `Videra.Platform.Linux` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Linux hosts | `alpha` | Current native path is X11 plus Vulkan; Wayland uses the `XWayland` bridge |
| `Videra.Platform.macOS` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | macOS hosts | `alpha` | Install with `Videra.Avalonia` on macOS |
| `Videra.SurfaceCharts.Core` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Chart-domain consumers and custom tile-source integrators | `alpha` | Chart-domain contracts, metadata, LOD, probe contracts |
| `Videra.SurfaceCharts.Rendering` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Advanced chart-runtime consumers | `alpha` | Rendering-runtime layer used transitively by `Videra.SurfaceCharts.Avalonia` |
| `Videra.SurfaceCharts.Processing` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Consumers that need pyramid/cache helpers | `alpha` | Add for the surface/cache-backed path |
| `Videra.SurfaceCharts.Avalonia` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Avalonia desktop applications that host `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView` | `alpha` | Main public chart control entry package |

## Repository-only entries

| Entry | Published | Audience | Notes |
| --- | --- | --- | --- |
| `Videra.Demo` | Repository only | Source evaluation | Viewer demo and diagnostics reference |
| `smoke/Videra.WpfSmoke` | Repository only | Validation and support evidence | Windows WPF smoke proof for validation and support evidence on the Avalonia-first public viewer path; not a second public UI package or release path |
| `smoke/Videra.SurfaceCharts.ConsumerSmoke` | Repository only | Validation and support evidence | Packaged SurfaceCharts smoke proof with `surfacecharts-support-summary.txt` |
| `Videra.SurfaceCharts.Demo` | Repository only | Source evaluation and support repro | Reference app for `Start here: In-memory first chart`, `Explore next: Cache-backed streaming`, `Try next: Analytics proof`, `Try next: Waterfall proof`, `Try next: Scatter proof`, and `Copy support summary`; it remains repository-only |
| `Videra.ExtensibilitySample` | Repository only | Contributors and integrators | Narrow public reference for extensibility flow |
| `Videra.InteractionSample` | Repository only | Contributors and integrators | Public sample for controlled interaction and inspection workflows |

## Feed notes

- `nuget.org` is the default public consumer path.
- `GitHub Packages` exists for `preview` / internal validation only.
- Do not treat demos or samples as installable public packages.
- Release-candidate review uses the read-only `Release Dry Run` workflow to pack the package set from `eng/public-api-contract.json`, reuse `scripts/Validate-Packages.ps1`, and upload `release-dry-run-evidence` without pushing assets to either feed.

## Viewer runtime baseline

The public viewer/runtime line still shares one backend-neutral asset catalog across `Videra.Core` and `Videra.Avalonia`: `SceneDocument`, `ImportedSceneAsset`, `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler`.

That viewer baseline remains deliberately scoped to static glTF/PBR with one bounded style-driven broader-lighting baseline on the native static-scene path: `metallic-roughness`, `normal-map-ready`, `tangent-aware`, per-primitive non-Blend material participation, occlusion texture binding/strength, `KHR_texture_transform` offset/scale/rotation plus texture-coordinate override, and repeated unchanged imports that can reuse retained imported scene assets are in scope today. The canonical runtime path may expand one imported entry into multiple internal runtime objects so mixed opaque and transparent primitive participation survives the upload/render bridge, while broader transparency-system breadth remains deferred. The current renderer path consumes baseColor texture sampling plus occlusion texture binding/strength, including `KHR_texture_transform` offset/scale/rotation and texture-coordinate override where those bindings request them. Emissive and normal-map-ready inputs remain retained runtime truth rather than broader renderer/shader/backend shading claims. `animation`, `skeletons`, `morph targets`, broader lighting systems beyond the bounded broader-lighting baseline, `shadows`, `environment maps`, `post-processing`, `extra UI adapters`, and `Wayland/OpenGL/WebGL/backend API expansion` stay outside the current product promise.

The current import/runtime path is also expected to stay stable for retained imported scene assets, so package consumers can reason about asset reuse without treating the chart line or demos as part of the viewer kernel.
