# Troubleshooting

[English](troubleshooting.md) | [中文](zh-CN/troubleshooting.md)

This document summarizes common build, runtime, and backend issues in Videra.

## Quick Triage

Run the standard repository verification entrypoint first:

```bash
# Unix shell
./scripts/verify.sh --configuration Release

# PowerShell
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

If the issue is specific to Linux or macOS native backends, enable the explicit native validation switches:

```bash
./scripts/verify.sh --configuration Release --include-native-linux
./scripts/verify.sh --configuration Release --include-native-macos

pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeMacOS
```

If you need a matching-host runbook or the hosted GitHub Actions entrypoint, use [Native Validation](native-validation.md).

## SurfaceCharts package and demo triage

For `area: surfacecharts`, start from `smoke/Videra.SurfaceCharts.ConsumerSmoke` for the packaged surface/cache-backed path on the supported host path, then move to `Videra.SurfaceCharts.Demo` only when you need the broader repository reference surface:

- `smoke/Videra.SurfaceCharts.ConsumerSmoke` should produce `surfacecharts-support-summary.txt` for the packaged surface/cache-backed proof.
- `Start here: In-memory first chart` is the default first repro path.
- `Explore next: Cache-backed streaming` is only the follow-up path after the in-memory chart already renders.
- `Try next: Analytics proof` covers the explicit-coordinate, independent-`ColorField` and pinned-probe analytic workflow on the same shell, `Try next: Waterfall proof` is the fourth demo path, and `Try next: Scatter proof` covers the repo-owned scatter path on that same shell.
- Use `Copy support summary` to paste the demo `Support summary` block into the issue so support has the same `ViewState`, `InteractionQuality`, `RenderingStatus`, and `OverlayOptions` contract the SurfaceCharts sample exposes.
- Keep the SurfaceCharts `Support summary` separate from the viewer diagnostics snapshot. The chart summary is chart-scoped and does not replace `VideraDiagnosticsSnapshotFormatter` output.
- Treat the SurfaceCharts summary as support evidence, not a benchmark result, pixel-perfect visual-regression gate, or GPU performance guarantee. A structured summary must include `GeneratedUtc`, `EvidenceKind`, `EvidenceOnly`, `ChartControl`, `EnvironmentRuntime`, `AssemblyIdentity`, `BackendDisplayEnvironment`, and `RenderingStatus`.

## Package Selection vs Backend Preference

- For Avalonia apps, install `Videra.Avalonia` together with the matching `Videra.Platform.Windows`, `Videra.Platform.Linux`, or `Videra.Platform.macOS` package.
- `smoke/Videra.WpfSmoke` is repository-only Windows WPF smoke evidence for the public viewer path; use it as validation/support evidence, not as a second public UI package or release path.
- Install `Videra.Core` directly only when you want the runtime kernel without the Avalonia UI layer.
- Add `Videra.Import.Gltf` and/or `Videra.Import.Obj` when you need `.gltf` / `.glb` / `.obj` ingestion on the core path.
- Software fallback helps with diagnostics only when explicitly enabled (`AllowSoftwareFallback = true` or `VIDERA_BACKEND=software`), but it does not install missing platform packages.
- `VIDERA_BACKEND` and `PreferredBackend` only change backend preference. `VIDERA_BACKEND` does not install missing platform packages and does not replace matching-host native validation.
- With default backend preference, missing native backends surface as initialization/readiness failure; software is not the automatic recovery path.
- `TransparentFeatureStatus` in diagnostics snapshots captures the shipped transparency contract: alpha mask rendering plus deterministic alpha blend ordering for per-primitive carried alpha sources.
- `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount` are backend-neutral scene diagnostics, not draw-call metrics or broader renderer promises.
- `LastSnapshotExportPath` and `LastSnapshotExportStatus` capture the most recent inspection snapshot export target and outcome when snapshot export is part of the report.

## Common Problems

| Problem | Platform | Suggested Action |
| --- | --- | --- |
| `Failed to create D3D11 device` | Windows | Update GPU drivers and confirm Direct3D 11 support |
| `Failed to create Vulkan instance` | Linux | Check Vulkan drivers, runtime libraries, and X11 availability |
| `Failed to create X11 Vulkan surface` | Linux | Confirm an active X11 session and a usable `libX11` |
| Wayland session resolves no native Linux host | Linux | Confirm the session exposes `XWayland` and that both `WAYLAND_DISPLAY` and `DISPLAY` are present |
| `Failed to create Metal device` | macOS | Confirm Metal support and validate on a real macOS host |
| Blank render area | Any | Try `VIDERA_BACKEND=software` first to isolate native-host vs GPU issues |
| Model import failure | Any | Confirm that the asset is `.gltf`, `.glb`, or `.obj` and valid |
| Demo starts but shows no content | Any | Wait for `VideraView` backend initialization before importing or mutating scene state |

## Platform Notes

### Windows

- Standard verification covers Windows backend tests and real HWND-backed lifecycle paths
- If your change affects D3D11 initialization, swapchain behavior, or native host handling, rerun:

```bash
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

### Linux

- The official Linux render path is still Vulkan with X11 handles
- In Wayland sessions, `Auto` currently resolves to the documented `XWayland` bridge when that bridge is available
- `ResolvedDisplayServer = X11` means the direct supported native-host path is active
- `ResolvedDisplayServer = XWayland` means the session is running through the documented X11 bridge, not compositor-native Wayland embedding
- `DisplayServerCompatibility` in the diagnostics snapshot summarizes that boundary in one line for bug reports and support artifacts
- Missing `libX11.so.6` can still be diagnosed with the repository fallback loader, but a usable X11 or `XWayland` runtime is still required

### macOS

- The current native path is `NSView` + `CAMetalLayer` + Metal
- Backend interop uses Objective-C runtime calls to communicate with system frameworks
- Full native validation must be run on a macOS host

## Environment Variables

| Variable | Purpose | Values |
| --- | --- | --- |
| `VIDERA_BACKEND` | Force a rendering backend | `software`, `d3d11`, `vulkan`, `metal`, `auto` |
| `VIDERA_FRAMELOG` | Enable frame logging | `1`, `true` |
| `VIDERA_INPUTLOG` | Enable input logging | `1`, `true` |

Start with `VIDERA_BACKEND=software` if you need to narrow down whether the problem is backend-specific.

## When Filing an Issue

Include:

- Operating system and version
- GPU and driver details
- Package install path (`nuget.org`, `GitHub Packages`, or source build)
- Package version or commit SHA
- Backend preference or `VIDERA_BACKEND` value
- diagnostics snapshot from `VideraDiagnosticsSnapshotFormatter`
- `surfacecharts-support-summary.txt` from `smoke/Videra.SurfaceCharts.ConsumerSmoke` or the `Support summary` from `Videra.SurfaceCharts.Demo` when the issue is in `area: surfacecharts`
- required SurfaceCharts support-summary fields: `GeneratedUtc`, `EvidenceKind`, `EvidenceOnly`, `ChartControl`, `EnvironmentRuntime`, `AssemblyIdentity`, `BackendDisplayEnvironment`, and `RenderingStatus`
- `LastSnapshotExportPath` and `LastSnapshotExportStatus` when an inspection snapshot was exported
- the exact SurfaceCharts demo path you used:
  - `Start here: In-memory first chart`
  - `Explore next: Cache-backed streaming`
  - `Try next: Analytics proof`
  - `Try next: Waterfall proof`
  - `Try next: Scatter proof`
- use `Copy support summary` after reproducing `Start here: In-memory first chart`; continue to `Explore next: Cache-backed streaming` only if needed. The copied SurfaceCharts summary is chart-scoped and carries `ViewState`, `InteractionQuality`, `RenderingStatus`, and `OverlayOptions` details.
- use `Try next: Analytics proof` when the issue is about explicit-coordinate/analysis workflow checks, `Try next: Waterfall proof` when the issue is about the waterfall chart-family path instead of the cache-backed path, and `Try next: Scatter proof` when the issue is about the direct scatter Plot path; the shipped control line also includes `VideraChartView`
- if the issue is about imported-material fidelity, include the smallest asset plus the relevant baseColor texture usage, occlusion texture binding/strength, emissive factor/texture details, normal-texture details, and `KHR_texture_transform` details; those values now affect shipped renderer output on the bounded static-scene path
- `ViewState`, `InteractionQuality`, `RenderingStatus`, `OverlayOptions`, `SeriesCount`, `ActiveSeries`, `ChartKind`, `ColorMap`, and `PrecisionProfile` from the SurfaceCharts support summary when relevant
- `TransparentFeatureStatus` from `VideraDiagnosticsSnapshotFormatter` when the issue involves transparency behavior
- inspection bundle from `VideraInspectionBundleService.ExportAsync(...)` when you need support to replay camera, clipping, measurements, and annotations together
- `CanReplayScene` and `ReplayLimitation` from `VideraInspectionBundleService.ExportAsync(...)` because they describe replayability semantics and should travel with any support bundle, especially when the bundle captures host-owned objects or any other scene state that cannot be replayed on import
- `ResolvedDisplayServer`, `DisplayServerFallbackUsed`, `DisplayServerFallbackReason`, and `DisplayServerCompatibility` on Linux when relevant
- Failing command and full error output
- Whether the issue reproduces on the matching native host
- Whether the issue reproduces in `Videra.MinimalSample`, `smoke/Videra.WpfSmoke`, `consumer smoke`, or `smoke/Videra.SurfaceCharts.ConsumerSmoke`
- Whether software rendering works

## Related Docs

- [README.md](../README.md)
- [ARCHITECTURE.md](../ARCHITECTURE.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
- [Alpha Feedback](alpha-feedback.md)
- [Native Validation](native-validation.md)
- [Chinese Troubleshooting Guide](zh-CN/troubleshooting.md)

