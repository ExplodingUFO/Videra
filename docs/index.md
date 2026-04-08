# Documentation Index

[English](index.md) | [中文](zh-CN/index.md)

This page maps the long-lived public documentation for Videra. Start with the repository README when you need package-source setup, package selection, or the current alpha support boundary.

## Start Here

- [Project README](../README.md) for package-source setup, `Videra.Avalonia` plus matching `Videra.Platform.*` combinations, and `Videra.Core`-only consumption
- [Extensibility Contract](extensibility.md) for the `VideraView.Engine` flow, `samples/Videra.ExtensibilitySample`, the ready / disposed / fallback behavior matrix, and the boundary that keeps `package discovery` and `plugin loading` out of scope
- [Troubleshooting](troubleshooting.md) for the relationship between package installation, `VIDERA_BACKEND`, software fallback, and matching-host validation
- [Native Validation](native-validation.md) for the GitHub Actions and local matching-host validation runbook
- [Architecture](../ARCHITECTURE.md)
- [Contributing](../CONTRIBUTING.md)

## Packages

- [Videra.Core](../src/Videra.Core/README.md) for core-only consumption and software fallback-oriented integration
- [Videra.Avalonia](../src/Videra.Avalonia/README.md) for the Avalonia entry package and backend preference configuration
- [Videra.Platform.Windows](../src/Videra.Platform.Windows/README.md) for the Windows Direct3D 11 platform package
- [Videra.Platform.Linux](../src/Videra.Platform.Linux/README.md) for the Linux Vulkan platform package, with X11 native hosting and XWayland compatibility in Wayland sessions
- [Videra.Platform.macOS](../src/Videra.Platform.macOS/README.md) for the macOS `NSView` / `CAMetalLayer` Metal platform package
- [Videra.Demo](../samples/Videra.Demo/README.md)
- [Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md) for the narrow public extensibility reference

## Decisions

- [ADR-005: Rust Boundary Policy](adr/ADR-005-rust-boundary.md)

## Archive

- [Archive Overview](archive/README.md)

Archived material is retained for historical reference only and is no longer maintained as an active entrypoint.
