# Architecture

**Analysis Date:** 2026-03-28

## Pattern Overview

**Overall:** Multi-layered 3D rendering engine with platform-specific graphics backends

**Key Characteristics:**
- Cross-platform graphics abstraction layer supporting native and software rendering
- Modular subsystem separation (graphics, camera, geometry, styles, I/O)
- Event-driven rendering style system with preset management
- Orbit camera controller with projection/view matrix management
- Wireframe rendering overlay system with multiple display modes

## Layers

**Core Graphics Layer:**
- Purpose: Central rendering engine coordination and main draw loop
- Location: `src/Videra.Core/Graphics/`
- Contains: `VideraEngine.cs`, `Object3D.cs`, renderer components
- Depends on: Platform-specific backends (via abstraction), geometry types, camera
- Used by: Avalonia UI layer (`VideraView`), demo application

**Graphics Abstraction Layer:**
- Purpose: Interface definitions for cross-platform rendering backends
- Location: `src/Videra.Core/Graphics/Abstractions/`
- Contains: `IGraphicsBackend.cs`, `IResourceFactory.cs`, `IPipeline.cs`, `ICommandExecutor.cs`, `IBuffer.cs`
- Depends on: System.Numerics types
- Used by: Engine, software backend, platform backends

**Backend Implementation Layer:**
- Purpose: Concrete graphics backends (software and platform-specific)
- Location: `src/Videra.Core/Graphics/Software/` and `src/Videra.Platform.*/`
- Contains: `SoftwareBackend.cs`, platform-specific D3D11/Metal/Vulkan implementations
- Depends on: Abstraction interfaces
- Used by: Factory pattern selection based on platform/preference

**Camera & Geometry Layer:**
- Purpose: 3D math and camera control
- Location: `src/Videra.Core/Cameras/`, `src/Videra.Core/Geometry/`
- Contains: `OrbitCamera.cs`, `VertexPositionNormalColor.cs`, `MeshData.cs`
- Depends on: System.Numerics for matrix/vector operations
- Used by: Engine, renderers

**Style & Parameters Layer:**
- Purpose: Render style configuration with presets and live updates
- Location: `src/Videra.Core/Styles/`
- Contains: `RenderStyleService.cs`, parameter types, presets, JSON serialization
- Depends on: Engine (for GPU uniform updates via events)
- Used by: UI controls, engine

**Avalonia Integration Layer:**
- Purpose: Bridge between Avalonia UI and 3D engine
- Location: `src/Videra.Avalonia/Controls/`
- Contains: `VideraView.cs`, platform-specific native hosts
- Depends on: Core engine, platform interop
- Used by: Demo application and end-user UIs

**I/O Layer:**
- Purpose: 3D model loading
- Location: `src/Videra.Core/IO/`
- Contains: `ModelImporter.cs` with SharpGLTF and simple OBJ parsers
- Depends on: Geometry types, graphics resource factory
- Used by: Demo application

## Data Flow

**Model Loading:**

1. User selects model file (GLTF/GLB/OBJ) via `ModelImporter.Load()`
2. SharpGLTF or internal parser processes file into `MeshData` (vertices, indices, normals, colors)
3. `Object3D.Initialize()` creates GPU buffers via `IResourceFactory`
4. Wireframe edges are extracted via `EdgeExtractor.ExtractUniqueEdges()` and stored separately
5. Object added to engine scene via `VideraEngine.AddObject()`

**Render Frame:**

1. UI layer triggers `VideraView.OnRenderTick()` (16ms DispatcherTimer)
2. `VideraEngine.Draw()` executes main render loop:
   - Backend `BeginFrame()` clears buffers
   - Camera uniforms (View/Projection matrices) updated to GPU buffer
   - Style uniforms (lighting, materials) updated to GPU buffer
   - Grid rendered (if visible)
   - Scene objects rendered (solid pass, unless WireframeOnly mode)
   - Wireframe overlay rendered (if mode enabled)
   - Axis gizmo rendered
   - Backend `EndFrame()` presents frame
3. For software backend, `CopyFrameTo()` copies pixels to Avalonia `WriteableBitmap`

**Style Update:**

1. User changes property via `VideraView` (e.g., `RenderStyle="Toon"`)
2. `RenderStyleService.ApplyPreset()` updates internal parameters
3. `StyleChanged` event fires with `StyleChangedEventArgs`
4. Engine handler updates GPU uniform buffer via `_styleUniformBuffer.Update()`

**State Management:**

- Scene objects stored in `List<Object3D>` in engine (thread-locked)
- Camera state maintained in `OrbitCamera` with lazy matrix updates
- GPU resources owned by `Object3D` instances (buffers managed per-object)
- Style state centralized in `RenderStyleService` with INotifyPropertyChanged

## Key Abstractions

**IGraphicsBackend:**
- Purpose: Platform-agnostic rendering interface
- Examples: `SoftwareBackend`, `D3D11Backend` (Windows), `MetalBackend` (macOS), `VulkanBackend` (Linux)
- Pattern: Abstract factory + dependency injection via `GraphicsBackendFactory`

**ICommandExecutor:**
- Purpose: Immediate-mode rendering command queue
- Examples: `SoftwareCommandExecutor` (CPU rasterization)
- Pattern: Command pattern with state binding (pipeline, buffers, uniforms)

**IRenderStyleService:**
- Purpose: Live style configuration with GPU sync
- Examples: `RenderStyleService` implementation
- Pattern: Event-driven state change propagation to GPU

**OrbitCamera:**
- Purpose: 3D navigation control
- Examples: Single instance per engine
- Pattern: State object with computed matrix properties (View, Projection)

## Entry Points

**VideraEngine:**
- Location: `src/Videra.Core/Graphics/VideraEngine.cs`
- Triggers: Programmatic initialization via `Initialize(IGraphicsBackend)`
- Responsibilities: Scene graph management, main render loop, renderer coordination

**VideraView (Avalonia):**
- Location: `src/Videra.Avalonia/Controls/VideraView.cs`
- Triggers: Avalonia visual tree attachment, size changes, property changes
- Responsibilities: Backend creation, render loop management, input routing, bitmap presentation

**Program (Demo):**
- Location: `samples/Videra.Demo/Program.cs`
- Triggers: Application launch
- Responsibilities: Avalonia app builder configuration, platform-specific options

## Error Handling

**Strategy:** Exception-based with console logging fallback

**Patterns:**
- Mesh validation throws `ArgumentException` for null/empty data
- Buffer size overflow checks throw `InvalidOperationException`
- Platform backend failures fallback to software backend via null-coalescing
- Render errors caught in render loop with graceful shutdown (timer stop)

## Cross-Cutting Concerns

**Logging:** Console-based output with `[ComponentName]` prefix pattern
- Debug frame logging via `VIDERA_FRAMELOG` environment variable
- Backend selection logged via `VIDERA_BACKEND` environment variable

**Validation:** Null/size checks at GPU resource creation boundaries
- Buffer creation validates size limits (uint.MaxValue check)
- Mesh importer validates topology support

**Threading:** Render loop synchronized via `lock (_lock)` on scene object list
- DispatcherTimer ensures render calls on UI thread
- No async rendering (single-threaded draw loop)

---

*Architecture analysis: 2026-03-28*
