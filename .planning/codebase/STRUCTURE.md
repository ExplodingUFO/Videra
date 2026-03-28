# Codebase Structure

**Analysis Date:** 2025-03-28

## Directory Layout

```
Videra/
├── src/                      # Source projects
│   ├── Videra.Core/          # Platform-agnostic core
│   ├── Videra.Avalonia/      # AvaloniaUI integration
│   ├── Videra.Platform.Windows/  # Windows D3D11 backend
│   ├── Videra.Platform.Linux/    # Linux Vulkan backend
│   └── Videra.Platform.macOS/    # macOS Metal backend
├── samples/                  # Example applications
│   └── Videra.Demo/         # Demo application
├── docs/                     # Documentation
├── .github/                  # GitHub Actions workflows
└── .planning/                # Planning documents
```

## Directory Purposes

**src/Videra.Core/:**
- Purpose: Platform-independent rendering engine
- Contains: Graphics abstractions, scene management, camera, software renderer, style system
- Key files: `Graphics/VideraEngine.cs`, `Graphics/Abstractions/IGraphicsBackend.cs`, `Cameras/OrbitCamera.cs`

**src/Videra.Avalonia/:**
- Purpose: AvaloniaUI control integration
- Contains: VideraView control, native window hosts, input handling, platform interop
- Key files: `Controls/VideraView.cs`, `Controls/VideraNativeHost.cs`, `Controls/VideraView.Input.cs`

**src/Videra.Platform.Windows/:**
- Purpose: Windows Direct3D 11 graphics backend
- Contains: D3D11 backend, buffers, pipelines, command executor, resource factory
- Key files: `D3D11Backend.cs`, `D3D11Buffer.cs`, `D3D11ResourceFactory.cs`

**src/Videra.Platform.Linux/:**
- Purpose: Linux Vulkan graphics backend
- Contains: Vulkan backend, buffers, pipelines, command executor, resource factory
- Key files: `VulkanBackend.cs`, `VulkanBuffer.cs`, `VulkanResourceFactory.cs`

**src/Videra.Platform.macOS/:**
- Purpose: macOS Metal graphics backend
- Contains: Metal backend, buffers, pipelines, command executor, resource factory
- Key files: `MetalBackend.cs`, `MetalBuffer.cs`, `MetalResourceFactory.cs`

**samples/Videra.Demo/:**
- Purpose: Example application demonstrating Videra usage
- Contains: MVVM application, ViewModels, Views, services
- Key files: `Program.cs`, `Views/MainWindow.axaml`, `ViewModels/MainWindowViewModel.cs`

## Key File Locations

**Entry Points:**
- `samples/Videra.Demo/Program.cs`: Application entry point
- `src/Videra.Avalonia/Controls/VideraView.cs`: Main 3D view control
- `src/Videra.Core/Graphics/VideraEngine.cs`: Core rendering engine

**Configuration:**
- `src/Videra.Avalonia/Videra.Avalonia.csproj`: Conditional platform references
- `samples/Videra.Demo/Program.cs`: Platform-specific Avalonia options

**Core Logic:**
- `src/Videra.Core/Graphics/`: Rendering pipeline, engine, objects
- `src/Videra.Core/Graphics/Abstractions/`: Graphics API interfaces
- `src/Videra.Core/Cameras/`: Camera implementation
- `src/Videra.Core/Styles/`: Render style system

**Platform Implementations:**
- `src/Videra.Platform.Windows/`: Direct3D 11 implementation
- `src/Videra.Platform.Linux/`: Vulkan implementation
- `src/Videra.Platform.macOS/`: Metal implementation
- `src/Videra.Core/Graphics/Software/`: Software rendering fallback

## Naming Conventions

**Files:**
- PascalCase: `VideraEngine.cs`, `OrbitCamera.cs`, `D3D11Backend.cs`
- Partial classes: `VideraView.Input.cs`, `VideraNativeHost.cs`
- Interface prefix: `IGraphicsBackend.cs`, `IResourceFactory.cs`

**Directories:**
- PascalCase: `Graphics/`, `Cameras/`, `Controls/`
- Platform suffix: `Videra.Platform.Windows/`, `Videra.Platform.Linux/`

**Namespaces:**
- Match directory structure: `Videra.Core.Graphics`, `Videra.Avalonia.Controls`
- Platform-specific: `Videra.Platform.Windows`, `Videra.Platform.Linux`, `Videra.Platform.macOS`

## Where to Add New Code

**New Graphics Feature:**
- Primary code: `src/Videra.Core/Graphics/[FeatureName].cs`
- Abstractions: `src/Videra.Core/Graphics/Abstractions/I[FeatureName].cs`
- Platform implementations: Each `Videra.Platform.*` project

**New UI Control:**
- Implementation: `src/Videra.Avalonia/Controls/[ControlName].cs`
- XAML (if needed): In consuming application

**New Render Style:**
- Parameters: `src/Videra.Core/Styles/Parameters/[Style]Parameters.cs`
- Preset: `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`

**New Platform Backend:**
- Project: `src/Videra.Platform.[PlatformName]/`
- Reference: Add to `Videra.Avalonia.csproj` with condition
- Implementation: Implement `IGraphicsBackend`, `IResourceFactory`, `ICommandExecutor`

**Utilities:**
- Shared helpers: `src/Videra.Core/Utilities/` (create if needed)

## Special Directories

**obj/ directories:**
- Purpose: Build output, generated files
- Generated: Yes
- Committed: No (in .gitignore)

**Interop/:**
- Purpose: Platform API declarations
- Location: `src/Videra.Avalonia/Interop/Win32.cs`
- Generated: No
- Committed: Yes

**docs/:**
- Purpose: Design documentation
- Contains: Architecture docs, design documents
- Generated: No
- Committed: Yes

**.github/workflows/:**
- Purpose: CI/CD pipeline definitions
- Contains: NuGet publishing workflow
- Generated: No
- Committed: Yes

---

*Structure analysis: 2025-03-28*
