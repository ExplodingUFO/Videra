# Videra.Platform.macOS

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-macos.md)

`Videra.Platform.macOS` provides the macOS-native Metal backend used by Videra.

## Responsibilities

- Metal device and command-queue setup
- `NSView` / `CAMetalLayer` host integration
- Metal resource factory and command executor wiring
- Retina-aware drawable sizing and native lifecycle support

## Install

```bash
dotnet add package Videra.Platform.macOS --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

This package is usually consumed transitively from `Videra.Avalonia`.

## Validation

macOS native validation is explicit and should be run on a real macOS host:

```bash
./verify.sh --configuration Release --include-native-macos
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

## Requirements

- macOS 10.15 or newer
- Metal-capable hardware
- Apple Silicon or Intel Mac with Metal support

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-macos.md)
