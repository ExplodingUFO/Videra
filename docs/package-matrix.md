# Package Matrix

This matrix is the public truth for what Videra publishes, what stays source-only, and what is sample/demo code.

For the explicit `1.0` product boundary and deferred capability split, use [Videra 1.0 Capability Matrix](capability-matrix.md). For the canonical shipped viewer composition and internal seam ownership, use [Hosting Boundary](hosting-boundary.md).

The canonical public viewer stack is:

1. `Videra.Avalonia`
2. exactly one matching `Videra.Platform.*` package
3. optional `Videra.Import.Gltf` / `Videra.Import.Obj` when you need explicit core-path ingestion

## Package-layer matrix

| Layer | Current mapping | Public status | Notes |
| --- | --- | --- | --- |
| `Core` | `Videra.Core` | Public package | Viewer/runtime kernel |
| `Import` | `Videra.Import.Gltf`, `Videra.Import.Obj` | Public packages | Dedicated file-format ingestion layered on top of `Videra.Core` |
| `Backend` | `Videra.Platform.Windows`, `Videra.Platform.Linux`, `Videra.Platform.macOS` | Public packages | Native graphics implementations |
| `UI adapter` | `Videra.Avalonia` | Public package | Public host-framework shell |
| `Charts` | `Videra.SurfaceCharts.*` + `Videra.SurfaceCharts.Demo` | Source-first | Independent chart family, not current public package promise |

## Published packages

| Package | Published | Official feed | Preview feed | Audience | Support level | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| `Videra.Core` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-only consumers and backend integrators | `alpha` | Core scene/runtime abstractions and software fallback |
| `Videra.Import.Gltf` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-first consumers that need `.gltf` / `.glb` ingestion | `alpha` | Dedicated glTF / GLB import package layered on `Videra.Core` |
| `Videra.Import.Obj` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Core-first consumers that need `.obj` ingestion | `alpha` | Dedicated OBJ import package layered on `Videra.Core` |
| `Videra.Avalonia` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Avalonia desktop applications | `alpha` | Main public UI entry package; scene loading flows transitively depend on the import packages |
| `Videra.Platform.Windows` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Windows hosts | `alpha` | Install with `Videra.Avalonia` on Windows |
| `Videra.Platform.Linux` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | Linux hosts | `alpha` | Current native path is X11 plus Vulkan; Wayland uses `XWayland` compatibility |
| `Videra.Platform.macOS` | Yes, on public release tags | `nuget.org` | `GitHub Packages` preview/internal only | macOS hosts | `alpha` | Install with `Videra.Avalonia` on macOS |

## Source-only modules

| Module | Published | Audience | Support level | Notes |
| --- | --- | --- | --- | --- |
| `Videra.SurfaceCharts.Core` | No | Source evaluators and contributors | Source-first `alpha` | Chart-domain contracts, LOD, picking, probe contracts |
| `Videra.SurfaceCharts.Avalonia` | No | Source evaluators and contributors | Source-first `alpha` | `SurfaceChartView`, overlays, built-in chart interaction |
| `Videra.SurfaceCharts.Processing` | No | Source evaluators and contributors | Source-first `alpha` | Cache/pyramid generation, payload sessions, ordered batch reads |

## Samples and demos

| Entry | Published | Audience | Notes |
| --- | --- | --- | --- |
| `Videra.Demo` | Repository only | Source evaluation | Viewer demo and diagnostics reference |
| `Videra.SurfaceCharts.Demo` | Repository only | Source evaluation | Independent demo for the surface-chart module family |
| `Videra.ExtensibilitySample` | Repository only | Contributors and integrators | Narrow public reference for extensibility flow |
| `Videra.InteractionSample` | Repository only | Contributors and integrators | Public sample for controlled interaction and host-owned state |

## Feed notes

- `nuget.org` is the default public consumer path.
- `GitHub Packages` exists for `preview` / internal validation only.
- Do not treat source-only modules or demos as installable public packages.
