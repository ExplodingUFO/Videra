# Videra

[English](README.md) | [中文](docs/zh-CN/README.md)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![CI](https://github.com/ExplodingUFO/Videra/actions/workflows/ci.yml/badge.svg)

## What It Is

Videra is a Cross-platform 3D viewer component stack for .NET desktop applications. It is built for embeddable viewing workflows in Avalonia apps, with a shared rendering core and native backends for Windows, Linux, and macOS.

Videra is not a general-purpose game engine. It is shaped around desktop visualization, diagnostics, controlled interaction, and host-owned application state.

The `1.0` line is specifically a native desktop viewer/runtime plus inspection workflows and the public `SurfaceCharts` package line on the Avalonia path. The public product boundary does not promise general Three.js-style runtime breadth, `WebGL` / `OpenGL` pursuit, or engine-first feature expansion. See [Videra 1.0 Capability Matrix](docs/capability-matrix.md).

On the viewer/runtime path, `SceneDocument` keeps backend-neutral imported scene truth in `ImportedSceneAsset` catalogs built from `SceneNode`, `MeshPrimitive`, `MaterialInstance`, `Texture2D`, and `Sampler`. Those runtime assets stay CPU-side until a ready resource factory uploads them.

That shipped viewer path now carries one direct static glTF/PBR baseline: UV-backed `Texture2D` / `Sampler` bindings, `MaterialInstance` metallic-roughness and alpha semantics, emissive and normal-map-ready inputs, tangent-aware mesh data, and repeated unchanged imports that can reuse retained imported scene assets while those retained assets stay available. It is intentionally static-scene-only; animation, skeletons, morph targets, and broader advanced-runtime expansion remain deferred.

## Who It Is For

- .NET desktop teams that need an Avalonia-facing 3D viewer control
- Contributors extending rendering, platform integration, or diagnostics behavior
- Teams building or evaluating the independent `SurfaceCharts` package family on the Avalonia path

## Current Status

- Early `alpha`
- Current repository baseline: `0.1.0-alpha.7`
- Public release tags are intended to publish the consumer packages on `nuget.org`
- `GitHub Packages` remains the `preview` / internal feed for contributors and pre-release validation
- `Videra.SurfaceCharts.*` now ships as a public `alpha` package line, while `Videra.SurfaceCharts.Demo` remains repository-only
- Linux native rendering remains `X11`-hosted, and Wayland sessions stay on the documented `XWayland compatibility` path
- GitHub Actions runs matching-host native validation, packaged viewer consumer smoke, packaged SurfaceCharts first-chart consumer smoke, and explicit sample-contract evidence on pull requests, and the [Native Validation runbook](docs/native-validation.md) documents how to use `Run workflow` for targeted reruns
- The current alpha-ready `green` line is repository verification + native validation + packaged viewer consumer smoke + packaged SurfaceCharts first-chart consumer smoke + sample-contract evidence, with `quality-gate-evidence` running the Windows packaged viewer and SurfaceCharts consumer smoke paths with warnings treated as errors, enforcing package-size budgets on the public package line, and keeping the curated Core test surfaces plus `Videra.MinimalSample` warning-clean while `Benchmark Gates` serves as the hard numeric runtime blocker

## Getting Started

### Source Evaluation

```bash
git clone https://github.com/ExplodingUFO/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

This is the recommended path when you want the full repository, demos, validation scripts, or the full chart sources alongside the packaged install path.

### Public Package Install

The official public package feed is `nuget.org`. For Avalonia apps, start with `Videra.Avalonia` and install exactly one matching platform package:

```bash
dotnet add package Videra.Avalonia
dotnet add package Videra.Platform.Windows
# or
dotnet add package Videra.Platform.Linux
# or
dotnet add package Videra.Platform.macOS
```

If you only need the runtime kernel and scene/render abstractions without the Avalonia UI layer, install `Videra.Core` directly:

```bash
dotnet add package Videra.Core
```

If you also need file-format ingestion on the core path, add the dedicated import packages:

```bash
dotnet add package Videra.Import.Gltf
dotnet add package Videra.Import.Obj
```

`Videra.Avalonia` already brings `Videra.Import.Gltf` and `Videra.Import.Obj` transitively for `LoadModelAsync(...)` and `LoadModelsAsync(...)`.

For surface charts, start with the dedicated Avalonia control package and add processing helpers when you want the current first-chart path:

```bash
dotnet add package Videra.SurfaceCharts.Avalonia
dotnet add package Videra.SurfaceCharts.Processing
```

`Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. Add `Videra.SurfaceCharts.Core` directly only when you are building chart-domain contracts or custom tile sources without the Avalonia shell. `Videra.SurfaceCharts.Demo` remains repository-only.

`Videra.Avalonia` remains the UI/control entry package. `PreferredBackend` and `VIDERA_BACKEND` only change backend preference. They do not install missing platform packages, and they do not replace matching-host native validation.
The public install flow does not install missing platform packages for you.

For the shortest copyable first-scene path, start with [Videra.MinimalSample](samples/Videra.MinimalSample/README.md) and the [Videra.Avalonia package README](src/Videra.Avalonia/README.md). The canonical alpha flow is `Options -> LoadModelAsync -> FrameAll / ResetCamera -> BackendDiagnostics`, and `VideraDiagnosticsSnapshotFormatter` is the copy-pasteable support artifact for that path. Move to [Videra.ExtensibilitySample](samples/Videra.ExtensibilitySample/README.md) only when you need `VideraView.Engine`, frame hooks, or pass contributors.

Current `alpha` preview builds may still be validated through `GitHub Packages`. Treat that path as `preview`, not as the default public install flow. Feed policy and package classification live in [docs/package-matrix.md](docs/package-matrix.md), [docs/support-matrix.md](docs/support-matrix.md), [docs/release-policy.md](docs/release-policy.md), and [docs/releasing.md](docs/releasing.md).

### Contribution

Use [CONTRIBUTING.md](CONTRIBUTING.md) when you want to build, validate, and submit changes. Usage questions and design discussion belong in GitHub Discussions, while private vulnerabilities belong in [SECURITY.md](SECURITY.md).
For alpha adoption feedback, use [Alpha Feedback](docs/alpha-feedback.md) before filing an issue so reports include the diagnostics and consumer-path details needed to reproduce them.

## Published packages

| Package | Audience | Official feed | Current support level |
| --- | --- | --- | --- |
| `Videra.Core` | Core-only consumers and backend integrators | `nuget.org` public tags | `alpha` |
| `Videra.Import.Gltf` | Core-first consumers that need `.gltf` / `.glb` ingestion | `nuget.org` public tags | `alpha` |
| `Videra.Import.Obj` | Core-first consumers that need `.obj` ingestion | `nuget.org` public tags | `alpha` |
| `Videra.Avalonia` | Avalonia desktop applications | `nuget.org` public tags | `alpha` |
| `Videra.Platform.Windows` | Windows Direct3D 11 hosts | `nuget.org` public tags | `alpha` |
| `Videra.Platform.Linux` | Linux Vulkan hosts | `nuget.org` public tags | `alpha` |
| `Videra.Platform.macOS` | macOS Metal hosts | `nuget.org` public tags | `alpha` |
| `Videra.SurfaceCharts.Core` | Chart-domain consumers and custom tile-source integrators | `nuget.org` public tags | `alpha` |
| `Videra.SurfaceCharts.Rendering` | SurfaceCharts rendering-runtime consumers | `nuget.org` public tags | `alpha` |
| `Videra.SurfaceCharts.Processing` | Surface preprocessing, cache, and pyramid helpers | `nuget.org` public tags | `alpha` |
| `Videra.SurfaceCharts.Avalonia` | Avalonia desktop applications that host `SurfaceChartView` | `nuget.org` public tags | `alpha` |

## Repository-only entries

| Entry | Status | Notes |
| --- | --- | --- |
| `Videra.Demo` | Repository-only | Viewer diagnostics and scene-pipeline reference app |
| `Videra.SurfaceCharts.Demo` | Repository-only | SurfaceCharts reference app and support-summary repro path |
| `Videra.ExtensibilitySample` | Repository-only | Public extensibility reference sample |
| `Videra.InteractionSample` | Repository-only | Public controlled-interaction and inspection workflow sample |

## Samples and demos

| Entry | Purpose |
| --- | --- |
| `Videra.MinimalSample` | Shortest first-scene reference for `VideraViewOptions`, `LoadModelAsync`, `FrameAll`, `ResetCamera`, `BackendDiagnostics`, and diagnostics snapshot export |
| `Videra.Demo` | Viewer demo for backend diagnostics, import feedback, and baseline interaction |
| `Videra.SurfaceCharts.Demo` | Independent surface-chart demo for `SurfaceChartView`, chart-local overlays, and rendering-path truth |
| `Videra.ExtensibilitySample` | Narrow public reference for `VideraView.Engine`, `RegisterPassContributor(...)`, and `RegisterFrameHook(...)` |
| `Videra.InteractionSample` | Public sample for the controlled interaction contract plus viewer-first inspection workflows such as measurement, clipping, state restore, snapshot export, and replayable inspection bundles |

`Videra.MinimalSample` is the quickest end-to-end viewer reference. It stays on the alpha happy path: `Options -> LoadModelAsync -> FrameAll / ResetCamera -> BackendDiagnostics`, then uses `VideraDiagnosticsSnapshotFormatter` to export the same support artifact requested by alpha bug reports.
`Videra.Demo` remains the broader diagnostics and import-feedback surface. It seeds a default demo cube on the ready path, summarizes import feedback in the status area, and includes a narrow `Scene Pipeline Lab` panel for `SceneDocument` versioning, the retained `SceneNode` / `MeshPrimitive` / `MaterialInstance` / `Texture2D` / `Sampler` runtime model, the shipped static glTF/PBR baseline, tangent-aware retained assets, repeated unchanged imports that can reuse retained imported scene assets while they remain retained, pending/resident/dirty upload counts, render-feature diagnostics, atomic batch replacement, and backend-rebind truth.

## Videra 1.0 Boundary

Use [Videra 1.0 Capability Matrix](docs/capability-matrix.md) when you need the explicit answer to “what is in `1.0`?” versus “what is intentionally deferred?”.
Use [Hosting Boundary](docs/hosting-boundary.md) when you need the canonical composition story for `Core` / `Import` / `UI adapter` / `Backend` and the internal seam owners behind `VideraView`.

Current package-layer vocabulary:

- `Core`: viewer/runtime kernel
- `Import`: asset-ingestion layer for viewer/runtime scenes
- `Backend`: native graphics implementations
- `UI adapter`: host-framework shell
- `Charts`: analytics-oriented chart family

That layer split is the guiding product boundary for `v1.20`. It keeps Videra focused on a native desktop viewer/runtime instead of letting the repository read like a general engine roadmap by implication.

## Extensibility Onboarding

Use [Videra.ExtensibilitySample](samples/Videra.ExtensibilitySample/README.md) as the narrow public reference and [docs/extensibility.md](docs/extensibility.md) as the long-lived behavior contract when you need custom pass contributors or frame hooks.

The advanced extensibility flow is `VideraView.Engine` -> `RegisterPassContributor(...)` / `RegisterFrameHook(...)` -> `LoadModelAsync(...)` -> `FrameAll()` -> inspect `RenderCapabilities` and `BackendDiagnostics`.

The shared feature vocabulary for those diagnostics is `RenderFeatureSet`: `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`. Host apps read that truth through `RenderCapabilities.SupportedFeatureNames`, `BackendDiagnostics.LastFrameFeatureNames`, and `BackendDiagnostics.SupportedRenderFeatureNames`.

Contract highlights:

- After the engine is `disposed`, additional contributor and hook registrations are ignored as a `no-op`.
- `RenderCapabilities` remains queryable before initialization and after disposal, with `IsInitialized = false` until the engine is ready.
- When `AllowSoftwareFallback = true`, native backend failures resolve to software and `BackendDiagnostics.FallbackReason` explains why the native backend was unavailable.
- When `AllowSoftwareFallback = false`, native backend resolution fails instead of silently falling back, so the view does not become ready until the package/runtime issue is fixed.
- `package discovery` and `plugin loading` remain out of scope for the public extension model.

## Interaction Onboarding

Use [Videra.InteractionSample](samples/Videra.InteractionSample/README.md) as the focused public reference for controlled viewer interaction and inspection.

Contract highlights:

- The `host owns` `SelectionState`, `Annotations`, and annotation state.
- Built-in interaction modes are `Navigate`, `Select`, `Annotate`, and `Measure`.
- Selection is object-level and changes only when the host applies `SelectionRequested`.
- Annotation clicks surface `AnnotationRequested` for either object anchors or world-point anchors.
- Hosts typically materialize those anchors through `VideraNodeAnnotation` and `VideraWorldPointAnnotation`.
- Measurements stay on the public `Measurements` surface, `MeasurementSnapMode` stays on `InteractionOptions`, clipping stays on `ClippingPlanes`, saved inspection sessions flow through `CaptureInspectionState()` / `ApplyInspectionState(...)` including host-owned annotations, snapshots stay on `ExportSnapshotAsync(...)`, and replayable support artifacts flow through `VideraInspectionBundleService` on top of that same typed session truth.
- Overlay responsibilities are split between `3D highlight/render state` and `2D label/feedback rendering`.

## Surface Charts Onboarding

For the canonical first-chart story, start from `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing`, and use [Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md) as the repository reference app for the same path.
Inside that demo, keep the default `Start here: In-memory first chart` path for the baseline repro, then move to `Explore next: Cache-backed streaming` when you want to validate lazy tile reads and the broader chart diagnostics surface.
Use `Copy support summary` when you need the chart support artifact that matches the docs and bug template. Public tags now publish the `Videra.SurfaceCharts.*` package assets, while `Videra.SurfaceCharts.Demo` remains repository-only and keeps the support-summary repro workflow.
The packaged first-chart proof lives separately in `smoke/Videra.SurfaceCharts.ConsumerSmoke`, which writes `surfacecharts-support-summary.txt` on the packaged install path without turning the broader demo into a public install story.

Contract highlights:

- The surface-chart module family is a sibling product area, independent from `VideraView`.
- The dedicated `SurfaceChartView` control remains the primary chart control entrypoint in `Videra.SurfaceCharts.Avalonia`.
- `Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.
- `Videra.SurfaceCharts.Core` owns chart-domain models, tile identities, probe contracts, and LOD selection.
- `Videra.SurfaceCharts.Rendering` owns chart render-state orchestration and the chart-local backend runtime.
- `Videra.SurfaceCharts.Processing` owns cache and pyramid generation.
- `Videra.SurfaceCharts.Avalonia` owns the UI shell, axis/legend overlays, hover/pinned probe behavior, and renderer-status surface.
- The shipped chart surface is `GPU-first` with explicit `software fallback`, and hosts can inspect `RenderingStatus` / `RenderStatusChanged`.
- The public rendering truth is `RenderingStatus` + `RenderStatusChanged` with `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, and `ResidentTileCount`.
- SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.
- SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, `Ctrl + left-drag` focus zoom, and `Shift + left-click` pinned probe on top of the `ViewState` runtime contract.
- The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
- The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.
- Hosts can keep professional axis, grid, and legend behavior chart-local through `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.
- The public overlay configuration seam is `SurfaceChartOverlayOptions` through `OverlayOptions`; overlay state types remain internal.
- Hosts own `ISurfaceTileSource`, persisted `ViewState`, color-map selection, and chart-local product UI.
- `SurfaceChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection.
- The `Videra.SurfaceCharts.*` family is part of the current public package promise, but `Videra.SurfaceCharts.Demo` remains repository-only.

## Current Boundaries

- Videra is a component-oriented viewer stack, not a full content creation toolchain
- The public package promise covers the viewer stack and `Videra.SurfaceCharts.*` package lines listed above; demo/sample applications remain repository-only
- Linux native rendering currently embeds through X11 handles; Wayland sessions rely on an `XWayland` compatibility path when available
- Linux and macOS native-host validation is expected to pass on matching-host GitHub Actions pull requests; local matching-host runs remain the fallback for targeted debugging
- The macOS backend currently relies on Objective-C runtime interop

## Documentation

- [Documentation Index](docs/index.md)
- [Videra 1.0 Capability Matrix](docs/capability-matrix.md)
- [Package Matrix](docs/package-matrix.md)
- [Support Matrix](docs/support-matrix.md)
- [Release Policy](docs/release-policy.md)
- [Releasing Runbook](docs/releasing.md)
- [Benchmark Gates](docs/benchmark-gates.md)
- [Alpha Feedback](docs/alpha-feedback.md)
- [Extensibility Contract](docs/extensibility.md)
- [Architecture](ARCHITECTURE.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Native Validation](docs/native-validation.md)
- [Contributing](CONTRIBUTING.md)
- [Chinese Documentation Entry](docs/zh-CN/index.md)
- [SurfaceCharts.Core](src/Videra.SurfaceCharts.Core/README.md)
- [SurfaceCharts.Rendering](src/Videra.SurfaceCharts.Rendering/README.md)
- [SurfaceCharts.Avalonia](src/Videra.SurfaceCharts.Avalonia/README.md)
- [SurfaceCharts.Processing](src/Videra.SurfaceCharts.Processing/README.md)
- [SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md)

## Contributing

Issues, documentation fixes, and pull requests are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Released under the [MIT License](LICENSE.txt).
