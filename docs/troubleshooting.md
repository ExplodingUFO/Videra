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

## Package Selection vs Backend Preference

- For Avalonia apps, install `Videra.Avalonia` together with the matching `Videra.Platform.Windows`, `Videra.Platform.Linux`, or `Videra.Platform.macOS` package.
- Install `Videra.Core` directly only when you want the rendering abstractions and import pipeline without the Avalonia UI layer.
- Software fallback helps with diagnostics, but it does not install missing platform packages.
- `VIDERA_BACKEND` and `PreferredBackend` only change backend preference. `VIDERA_BACKEND` does not install missing platform packages and does not replace matching-host native validation.

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
- In Wayland sessions, `Auto` currently resolves to an `XWayland` compatibility path when that bridge is available
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
- `BackendDiagnostics` snapshot
- `ResolvedDisplayServer`, `DisplayServerFallbackUsed`, and `DisplayServerFallbackReason` on Linux when relevant
- Failing command and full error output
- Whether the issue reproduces on the matching native host
- Whether the issue reproduces in `Videra.MinimalSample` or `consumer smoke`
- Whether software rendering works

## Related Docs

- [README.md](../README.md)
- [ARCHITECTURE.md](../ARCHITECTURE.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
- [Alpha Feedback](alpha-feedback.md)
- [Native Validation](native-validation.md)
- [Chinese Troubleshooting Guide](zh-CN/troubleshooting.md)

