# Videra.Platform.Linux

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-linux.md)

`Videra.Platform.Linux` provides the Linux-native Vulkan backend used by Videra.

## Responsibilities

- Vulkan instance, device, queue, and swapchain setup
- X11-backed native host integration
- Vulkan resource factory, command executor, and shader pipeline wiring
- Native render-path validation on Linux hosts

## Install

```bash
dotnet add package Videra.Platform.Linux --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

This package is usually consumed transitively from `Videra.Avalonia`.

## Validation

Linux native validation is explicit and should be run on a real Linux host:

```bash
./verify.sh --configuration Release --include-native-linux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
```

## Requirements

- Linux with X11
- Vulkan 1.2+ capable GPU
- `libX11.so.6` (or a compatible fallback name)
- Vulkan drivers and runtime libraries

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-linux.md)
