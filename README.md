# Videra

[English](README.md) | [中文](docs/zh-CN/README.md)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![CI](https://github.com/ExplodingUFO/Videra/actions/workflows/ci.yml/badge.svg)

## What It Is

Videra is a Cross-platform 3D viewer component stack for .NET desktop applications. It is built for embeddable viewing workflows in Avalonia apps, with a shared rendering core and native backends for Windows, Linux, and macOS.

Videra is not a general-purpose game engine. It is shaped around desktop visualization, diagnostics, controlled interaction, and host-owned application state.

## Who It Is For

- .NET desktop teams that need an Avalonia-facing 3D viewer control
- Contributors extending rendering, platform integration, or diagnostics behavior
- Teams evaluating the independent surface-chart module family from source before any public package commitment

## Current Status

- Early `alpha`
- Current repository baseline: `0.1.0-alpha.5`
- Public release tags are intended to publish the consumer packages on `nuget.org`
- `GitHub Packages` remains the `preview` / internal feed for contributors and pre-release validation
- `Videra.SurfaceCharts.*` is still a source-first module family, not a published public package line
- Linux native rendering remains `X11`-hosted, and Wayland sessions stay on the documented `XWayland compatibility` path
- GitHub Actions runs matching-host native validation on pull requests, and the [Native Validation runbook](docs/native-validation.md) documents how to use `Run workflow` for targeted reruns

## Getting Started

### Source Evaluation

```bash
git clone https://github.com/ExplodingUFO/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

This is the recommended path when you want the full repository, demos, validation scripts, or the source-only surface-chart modules.

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

If you only need the rendering abstractions and import pipeline, install `Videra.Core` directly:

```bash
dotnet add package Videra.Core
```

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
| `Videra.Avalonia` | Avalonia desktop applications | `nuget.org` public tags | `alpha` |
| `Videra.Platform.Windows` | Windows Direct3D 11 hosts | `nuget.org` public tags | `alpha` |
| `Videra.Platform.Linux` | Linux Vulkan hosts | `nuget.org` public tags | `alpha` |
| `Videra.Platform.macOS` | macOS Metal hosts | `nuget.org` public tags | `alpha` |

## Source-only modules

| Module | Status | Notes |
| --- | --- | --- |
| `Videra.SurfaceCharts.Core` | Source-only | Chart-domain contracts, tile identities, probe contracts, and LOD selection |
| `Videra.SurfaceCharts.Avalonia` | Source-only | Dedicated `SurfaceChartView` control, overlays, renderer-status surface, and built-in chart interaction |
| `Videra.SurfaceCharts.Processing` | Source-only | Offline pyramid generation, cache IO, payload sessions, and batch tile reads |

## Samples and demos

| Entry | Purpose |
| --- | --- |
| `Videra.MinimalSample` | Shortest first-scene reference for `VideraViewOptions`, `LoadModelAsync`, `FrameAll`, `ResetCamera`, `BackendDiagnostics`, and diagnostics snapshot export |
| `Videra.Demo` | Viewer demo for backend diagnostics, import feedback, and baseline interaction |
| `Videra.SurfaceCharts.Demo` | Independent surface-chart demo for `SurfaceChartView`, chart-local overlays, and rendering-path truth |
| `Videra.ExtensibilitySample` | Narrow public reference for `VideraView.Engine`, `RegisterPassContributor(...)`, and `RegisterFrameHook(...)` |
| `Videra.InteractionSample` | Public sample for the controlled interaction contract plus viewer-first inspection workflows such as measurement, clipping, state restore, and snapshot export |

`Videra.MinimalSample` is the quickest end-to-end viewer reference. It stays on the alpha happy path: `Options -> LoadModelAsync -> FrameAll / ResetCamera -> BackendDiagnostics`, then uses `VideraDiagnosticsSnapshotFormatter` to export the same support artifact requested by alpha bug reports.
`Videra.Demo` remains the broader diagnostics and import-feedback surface. It seeds a default demo cube on the ready path, summarizes import feedback in the status area, and includes a narrow `Scene Pipeline Lab` panel for `SceneDocument` versioning, pending/resident/dirty upload counts, atomic batch replacement, and backend-rebind truth.

## Extensibility Onboarding

Use [Videra.ExtensibilitySample](samples/Videra.ExtensibilitySample/README.md) as the narrow public reference and [docs/extensibility.md](docs/extensibility.md) as the long-lived behavior contract when you need custom pass contributors or frame hooks.

The advanced extensibility flow is `VideraView.Engine` -> `RegisterPassContributor(...)` / `RegisterFrameHook(...)` -> `LoadModelAsync(...)` -> `FrameAll()` -> inspect `RenderCapabilities` and `BackendDiagnostics`.

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
- Measurements stay on the public `Measurements` surface, clipping stays on `ClippingPlanes`, and saved inspection views flow through `CaptureInspectionState()`, `ApplyInspectionState(...)`, and `ExportSnapshotAsync(...)`.
- Overlay responsibilities are split between `3D highlight/render state` and `2D label/feedback rendering`.

## Surface Charts Onboarding

Use [Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md) as the public reference for the independent surface-chart module family.

Contract highlights:

- The surface-chart module family is a sibling product area, independent from `VideraView`.
- The dedicated `SurfaceChartView` control lives in `Videra.SurfaceCharts.Avalonia`.
- `Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.
- `Videra.SurfaceCharts.Core` owns chart-domain models, tile identities, probe contracts, and LOD selection.
- `Videra.SurfaceCharts.Processing` owns cache and pyramid generation.
- `Videra.SurfaceCharts.Avalonia` owns the UI shell, axis/legend overlays, hover/pinned probe behavior, and renderer-status surface.
- The shipped chart surface is `GPU-first` with explicit `software fallback`, and hosts can inspect `RenderingStatus` / `RenderStatusChanged`.
- SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.
- SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom on top of the `ViewState` runtime contract.
- The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
- Hosts can keep professional axis, grid, and legend behavior chart-local through `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.

## Current Boundaries

- Videra is a component-oriented viewer stack, not a full content creation toolchain
- The public package promise currently covers the viewer stack packages listed above, not the `Videra.SurfaceCharts.*` source-only modules
- Linux native rendering currently embeds through X11 handles; Wayland sessions rely on an `XWayland` compatibility path when available
- Linux and macOS native-host validation is expected to pass on matching-host GitHub Actions pull requests; local matching-host runs remain the fallback for targeted debugging
- The macOS backend currently relies on Objective-C runtime interop

## Documentation

- [Documentation Index](docs/index.md)
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
- [SurfaceCharts.Avalonia](src/Videra.SurfaceCharts.Avalonia/README.md)
- [SurfaceCharts.Processing](src/Videra.SurfaceCharts.Processing/README.md)
- [SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md)

## Contributing

Issues, documentation fixes, and pull requests are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Released under the [MIT License](LICENSE.txt).
