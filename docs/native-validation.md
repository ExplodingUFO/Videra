# Native Validation

[English](native-validation.md) | [中文](zh-CN/native-validation.md)

This runbook covers the matching-host validation path for Videra's Linux, macOS, and Windows native backends. GitHub Actions pull requests now use this path as the primary native-validation gate. Use this guide when you need to inspect that CI behavior, rerun it manually with `workflow_dispatch`, or reproduce host-specific backend issues locally.

## What This Covers

- Linux X11 native validation: real X11-hosted Vulkan lifecycle and draw-path tests
- Linux Wayland-session validation: `XWayland` compatibility fallback with real Vulkan lifecycle and draw-path coverage
- macOS native validation: real `NSView`-hosted Metal lifecycle and draw-path tests
- Windows native validation: real HWND-hosted D3D11 lifecycle and draw-path tests
- An automatic GitHub Actions gate for pull requests and pushes to `master`
- A manual GitHub Actions entrypoint for hosted Linux/macOS/Windows validation
- A local script entrypoint for real matching hosts

Standard repository verification remains:

```bash
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

That standard path does not automatically exercise the dedicated matching-host native jobs. Use the native-specific entrypoints below when you need the same path that GitHub Actions pull requests enforce.

## GitHub Actions Entry

The repository now includes a hosted workflow at `.github/workflows/native-validation.yml`.

Automatic triggers:

- `pull_request`
- `push` to `master`

Manual trigger:

- `workflow_dispatch`
- Targets: `all`, `linux-x11`, `linux-wayland-xwayland`, `macos`, `windows`

From the GitHub Actions tab, use `Run workflow` when you want a targeted rerun:

1. Open `Native Validation`
2. Click `Run workflow`
3. Pick `all`, `linux-x11`, `linux-wayland-xwayland`, `macos`, or `windows`

Notes:

- The Linux X11 job installs `xvfb`, `libX11`, `libshaderc1`, and Vulkan runtime packages, then runs the native validation under `xvfb-run`
- The Linux Wayland-session job installs `xwayland-run`, starts a headless Wayland compositor with `XWayland` via `xwfb-run`, and validates the compatibility path with both `DISPLAY` and `WAYLAND_DISPLAY` available
- The macOS job runs the hosted Metal/`NSView` validation path directly
- The Windows job runs the hosted HWND/D3D11 validation path directly through the PowerShell wrapper
- If hosted runners turn out to be insufficient for a specific native issue, use the local matching-host path below

## Local Matching-Host Entry

Use the local matching-host path when:

- you need to reproduce a CI-only native failure
- you need to inspect logs or graphics prerequisites interactively
- hosted runners are insufficient for a specific platform issue

### Linux X11

Prerequisites:

- .NET 8 SDK
- A Linux host
- An active X11 session, or `xvfb-run`
- Vulkan runtime and drivers
- `libX11.so.6`
- `libshaderc.so.1` (Ubuntu package: `libshaderc1`)

Shell:

```bash
./scripts/run-native-validation.sh --platform linux --linux-display-server x11 --configuration Release
```

PowerShell:

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Linux -LinuxDisplayServer X11 -Configuration Release
```

If you are on a headless host, run the shell entry under Xvfb:

```bash
xvfb-run -a bash ./scripts/run-native-validation.sh --platform linux --linux-display-server x11 --configuration Release
```

### Linux Wayland Session With XWayland

Prerequisites:

- .NET 8 SDK
- A Linux host
- A Wayland session that also exposes `XWayland`
- `DISPLAY` and `WAYLAND_DISPLAY`
- Vulkan runtime and drivers
- `libX11.so.6`
- `libshaderc.so.1` (Ubuntu package: `libshaderc1`)
- `xwayland-run` or an equivalent headless Wayland compositor + `XWayland` setup

Shell:

```bash
./scripts/run-native-validation.sh --platform linux --linux-display-server xwayland --configuration Release
```

PowerShell:

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Linux -LinuxDisplayServer XWayland -Configuration Release
```

For a headless reproduction that still uses a real Wayland compositor plus `XWayland`, use `xwfb-run` from the Ubuntu `xwayland-run` package:

```bash
xwfb-run -- bash ./scripts/run-native-validation.sh --platform linux --linux-display-server xwayland --configuration Release
```

### macOS

Prerequisites:

- .NET 8 SDK
- A macOS host
- Metal-capable hardware
- AppKit / Objective-C runtime availability

Shell:

```bash
./scripts/run-native-validation.sh --platform macos --configuration Release
```

PowerShell:

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform macOS -Configuration Release
```

### Windows

Prerequisites:

- .NET 8 SDK
- A Windows host
- Direct3D 11-capable hardware/driver stack

PowerShell:

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Windows -Configuration Release
```

## What The Scripts Run

- Linux X11: `./scripts/verify.sh --configuration Release --include-native-linux`
- Linux Wayland-session `XWayland`: `./scripts/verify.sh --configuration Release --include-native-linux-xwayland`
- macOS: `./scripts/verify.sh --configuration Release --include-native-macos`
- Windows: `pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeWindows`

The PowerShell wrapper calls the equivalent `scripts/verify.ps1` entrypoint.

## Success Evidence

Native validation is considered meaningful when all of the following are true:

- The matching-host platform test project runs on its own host
- The real native fixture path executes
- Lifecycle and draw-path tests pass
- Linux session diagnostics resolve to the expected display server (`X11` or `XWayland`)
- Success is not based only on constructor assertions, `IntPtr.Zero` guards, or placeholder tests

For project-state tracking, the first successful hosted pull-request or tag-gated run on the matching host is the evidence that closes the old local-only execution assumption. The local matching-host path remains the reproduction and troubleshooting fallback.

Relevant test projects:

- `tests/Videra.Platform.Linux.Tests`
- `tests/Videra.Platform.macOS.Tests`
- `tests/Videra.Platform.Windows.Tests`

## Related Docs

- [README](../README.md)
- [Troubleshooting](troubleshooting.md)
- [Documentation Index](index.md)
- [Chinese Native Validation Guide](zh-CN/native-validation.md)

