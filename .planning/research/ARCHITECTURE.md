# Architecture Patterns

**Domain:** .NET 8 Cross-Platform 3D Rendering Engine Library
**Researched:** 2026-03-28
**Overall confidence:** HIGH

## Executive Summary

High-quality .NET libraries in 2025 follow a set of well-established architectural patterns that prioritize maintainability, testability, and cross-platform compatibility. For a 3D rendering engine like Videra, the key architectural principles include: clear separation between core abstractions and platform-specific implementations, comprehensive testing infrastructure, proper dependency injection patterns, and multi-targeting support for different platforms. The research reveals that the .NET ecosystem has converged on specific patterns for solution structure, testing organization, and package management that should guide Videra's open-source preparation.

## Recommended Architecture

### Solution Structure

**Standard .NET Library Pattern:**

```
Videra.sln
├── src/
│   ├── Videra.Core/              # Platform-agnostic core engine
│   ├── Videra.Avalonia/          # Avalonia UI integration
│   ├── Videra.Platform.Windows/  # Windows-specific backend
│   ├── Videra.Platform.macOS/    # macOS-specific backend
│   └── Videra.Platform.Linux/    # Linux-specific backend
├── tests/
│   ├── Videra.Core.Tests/                    # Core unit tests
│   ├── Videra.Core.IntegrationTests/         # Integration tests
│   ├── Videra.Avalonia.Tests/                # UI layer tests
│   ├── Videra.Platform.Windows.Tests/        # Windows backend tests
│   ├── Videra.Platform.macOS.Tests/          # macOS backend tests
│   └── Videra.Platform.Linux.Tests/          # Linux backend tests
├── samples/
│   └── Videra.Demo/              # Sample application
└── docs/                         # Documentation
```

**Key Principles:**
- **src/ and tests/ as solution folders** (not just directory folders) - de facto standard in .NET community
- **Clear separation of concerns** - each project has a single, well-defined responsibility
- **Test project naming** - follows `[ProjectName].Tests` or `[ProjectName].IntegrationTests` pattern
- **Platform-specific code isolated** - in separate platform projects with shared abstractions

This structure is supported by official Microsoft documentation and has become the de facto standard for scalable .NET projects. The separation enables clear navigation, supports parallel team development, and facilitates easy refactoring.

### Component Boundaries

| Component | Responsibility | Communicates With | Public API Surface |
|-----------|---------------|-------------------|-------------------|
| **Videra.Core** | Platform-agnostic 3D rendering engine, abstractions, software backend | All components (via abstractions) | IGraphicsBackend, IResourceFactory, VideraEngine, Object3D, OrbitCamera |
| **Videra.Platform.*** | Platform-specific graphics backend implementations | Core (via IGraphicsBackend interface) | PlatformBackendFactory (internal registration) |
| **Videra.Avalonia** | UI framework integration, native host controls | Core, Platform projects (via interop) | VideraView control, Avalonia-specific properties |
| **Test Projects** | Quality assurance, regression prevention | Core abstractions (can be mocked) | Test fixtures, test helpers |

**Dependency Flow:**
```
Videra.Avalonia → Videra.Core (abstractions)
Videra.Platform.* → Videra.Core (abstractions)
Test Projects → Videra.Core (public API)
```

**Critical Rule:** Platform projects must ONLY depend on Videra.Core abstractions. Core must NEVER reference platform projects directly. This enables:
- Runtime backend selection via factory pattern
- Testability (platform backends can be mocked)
- Clear architectural boundaries

### Data Flow

**Initialization Flow:**
```
1. User Code / Avalonia UI
   ↓
2. GraphicsBackendFactory.SelectBackend(platform)
   ↓
3. Platform-specific backend created (via reflection or direct instantiation)
   ↓
4. VideraEngine.Initialize(IGraphicsBackend)
   ↓
5. GPU resources created via IResourceFactory
```

**Render Frame Flow:**
```
1. UI Layer (VideraView.OnRenderTick)
   ↓
2. VideraEngine.Draw()
   ↓
3. Backend.BeginFrame() → Clear buffers
   ↓
4. Update uniforms (Camera, Style parameters)
   ↓
5. Render passes (Grid, Objects, Wireframe, Gizmo)
   ↓
6. Backend.EndFrame() → Present
   ↓
7. Software backend: CopyFrameTo() → Avalonia WriteableBitmap
```

**Style Update Flow:**
```
1. User changes property (VideraView.RenderStyle)
   ↓
2. RenderStyleService.ApplyPreset(preset)
   ↓
3. StyleChanged event fires
   ↓
4. Engine event handler updates GPU uniform buffer
```

## Patterns to Follow

### Pattern 1: Abstract Factory for Backend Selection

**What:** Factory pattern that creates platform-specific graphics backends without coupling core code to concrete implementations.

**When:** Use when you need to support multiple implementations of an interface that are selected at runtime or compile-time.

**Example:**
```csharp
// Core abstraction
public interface IGraphicsBackend
{
    void BeginFrame();
    void EndFrame();
    IResourceFactory CreateResourceFactory();
}

// Factory in Core
public static class GraphicsBackendFactory
{
    public static IGraphicsBackend CreateBackend(BackendType type)
    {
        return type switch
        {
            BackendType.Software => new SoftwareBackend(),
            BackendType.Direct3D11 => CreatePlatformBackend<Direct3D11Backend>(),
            BackendType.Metal => CreatePlatformBackend<MetalBackend>(),
            BackendType.Vulkan => CreatePlatformBackend<VulkanBackend>(),
            _ => throw new NotSupportedException()
        };
    }

    private static IGraphicsBackend CreatePlatformBackend<T>() where T : IGraphicsBackend
    {
        // Uses reflection to load platform assembly
        // Or direct instantiation if referenced
    }
}
```

**Benefits:**
- Core code never references platform assemblies directly
- Backends can be loaded dynamically based on runtime platform
- Test code can inject mock backends

### Pattern 2: Dependency Injection for Services

**What:** Use Microsoft.Extensions.DependencyInjection for service registration and resolution.

**When:** Use for application-level services (logging, configuration, style management) but NOT for per-object graphics resources.

**Example:**
```csharp
// Service registration (in Avalonia app or host)
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVideraCore(this IServiceCollection services)
    {
        services.AddSingleton<IRenderStyleService, RenderStyleService>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("Videra"));
        return services;
    }
}

// Service consumption
public class VideraEngine
{
    private readonly IRenderStyleService _styleService;
    private readonly ILogger _logger;

    public VideraEngine(IRenderStyleService styleService, ILogger logger)
    {
        _styleService = styleService;
        _logger = logger;
        _styleService.StyleChanged += OnStyleChanged;
    }
}
```

**Critical:** GPU resources (buffers, pipelines, shaders) should NOT be managed by DI container. They are per-object, short-lived resources that should be created/destroyed with their owning Object3D.

### Pattern 3: Layered Architecture with Clear Boundaries

**What:** Organize code into distinct layers with dependency rules enforced.

**When:** Use for any library with multiple subsystems to maintain maintainability.

**Layer Responsibilities:**

| Layer | Responsibility | Examples |
|-------|---------------|----------|
| **Domain Layer** | Core types, no external dependencies | Vector3, Matrix4x4, Vertex types, MeshData |
| **Abstraction Layer** | Interfaces for platform functionality | IGraphicsBackend, IResourceFactory, IBuffer |
| **Application Layer** | Engine orchestration, business logic | VideraEngine, Object3D, OrbitCamera |
| **Infrastructure Layer** | Platform implementations, external services | D3D11Backend, MetalBackend, VulkanBackend, SoftwareBackend |
| **Presentation Layer** | UI framework integration | VideraView, ViewModels, converters |

**Dependency Rule:** Dependencies flow inward only. Presentation → Application → Abstraction → Domain. Infrastructure depends on Abstractions.

### Pattern 4: Test Pyramid Organization

**What:** Organize tests by type and scope using separate projects.

**When:** Use for any non-trivial library to ensure quality.

**Test Project Structure:**

```
tests/
├── Videra.Core.UnitTests/           # Fast, isolated tests
│   ├── Geometry/                    # Vertex types, mesh data
│   ├── Cameras/                     # Orbit camera calculations
│   ├── Styles/                      # Style parameters, presets
│   └── Abstractions/                # Interface contracts
├── Videra.Core.IntegrationTests/    # Slower, real dependencies
│   ├── Rendering/                   # Full render pipeline
│   ├── ModelLoading/                # GLTF/OBJ import
│   └── BackendSelection/            # Factory pattern tests
└── Videra.Platform.*.Tests/         # Platform-specific tests
    ├── Windows/                     # D3D11 backend
    ├── macOS/                       # Metal backend
    └── Linux/                       # Vulkan backend
```

**Best Practices:**
- **Unit Tests:** Test public methods via public APIs. Never test private methods directly.
- **Naming:** `[MethodName]_[Scenario]_[ExpectedBehavior]`
- **Arrange-Act-Assert:** Clear separation in each test
- **No Logic in Tests:** Avoid conditionals, loops in test code
- **Minimal Assertions:** One logical assert per test (use parameterized tests for multiple cases)

**Example:**
```csharp
public class OrbitCameraTests
{
    [Fact]
    public void RotatePitch_PositiveAngle_LookAtVectorUpdatesCorrectly()
    {
        // Arrange
        var camera = new OrbitCamera();
        var initialPitch = camera.Pitch;

        // Act
        camera.RotatePitch(0.5f);

        // Assert
        Assert.Equal(initialPitch + 0.5f, camera.Pitch);
        Assert.True(camera.ViewMatrix.M43 > initialPitch); // Camera moved up
    }
}
```

### Pattern 5: Multi-Targeting for Platform Specifics

**What:** Use multi-targeting to include platform-specific code while sharing common code.

**When:** Use when you need platform-specific APIs but want to share most code.

**Example:**
```xml
<!-- Videra.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Multi-target for platforms with different APIs -->
    <TargetFrameworks>net8.0</TargetFrameworks>
    <!-- Or use runtime-specific code with #if directives -->
  </PropertyGroup>

  <!-- Platform-specific reference assemblies -->
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0-windows'">
    <PackageReference Include="Silk.NET.Direct3D11" Version="2.21.0" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Silk.NET.Core" Version="2.21.0" />
  </ItemGroup>
</Project>
```

**Alternative Approach (Current Videra Pattern):**
- Keep core as `net8.0` (cross-platform)
- Use separate platform projects instead of multi-targeting
- Better for: Large platform-specific implementations, different dependencies per platform

**Recommendation for Videra:** Continue with separate platform projects. The D3D11/Metal/Vulkan implementations are substantial enough to warrant their own projects.

## Anti-Patterns to Avoid

### Anti-Pattern 1: Concrete Dependencies in Core

**What:** Core project directly references platform projects or uses concrete platform classes.

**Why bad:** Breaks platform agnosticism, makes testing impossible, couples all platforms together.

**Instead:**
```csharp
// BAD:
public class VideraEngine
{
    private Direct3D11Backend _backend; // Concrete dependency!
}

// GOOD:
public class VideraEngine
{
    private IGraphicsBackend _backend; // Abstract dependency
}
```

### Anti-Pattern 2: God Object

**What:** VideraEngine or similar class knows too much, does too much.

**Why bad:** Hard to test, hard to maintain, violates Single Responsibility Principle.

**Instead:** Delegate responsibilities:
- `VideraEngine` → Scene management, render loop orchestration
- `RenderStyleService` → Style state, GPU uniform updates
- `CameraController` → Input handling, camera state
- `ResourceManager` → GPU resource lifecycle

### Anti-Pattern 3: Static State

**What:** Using static fields for engine state, backend instances, or configuration.

**Why bad:** Prevents multiple engine instances, complicates testing, creates hidden dependencies.

**Instead:**
```csharp
// BAD:
public static class GraphicsBackend
{
    public static IGraphicsBackend Current { get; set; }
}

// GOOD:
public class VideraEngine
{
    private readonly IGraphicsBackend _backend;
    public VideraEngine(IGraphicsBackend backend) { _backend = backend; }
}
```

### Anti-Pattern 4: Console.WriteLine for Logging

**What:** Using `Console.WriteLine` for debug output in a library.

**Why bad:** Library consumers can't control logging, output goes to wrong place, can't be disabled in release builds.

**Instead:** Use `ILogger` abstraction:
```csharp
public class VideraEngine
{
    private readonly ILogger _logger;

    public VideraEngine(ILogger logger)
    {
        _logger = logger;
    }

    public void Draw()
    {
        _logger.LogDebug("Starting render frame");
        // ...
        _logger.LogTrace("Rendered {ObjectCount} objects", _objects.Count);
    }
}
```

## Scalability Considerations

| Concern | At 10 users | At 1K users | At 100K users |
|---------|-------------|-------------|---------------|
| **Solution size** | Current structure is fine | Consider solution folders for organization | May need to split into multiple solutions |
| **Test execution time** | < 1 second | Parallel test execution needed | CI/CD with test sharding |
| **Platform backend selection** | Runtime factory | Runtime factory with caching | Runtime factory with lazy loading |
| **GPU resource management** | Per-object allocation | Simple object pooling | Advanced pooling + ring buffers |
| **Shader management** | Load on demand | Cache compiled shaders | Async compilation + background loading |
| **API surface stability** | Can change freely | Use [Obsolete] for breaking changes | Semantic versioning required |

**Key Insight:** Videra's architecture scales well. The main scaling challenges are:
1. **Test execution** - Use parallelization and selective test execution
2. **Resource management** - Implement pooling and caching as usage grows
3. **API stability** - Establish semantic versioning once public API stabilizes

## Build Configuration

### Debug vs Release

**Debug Configuration:**
- **Purpose:** Development and active debugging
- **Optimization:** Disabled (`<Optimize>false</Optimize>`)
- **Debug Symbols:** Full (`<DebugType>full</DebugType>`)
- **Assertions:** Debug assertions enabled
- **Checks:** Additional runtime checks (bounds checking, etc.)
- **Use When:** Developing features, debugging issues

**Release Configuration:**
- **Purpose:** Production deployment
- **Optimization:** Enabled (`<Optimize>true</Optimize>`)
- **Debug Symbols:** Portable (`<DebugType>portable</DebugType>` or `embedded`)
- **Assertions:** Disabled (release builds)
- **Checks:** Minimal runtime checks for performance
- **Use When:** Publishing NuGet packages, production deployments

**Recommended Configuration:**
```xml
<PropertyGroup Condition="'$(Configuration)'=='Debug'">
  <Optimize>false</Optimize>
  <DebugSymbols>true</DebugSymbols>
  <DebugType>full</DebugType>
  <DefineConstants>DEBUG;TRACE</DefineConstants>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)'=='Release'">
  <Optimize>true</Optimize>
  <DebugType>portable</DebugType>
  <DefineConstants>TRACE</DefineConstants>
  <!-- Enable for better stack traces in release -->
  <PublishAot>false</PublishAot>
</PropertyGroup>
```

### Symbol Packages

**Recommendation:** Publish symbol packages (`*.snupkg`) to NuGet.org

**Benefits:**
- Users can debug into Videra source code
- Better crash diagnostics with stack traces
- No bloat in main package (symbols on-demand)

**Configuration:**
```xml
<PropertyGroup>
  <!-- Generate symbol package automatically -->
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

## Package Structure

### NuGet Package Organization

**Recommended Approach:** Separate packages for each major component

```
Videra.Core                      # Core engine + software backend
Videra.Avalonia                  # Avalonia UI integration
Videra.Platform.Windows          # Windows D3D11 backend
Videra.Platform.macOS            # macOS Metal backend
Videra.Platform.Linux            # Linux Vulkan backend
```

**Alternative (Simpler):**
```
Videra                           # Everything in one package
```

**Recommendation for Videra:** Start with separate packages:
- **Videra.Core** - Core + Software backend (no native dependencies)
- **Videra.Avalonia** - Avalonia integration (depends on Videra.Core)
- **Videra.Platform.All** - All platform backends (depends on Videra.Core)

**Package Dependencies:**
```
Videra.Platform.All
  ├─> Videra.Core
Videra.Avalonia
  ├─> Videra.Core
  └─> Avalonia (11.3.9+)
```

**Why:** Enables users to install only what they need. Software-only users can skip platform packages. Avalonia users get the UI integration automatically.

## Platform-Specific Code Organization

### Recommended Pattern: Separate Platform Projects

**Why separate projects instead of multi-targeting:**
1. **Large implementations** - D3D11/Metal/Vulkan code is substantial (100s-1000s of lines)
2. **Different dependencies** - Each platform needs different NuGet packages
3. **Clean separation** - No risk of platform code leaking into other platforms
4. **Team development** - Platform experts can work independently

**Code Sharing Strategies:**

1. **Shared Abstractions** (in Core):
   ```csharp
   // Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs
   public interface IGraphicsBackend { /* ... */ }
   ```

2. **Platform Implementation** (in Platform projects):
   ```csharp
   // Videra.Platform.Windows/D3D11Backend.cs
   internal class D3D11Backend : IGraphicsBackend { /* ... */ }
   ```

3. **Factory Registration** (in Core or separate):
   ```csharp
   // Videra.Core/Graphics/GraphicsBackendFactory.cs
   public static class GraphicsBackendFactory
   {
       public static IGraphicsBackend CreateForPlatform()
       {
           if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
               return LoadPlatformBackend("Videra.Platform.Windows");
           // ...
       }
   }
   ```

## Testing Infrastructure

### Test Project Setup

**Required NuGet Packages:**

```xml
<!-- Unit Tests -->
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />

<!-- Mocking -->
<PackageReference Include="Moq" Version="4.20.72" />

<!-- Assertions -->
<PackageReference Include="FluentAssertions" Version="7.0.0" />

<!-- Code Coverage -->
<PackageReference Include="Coverlet.collector" Version="6.0.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

**Test Runner Configuration:**

```xml
<!-- coverlet.runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>opencover</Format>
          <Exclude>[Videra.*.Tests]*</Exclude>
          <Exclude>[Videra.*]*.Generated.*</Exclude>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

### Test Organization Best Practices

**Folder Structure:**
```
Videra.Core.Tests/
├── Geometry/
│   ├── VertexTests.cs
│   ├── MeshDataTests.cs
│   └── EdgeExtractorTests.cs
├── Cameras/
│   └── OrbitCameraTests.cs
├── Styles/
│   ├── RenderStyleServiceTests.cs
│   └── StyleParametersTests.cs
├── Graphics/
│   ├── SoftwareBackendTests.cs
│   └── ResourceFactoryTests.cs
└── Helpers/
    └── TestDataBuilder.cs
```

**Test Helper Pattern:**
```csharp
// Helpers/TestDataBuilder.cs
public static class TestDataBuilder
{
    public static MeshData CreateSimpleTriangle()
    {
        return new MeshData
        {
            Vertices = new[] {
                new VertexPositionNormalColor(new Vector3(0, 0, 0), Vector3.UnitZ, Color.Red),
                new VertexPositionNormalColor(new Vector3(1, 0, 0), Vector3.UnitZ, Color.Green),
                new VertexPositionNormalColor(new Vector3(0, 1, 0), Vector3.UnitZ, Color.Blue)
            },
            Indices = new[] { 0, 1, 2 }
        };
    }

    public static IGraphicsBackend CreateMockBackend()
    {
        var mock = new Mock<IGraphicsBackend>();
        // Setup common behaviors
        mock.Setup(b => b.BeginFrame()).Verifiable();
        mock.Setup(b => b.EndFrame()).Verifiable();
        return mock.Object;
    }
}
```

## Dependency Injection Patterns

### Service Registration

**Microsoft.Extensions.DependencyInjection Integration:**

```csharp
// Videra.Core/ServiceCollectionExtensions.cs
public static class VideraServiceExtensions
{
    public static IServiceCollection AddVideraCore(
        this IServiceCollection services,
        Action<VideraOptions> configure = null)
    {
        // Configure options
        var options = new VideraOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Register core services
        services.AddSingleton<IRenderStyleService, RenderStyleService>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("Videra"));

        return services;
    }
}

// Usage in Avalonia app
public static class AvaloniaConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddVideraCore(options =>
        {
            options.DefaultBackend = BackendType.Software;
            options.EnableValidation = true;
        });
    }
}
```

### Scoped vs Singleton Services

**Guidelines for Videra:**

| Service Type | Lifetime | Reason |
|--------------|----------|--------|
| `ILogger` | Singleton | Thread-safe, shared across application |
| `IRenderStyleService` | Singleton | Global style state, event-driven |
| `VideraEngine` | Scoped/Transient | May have multiple engine instances |
| GPU Resources | Not in DI | Short-lived, per-object lifecycle |

**Critical:** NEVER register GPU resources (buffers, pipelines, shaders) in DI container. They are:
- Too numerous (thousands in a scene)
- Short-lived (created/destroyed with objects)
- Not thread-safe (must be used on graphics thread)

## Implementation Order for Roadmap

Based on architectural dependencies, recommended component build order:

### Phase 1: Foundation (No dependencies)
1. **Domain Types** - Geometry types (Vector3, Matrix4x4 wrappers, Vertex types)
2. **Abstractions** - Core interfaces (IGraphicsBackend, IResourceFactory)
3. **Logging Infrastructure** - ILogger abstraction, basic console implementation

### Phase 2: Core Engine (Depends on Phase 1)
4. **Camera System** - OrbitCamera, camera controllers
5. **Style Service** - RenderStyleService, parameters, presets
6. **Engine Core** - VideraEngine (without platform backends)

### Phase 3: Testing Infrastructure (Depends on Phase 1-2)
7. **Test Framework** - Setup test projects, helpers, mocks
8. **Unit Tests** - Core type tests, camera tests, style tests

### Phase 4: Platform Backends (Depends on Phase 1-2)
9. **Software Backend** - CPU rasterization (no native dependencies)
10. **Platform Backends** - D3D11, Metal, Vulkan implementations

### Phase 5: Integration & UI (Depends on Phase 1-4)
11. **Avalonia Integration** - VideraView control, native hosts
12. **Model Loading** - GLTF/OBJ importers
13. **Integration Tests** - Full render pipeline tests

### Phase 6: Quality & Polish (Depends on Phase 1-5)
14. **Documentation** - XML comments, API docs, user guide
15. **Performance** - Profiling, optimization, pooling
16. **Validation** - Cross-platform testing, edge case handling

**Critical Path:** Phase 1 → Phase 2 → Phase 4 → Phase 5
**Can Parallelize:** Phase 3 (with Phase 2), Phase 6 sub-tasks

## Architecture Validation Checklist

Use this checklist to validate architectural decisions:

- [ ] **Dependency Rule:** Do all dependencies flow toward abstractions?
- [ ] **Testability:** Can core functionality be tested without platform backends?
- [ ] **Separation of Concerns:** Does each component have a single responsibility?
- [ ] **Interface Segregation:** Are interfaces focused and not overly broad?
- [ ] **No Static State:** Can multiple engine instances coexist?
- [ ] **DI Container Boundaries:** Are GPU resources excluded from DI?
- [ ] **Platform Isolation:** Does core code never reference platform assemblies?
- [ ] **Logging Abstraction:** Is all logging through ILogger (no Console.WriteLine)?
- [ ] **Test Organization:** Are tests separated by type (unit/integration/platform)?

## Sources

- [Cross-platform targeting for .NET libraries - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting) (HIGH confidence - Official Microsoft documentation)
- [NuGet and .NET libraries - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget) (HIGH confidence - Official Microsoft documentation)
- [Best practices for writing unit tests - .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) (HIGH confidence - Official Microsoft documentation)
- [Multi-targeting for NuGet Packages - Microsoft Learn](https://learn.microsoft.com/ar-sa/nuget/create-packages/multiple-target-frameworks-project-file) (MEDIUM confidence - Official Microsoft documentation)
- [Microsoft.Extensions.DependencyInjection Namespace](https://learn.microsoft.com/ca-es/dotnet/api/microsoft.extensions.dependencyinjection) (HIGH confidence - Official API documentation)
- [Next-Level Clean Architecture Boilerplate - Microsoft Dev Blog](https://devblogs.microsoft.com/ise/next-level-clean-architecture-boilerplate/) (HIGH confidence - Official Microsoft blog)
- [Clean Architecture .NET discussion - Reddit](https://www.reddit.com/r/dotnet/comments/1iqsl9x/best_practices_for_structuring_test_projects_and/) (LOW confidence - Community discussion, needs validation)
- [Ardalis CleanArchitecture Template](https://github.com/ardalis/cleanarchitecture) (MEDIUM confidence - Well-regarded community template)

---

*Architecture research: 2026-03-28*
