# Native Validation

[English](native-validation.md) | [中文](zh-CN/native-validation.md)

This runbook covers the matching-host validation path for Videra's Linux, macOS, and Windows native backends. Use it when you need to reproduce host-specific backend issues or close the remaining `TEST-03` execution gap.

## What This Covers

- Linux native validation: real X11-hosted Vulkan lifecycle and draw-path tests
- macOS native validation: real `NSView`-hosted Metal lifecycle and draw-path tests
- Windows native validation: real HWND-hosted D3D11 lifecycle and draw-path tests
- A manual GitHub Actions entrypoint for hosted Linux/macOS/Windows validation
- A local script entrypoint for real matching hosts

Standard repository verification remains:

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

That standard path does not automatically close the dedicated matching-host validation gap. Use the native-specific entrypoints below.

## GitHub Actions Entry

The repository now includes a manual workflow:

- Workflow: `.github/workflows/native-validation.yml`
- Trigger: `workflow_dispatch`
- Targets: `all`, `linux`, `macos`, `windows`

From the GitHub Actions tab:

1. Open `Native Validation`
2. Click `Run workflow`
3. Pick `all`, `linux`, `macos`, or `windows`

Notes:

- The Linux job installs `xvfb`, `libX11`, `libshaderc1`, and Vulkan runtime packages, then runs the native validation under `xvfb-run`
- The macOS job runs the hosted Metal/`NSView` validation path directly
- The Windows job runs the hosted HWND/D3D11 validation path directly through the PowerShell wrapper
- If hosted runners turn out to be insufficient for a specific native issue, use the local matching-host path below

## Local Matching-Host Entry

### Linux

Prerequisites:

- .NET 8 SDK
- A Linux host
- An active X11 session, or `xvfb-run`
- Vulkan runtime and drivers
- `libX11.so.6`
- `libshaderc.so.1` (Ubuntu package: `libshaderc1`)

Shell:

```bash
./scripts/run-native-validation.sh --platform linux --configuration Release
```

PowerShell:

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Linux -Configuration Release
```

If you are on a headless host, run the shell entry under Xvfb:

```bash
xvfb-run -a bash ./scripts/run-native-validation.sh --platform linux --configuration Release
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

- Linux: `./verify.sh --configuration Release --include-native-linux`
- macOS: `./verify.sh --configuration Release --include-native-macos`
- Windows: `pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeWindows`

The PowerShell wrapper calls the equivalent `verify.ps1` entrypoint.

## Success Evidence

Native validation is considered meaningful when all of the following are true:

- The matching-host platform test project runs on its own host
- The real native fixture path executes
- Lifecycle and draw-path tests pass
- Success is not based only on constructor assertions, `IntPtr.Zero` guards, or placeholder tests

Relevant test projects:

- `tests/Videra.Platform.Linux.Tests`
- `tests/Videra.Platform.macOS.Tests`
- `tests/Videra.Platform.Windows.Tests`

## Related Docs

- [README](../README.md)
- [Troubleshooting](troubleshooting.md)
- [Documentation Index](index.md)
- [Chinese Native Validation Guide](zh-CN/native-validation.md)
