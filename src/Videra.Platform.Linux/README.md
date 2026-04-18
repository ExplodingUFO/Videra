# Videra.Platform.Linux

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-linux.md)

`Videra.Platform.Linux` provides the Linux-native Vulkan backend used by Videra.

Current status: `alpha`. This package is the Linux platform companion for `Videra.Avalonia`. Native embedding currently uses X11 handles, and Wayland sessions rely on an `XWayland` compatibility path when available.

## Responsibilities

- Vulkan instance, device, queue, and swapchain setup
- X11-backed native host integration
- Wayland-session `XWayland` compatibility validation
- Vulkan resource factory, command executor, and shader pipeline wiring
- Native render-path validation on Linux hosts

## Install

The default public consumer path is `nuget.org`:

```bash
dotnet add package Videra.Avalonia
dotnet add package Videra.Platform.Linux
```

Current `alpha` and contributor `preview` validation can still use `GitHub Packages`, but that feed is not the default public install route:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Avalonia --version 0.1.0-alpha.7 --source github-ExplodingUFO
dotnet add package Videra.Platform.Linux --version 0.1.0-alpha.7 --source github-ExplodingUFO
```

This package supplies the Vulkan backend for the current Linux native path. On Avalonia 11, native embedding remains X11-based; Wayland sessions use `XWayland` when available. `VIDERA_BACKEND` can prefer Vulkan, but it does not install missing platform packages.

## Validation

Linux matching-host validation is explicit and should be run on a real Linux host:

```bash
./scripts/verify.sh --configuration Release --include-native-linux
./scripts/verify.sh --configuration Release --include-native-linux-xwayland
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeLinuxXWayland
```

## Requirements

- Linux with X11, or a Wayland session that exposes `XWayland`
- Vulkan 1.2+ capable GPU
- `libX11.so.6` (or a compatible fallback name)
- Vulkan drivers and runtime libraries

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-linux.md)

