# Architecture

**Analysis Date:** 2025-03-28

## Pattern Overview

**Overall:** Layered architecture with platform abstraction backend pattern

**Key Characteristics:**
- Platform-agnostic core rendering logic in `Videra.Core`
- Platform-specific graphics backends implementing `IGraphicsBackend`
- AvaloniaUI integration layer with native window hosting
- Dependency inversion through abstract interfaces for all graphics operations
- Software rendering fallback when hardware acceleration unavailable

## Layers

**Application Layer:**
- Purpose: Example application demonstrating Videra usage
- Location: `samples/Videra.Demo/`
- Contains: MVVM ViewModels, Views, Services
- Depends on: `Videra.Avalonia`
- Used by: End users as reference

**UI Integration Layer:**
- Purpose: AvaloniaUI controls and native window hosting
- Location: `src/Videra.Avalonia/`
- Contains: `VideraView` control, platform-specific native hosts, input handling
- Depends on: `Videra.Core`, `Avalonia 11.x`
- Used by: Application layer

**Core Layer:**
- Purpose: Platform-independent 3D rendering engine
- Location: `src/Videra.Core/`
- Contains: `VideraEngine`, scene management, camera, abstract interfaces, software renderer
- Depends on: System.Numerics, SharpGLTF.Toolkit
- Used by: UI layer, platform backends

**Platform Layer:**
- Purpose: Native graphics API implementations
- Location: `src/Videra.Platform.{Windows/Linux/macOS}/`
- Contains: D3D11/Vulkan/Metal backends, buffers, pipelines, command executors
- Depends on: `Videra.Core`, Silk.NET (Windows/Linux), Objective-C Runtime (macOS)
- Used by: Core layer via `IGraphicsBackend`

## Data Flow

**Initialization Flow:**

1. `VideraView` attached to visual tree
2. Platform-specific native host created (Windows HWND, Linux X11 Window, macOS NSView)
3. `GraphicsBackendFactory` creates appropriate backend (D3D11/Vulkan/Metal/Software)
4. Backend initialized with native window handle
5. `VideraEngine` initialized with backend
6. Resources created (camera buffer, style buffer, pipelines)

**Render Loop (60 FPS):**

1. `VideraView` render timer triggers
2. `VideraEngine.Draw()` called
3. Backend `BeginFrame()` - clears buffers
4. Camera uniforms updated
5. `GridRenderer.Draw()` - renders grid if visible
6. `AxisRenderer.Draw()` - renders XYZ axes if enabled
7. For each `Object3D` in scene:
   - World matrix updated
   - `ICommandExecutor.DrawIndexed()` called
8. `WireframeRenderer.Draw()` - renders wireframe overlay if enabled
9. Backend `EndFrame()` - presents to screen

**Input Handling Flow:**

1. Mouse/pointer events captured by `VideraView.Input`
2. Events routed to `OrbitCamera`
3. Camera updates view/projection matrices
4. Next render frame uses updated camera state

**State Management:**
- `OrbitCamera` maintains position, target, yaw, pitch, distance
- Scene objects in `List<Object3D>` managed by engine
- Render style parameters managed by `IRenderStyleService`
- GPU buffers updated on each frame

## Key Abstractions

**IGraphicsBackend:**
- Purpose: Platform-agnostic graphics operations interface
- Examples: `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`
- Pattern: Interface segregation, single responsibility
- Implementations: `D3D11Backend`, `VulkanBackend`, `MetalBackend`, `SoftwareBackend`

**IResourceFactory:**
- Purpose: Create GPU resources (buffers, pipelines, shaders)
- Examples: `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`
- Pattern: Abstract factory
- Implementations: `D3D11ResourceFactory`, `VulkanResourceFactory`, `MetalResourceFactory`, `SoftwareResourceFactory`

**ICommandExecutor:**
- Purpose: Execute draw commands and state changes
- Examples: `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`
- Pattern: Command pattern
- Implementations: `D3D11CommandExecutor`, `VulkanCommandExecutor`, `MetalCommandExecutor`, `SoftwareCommandExecutor`

**IBuffer:**
- Purpose: GPU buffer abstraction (vertex, index, uniform)
- Examples: `src/Videra.Core/Graphics/Abstractions/IBuffer.cs`
- Pattern: Bridge pattern
- Implementations: Platform-specific buffer classes

## Entry Points

**VideraDemo Application:**
- Location: `samples/Videra.Demo/Program.cs`
- Triggers: Application launch
- Responsibilities: Avalonia app initialization, platform detection, composition mode configuration

**VideraView Control:**
- Location: `src/Videra.Avalonia/Controls/VideraView.cs`
- Triggers: Added to visual tree
- Responsibilities: Native host creation, backend initialization, render loop management, input handling

**VideraEngine:**
- Location: `src/Videra.Core/Graphics/VideraEngine.cs`
- Triggers: Initialized by VideraView
- Responsibilities: Scene management, render coordination, camera management, renderer coordination

**Backend Selection:**
- Location: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
- Triggers: VideraView initialization
- Responsibilities: Environment variable check, OS detection, backend instantiation, fallback to software

## Error Handling

**Strategy:** Graceful degradation with fallback

**Patterns:**
- Hardware backend initialization fails → Fallback to software rendering
- Invalid mesh data → Exception with clear error message, resource cleanup
- Window resize failures → Logged warning, render continues with previous size

## Cross-Cutting Concerns

**Logging:** Console-based logging with `[ComponentName]` prefix pattern

**Validation:**
- Buffer size validation before GPU allocation
- Null checks for backend/executor before render calls
- Input validation in `Object3D.Initialize()`

**Threading:**
- Render loop on UI thread via `DispatcherTimer`
- Lock on `VideraEngine._lock` during draw
- Native platform calls may be cross-platform (X11, Objective-C runtime)

**Memory Management:**
- `IDisposable` pattern for all GPU resources
- Explicit disposal in `VideraEngine.Dispose()`
- ComPtr usage for D3D11 reference counting

---

*Architecture analysis: 2025-03-28*
