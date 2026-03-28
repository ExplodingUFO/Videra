# Technology Stack

**Analysis Date:** 2026-03-28

## Languages

**Primary:**
- C# 12 - All source code (.NET 8.0)

**Secondary:**
- Metal Shading Language - `src/Videra.Platform.macOS/Shaders.metal` (macOS GPU shaders)
- Objective-C Runtime interop - P/Invoke bindings in macOS platform code
- HLSL (inferred) - DirectX shaders compiled at runtime via Silk.NET.Direct3D.Compilers on Windows
- GLSL/SPIR-V (inferred) - Vulkan shaders compiled via Silk.NET.Shaderc on Linux

## Runtime

**Environment:**
- .NET 8.0 - All projects target `net8.0`

**Package Manager:**
- NuGet - Central package management
- Lockfile: Not present (project-level references only)

## Frameworks

**Core:**
- System.Numerics.Vectors - Built-in vector math (via .NET 8.0)
- System.Runtime.InteropServices - P/Invoke for native interop

**Graphics:**
- Silk.NET 2.21.0 - Cross-platform graphics API bindings
  - Silk.NET.Direct3D11 - Windows D3D11 bindings
  - Silk.NET.DXGI - DirectX Graphics Infrastructure
  - Silk.NET.Direct3D.Compilers - HLSL shader compilation
  - Silk.NET.Vulkan - Vulkan bindings
  - Silk.NET.Vulkan.Extensions.KHR - Vulkan KHR extensions
  - Silk.NET.Shaderc - Vulkan GLSL compiler
  - Silk.NET.Core - Core utilities

**UI Framework:**
- Avalonia 11.3.9 - Cross-platform XAML-based UI framework
  - Avalonia.Desktop - Desktop platform support
  - Avalonia.Themes.Fluent - Fluent design theme
  - Avalonia.Controls.ColorPicker - Color selection control
  - Avalonia.Fonts.Inter - Inter font family
  - Avalonia.Diagnostics - Dev tools (debug only)

**Model Loading:**
- SharpGLTF.Toolkit 1.0.6 - glTF 2.0 file format reader

**MVVM:**
- CommunityToolkit.Mvvm 8.2.1 - ViewModel base classes, commands

**DI:**
- Microsoft.Extensions.DependencyInjection 9.0.11 - IoC container
- Microsoft.Extensions.Hosting 9.0.11 - Application hosting abstractions

## Key Dependencies

**Critical:**
- Silk.NET suite - Graphics API interop layer
- SharpGLTF.Toolkit - 3D model import (glTF/GLB/OBJ)
- Avalonia UI - Presentation layer

**Infrastructure:**
- System.Runtime.InteropServices - Native library interop
- Microsoft.Extensions.* - Dependency injection and hosting

## Configuration

**Environment:**
- `VIDERA_BACKEND` - Force graphics backend (software/d3d11/vulkan/metal/auto)
- `VIDERA_FRAMELOG` - Enable frame logging (1/true)
- `VIDERA_INPUTLOG` - Enable input logging (1/true)

**Build:**
- MSBuild SDK-style projects (.csproj)
- Runtime-specific builds via `<RuntimeIdentifier>`:
  - `win-x64` - Windows platform
  - `linux-x64` - Linux platform
  - macOS uses framework imports (no RID specified in csproj)

## Platform Requirements

**Development:**
- .NET 8.0 SDK
- OS-specific tools:
  - Windows: None beyond .NET SDK
  - Linux: X11 development headers, Vulkan driver
  - macOS: Xcode Command Line Tools (for Metal shader compiler via xcrun)

**Production:**
- **Windows:** Windows 10+, Direct3D 11-compatible GPU, .NET 8 Runtime
- **Linux:** X11 window system, Vulkan 1.2+ GPU, .NET 8 Runtime
- **macOS:** macOS 10.15+, Metal-compatible GPU, .NET 8 Runtime

---

*Stack analysis: 2026-03-28*
