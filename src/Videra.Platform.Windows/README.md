# Videra.Platform.Windows

[English](README.md) | [中文](../../docs/zh-CN/modules/platform-windows.md)

`Videra.Platform.Windows` provides the Windows-native Direct3D 11 backend used by Videra.

## Responsibilities

- Direct3D 11 device and swapchain setup
- Render-target and depth-buffer lifecycle
- D3D11 resource factory and command executor
- Real `HWND`-backed rendering integration

## Install

```bash
dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

This package is usually consumed transitively from `Videra.Avalonia`.

## Validation

Standard repository verification already covers Windows-specific tests and real `HWND` lifecycle checks:

```bash
pwsh -File ./verify.ps1 -Configuration Release
```

## Requirements

- Windows 10 or newer
- Direct3D 11-capable GPU
- Feature Level 11_0 support

## Related Docs

- [Repository README](../../README.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/platform-windows.md)
