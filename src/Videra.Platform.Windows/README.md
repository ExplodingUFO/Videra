# Videra.Platform.Windows

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-windows.md)

`Videra.Platform.Windows` provides the Windows-native Direct3D 11 backend used by Videra.

Current status: `alpha`. This package is the Windows platform companion for `Videra.Avalonia`, not a replacement for the Avalonia entry package.

## Responsibilities

- Direct3D 11 device and swapchain setup
- Render-target and depth-buffer lifecycle
- D3D11 resource factory and command executor
- Real `HWND`-backed rendering integration

## Install

Configure GitHub Packages before adding the package:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Install it together with `Videra.Avalonia` on Windows:

```bash
dotnet add package Videra.Avalonia --version 0.1.0-alpha.1 --source github-ExplodingUFO
dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

This package supplies the Direct3D 11 backend on a matching Windows host.

Windows repository validation already covers matching-host validation through the standard hosted and local verification flows.

## Validation

Standard repository verification already covers Windows-specific tests and real `HWND` lifecycle checks:

```bash
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

## Requirements

- Windows 10 or newer
- Direct3D 11-capable GPU
- Feature Level 11_0 support

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-windows.md)
