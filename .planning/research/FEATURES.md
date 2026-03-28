# Feature Landscape

**Domain:** .NET Cross-Platform 3D Rendering Engine (Open Source)
**Researched:** 2026-03-28
**Overall Confidence:** HIGH

Based on analysis of official .NET documentation ([Microsoft Learn: Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)), established .NET open source projects ([dotnet/runtime](https://github.com/dotnet/runtime), [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore)), and Videra's current state documented in `.planning/codebase/CONCERNS.md` and `.planning/codebase/TESTING.md`.

---

## Table Stakes

Features users expect in a high-quality .NET open source project. Missing these makes the project feel incomplete or unmaintained.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| **Unit Tests** | Zero test coverage identified in TESTING.md; prevents safe refactoring | Medium | xUnit is de facto standard for .NET; start with core abstractions (IBuffer, IPipeline, ICommandExecutor) |
| **Integration Tests** | Cross-platform rendering code needs validation on actual platforms | High | Platform-specific tests using conditional compilation or test inheritance |
| **Test Runner Integration** | CI/CD workflow has no test execution step (TESTING.md line 101) | Low | Add `dotnet test` to GitHub Actions before packaging |
| **XML API Documentation** | Required for NuGet package documentation generation | Low | `///` comments on all public APIs; enables Intellisense |
| **Structured Logging** | CONCERNS.md identifies extensive Console.WriteLine debug code in production | Medium | Use `Microsoft.Extensions.Logging` with ILogger<T> pattern; remove all Console.WriteLine |
| **Domain-Specific Exceptions** | CONCERNS.md notes generic Exception throws with HRESULT/Vulkan codes scattered | Medium | Create GraphicsInitializationException, ShaderCompilationException, ResourceCreationException |
| **README with Quick Start** | Current README is comprehensive (architectural diagrams, usage examples) | Low | Maintain existing structure; ensure accuracy after refactoring |
| **License File** | MIT license displayed in README badge but LICENSE file should exist | Low | Add LICENSE file at repo root for open source compliance |
| **Contributing Guidelines** | Lowers contribution barrier; clarifies development workflow | Low | Document build/test steps, coding standards, PR process |
| **Code Coverage Reporting** | PROJECT.md targets 80%+ coverage; needs visibility | Medium | CollectCoverage dotnet test parameter; Coverlet or similar |
| **Resource Cleanup Verification** | CONCERNS.md identifies RAII pattern gaps and partial initialization rollback | High | Tests for disposal patterns; memory leak detection |
| **Cross-Platform Build Validation** | PROJECT.md BUILD-01 requires Windows/Linux/macOS compilation tests | Medium | Matrix builds in CI or local verification before releases |

---

## Differentiators

Features that set Videra apart from other .NET 3D rendering projects. Not expected, but highly valued.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| **Software Rendering Fallback** | CPU rasterizer ensures functionality on systems without GPU acceleration | High | Already implemented (SoftwareBackend.cs); single-threaded - could add parallelization |
| **Wireframe Rendering System** | Multiple wireframe modes (AllEdges, VisibleOnly, Overlay, WireframeOnly) not commonly available | Medium | Already implemented; edge extraction algorithm needs test coverage |
| **Render Style Presets** | Event-driven style system (Realistic, Tech, Cartoon, X-Ray, Clay, Wireframe) provides immediate visual variety | Low-Medium | Already implemented; unique value for demo/visualization apps |
| **Avalonia UI Integration** | WPF-like control wrapper enables desktop app development across platforms | Medium | Already implemented; native window host handling needs tests |
| **Three Platform Parity** | D3D11 (Windows), Vulkan (Linux), Metal (macOS) with equal priority is rare | High | Most projects prioritize one platform; CONCERNS.md notes each backend has unique fragile areas |
| **Model Format Support** | glTF/GLB via SharpGLTF + basic OBJ loader | Low-Medium | glTF is modern standard; OBJ loader limited (no texture coords per CONCERNS.md) |
| **Developer-Friendly Debugging** | Environment variables for backend selection and frame logging (VIDERA_BACKEND, VIDERA_FRAMELOG) | Low | Already implemented; useful for cross-platform development |

---

## Anti-Features

Features to explicitly NOT build.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| **GUI Property Editors** | Videra is a rendering engine, not a full editor; adds maintenance burden | Keep Avalonia demo as reference implementation; document MVVM pattern |
| **Asset Pipeline Tools** | Out of scope for rendering engine; existing tools (Blender, glTF utilities) are mature | Focus on runtime loading; document supported formats and limitations |
| **Physics Simulation** | Separate concern; would require major new subsystem | Integrate with existing physics engines (bevy, Jolt) if users request |
| **Networked Multiplayer** | Outside scope of rendering engine | Document how to synchronize transform state for users' networking layers |
| **VR/AR Support** | Requires platform-specific SDKs (OpenXR, Oculus, etc.); diverges from core mission | Keep API extensible; allow community VR implementations as separate packages |
| **Mobile Platforms (iOS/Android)** | PROJECT.md constrains to Windows/Linux/macOS; mobile requires different UI framework | Extend when Avalonia mobile support matures and use case justified |
| **Backward Compatibility Shims** | PROJECT.md explicitly allows breaking existing API for refactoring | Clean break now; establish stable API after cleanup complete |
| **Custom Shader Language** | Three platforms already have inline HLSL/GLSL/Metal; adding fourth language multiplies complexity | Use platform-native shaders; consider SPIRV-Cross for cross-compilation later |
| **Animation System** | Current focus is static model rendering; animation adds significant complexity | Support basic transform animation; defer skeletal/vertex animation to future |
| **Level of Detail (LOD)** | CONCERNS.md notes this as out of scope; requires mesh simplification | Document manual LOD approach (multiple Object3D instances) |

---

## Feature Dependencies

```
Unit Tests → Safe Refactoring → Domain Exceptions
Unit Tests → Resource Cleanup Verification
Structured Logging → Performance Profiling → Bottleneck Identification
Code Coverage Reporting → 80% Coverage Target (PROJECT.md)
Integration Tests → Cross-Platform Validation → Platform Parity
XML API Documentation → NuGet Package Generation
Contributing Guidelines → Lower Barrier to Entry → Community Contributions
Test Runner Integration → CI/CD Quality Gate → Release Confidence
```

**Critical path:** Unit Tests must be established before major refactoring (domain exceptions, logging cleanup) can proceed safely.

---

## MVP Recommendation

For open source readiness (PROJECT.md Active requirements), prioritize in this order:

### Immediate (Phase 1 - Foundation)
1. **Unit Test Infrastructure** - xUnit project for Videra.Core; test discovery and execution working
2. **Test Runner Integration** - Add `dotnet test` to CI workflow
3. **Structured Logging** - Replace Console.WriteLine with ILogger<T>; remove debug counters
4. **XML API Documentation** - Document public APIs; enable NuGet doc generation

### Short-term (Phase 2 - Quality)
1. **Domain-Specific Exceptions** - Replace generic Exception throws
2. **Code Coverage Reporting** - Achieve and report 80%+ target
3. **Contributing Guidelines** - Document workflow; set expectations
4. **Resource Cleanup Verification** - Test disposal patterns and RAII

### Medium-term (Phase 3 - Robustness)
1. **Integration Tests** - Platform-specific validation; model loading with test assets
2. **Cross-Platform Build Validation** - Automated matrix builds or verified local process
3. **License File** - Add for open source compliance

### Defer
- **Performance Benchmarking** - After CONCERNS.md performance bottlenecks addressed; use BenchmarkDotNet
- **Visual Regression Tests** - High complexity; manual testing via Demo app acceptable initially
- **Shader Cross-Compilation** - Current approach works; SPIRV-Cross evaluation when maintenance burden justified

---

## Testing Organization Pattern

Based on .NET community standards observed in dotnet/runtime and dotnet/aspnetcore:

### Project Structure
```
test/
├── Videra.Core.Tests/
│   ├── Abstractions/              # IBuffer, IPipeline, etc.
│   ├── Cameras/                   # OrbitCamera tests
│   ├── Geometry/                  # Vertex structure tests
│   ├── Graphics/                  # VideraEngine, Object3D tests
│   │   ├── Wireframe/            # Edge extraction tests
│   │   └── Software/             # Software rasterizer tests
│   └── IO/                        # ModelImporter tests
├── Videra.Platform.Windows.Tests/  # D3D11-specific tests
├── Videra.Platform.Linux.Tests/    # Vulkan-specific tests
├── Videra.Platform.macOS.Tests/    # Metal-specific tests
└── Videra.Avalonia.Tests/          # UI integration tests
```

### Test Categories
- **Unit**: Fast, isolated, no native resources (Core abstractions, math, serialization)
- **Integration**: Platform backend initialization, resource creation/disposal
- **E2E**: Full render pipeline with test scenes (requires GPU)

### Cross-Platform Strategy
Use shared test project with conditional compilation:
```csharp
#if WINDOWS
using Videra.Platform.Windows;
#elif LINUX
using Videra.Platform.Linux;
#elif MACOS
using Videra.Platform.macOS;
#endif
```

Or use base test class with platform-specific derived classes.

---

## Documentation Structure Pattern

Based on Microsoft Learn documentation model:

### API Documentation
- XML comments on all public types and members
- `<summary>`, `<param>`, `<returns>`, `<exception>`, `<example>` tags
- Generate with `docfx` or `sharpf` for website

### User Documentation (Maintain Existing)
- **README.md**: Project overview, quick start, architecture (already comprehensive)
- **CONTRIBUTING.md**: Setup, build, test, PR guidelines
- **TROUBLESHOOTING.md**: Common issues per platform (backed by CONCERNS.md content)

### Contributor Documentation
- **ARCHITECTURE.md**: System architecture, rendering pipeline, platform considerations
- **PLATFORM_NOTES.md**: Per-platform backend details (D3D11/Vulkan/Metal specifics)
- **STYLE_GUIDE.md**: Coding standards, naming conventions, when to use unsafe

---

## Error Handling Pattern

Based on CONCERNS.md findings and Microsoft patterns:

### Exception Hierarchy
```csharp
public class VideraException : Exception
{
    public ErrorCategory Category { get; }
    public DiagnosticInfo Diagnostics { get; }
}

public class GraphicsInitializationException : VideraException { }
public class ShaderCompilationException : VideraException { }
public class ResourceCreationException : VideraException { }
public class PlatformNotSupportedException : VideraException { }
```

### Diagnostic Information
- Include HRESULT for D3D11 errors
- Include VkResult for Vulkan errors
- Include Metal error codes when available
- Include current backend, platform, GPU info

### Logging Integration
- Log at Error level when throwing exceptions
- Include structured parameters (backend, resource type, operation)
- Use event IDs for categorization (Microsoft.Extensions.Logging pattern)

---

## Logging Practice Pattern

Based on [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) official documentation:

### Logger Injection
```csharp
private readonly ILogger<ClassName> _logger;
public ClassName(ILogger<ClassName> logger)
{
    _logger = logger;
}
```

### Log Levels
- **Trace**: Detailed per-frame data (disabled by default, production off)
- **Debug**: Development diagnostics (use sparingly in production)
- **Information**: Significant lifecycle events (init, dispose, model load)
- **Warning**: Recoverable issues (fallback to software renderer, missing features)
- **Error**: Operation failures (resource creation, shader compile)
- **Critical**: Data loss, complete failure (GPU crash, invalid state)

### Structured Logging
```csharp
// Instead of:
Console.WriteLine($"Creating buffer of size {size} for {objectName}");

// Use:
_logger.LogDebug(
    EventIds.BufferCreation,
    "Creating buffer of size {BufferSize} for {ObjectName}",
    size, objectName
);
```

### Configuration
- Support `appsettings.json` for log level configuration
- Environment variable override (`Logging__LogLevel__Default`)
- Remove all `Console.WriteLine` from hot paths (CONCERNS.md identifies this as performance bottleneck)

---

## Cross-Platform Support Pattern

Based on current Videra architecture and CONCERNS.md platform-specific issues:

### Backend Selection
- Maintain existing `GraphicsBackendFactory` runtime detection
- Add environment variable `VIDERA_BACKEND` override (already exists)
- Graceful degradation: Hardware → Software fallback

### Platform-Specific Handling
| Platform | Current Status | Known Issues (CONCERNS.md) | Strategy |
|----------|---------------|---------------------------|----------|
| **Windows (D3D11)** | Functional | Generic exceptions, no resource cleanup verification | Add tests, improve error messages |
| **Linux (Vulkan)** | Functional | X11-only, no Wayland, hardcoded library path | Add dlopen, Wayland detection, fallback to software |
| **macOS (Metal)** | Functional | Fragile Objective-C runtime interop, manual selectors | Migrate to Silk.NET.Metal typed bindings if available |

### Conditional Compilation
```csharp
#if NET8_0_WINDOWS
    // Windows-specific code
#elif NET8_0_LINUX
    // Linux-specific code
#elif NET8_0_MACOS
    // macOS-specific code
#else
    // Fallback or throw
#endif
```

### Testing Strategy
- Unit tests run on all platforms (test Core abstractions)
- Integration tests run on native platform only
- CI: matrix builds or platform-specific workflows

---

## Performance Benchmarking Pattern

Based on BenchmarkDotNet usage in .NET ecosystem:

### Benchmark Categories
- **Micro-benchmarks**: Individual operations (matrix math, buffer updates)
- **Component benchmarks**: Renderer pipeline stages (draw call overhead)
- **Scenario benchmarks**: Full frame render with N objects

### Benchmark Project Structure
```
benchmark/
└── Videra.Benchmarks/
    ├── Math/
    │   └── MatrixOperationsBench.cs
    ├── Graphics/
    │   ├── BufferUpdateBench.cs
    │   └── DrawCallBench.cs
    └── Scenarios/
        └── FrameRenderBench.cs
```

### BenchmarkDotNet Configuration
```csharp
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class BufferUpdateBench
{
    [Benchmark]
    public void UpdateUniforms() { /* ... */ }
}
```

### Integration
- Run benchmarks separately from unit tests (dotnet run -c Release)
- Store baseline results; alert on regressions
- Profile before/after optimization (CONCERNS.md lists hot paths)

---

## Examples and Tutorials Pattern

Based on current Demo app structure:

### Maintain Existing
- **Videra.Demo**: Comprehensive interactive viewer (keep as showcase)

### Add
- **Minimal Example**: 50-line "hello triangle" for quick start
- **Model Loading Example**: glTF/OBJ import workflow
- **Custom Style Example**: Creating render presets
- **Avalonia Integration Example**: MVVM pattern with VideraView

### Tutorial Structure
1. **Quick Start**: Get a triangle on screen in 10 minutes
2. **Loading Models**: Import and display glTF files
3. **Camera Controls**: Implement orbit, pan, zoom
4. **Render Styles**: Use and customize presets
5. **Wireframe Modes**: Understand edge extraction and rendering
6. **Platform Backends**: Choose and debug specific backends

---

## Sources

| Area | Sources | Confidence |
|------|---------|------------|
| .NET Logging | [Microsoft Learn: Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) (official documentation) | HIGH |
| Project Structure | [dotnet/runtime GitHub](https://github.com/dotnet/runtime) (reference .NET project) | HIGH |
| Project Structure | [dotnet/aspnetcore GitHub](https://github.com/dotnet/aspnetcore) (reference .NET project) | HIGH |
| Error Handling | Medium article: "Modern C# Error Handling Patterns You Should Be Using in 2026" (community patterns) | MEDIUM |
| Error Handling | "Best Practices for Error Handling in .NET Applications 2026" (community guide) | MEDIUM |
| Current State | `.planning/codebase/CONCERNS.md` (internal analysis, 2026-03-28) | HIGH |
| Current State | `.planning/codebase/TESTING.md` (internal analysis, 2026-03-28) | HIGH |
| Current State | `.planning/PROJECT.md` (project requirements, 2026-03-28) | HIGH |
| Benchmarking | YouTube: "How to benchmark a .NET application using BenchmarkDotNet" (community resource) | MEDIUM |

**Confidence Notes:**
- Official Microsoft documentation is authoritative for logging patterns
- dotnet/runtime and dotnet/aspnetcore are canonical examples of .NET open source project structure
- Error handling patterns from community articles are MEDIUM confidence as they represent contemporary practices but not official guidance
- Internal project documents (CONCERNS.md, TESTING.md, PROJECT.md) are HIGH confidence as they directly reflect current state
