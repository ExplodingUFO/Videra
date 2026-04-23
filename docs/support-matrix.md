# Support Matrix

This matrix describes the current support boundary for the Avalonia-first public viewer stack, the public SurfaceCharts package line, and the repository-only demo/sample surfaces.

Use [Videra 1.0 Capability Matrix](capability-matrix.md) for the explicit shipped-vs-deferred product boundary and the `Core` / `Import` / `Backend` / `UI adapter` / `Charts` layer vocabulary. Use [Hosting Boundary](hosting-boundary.md) for the canonical viewer composition rules behind these support entries.

The public viewer install rule stays simple: start with `Videra.Avalonia`, add exactly one matching `Videra.Platform.*` package, and treat `Videra.Import.*` as explicit core-path ingestion packages rather than backend selection knobs. `smoke/Videra.WpfSmoke` is repository-only Windows WPF smoke evidence for that viewer path; it is not a second public UI package or release path.

The public SurfaceCharts install rule is similarly narrow: start with `Videra.SurfaceCharts.Avalonia`, add `Videra.SurfaceCharts.Processing` only for the surface/cache-backed path, and treat `Videra.SurfaceCharts.Core` / `Videra.SurfaceCharts.Rendering` as lower-level package seams rather than the normal starting point.

## Viewer stack

| Surface | Platform | Backend | Package path | Validation truth | Support level |
| --- | --- | --- | --- | --- | --- |
| `Videra.Avalonia` + `Videra.Platform.Windows` | Windows 10+ | Direct3D 11 | Public package | Hosted and local matching-host validation | `alpha` |
| `Videra.Avalonia` + `Videra.Platform.Linux` | Linux | Vulkan | Public package | Hosted `X11` validation plus Wayland-session `XWayland` validation | `alpha` |
| `Videra.Avalonia` + `Videra.Platform.macOS` | macOS 10.15+ | Metal | Public package | Hosted and local matching-host validation | `alpha` |
| `Videra.Core` | Any .NET 8 host | Core abstractions only | Public package | Repository verification and integration tests | `alpha` |
| `Videra.Import.Gltf` | Any .NET 8 host | n/a | Public package | Repository verification plus importer-focused tests | `alpha` |
| `Videra.Import.Obj` | Any .NET 8 host | n/a | Public package | Repository verification plus importer-focused tests | `alpha` |
| Software fallback | Any supported desktop host | Software | Runtime fallback | Covered by repository verification | Diagnostics/fallback only |

## Repository-only viewer proof

| Surface | Platform | Backend | Package path | Validation truth | Support level |
| --- | --- | --- | --- | --- | --- |
| `smoke/Videra.WpfSmoke` | Windows 10+ | Direct3D 11 | Repository only | Local Windows WPF smoke proof plus repository-truth tests | Validation/support evidence only |

## SurfaceCharts stack

| Surface | Distribution | Validation truth | Support level | Notes |
| --- | --- | --- | --- | --- |
| `Videra.SurfaceCharts.Core` | Public package | Core tests and package validation | `alpha` | Chart-domain contracts, tile identities, probe contracts, and LOD selection |
| `Videra.SurfaceCharts.Rendering` | Public package | Rendering-focused tests and package validation | `alpha` | Rendering-runtime layer used transitively by `Videra.SurfaceCharts.Avalonia` |
| `Videra.SurfaceCharts.Processing` | Public package | Processing tests, benchmarks, and package validation | `alpha` | Data-preparation layer for the surface/cache-backed path |
| `Videra.SurfaceCharts.Avalonia` | Public package | Avalonia integration tests, package validation, and demo alignment checks | `alpha` | Public `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` entry package, independent from `VideraView` |
| `Videra.SurfaceCharts.ConsumerSmoke` | Repository only | Windows packaged consumer smoke and `surfacecharts-support-summary.txt` artifact validation | `alpha` | Packaged surface/cache-backed proof for `Videra.SurfaceCharts.Avalonia` + `Videra.SurfaceCharts.Processing` on the supported host path |
| `Videra.SurfaceCharts.Demo` | Repository only | `sample-contract-evidence`, SurfaceCharts runtime evidence, and `Support summary` validation | `alpha` | `Start here: In-memory first chart` first, `Explore next: Cache-backed streaming`, `Try next: Analytics proof`, `Try next: Waterfall proof`, `Try next: Scatter proof`, `Copy support summary`, repository-only reference app |

## Boundary notes

- `VIDERA_BACKEND` and `PreferredBackend` change backend preference only.
- They do not install missing platform packages.
- They do not replace matching-host native validation.
- The built-in backend minimum contract is buffer creation, current-viewer pipeline creation, direct buffer binding, draw calls, viewport/scissor, clear, and standard frame depth behavior with best-effort depth-state toggles.
- `CreateShader(...)`, `CreateResourceSet(...)`, and `SetResourceSet(...)` are not a cross-backend portability promise for the shipped native backends.
- This matrix does not imply an `OpenGL` product promise; the current native support promise remains `D3D11`, `Vulkan`, and `Metal`.
- Linux Wayland uses the documented `XWayland` bridge, not compositor-native embedding.
- Imported-material truth is supported at the imported asset/runtime level only: per-primitive non-Blend material participation, occlusion texture binding/strength, and `KHR_texture_transform` offset/scale/rotation plus texture-coordinate override are part of the shipped viewer path, but renderer/shader/backend consumption of that metadata is not being claimed here.
- This matrix stays scoped to the shipped native backends listed above.
- Use [Alpha Feedback](alpha-feedback.md) when reporting integration issues so the report carries package path, diagnostics, and display-server truth.
