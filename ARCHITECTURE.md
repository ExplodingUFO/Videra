# Videra Architecture

[English](ARCHITECTURE.md) | [中文](docs/zh-CN/ARCHITECTURE.md)

This document describes Videra's public architecture boundaries, module responsibilities, and runtime flow for contributors and evaluators.

## Design Goals

Videra is designed to provide stable 3D viewer capabilities inside Avalonia desktop applications:

- Unify cross-platform 3D view behavior
- Keep core rendering logic decoupled from platform graphics APIs
- Select the most suitable native backend per platform
- Provide a software fallback for diagnostics and no-GPU environments
- Maintain clear boundaries between demo code, library code, and validation

## Layering

```mermaid
graph TB
    Demo[Videra.Demo]
    Avalonia[Videra.Avalonia]
    Core[Videra.Core]
    Win[Videra.Platform.Windows]
    Linux[Videra.Platform.Linux]
    Mac[Videra.Platform.macOS]

    Demo --> Avalonia
    Avalonia --> Core
    Core --> Win
    Core --> Linux
    Core --> Mac
```

### `Videra.Core`

Platform-agnostic rendering layer responsible for:

- Rendering abstractions
- Scene object and engine lifecycle
- Camera, grid, axis, and wireframe logic
- Model import
- Render-style presets
- Software fallback rendering

Key abstractions:

- `IGraphicsBackend`
- `IResourceFactory`
- `ICommandExecutor`
- `GraphicsBackendFactory`

### `Videra.Avalonia`

UI integration layer responsible for:

- The `VideraView` control surface
- Bridging Avalonia with native-host handles
- Backend preference and render-session coordination
- Pointer input and camera interaction mapping

This layer lets host apps use the 3D view from XAML or code without directly coupling UI code to backend implementation details.

### Native Backend Packages

Each backend implements `IGraphicsBackend` against a native graphics API:

- `Videra.Platform.Windows`: Direct3D 11
- `Videra.Platform.Linux`: Vulkan
- `Videra.Platform.macOS`: Metal

These packages handle:

- Device initialization
- Swapchain / drawable lifecycle
- Depth-buffer and frame management
- Resource factories and command executors

### `Videra.Demo`

The demo application shows:

- `VideraView` integration
- Model import flows
- Render-style and wireframe switching
- Grid, axes, and basic object transforms
- Backend status and default scene bootstrapping

## Repository Layout

```text
Videra/
├── src/
│   ├── Videra.Core/
│   ├── Videra.Avalonia/
│   ├── Videra.Platform.Windows/
│   ├── Videra.Platform.Linux/
│   └── Videra.Platform.macOS/
├── samples/
│   └── Videra.Demo/
├── tests/
├── docs/
├── verify.sh
└── verify.ps1
```

## Runtime Flow

```mermaid
sequenceDiagram
    participant App as Host App
    participant View as VideraView
    participant Session as RenderSession
    participant Engine as VideraEngine
    participant Backend as IGraphicsBackend

    App->>View: Create control
    View->>Session: Apply backend preference
    Session->>Backend: Resolve and create backend
    View->>Engine: Initialize engine
    Engine->>Backend: Initialize(...)

    loop Per frame
        View->>Engine: Draw()
        Engine->>Backend: BeginFrame()
        Engine->>Backend: Draw scene
        Engine->>Backend: EndFrame()
    end
```

## Render Pipeline Contract

Phase 9 makes the current frame orchestration explicit without changing the public rendering model. `VideraEngine` now builds a frame plan, executes it in a stable order, and captures the result in `LastPipelineSnapshot`.

Stable stage vocabulary for one frame:

- `PrepareFrame`
- `BindSharedFrameState`
- `GridPass`
- `SolidGeometryPass`
- `WireframePass`
- `AxisPass`
- `PresentFrame`

Contract notes:

- `WireframePass` is conditional. Standard frames omit it, wireframe-overlay frames include it after `SolidGeometryPass`, and `WireframeOnly` frames skip `SolidGeometryPass`.
- `LastPipelineSnapshot` records the executed stages plus the effective pipeline profile for the last completed frame.
- `VideraView.BackendDiagnostics` mirrors the same read-only truth through `RenderPipelineProfile`, `LastFrameStageNames`, and `UsesSoftwarePresentationCopy`.
- This milestone documents the existing pipeline shape only. It does not ship public custom-pass registration or frame-hook extensibility APIs yet.

## Backend Selection

Videra exposes two backend-selection paths:

1. `VideraView.PreferredBackend`
2. `VIDERA_BACKEND` environment variable

When set to `Auto`, the default preference is:

- Windows: `D3D11`
- Linux: `Vulkan`
- macOS: `Metal`

If the native backend is unavailable, or if `software` is selected explicitly, rendering falls back to the software path.

## Supported Capabilities

- Model import: `.gltf`, `.glb`, `.obj`
- Orbit camera and basic scene interaction
- Render-style presets
- Wireframe and overlay modes
- Grid and axis helpers
- Native rendering backends
- Software fallback backend

## Validation Strategy

Repository-wide validation entrypoints:

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

By default:

- Standard validation covers solution build, tests, and common checks through `verify.sh` / `verify.ps1`
- GitHub-hosted required checks additionally cover `windows-native`, `macos-native`, `linux-x11-native`, and `linux-wayland-xwayland-native`
- Linux Wayland sessions are validated through the XWayland compatibility path, not compositor-native embedding

## Current Limits

- Videra targets componentized 3D viewing rather than a full content creation pipeline
- Linux native support currently means `X11` plus `Wayland` sessions running through `XWayland` compatibility fallback
- The macOS backend relies on Objective-C runtime interop
- Public render-pipeline extensibility is not exposed yet; current pipeline diagnostics are read-only contract truth

## Related Docs

- [README.md](README.md)
- [Documentation Index](docs/index.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Chinese Architecture Doc](docs/zh-CN/ARCHITECTURE.md)
