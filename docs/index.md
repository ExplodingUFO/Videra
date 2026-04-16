# Documentation Index

[English](index.md) | [中文](zh-CN/index.md)

This page maps the long-lived public documentation for Videra. Start with the repository README when you need the public package boundary, source-only module truth, or the current alpha support contract.

## Start Here

- [Project README](../README.md) for what Videra is, who it is for, and the source / public-package / contribution entry paths
- [Package Matrix](package-matrix.md) for published packages, source-only modules, samples, demos, and feed boundaries
- [Support Matrix](support-matrix.md) for platform, validation, and support-level truth
- [Release Policy](release-policy.md) for public-vs-preview feed rules, versioning scope, and what is or is not published
- [Releasing Runbook](releasing.md) for tag, asset, and release-page workflow
- [Extensibility Contract](extensibility.md) for the `VideraView.Engine` flow, `samples/Videra.ExtensibilitySample`, the ready / disposed / fallback behavior matrix, and the boundary that keeps `package discovery` and `plugin loading` out of scope
- [Troubleshooting](troubleshooting.md) for the relationship between package installation, `VIDERA_BACKEND`, software fallback, and matching-host validation
- [Native Validation](native-validation.md) for the GitHub Actions and local matching-host validation runbook
- [Architecture](../ARCHITECTURE.md)
- [Contributing](../CONTRIBUTING.md)

## Packages

- [Videra.Core](../src/Videra.Core/README.md) for core-only consumption and software-fallback-oriented integration
- [Videra.Avalonia](../src/Videra.Avalonia/README.md) for the Avalonia entry package and backend preference configuration
- [Videra.Platform.Windows](../src/Videra.Platform.Windows/README.md) for the Windows Direct3D 11 platform package
- [Videra.Platform.Linux](../src/Videra.Platform.Linux/README.md) for the Linux Vulkan platform package, with X11 native hosting and XWayland compatibility in Wayland sessions
- [Videra.Platform.macOS](../src/Videra.Platform.macOS/README.md) for the macOS `NSView` / `CAMetalLayer` Metal platform package

## Source-Only Modules and Samples

- [Videra.SurfaceCharts.Core](../src/Videra.SurfaceCharts.Core/README.md)
- [Videra.SurfaceCharts.Avalonia](../src/Videra.SurfaceCharts.Avalonia/README.md)
- [Videra.SurfaceCharts.Processing](../src/Videra.SurfaceCharts.Processing/README.md)
- [Videra.Demo](../samples/Videra.Demo/README.md)
- [Videra.SurfaceCharts.Demo](../samples/Videra.SurfaceCharts.Demo/README.md)
- [Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md)
- [Videra.InteractionSample](../samples/Videra.InteractionSample/README.md)
