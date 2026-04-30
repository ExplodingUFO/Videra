# Codebase Structure

**Analysis Date:** 2026-04-02

## Directory Layout

```text
[project-root]/
├── src/                        # Shipping libraries
│   ├── Videra.Core/            # Backend-neutral engine, geometry, import, styles, software fallback
│   ├── Videra.Avalonia/        # Avalonia control and native-host bridge
│   ├── Videra.Platform.Windows/# Windows D3D11 backend
│   ├── Videra.Platform.Linux/  # Linux Vulkan backend and X11 surface glue
│   └── Videra.Platform.macOS/  # macOS Metal backend and Objective-C interop
├── samples/
│   └── Videra.Demo/            # Desktop demo app using the reusable control
├── tests/                      # Project-mirrored unit/integration tests plus shared helpers
├── docs/                       # Handwritten project documentation
├── .planning/                  # GSD planning state and generated codebase maps
├── coverage/                   # Generated coverage output
├── Directory.Build.props       # Shared analyzer and warning policy
├── verify.ps1                  # PowerShell verification entry point
├── verify.sh                   # Shell verification entry point
└── Videra.slnx                 # Solution entry point
```

## Directory Purposes

**`src/Videra.Core/`:**
- Purpose: Put backend-neutral rendering code here first.
- Contains: `Cameras/`, `Exceptions/`, `Geometry/`, `Graphics/`, `IO/`, `Logging/`, `NativeLibrary/`, `Styles/`
- Key files: `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`, `src/Videra.Core/Graphics/Object3D.cs`, `src/Videra.Core/Cameras/OrbitCamera.cs`, `src/Videra.Core/IO/ModelImporter.cs`

**`src/Videra.Core/Graphics/Abstractions/`:**
- Purpose: Keep cross-backend contracts isolated from implementation details.
- Contains: `IGraphicsBackend.cs`, `ICommandExecutor.cs`, `IResourceFactory.cs`, `IBuffer.cs`, `IPipeline.cs`, `ISoftwareBackend.cs`
- Key files: `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`, `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`, `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`

**`src/Videra.Core/Graphics/Software/`:**
- Purpose: Preserve the CPU fallback backend used when native backend loading fails or software rendering is requested.
- Contains: `SoftwareBackend.cs`, `SoftwareCommandExecutor.cs`, `SoftwareFrameBuffer.cs`, `SoftwareResourceFactory.cs`, `SoftwarePipeline.cs`
- Key files: `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`, `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs`

**`src/Videra.Avalonia/`:**
- Purpose: House the reusable UI adapter layer; do not put engine logic or native GPU code here.
- Contains: `Controls/`, `Interop/`
- Key files: `src/Videra.Avalonia/Controls/VideraView.cs`, `src/Videra.Avalonia/Controls/VideraView.Input.cs`, `src/Videra.Avalonia/Controls/VideraNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`

**`src/Videra.Platform.Windows/`:**
- Purpose: Keep Windows-only D3D11 code isolated from cross-platform layers.
- Contains: backend, command executor, resource factory, buffers, pipeline
- Key files: `src/Videra.Platform.Windows/D3D11Backend.cs`, `src/Videra.Platform.Windows/D3D11CommandExecutor.cs`, `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`

**`src/Videra.Platform.Linux/`:**
- Purpose: Keep Linux-only Vulkan and X11 code isolated from cross-platform layers.
- Contains: Vulkan backend/resource types, surface creation strategy, X11-specific implementation
- Key files: `src/Videra.Platform.Linux/VulkanBackend.cs`, `src/Videra.Platform.Linux/ISurfaceCreator.cs`, `src/Videra.Platform.Linux/X11SurfaceCreator.cs`

**`src/Videra.Platform.macOS/`:**
- Purpose: Keep macOS-only Metal and Objective-C interop code isolated from cross-platform layers.
- Contains: Metal backend/resource types, runtime helpers, shader source
- Key files: `src/Videra.Platform.macOS/MetalBackend.cs`, `src/Videra.Platform.macOS/MetalCommandExecutor.cs`, `src/Videra.Platform.macOS/MetalResourceFactory.cs`, `src/Videra.Platform.macOS/ObjCRuntime.cs`, `src/Videra.Platform.macOS/Shaders.metal`

**`samples/Videra.Demo/`:**
- Purpose: Show the expected composition pattern for a host application.
- Contains: `Assets/`, `Converters/`, `Models/`, `Services/`, `ViewModels/`, `Views/`, platform manifests, app bootstrap files
- Key files: `samples/Videra.Demo/Program.cs`, `samples/Videra.Demo/App.axaml.cs`, `samples/Videra.Demo/Views/MainWindow.axaml`, `samples/Videra.Demo/Views/MainWindow.axaml.cs`, `samples/Videra.Demo/Services/AvaloniaModelImporter.cs`

**`tests/`:**
- Purpose: Mirror production projects and keep shared test helpers in one place.
- Contains: `tests/Videra.Core.Tests/`, `tests/Videra.Core.IntegrationTests/`, `tests/Videra.Platform.Windows.Tests/`, `tests/Videra.Platform.Linux.Tests/`, `tests/Videra.Platform.macOS.Tests/`, `tests/Tests.Common/`
- Key files: `tests/Videra.Core.Tests/Videra.Core.Tests.csproj`, `tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj`, `tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj`, `tests/Tests.Common/Tests.Common.csproj`

**`.planning/`:**
- Purpose: Store planning state and generated maps; this is not runtime code.
- Contains: `PROJECT.md`, `REQUIREMENTS.md`, `ROADMAP.md`, `STATE.md`, `codebase/`, `phases/`, `research/`
- Key files: `.planning/codebase/ARCHITECTURE.md`, `.planning/codebase/STRUCTURE.md`

## Key File Locations

**Entry Points:**
- `samples/Videra.Demo/Program.cs`: Desktop process entry point and Windows backend defaulting
- `samples/Videra.Demo/App.axaml.cs`: DI and host bootstrap
- `samples/Videra.Demo/Views/MainWindow.axaml`: Demo window composition and `VideraView` bindings
- `samples/Videra.Demo/Views/MainWindow.axaml.cs`: Backend-ready orchestration for importer and demo scene setup
- `src/Videra.Avalonia/Controls/VideraView.cs`: Reusable control entry into the rendering engine
- `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`: Runtime backend selection

**Configuration:**
- `Videra.slnx`: Solution entry point
- `Directory.Build.props`: Analyzer policy and debug warning behavior
- `src/Videra.Avalonia/Videra.Avalonia.csproj`: Avalonia package set plus OS-conditioned platform project references
- `samples/Videra.Demo/Videra.Demo.csproj`: Demo dependencies and platform packaging assets
- `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`: Metal shader compile target
- `verify.ps1`: PowerShell verification wrapper
- `verify.sh`: Shell verification wrapper

**Core Logic:**
- `src/Videra.Core/Graphics/VideraEngine.cs`: Frame orchestration and scene ownership
- `src/Videra.Core/Graphics/Object3D.cs`: Renderable object and GPU resource ownership
- `src/Videra.Core/Cameras/OrbitCamera.cs`: Camera movement and projection math
- `src/Videra.Core/IO/ModelImporter.cs`: Asset parsing and `Object3D` creation
- `src/Videra.Core/Styles/Services/RenderStyleService.cs`: Render-style state and preset management
- `src/Videra.Avalonia/Controls/VideraView.Input.cs`: Pointer and camera interaction bridge
- `src/Videra.Platform.Windows/D3D11Backend.cs`: Windows backend root
- `src/Videra.Platform.Linux/VulkanBackend.cs`: Linux backend root
- `src/Videra.Platform.macOS/MetalBackend.cs`: macOS backend root

**Testing:**
- `tests/Videra.Core.Tests/`: Backend-neutral unit tests mirroring core namespaces
- `tests/Videra.Core.IntegrationTests/`: Multi-type core integration coverage
- `tests/Videra.Platform.Windows.Tests/Backend/`: Windows backend smoke and behavior tests
- `tests/Videra.Platform.Linux.Tests/`: Linux backend tests
- `tests/Videra.Platform.macOS.Tests/`: macOS backend tests
- `tests/Tests.Common/`: Shared mocks and test helpers

## Naming Conventions

**Files:**
- `TypeName.cs` for one primary type per file: `VideraEngine.cs`, `Object3D.cs`, `OrbitCamera.cs`
- `TypeName.PartialConcern.cs` for partial control splits: `VideraView.cs`, `VideraView.Input.cs`
- `BackendPrefixType.cs` for platform implementations: `D3D11Backend.cs`, `VulkanCommandExecutor.cs`, `MetalResourceFactory.cs`, `SoftwareBackend.cs`
- `ViewName.axaml` plus `ViewName.axaml.cs` for Avalonia views: `MainWindow.axaml`, `App.axaml.cs`

**Directories:**
- `Videra.{Area}` at the project level: `Videra.Core`, `Videra.Avalonia`, `Videra.Platform.Windows`
- Domain subdirectories inside `src/Videra.Core/` are noun-based and feature-scoped: `Graphics`, `Styles`, `IO`, `Cameras`
- Test directories mirror the production project or namespace they cover: `tests/Videra.Core.Tests/Graphics/`, `tests/Videra.Platform.Windows.Tests/Backend/`

## Where to Add New Code

**New backend-neutral rendering feature:**
- Primary code: `src/Videra.Core/Graphics/` or `src/Videra.Core/Styles/`
- Tests: `tests/Videra.Core.Tests/Graphics/` or `tests/Videra.Core.IntegrationTests/` when the feature spans multiple core types

**New camera, geometry, import, or native-library helper:**
- Primary code: `src/Videra.Core/Cameras/`, `src/Videra.Core/Geometry/`, `src/Videra.Core/IO/`, or `src/Videra.Core/NativeLibrary/`
- Tests: Mirror the same subfolder under `tests/Videra.Core.Tests/`

**New Avalonia control behavior or input/native-host glue:**
- Implementation: `src/Videra.Avalonia/Controls/`
- Supporting code: `src/Videra.Avalonia/Interop/` only when the helper is Avalonia-specific and not reusable by backends

**New platform backend or platform-specific GPU capability:**
- Implementation: `src/Videra.Platform.{Platform}/`
- Supporting wiring: `src/Videra.Avalonia/Videra.Avalonia.csproj` for OS-conditioned references and `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` for runtime selection
- Tests: `tests/Videra.Platform.{Platform}.Tests/`

**New demo feature or usage example:**
- Implementation: `samples/Videra.Demo/ViewModels/`, `samples/Videra.Demo/Views/`, `samples/Videra.Demo/Services/`
- Supporting assets: `samples/Videra.Demo/Assets/`, `samples/Videra.Demo/Converters/`, `samples/Videra.Demo/Models/`

**Shared test helpers or reusable mocks:**
- Shared helpers: `tests/Tests.Common/`
- Consumers: Reference `tests/Tests.Common/Tests.Common.csproj` instead of duplicating helpers in each test project

## Structure Boundaries

**Stable boundaries:**
- Keep anything that can compile without Avalonia or native APIs inside `src/Videra.Core/`.
- Keep anything that depends on Avalonia control lifecycle inside `src/Videra.Avalonia/`.
- Keep anything that directly touches D3D11, Vulkan, Metal, X11, Win32, or Objective-C inside the matching `src/Videra.Platform.*` project.

**Current transition zones:**
- `src/Videra.Avalonia/Controls/` currently contains both reusable control logic and per-platform native-host implementations. If the host layer grows, split by platform or host concern instead of extending `VideraView.cs`.
- `src/Videra.Platform.macOS/ObjCRuntime.cs` is the preferred home for new Objective-C interop, but older files still carry inline interop from the migration period.
- `src/Videra.Platform.Linux/ISurfaceCreator.cs` is the extension point for new Linux surface types. New display-server support belongs beside `X11SurfaceCreator.cs`, not inside `VulkanBackend.cs`.
- `samples/Videra.Demo/Views/MainWindow.axaml` is Windows-biased today because it fixes `PreferredBackend="D3D11"`. Sample portability changes belong in the demo layer, not in `src/Videra.Core/`.

## Special Directories

**`bin/` and `obj/`:**
- Purpose: Build outputs for each project
- Generated: Yes
- Committed: No

**`coverage/`:**
- Purpose: Coverage artifacts from local verification
- Generated: Yes
- Committed: No

**`tests/Tests.Common/`:**
- Purpose: Shared test infrastructure referenced by multiple test projects
- Generated: No
- Committed: Yes

**`samples/Videra.Demo/Models/`:**
- Purpose: Placeholder location for demo-side model assets if they are added
- Generated: No
- Committed: Yes

**`.planning/codebase/`:**
- Purpose: Generated codebase maps consumed by later GSD steps
- Generated: Yes
- Committed: Yes

---

*Structure analysis: 2026-04-02*
