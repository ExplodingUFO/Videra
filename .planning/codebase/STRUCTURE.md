# Codebase Structure

**Analysis Date:** 2026-03-28

## Directory Layout

```
Videra/
├── src/                          # Source code projects
│   ├── Videra.Core/              # Core engine (platform-agnostic)
│   ├── Videra.Avalonia/          # Avalonia UI integration
│   ├── Videra.Platform.Windows/  # Windows D3D11 backend
│   ├── Videra.Platform.macOS/    # macOS Metal backend
│   ├── Videra.Platform.Linux/    # Linux Vulkan backend
├── samples/                      # Demo applications
│   └── Videra.Demo/              # Sample Avalonia app
├── docs/                         # Documentation
│   └── plans/                    # Design documents (markdown)
├── .github/                      # GitHub workflows
│   └── workflows/                # CI/CD definitions
├── .planning/                    # Planning artifacts (GSD)
│   └── codebase/                 # Codebase analysis documents
└── [build outputs]               # bin/, obj/, .vs/, .vs/ (gitignored)
```

## Directory Purposes

**src/Videra.Core/:**
- Purpose: Platform-agnostic 3D rendering engine
- Contains: Graphics abstractions, software backend, camera, geometry, I/O, styles
- Key files: `VideraEngine.cs`, `Object3D.cs`, `OrbitCamera.cs`, `ModelImporter.cs`

**src/Videra.Avalonia/:**
- Purpose: Avalonia UI controls for embedding 3D views
- Contains: `VideraView` control, platform-specific native hosts, interop
- Key files: `VideraView.cs`, `VideraNativeHost.cs`, `VideraLinuxNativeHost.cs`, `VideraMacOSNativeHost.cs`

**src/Videra.Platform.*/:**
- Purpose: Platform-specific graphics backend implementations
- Contains: D3D11/Metal/Vulkan backend implementations
- Pattern: Each platform project loaded dynamically via reflection

**samples/Videra.Demo/:**
- Purpose: Example application showing engine usage
- Contains: ViewModels, Views, Services, converters
- Key files: `Program.cs`, `MainWindow.axaml`, `MainWindowViewModel.cs`

**docs/plans/:**
- Purpose: Design and planning documents
- Contains: Render style system design, wireframe rendering design, shader update plans
- Format: Markdown with date prefixes

**.github/workflows/:**
- Purpose: CI/CD automation
- Contains: NuGet package publishing workflow

## Key File Locations

**Entry Points:**
- `samples/Videra.Demo/Program.cs`: Demo application entry point
- `src/Videra.Core/Graphics/VideraEngine.cs`: Core rendering engine
- `src/Videra.Avalonia/Controls/VideraView.cs`: Main UI control

**Configuration:**
- `samples/Videra.Demo/Videra.Demo.csproj`: Demo project configuration
- `src/Videra.Core/Videra.Core.csproj`: Core project dependencies (SharpGLTF.Toolkit)
- `src/Videra.Avalonia/Videra.Avalonia.csproj`: UI layer (Avalonia 11.3.9)

**Core Logic:**
- `src/Videra.Core/Graphics/`: Main rendering subsystem
- `src/Videra.Core/Graphics/Abstractions/`: Backend interfaces
- `src/Videra.Core/Graphics/Software/`: CPU rasterization backend
- `src/Videra.Core/Graphics/Wireframe/`: Edge extraction and wireframe rendering
- `src/Videra.Core/Cameras/`: Orbit camera controller
- `src/Videra.Core/IO/`: Model import (GLTF/GLB/OBJ)
- `src/Videra.Core/Styles/`: Render style parameters and presets
- `src/Videra.Core/Geometry/`: Vertex types and mesh data structures

**Testing:**
- No dedicated test directory (inferred: testing not present)

## Naming Conventions

**Files:**
- PascalCase for classes: `VideraEngine.cs`, `OrbitCamera.cs`, `ModelImporter.cs`
- Interface prefix: `IGraphicsBackend.cs`, `IResourceFactory.cs`
- AXAML for UI: `MainWindow.axaml`, `MainWindow.axaml.cs`

**Directories:**
- PascalCase for features: `Graphics/`, `Cameras/`, `Styles/`
- Platform suffix for backends: `Videra.Platform.Windows/`, `Videra.Platform.macOS/`

**Types:**
- Classes: PascalCase (`Object3D`, `RenderStyleService`)
- Interfaces: IPrefix (`IGraphicsBackend`, `ICommandExecutor`)
- Enums: PascalCase (`WireframeMode`, `MeshTopology`, `RenderStylePreset`)
- Methods: PascalCase (`Initialize`, `Draw`, `UpdateUniforms`)
- Properties: PascalCase (`IsInitialized`, `BackgroundColor`, `WorldMatrix`)
- Fields: _camelCase for private (`_backend`, `_factory`, `_width`)

## Where to Add New Code

**New Feature:**
- Primary code: `src/Videra.Core/` (appropriate subsystem directory)
- Tests: Create `tests/Videra.Core.Tests/` (inferred - not present)
- Demo usage: `samples/Videra.Demo/ViewModels/` or `Services/`

**New Component/Module:**
- Implementation: `src/Videra.Core/[SubsystemName]/`
- Example: New renderer → `src/Videra.Core/Graphics/NewRenderer/`

**Utilities:**
- Shared helpers: `src/Videra.Core/[Domain]/Utils/` or `src/Videra.Core/[Domain]/Helpers/`
- Example: Mesh processing → `src/Videra.Core/Geometry/Utils/`

**Platform-Specific Code:**
- New platform backend: `src/Videra.Platform.[PlatformName]/`
- Registration: Update `GraphicsBackendFactory.cs` with new backend type

**New Style/Parameter:**
- Parameter class: `src/Videra.Core/Styles/Parameters/[Feature]Parameters.cs`
- Preset: Add to `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`
- Service integration: Update `RenderStyleParameters` aggregate class

## Special Directories

**bin/ and obj/:**
- Purpose: Build outputs (assemblies, intermediate compilation)
- Generated: Yes
- Committed: No (gitignored via standard .NET patterns)

**.vs/ and .vs/:**
- Purpose: IDE state (Visual Studio, Rider)
- Generated: Yes
- Committed: No

**.github/workflows/:**
- Purpose: CI/CD automation
- Generated: No (authored configuration)
- Committed: Yes

**docs/plans/:**
- Purpose: Historical design documentation
- Generated: No
- Committed: Yes

**.planning/codebase/:**
- Purpose: GSD codebase analysis output (this directory)
- Generated: Yes (by GSD agents)
- Committed: Yes (consumed by other GSD commands)

---

*Structure analysis: 2026-03-28*
