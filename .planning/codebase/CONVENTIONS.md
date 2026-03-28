# Coding Conventions

**Analysis Date:** 2026-03-28

## Naming Patterns

**Namespaces:**
- PascalCase, hierarchical by project structure: `Videra.Core.Graphics.Abstractions`, `Videra.Platform.Windows`, `Videra.Demo.ViewModels`
- Core namespace: `Videra.Core` for platform-agnostic code
- Platform namespaces: `Videra.Platform.{Windows|Linux|macOS}`

**Classes:**
- PascalCase: `OrbitCamera`, `RenderStyleService`, `D3D11Backend`
- Interfaces: Prefix `I`, PascalCase: `IGraphicsBackend`, `IResourceFactory`, `ICommandExecutor`
- Abstract/Sealed: Use `sealed` modifier for non-inheritable classes: `public sealed class RenderStyleService`

**Methods:**
- PascalCase: `InitializeWireframe`, `UpdateProjection`, `ExtractUniqueEdges`
- Async methods: `Async` suffix: `ImportModelsAsync`, `LoadFromFileAsync`

**Properties and Fields:**
- Public properties: PascalCase: `IsInitialized`, `FieldOfView`, `ViewMatrix`
- Private fields: `_camelCase`: `_radius`, `_width`, `_clearColor`
- Constants/static readonly: PascalCase: `Red`, `Vector3.UnitY`

**Structs:**
- PascalCase: `RgbaFloat`, `VertexPositionNormalColor`, `Edge`
- Implement `IEquatable<T>` for value types

**Enums:**
- PascalCase: `WireframeMode`, `RenderStylePreset`, `PrimitiveTopology`
- Flags enums for bitmasks: `[Flags] ShaderStage`

## Code Style

**Formatting:**
- Implicit usings enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Unsafe blocks allowed in platform projects (`<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`)
- Target framework: .NET 8.0
- No .editorconfig detected - using C# defaults

**Linting:**
- No explicit linter configuration found
- Implicit C# compiler warnings and nullable reference type warnings

## Import Organization

**Order (observed in source):**
1. System namespaces (`System`, `System.Numerics`, `System.IO`)
2. Third-party libraries (`Silk.NET.*`, `Avalonia.*`, `CommunityToolkit.Mvvm`)
3. Videra namespaces (`Videra.Core.*`, `Videra.Platform.*`)

**Global usings:**
- Generated `GlobalUsings.g.cs` files in obj/ directories
- Core global imports likely defined but not in source

**Path aliases:**
- No path aliasing detected

## Error Handling

**Patterns:**
- Exception throwing with descriptive messages: `throw new Exception("Failed to create Metal device")`
- Argument validation: `throw new ArgumentException("Invalid mesh data")`
- Console logging for errors: `Console.WriteLine($"[Object3D '{Name}'] ✗ Initialization failed: {ex.Message}")`

**Resource cleanup:**
- `IDisposable` implementation for GPU resources
- Dispose pattern with cleanup of owned resources: `VertexBuffer?.Dispose()`
- Cleanup on initialization failure in try/catch blocks

**Platform-specific fallback:**
- Backend loading failures fall back to software renderer
- Try/catch around native library loading with console logging

## Logging

**Framework:** Console.WriteLine (no structured logging framework)

**Patterns:**
- Prefix tags for identification: `[ModelImporter]`, `[Metal]`, `[Object3D 'Name']`
- Success indicators: `✓ Success`, `✗ Failed`
- Detailed info: buffer sizes, vertex counts, initialization steps
- Debug output: `System.Diagnostics.Debug.WriteLine` for platform-specific setup

**When to log:**
- Initialization/lifecycle events
- Resource creation with sizes
- Failures with exception details
- Platform-specific operations (Metal, D3D11 setup)

## Comments

**When to Comment:**
- XML documentation on public APIs: `/// <summary>跨平台图形后端抽象接口</summary>`
- Inline comments for complex math (projection matrix construction)
- Section dividers in large classes: `// ==========================================`
- Chinese comments used alongside English for documentation

**JSDoc/TSDoc:**
- C# XML doc comments (`/// <summary>`)
- Parameter descriptions in XML tags
- Return value documentation

## Function Design

**Size:**
- Functions typically 10-100 lines
- Platform initialization can be longer (100+ lines) with clear sections
- Single-responsibility methods: `CreateDeviceAndSwapchain`, `CreateDepthStencil`

**Parameters:**
- Primitive types and simple structs preferred
- Complex data passed as dedicated types: `MeshData`, `PipelineDescription`
- `IntPtr` for native handles
- Optional parameters with defaults: `float minDepth = 0f`

**Return Values:**
- `void` for methods that mutate state
- Value types for data: `Matrix4x4`, `RenderStyleParameters`
- Tuples for multiple returns: `(RenderStyleParameters Parameters, RenderStylePreset Preset)`
- Factory methods return constructed objects: `CreateVertexBuffer`, `GetResourceFactory`

## Module Design

**Exports:**
- Public classes exported from each project
- Internal classes for implementation details: `internal sealed class SoftwareBackend`
- Platform-specific implementations are `internal` to their projects

**Barrel Files:**
- Not used - namespace organization through folder structure
- Each folder typically has its own namespace

**Dependency flow:**
- `Videra.Core` - no platform dependencies
- `Videra.Platform.*` - reference `Videra.Core`
- `Videra.Avalonia` - references `Videra.Core` and platform projects conditionally
- `Videra.Demo` - references `Videra.Avalonia`

## Special Conventions

**Unsafe code:**
- Used for native interop in platform backends
- `fixed` pointers for D3D11
- `IntPtr` for Metal Objective-C runtime calls
- Silk.NET requires unsafe contexts

**MVVM pattern (Avalonia):**
- ViewModels inherit `ObservableObject` (CommunityToolkit.Mvvm)
- `[ObservableProperty]` generates properties
- `[RelayCommand]` generates commands
- `partial void OnPropertyChanged()` for property change hooks

**Factory pattern:**
- `IResourceFactory` for creating GPU resources
- `GraphicsBackendFactory` for backend instantiation
- Static factory methods: `RenderStylePresets.CreateRealistic()`

**Service pattern:**
- Interface-based services: `IRenderStyleService`, `IModelImporter`
- Dependency injection in Avalonia app
- `sealed` class implementations

---

*Convention analysis: 2026-03-28*
