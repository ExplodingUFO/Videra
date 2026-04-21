# Videra 1.0 Capability Matrix

This page is the product-boundary truth for `Videra 1.0`.

The `1.0` line is a native desktop viewer/runtime for .NET applications, centered on embeddable viewing, inspection, diagnostics, and the source-first `SurfaceCharts` family. It is not a Three.js-style general runtime, not a game engine, and not a promise to chase `WebGL` / `OpenGL` style API breadth.

## Shipped in the 1.0 Line

| Capability area | Included in `1.0` | Current truth |
| --- | --- | --- |
| Native desktop viewer runtime | Yes | Cross-platform viewer stack for Avalonia hosts with Windows=`D3D11`, Linux=`Vulkan`, and macOS=`Metal` native backends |
| Software fallback and diagnostics | Yes | Viewer path includes software fallback, backend diagnostics, and matching-host validation truth |
| Scene truth and upload/runtime services | Yes | `SceneDocument`, delta planning, residency, upload queueing, and backend rebind/recovery are part of the shipped viewer runtime |
| Dedicated scene import | Yes | `Videra.Import.Gltf` and `Videra.Import.Obj` deliver `.gltf`, `.glb`, and `.obj` ingestion on the viewer/runtime path |
| Viewer-first inspection workflows | Yes | Picking, measurement, clipping, snapshot export, inspection-state capture/restore, and inspection bundles are part of the product surface |
| Narrow public extensibility | Yes | Pass contributors, frame hooks, capability snapshots, and backend diagnostics are part of the shipped viewer contract |
| Source-first `SurfaceCharts` family | Yes, repository-first | `SurfaceChartView`, chart overlays/interaction, processing/cache helpers, and demo/support truth are part of the repo product story but not the current public package line |

## Explicitly Deferred After 1.0

| Capability area | Deferred status | Reason it is not in `1.0` |
| --- | --- | --- |
| General engine/runtime parity | Deferred | The current target is an embeddable viewer/runtime, not a broad engine surface |
| PBR material/runtime breadth | Deferred | Static material/texture runtime deepening belongs after the product/package boundary is stable |
| Animation, skeleton, morph, and mixer APIs | Deferred | These are engine-style feature lines, not part of the current viewer-first scope |
| Lights, shadows, environment maps, transparency sorting, and post-processing | Deferred | Advanced render breadth is intentionally out of the `1.0` promise |
| Extra UI adapters beyond Avalonia | Deferred | Avalonia remains the current public UI adapter while the runtime boundary is tightened |
| Generic `Chart3D` kernel and additional 3D chart families | Deferred | `SurfaceCharts` remains the concrete source-first chart product line for now |
| `WebGL` / `OpenGL` backend pursuit | Deferred | The current native support promise remains `D3D11` / `Vulkan` / `Metal` |

## Package-Layer Matrix

| Layer | Product role | Current repository mapping | Distribution status |
| --- | --- | --- | --- |
| `Core` | Viewer/runtime kernel | `src/Videra.Core` | Public package |
| `Import` | Asset ingestion for viewer/runtime scenes | `src/Videra.Import.Gltf` and `src/Videra.Import.Obj` | Public packages |
| `Backend` | Native graphics implementation | `src/Videra.Platform.Windows`, `src/Videra.Platform.Linux`, `src/Videra.Platform.macOS` | Public packages |
| `UI adapter` | Host-framework shell and orchestration | `src/Videra.Avalonia` | Public package |
| `Charts` | Analytics-oriented chart product family | `src/Videra.SurfaceCharts.*` plus `samples/Videra.SurfaceCharts.Demo` | Source-first, repository-only |

## Reader Guidance

- Start with `Videra.Avalonia` plus one matching `Videra.Platform.*` package when you need the normal viewer path.
- Treat `Videra.Core` as the runtime kernel for core-first integrations, not as evidence that Videra is aiming at full engine breadth.
- Add `Videra.Import.Gltf` and/or `Videra.Import.Obj` when you need direct file-format ingestion without the Avalonia UI shell.
- Treat `Videra.SurfaceCharts.*` as an independent source-first chart family inside the repository, not as part of the current public package promise.
- Use [Package Matrix](package-matrix.md) for distribution truth and [Architecture](../ARCHITECTURE.md) for runtime flow and ownership boundaries.
