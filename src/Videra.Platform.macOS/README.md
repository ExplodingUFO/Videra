# Videra.Platform.macOS

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-macos.md)

`Videra.Platform.macOS` provides the macOS-native Metal backend used by Videra.

Current status: `alpha`. This package is the macOS platform companion for `Videra.Avalonia`, and the native path is `NSView` plus `CAMetalLayer` plus Metal.

## Responsibilities

- Metal device and command-queue setup
- `NSView` / `CAMetalLayer` host integration
- Metal resource factory and command executor wiring
- Retina-aware drawable sizing and native lifecycle support

## Install

Configure GitHub Packages before adding the package:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Install it together with `Videra.Avalonia` on macOS:

```bash
dotnet add package Videra.Avalonia --version 0.1.0-alpha.1 --source github-ExplodingUFO
dotnet add package Videra.Platform.macOS --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

This package supplies the Metal backend for the `NSView` / `CAMetalLayer` native path. `VIDERA_BACKEND` can prefer Metal, but it does not install missing platform packages.

## Validation

macOS matching-host validation is explicit and should be run on a real macOS host:

```bash
./scripts/verify.sh --configuration Release --include-native-macos
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeMacOS
```

## Requirements

- macOS 10.15 or newer
- Metal-capable hardware
- Apple Silicon or Intel Mac with Metal support

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-macos.md)

