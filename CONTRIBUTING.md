# Contributing to Videra

Thank you for your interest in contributing to Videra! This guide covers everything you need to get started.

**Contributions welcome** -- whether it is a bug fix, a new feature, improved documentation, or a platform-specific enhancement, we appreciate your help.

## Table of Contents

- [Development Environment Setup](#development-environment-setup)
- [Building from Source](#building-from-source)
- [Running the Demo](#running-the-demo)
- [Code Style](#code-style)
- [Commit Messages](#commit-messages)
- [Branching Strategy and PR Process](#branching-strategy-and-pr-process)
- [Testing Requirements](#testing-requirements)
- [Clean Code Policy](#clean-code-policy)
- [Visual Examples and Screenshots](#visual-examples-and-screenshots)
- [Key Public APIs](#key-public-apis)
- [Platform-Specific Notes](#platform-specific-notes)
- [Model Import Workflow](#model-import-workflow)
- [Architecture and Documentation](#architecture-and-documentation)
- [Troubleshooting](#troubleshooting)

---

## Development Environment Setup

**Prerequisites:**

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A compatible IDE: Visual Studio 2022, JetBrains Rider, or VS Code with the C# Dev Kit
- Git

**Platform-specific requirements:**

| Platform | Graphics API | Requirements |
|----------|-------------|--------------|
| Windows 10+ | Direct3D 11 | D3D11-compatible GPU |
| Linux (X11) | Vulkan 1.2+ | Vulkan-compatible GPU, `libX11.so.6` |
| macOS 10.15+ | Metal | Metal-compatible GPU |

## Building from Source

Clone the repository and build the solution:

```bash
git clone https://github.com/<owner>/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
```

**Platform-specific build and run:**

```bash
# Windows
dotnet build Videra.slnx
dotnet run --project samples/Videra.Demo

# Linux
dotnet build Videra.slnx
dotnet run --project samples/Videra.Demo

# macOS
dotnet build Videra.slnx
dotnet run --project samples/Videra.Demo
```

To publish a release build:

```bash
dotnet publish -c Release -r win-x64    # Windows
dotnet publish -c Release -r linux-x64  # Linux
dotnet publish -c Release -r osx-x64    # macOS
```

## Running the Demo

The demo application is the primary way to verify changes interactively. Always run it before opening a PR.

```bash
cd samples/Videra.Demo
dotnet run
```

The demo provides:

- Interactive 3D model viewer with orbit camera (left-drag to rotate, right-drag to pan, scroll to zoom)
- Model import from `.gltf`, `.glb`, and `.obj` files
- Render style switching (Realistic, Tech, Cartoon, X-Ray, Clay, Wireframe)
- Wireframe mode toggling (None, AllEdges, SharpEdges)
- Grid and axis rendering controls
- Transform editing (position, rotation, scale)

## Code Style

Follow the conventions documented in [`.planning/codebase/CONVENTIONS.md`](.planning/codebase/CONVENTIONS.md). Key rules:

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Namespaces | PascalCase, hierarchical | `Videra.Core.Graphics.Abstractions` |
| Classes | PascalCase | `OrbitCamera`, `D3D11Backend` |
| Interfaces | `I` prefix + PascalCase | `IGraphicsBackend`, `IResourceFactory` |
| Methods | PascalCase | `InitializeWireframe`, `UpdateProjection` |
| Async methods | PascalCase + `Async` suffix | `ImportModelsAsync` |
| Properties | PascalCase | `IsInitialized`, `FieldOfView` |
| Private fields | `_camelCase` | `_radius`, `_width`, `_clearColor` |
| Structs | PascalCase, implement `IEquatable<T>` | `RgbaFloat`, `VertexPositionNormalColor` |
| Enums | PascalCase | `WireframeMode`, `RenderStylePreset` |
| Flags enums | `[Flags]` attribute | `[Flags] enum ShaderStage` |

### Formatting

- Implicit usings enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Target framework: .NET 8.0 across all projects

### Import Organization

Organize `using` statements in this order:

1. System namespaces (`System`, `System.Numerics`, `System.IO`)
2. Third-party libraries (`Silk.NET.*`, `Avalonia.*`, `CommunityToolkit.Mvvm`, `SharpGLTF.*`)
3. Videra namespaces (`Videra.Core.*`, `Videra.Platform.*`)

### Error Handling

- Throw exceptions with descriptive messages: `throw new PlatformNotSupportedException("Native GPU host is only implemented on Windows.")`
- Format HRESULT codes for native errors: `throw new Exception($"Failed to create D3D11 device. HRESULT: 0x{result:X8}")`
- Use `IDisposable` for GPU resources with proper cleanup: `VertexBuffer?.Dispose()`
- Clean up partially initialized resources in `try`/`catch` blocks

### Logging

- Use `ILogger` (Microsoft.Extensions.Logging) for structured logging -- avoid raw `Console.WriteLine` in production code
- Use prefix tags for component identification: `[ModelImporter]`, `[VideraEngine]`
- Use `System.Diagnostics.Debug.WriteLine` for platform-specific diagnostics only

### Comments and Documentation

- Add XML doc comments (`/// <summary>`) on all public APIs
- Inline comments for complex math (projection matrices, depth range handling)
- Section dividers in large classes: `// ==========================================`

### Design Patterns

- **Factory pattern**: `IResourceFactory` for GPU resource creation, `GraphicsBackendFactory` for backend instantiation
- **Service pattern**: Interface-based services (`IRenderStyleService`, `IModelImporter`) with `sealed` implementations
- **MVVM (Avalonia)**: ViewModels inherit `ObservableObject`; use `[ObservableProperty]` and `[RelayCommand]` source generators

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <description>

[optional body]
```

**Types:**

| Type | Use for |
|------|---------|
| `feat` | New features |
| `fix` | Bug fixes |
| `docs` | Documentation changes |
| `style` | Formatting, no code change |
| `refactor` | Code restructuring without behavior change |
| `test` | Adding or updating tests |
| `chore` | Build, CI, or tooling changes |
| `perf` | Performance improvements |

**Scopes** correspond to project modules:

- `core` -- `Videra.Core`
- `avalonia` -- `Videra.Avalonia`
- `windows` -- `Videra.Platform.Windows`
- `linux` -- `Videra.Platform.Linux`
- `macos` -- `Videra.Platform.macOS`
- `demo` -- `Videra.Demo`

**Examples:**

```
feat(core): add STL format support to ModelImporter
fix(windows): resolve D3D11 swapchain resize crash
docs(avalonia): update VideraView property reference
test(core): add edge extraction unit tests
chore(demo): update Avalonia to 11.4
```

## Branching Strategy and PR Process

We use the **fork-and-pull** model:

1. **Fork** the repository to your GitHub account
2. **Create a branch** from `master`:
   ```
   git checkout -b feat/my-new-feature
   git checkout -b fix/d3d11-resize-crash
   git checkout -b docs/api-reference
   ```
3. **Make your changes** in small, focused commits
4. **Push** to your fork:
   ```
   git push origin feat/my-new-feature
   ```
5. **Open a Pull Request** against the `master` branch of the upstream repository

**PR checklist:**

- [ ] Code follows the conventions in [CONVENTIONS.md](.planning/codebase/CONVENTIONS.md)
- [ ] `dotnet build Videra.slnx` succeeds with no warnings
- [ ] Repository verification passes (`./verify.sh --configuration Release` or `pwsh -File ./verify.ps1 -Configuration Release`)
- [ ] Native-platform work includes matching-host validation when applicable
- [ ] No leftover debugging artifacts (see [Clean Code Policy](#clean-code-policy))
- [ ] Public APIs have XML doc comments
- [ ] Commit messages follow Conventional Commits format
- [ ] The demo runs correctly with your changes

## Testing Requirements

Run the standard verification entrypoint before pushing:

```bash
./verify.sh --configuration Release
# PowerShell equivalent
pwsh -File ./verify.ps1 -Configuration Release
```

For native backend validation on platform hosts, enable the explicit switches:

```bash
# Linux host (X11/Vulkan available)
./verify.sh --configuration Release --include-native-linux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux

# macOS host (NSView/Metal available)
./verify.sh --configuration Release --include-native-macos
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

Run the full test suite directly when needed:

```bash
dotnet test Videra.slnx
```

Before running tests, clean previous build outputs:

```bash
dotnet clean Videra.slnx
dotnet test Videra.slnx
```

When modifying specific files, you can target test projects directly:

```bash
dotnet clean
dotnet test --filter "FullyQualifiedName~Videra.Core"
```

**Testing priorities:**

- Core abstractions (`IGraphicsBackend`, `IBuffer`, `IPipeline`, `ICommandExecutor`)
- Model import logic (`ModelImporter.Load`, format detection, vertex parsing)
- Camera matrix calculations (`OrbitCamera.UpdateProjection`)
- Wireframe edge extraction (`EdgeExtractor.ExtractUniqueEdges`)
- Style parameter serialization (`StyleJsonConverter`)

See [`.planning/codebase/TESTING.md`](.planning/codebase/TESTING.md) for details on testing patterns and gaps.

## Clean Code Policy

**No leftover debugging artifacts in PRs.** This means:

- Remove all `Console.WriteLine` debug statements before committing
- Remove debug counters, temporary variables, and commented-out code
- Remove any `#if DEBUG` blocks that were used for temporary debugging
- Ensure `dotnet clean Videra.slnx` followed by `dotnet build Videra.slnx` succeeds cleanly

```bash
# Verify a clean build
dotnet clean Videra.slnx
dotnet build Videra.slnx
```

**Resource cleanup:**

- Always implement `IDisposable` for classes holding GPU or native resources
- Use the dispose pattern with conditional cleanup: `VertexBuffer?.Dispose()`
- Release native handles in `Dispose()` methods

## Visual Examples and Screenshots

When adding visual features or UI changes:

- **Include screenshots** showing the visual output before and after your change
- **Add examples** to the per-module README files (e.g., `src/Videra.Core/README.md`, `src/Videra.Avalonia/README.md`)
- Use the demo application (`samples/Videra.Demo`) to capture screenshots
- Place screenshots in a `docs/screenshots/` directory or reference them from per-module README files
- Ensure visual output is ready for first impressions -- see the **Features** section of the root [README.md](README.md) for the style of presentation

**Features to highlight in screenshots:**

- Cross-platform 3D rendering with native GPU backends
- Wireframe rendering overlay (AllEdges, SharpEdges, WireframeOnly modes)
- Render style presets (Realistic, Tech, Cartoon, X-Ray, Clay)
- Orbit camera controls with grid and axis visualization
- Depth buffering management with unified settings
- Backend selection (D3D11, Vulkan, Metal, Software fallback)
- Model import from `.gltf`, `.glb`, and `.obj` formats

**Per-module README guidance:**

Each module has its own README with API documentation, diagrams, and architecture info:

| Module | README | Content |
|--------|--------|---------|
| Videra.Core | [`src/Videra.Core/README.md`](src/Videra.Core/README.md) | Core API, abstractions, rendering pipeline |
| Videra.Avalonia | [`src/Videra.Avalonia/README.md`](src/Videra.Avalonia/README.md) | VideraView control, native hosts, input handling |
| Videra.Platform.Windows | [`src/Videra.Platform.Windows/README.md`](src/Videra.Platform.Windows/README.md) | D3D11 backend details |
| Videra.Platform.Linux | [`src/Videra.Platform.Linux/README.md`](src/Videra.Platform.Linux/README.md) | Vulkan backend details |
| Videra.Platform.macOS | [`src/Videra.Platform.macOS/README.md`](src/Videra.Platform.macOS/README.md) | Metal backend details |
| Videra.Demo | [`samples/Videra.Demo/README.md`](samples/Videra.Demo/README.md) | Demo app usage and MVVM patterns |

## Key Public APIs

### VideraView (Avalonia Control)

The primary 3D view control. See [`src/Videra.Avalonia/Controls/VideraView.cs`](src/Videra.Avalonia/Controls/VideraView.cs) and the [Videra.Avalonia README](src/Videra.Avalonia/README.md) for full details.

```xml
<!-- XAML usage -->
<controls:VideraView Name="View3D"
                     Items="{Binding SceneObjects}"
                     BackgroundColor="{Binding BgColor}"
                     IsGridVisible="True"
                     RenderStyle="Realistic"
                     WireframeMode="None"
                     PreferredBackend="Auto" />
```

```csharp
// C# usage
var view = new VideraView();
view.Engine.AddObject(myObject3D);
```

**Key properties:**

| Property | Type | Description |
|----------|------|-------------|
| `BackgroundColor` | `Color` | Scene background color |
| `Items` | `IEnumerable` | Collection of `Object3D` instances |
| `IsGridVisible` | `bool` | Toggle ground grid |
| `GridHeight` | `float` | Grid vertical offset |
| `GridColor` | `Color` | Grid line color |
| `RenderStyle` | `RenderStylePreset` | Active render style preset |
| `WireframeMode` | `WireframeMode` | Wireframe overlay mode |
| `PreferredBackend` | `GraphicsBackendPreference` | Backend selection preference |

### VideraEngine

The core rendering engine. See [`src/Videra.Core/Graphics/VideraEngine.cs`](src/Videra.Core/Graphics/VideraEngine.cs).

```csharp
public class VideraEngine : IDisposable
{
    public OrbitCamera Camera { get; }
    public void Initialize(IGraphicsBackend backend);
    public void AddObject(Object3D obj);
    public void RemoveObject(Object3D obj);
    public void Draw();
}
```

### IGraphicsBackend

The platform-agnostic rendering interface. See [`src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`](src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs).

```csharp
public interface IGraphicsBackend : IDisposable
{
    bool IsInitialized { get; }
    void Initialize(IntPtr windowHandle, int width, int height);
    void Resize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void SetClearColor(Vector4 color);
    IResourceFactory GetResourceFactory();
    ICommandExecutor GetCommandExecutor();
}
```

### IResourceFactory

GPU resource creation factory. See [`src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`](src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs).

```csharp
public interface IResourceFactory
{
    IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices);
    IBuffer CreateVertexBuffer(uint sizeInBytes);
    IBuffer CreateIndexBuffer(uint[] indices);
    IBuffer CreateIndexBuffer(uint sizeInBytes);
    IBuffer CreateUniformBuffer(uint sizeInBytes);
    IPipeline CreatePipeline(PipelineDescription description);
    IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors);
    IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint);
    IResourceSet CreateResourceSet(ResourceSetDescription description);
}
```

## Platform-Specific Notes

Each platform uses a dedicated native host for window embedding:

| Platform | Native Host | File | Technique |
|----------|------------|------|-----------|
| Windows | `VideraNativeHost` | [`src/Videra.Avalonia/Controls/VideraNativeHost.cs`](src/Videra.Avalonia/Controls/VideraNativeHost.cs) | Win32 HWND child window for D3D11 rendering |
| Linux | `VideraLinuxNativeHost` | [`src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`](src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs) | X11 window with Vulkan surface |
| macOS | `VideraMacOSNativeHost` | [`src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`](src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs) | Objective-C runtime NSView with CAMetalLayer |

**What each backend supports:**

- **D3D11 (Windows)**: Hardware-accelerated rendering, depth buffering, swapchain management, DPI scaling, HWND-based native window embedding
- **Vulkan (Linux)**: Hardware-accelerated rendering, depth buffering, X11 surface creation, swapchain management. Requires X11 and Vulkan 1.2+ driver
- **Metal (macOS)**: Hardware-accelerated rendering via Obj-C runtime, CAMetalLayer-backed NSView, Retina display support. Depth buffering support is in progress
- **Software (fallback)**: CPU-based rasterization, cross-platform. Automatically selected when hardware backends are unavailable

**Backend selection flow:**

1. Check `VIDERA_BACKEND` environment variable (`software`, `d3d11`, `vulkan`, `metal`, `auto`)
2. If `auto` or unset, detect the OS and select the native backend
3. If the native backend fails to initialize, fall back to the software renderer

## Model Import Workflow

The `ModelImporter` class provides a simplified, fixed workflow for loading 3D models from files. See [`src/Videra.Core/IO/ModelImporter.cs`](src/Videra.Core/IO/ModelImporter.cs).

**Supported formats:** `.gltf`, `.glb`, `.obj`

```csharp
using Videra.Core.IO;
using Videra.Core.Graphics.Abstractions;

// Load a model from file -- returns a fully initialized Object3D
// with GPU buffers allocated and ready to render
Object3D model = ModelImporter.Load(
    filePath: "path/to/model.glb",
    factory: engine.GetResourceFactory()
);

// Add to the scene
engine.AddObject(model);
```

The `ModelImporter.Load()` method handles:

1. File format detection by extension
2. glTF/GLB parsing via SharpGLTF with scene graph traversal and vertex transformations
3. OBJ parsing with basic triangle face support
4. Mesh data conversion to `VertexPositionNormalColor[]` and index buffers
5. GPU buffer allocation via `IResourceFactory`
6. Wireframe edge extraction via `EdgeExtractor.ExtractUniqueEdges()`

## Architecture and Documentation

For in-depth understanding of the codebase:

- **Architecture overview**: [`.planning/codebase/ARCHITECTURE.md`](.planning/codebase/ARCHITECTURE.md) -- layer descriptions, data flow diagrams, key abstractions, entry points
- **Codebase structure**: [`.planning/codebase/STRUCTURE.md`](.planning/codebase/STRUCTURE.md) -- directory layout, file locations, where to add new code
- **Coding conventions**: [`.planning/codebase/CONVENTIONS.md`](.planning/codebase/CONVENTIONS.md) -- naming, formatting, patterns, error handling
- **Testing patterns**: [`.planning/codebase/TESTING.md`](.planning/codebase/TESTING.md) -- test framework, coverage gaps, manual testing procedures
- **Known concerns**: [`.planning/codebase/CONCERNS.md`](.planning/codebase/CONCERNS.md) -- tech debt, known bugs, fragile areas, performance bottlenecks

**Dependency flow:**

```
Videra.Demo --> Videra.Avalonia --> Videra.Core
                                  Videra.Core --> Videra.Platform.Windows
                                  Videra.Core --> Videra.Platform.Linux
                                  Videra.Core --> Videra.Platform.macOS
```

## Troubleshooting

For common rendering issues, platform-specific problems, and fix workarounds, see [`docs/troubleshooting.md`](docs/troubleshooting.md).

**Common issues:**

| Issue | Platform | Suggested Fix |
|-------|----------|---------------|
| "Failed to create D3D11 device" | Windows | Update GPU drivers; ensure D3D11 feature level 11_0 support |
| "Failed to create Vulkan instance" | Linux | Install Vulkan drivers (`mesa-vulkan-drivers`); verify with `vulkaninfo` |
| "Failed to create Metal device" | macOS | Ensure Metal-compatible GPU; check System Profiler |
| Blank render output | All | Try `VIDERA_BACKEND=software` to rule out driver issues |
| Window not rendering | Linux | Verify X11 is running; Wayland is not currently supported |
| Model fails to load | All | Verify file is valid glTF 2.0, GLB, or Wavefront OBJ |

**Debug environment variables:**

| Variable | Purpose | Values |
|----------|---------|--------|
| `VIDERA_BACKEND` | Force a specific rendering backend | `software`, `d3d11`, `vulkan`, `metal`, `auto` |
| `VIDERA_FRAMELOG` | Enable per-frame render logging | `1`, `true` |
| `VIDERA_INPUTLOG` | Enable input event logging | `1`, `true` |

---

## Improving This Guide

This contributing guide is maintained alongside the codebase. If you spot gaps, inaccuracies, or have suggestions:

- Open an issue at the repository with the label `documentation`
- Submit a pull request with improvements directly to this file

Thank you for contributing to Videra!
