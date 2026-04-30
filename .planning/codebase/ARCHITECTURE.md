# Architecture

**Analysis Date:** 2026-04-02

## Pattern Overview

**Overall:** Multi-project layered desktop rendering stack. `samples/Videra.Demo/` boots an Avalonia desktop shell, `src/Videra.Avalonia/` hosts the reusable control and native-surface glue, `src/Videra.Core/` owns renderer state and shared contracts, and `src/Videra.Platform.*` projects implement platform GPU backends behind a common interface.

**Key Characteristics:**
- `src/Videra.Core/` is intentionally UI-agnostic and platform-agnostic. It depends on `IGraphicsBackend`, `IResourceFactory`, and `ICommandExecutor` instead of Avalonia or native SDKs.
- `src/Videra.Avalonia/` is the adapter layer. It decides between software bitmap presentation and native child-surface presentation, then wires Avalonia lifecycle and input into `VideraEngine`.
- Backend selection is hybrid. `src/Videra.Avalonia/Videra.Avalonia.csproj` conditionally references platform projects by host OS, while `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` does runtime assembly loading and environment-based selection.
- Rendering is immediate-mode and timer-driven. `src/Videra.Avalonia/Controls/VideraView.cs` owns a `DispatcherTimer`, and `src/Videra.Core/Graphics/VideraEngine.cs` redraws the full frame on each tick.
- Scene state is object-centric. `src/Videra.Core/Graphics/Object3D.cs` owns transforms and GPU buffers, while `src/Videra.Core/Graphics/VideraEngine.cs` owns the active scene list and helper renderers.
- The repo is a desktop library plus demo. There is no web tier, API tier, service layer, or persistence layer beyond local file import.

## Layers

**Application Shell:**
- Purpose: Start the desktop process, configure DI, and compose the demo window.
- Location: `samples/Videra.Demo/`
- Contains: `samples/Videra.Demo/Program.cs`, `samples/Videra.Demo/App.axaml.cs`, `samples/Videra.Demo/Views/MainWindow.axaml`, `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs`
- Depends on: `src/Videra.Avalonia/Videra.Avalonia.csproj`, Avalonia desktop hosting, `Microsoft.Extensions.Hosting`
- Used by: Local development and manual verification

**UI Integration Layer:**
- Purpose: Expose `VideraView` as an Avalonia control and bridge Avalonia lifecycle, sizing, and pointer input to the rendering engine.
- Location: `src/Videra.Avalonia/Controls/`
- Contains: `src/Videra.Avalonia/Controls/VideraView.cs`, `src/Videra.Avalonia/Controls/VideraView.Input.cs`, `src/Videra.Avalonia/Controls/VideraNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`
- Depends on: `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`, Avalonia primitives, platform-native host implementations
- Used by: `samples/Videra.Demo/Views/MainWindow.axaml` and downstream Avalonia apps embedding the control

**Core Rendering Layer:**
- Purpose: Own scene state, camera state, per-frame orchestration, shared helper renderers, and backend-neutral draw flow.
- Location: `src/Videra.Core/Graphics/`
- Contains: `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Core/Graphics/Object3D.cs`, `src/Videra.Core/Graphics/GridRenderer.cs`, `src/Videra.Core/Graphics/AxisRenderer.cs`, `src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs`, `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
- Depends on: `src/Videra.Core/Cameras/OrbitCamera.cs`, `src/Videra.Core/Styles/Services/RenderStyleService.cs`, `src/Videra.Core/Graphics/Abstractions/*`
- Used by: `src/Videra.Avalonia/Controls/VideraView.cs`, import services, and demo scene state

**Graphics Contract Layer:**
- Purpose: Define the backend-neutral GPU contract every renderer implementation follows.
- Location: `src/Videra.Core/Graphics/Abstractions/`
- Contains: `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`, `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`, `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`, `src/Videra.Core/Graphics/Abstractions/IBuffer.cs`, `src/Videra.Core/Graphics/Abstractions/IPipeline.cs`, `src/Videra.Core/Graphics/Abstractions/ISoftwareBackend.cs`
- Depends on: Core geometry/value types only
- Used by: `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Core/Graphics/Object3D.cs`, `src/Videra.Core/Graphics/Software/*`, and each `src/Videra.Platform.*` project

**Platform Backend Layer:**
- Purpose: Implement the shared GPU contract for each supported native graphics API.
- Location: `src/Videra.Platform.Windows/`, `src/Videra.Platform.Linux/`, `src/Videra.Platform.macOS/`
- Contains: `src/Videra.Platform.Windows/D3D11Backend.cs` plus D3D11 command/resource/pipeline types, `src/Videra.Platform.Linux/VulkanBackend.cs` plus Vulkan command/resource/surface types, `src/Videra.Platform.macOS/MetalBackend.cs` plus Metal command/resource/runtime types
- Depends on: `src/Videra.Core/Graphics/Abstractions/*`, platform SDK bindings, and native interop
- Used by: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` at runtime and `src/Videra.Avalonia/Videra.Avalonia.csproj` at build time on matching host OS

**Software Fallback Layer:**
- Purpose: Preserve a backend-neutral fallback path that renders into CPU memory and can be copied into an Avalonia bitmap.
- Location: `src/Videra.Core/Graphics/Software/`
- Contains: `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`, `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs`, `src/Videra.Core/Graphics/Software/SoftwareResourceFactory.cs`, `src/Videra.Core/Graphics/Software/SoftwareFrameBuffer.cs`
- Depends on: Core abstractions only
- Used by: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` fallback paths and `src/Videra.Avalonia/Controls/VideraView.cs` when the chosen backend implements `ISoftwareBackend`

**Scene Import and Styling Layer:**
- Purpose: Convert external assets and UI-driven style changes into engine-ready objects and uniform data.
- Location: `src/Videra.Core/IO/`, `src/Videra.Core/Styles/`
- Contains: `src/Videra.Core/IO/ModelImporter.cs`, `src/Videra.Core/Styles/Services/RenderStyleService.cs`, `src/Videra.Core/Styles/Presets/RenderStylePresets.cs`, `src/Videra.Core/Styles/Parameters/*`
- Depends on: `src/Videra.Core/Graphics/Object3D.cs`, `IResourceFactory`, SharpGLTF, JSON serialization helpers
- Used by: `samples/Videra.Demo/Services/AvaloniaModelImporter.cs`, `src/Videra.Avalonia/Controls/VideraView.cs`, `src/Videra.Core/Graphics/VideraEngine.cs`

## Data Flow

**Desktop Startup and Composition:**

1. `samples/Videra.Demo/Program.cs` builds the Avalonia app and, on Windows, sets `VIDERA_BACKEND=d3d11` while forcing specific Avalonia Win32 composition and UI rendering modes.
2. `samples/Videra.Demo/App.axaml.cs` creates an `IHost`, registers `MainWindow`, `MainWindowViewModel`, and `IModelImporter`, then resolves `MainWindow` from DI.
3. `samples/Videra.Demo/Views/MainWindow.axaml` declares `controls:VideraView` and binds view-model state such as `SceneObjects`, camera inversion, grid settings, render style, and wireframe mode.
4. `samples/Videra.Demo/Views/MainWindow.axaml.cs` waits for `View3D.BackendReady`, then creates `AvaloniaModelImporter` from the live `IResourceFactory`, updates UI status, and seeds the scene with `DemoMeshFactory.CreateCube(factory)`.

**Backend Selection and Surface Binding:**

1. `src/Videra.Avalonia/Controls/VideraView.cs` decides whether it wants a native host through `WantsNativeBackend()`.
2. If native presentation is desired, `EnsureNativeHost()` creates `VideraNativeHost`, `VideraLinuxNativeHost`, or `VideraMacOSNativeHost` and waits for `HandleCreated`.
3. `TryInitializeOrResize()` calls `InitializeGraphicsDevice()`, which invokes `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` with the `PreferredBackend` property.
4. `GraphicsBackendFactory` optionally honors `VIDERA_BACKEND` when the preference is `Auto`, then reflection-loads `Videra.Platform.Windows`, `Videra.Platform.Linux`, or `Videra.Platform.macOS`; any failure falls back to `SoftwareBackend`.
5. `VideraView` stores whether the chosen backend implements `ISoftwareBackend`; software mode creates a `WriteableBitmap`, while native mode presents directly to the child platform handle.

**Per-Frame Rendering Flow:**

1. `src/Videra.Avalonia/Controls/VideraView.cs` starts a `DispatcherTimer` at about 16 ms and calls `RenderFrame()` on each tick.
2. `RenderFrame()` delegates to `src/Videra.Core/Graphics/VideraEngine.cs`.
3. `VideraEngine.Draw()` updates clear color, begins the backend frame, clears buffers, sets the viewport, and uploads camera and style uniforms.
4. `GridRenderer.Draw()` renders the reference grid, then the engine iterates `_sceneObjects` and draws solid geometry from each `Object3D`.
5. `WireframeRenderer.RenderWireframes()` optionally draws overlay, visible, or hidden edges using the same executor and fixed buffer slots.
6. `AxisRenderer.Draw()` renders the mini axis widget in a smaller viewport and restores the main viewport.
7. The engine ends the frame. If the active backend is `ISoftwareBackend`, `VideraView` copies the CPU framebuffer into the Avalonia bitmap and calls `InvalidateVisual()`.

**Scene and Asset Flow:**

1. `samples/Videra.Demo/Services/AvaloniaModelImporter.cs` opens files via Avalonia storage and passes them to `src/Videra.Core/IO/ModelImporter.cs`.
2. `ModelImporter.Load()` parses `gltf`, `glb`, or `obj`, produces `MeshData`, constructs an `Object3D`, and initializes GPU buffers against the live `IResourceFactory`.
3. `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs` stores imported meshes in `SceneObjects`, an `ObservableCollection<Object3D>`.
4. `src/Videra.Avalonia/Controls/VideraView.cs` subscribes to collection changes, mirrors the collection into `VideraEngine`, and lets the engine draw the updated scene on the next timer tick.

**State Management:**
- UI state lives in `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs` and `samples/Videra.Demo/ViewModels/CameraViewModel.cs`.
- Control-level rendering state lives in `src/Videra.Avalonia/Controls/VideraView.cs` (`_backend`, `_bitmap`, `_renderTimer`, `_renderHandle`, `_isReady`).
- Render state lives in `src/Videra.Core/Graphics/VideraEngine.cs` (`_sceneObjects`, pipelines, buffers, camera, style service, helper renderers).
- Object transform and per-object GPU resources live in `src/Videra.Core/Graphics/Object3D.cs`.
- There is no centralized application store. Updates flow by direct property mutation, collection notifications, and control-to-engine propagation.

## Key Abstractions

**`VideraEngine`:**
- Purpose: The orchestration boundary between the UI/control layer and GPU command submission.
- Examples: `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Avalonia/Controls/VideraView.cs`
- Pattern: Stateful engine service with an owned camera, scene list, helper renderers, and lock-serialized frame submission

**`IGraphicsBackend`:**
- Purpose: Abstract platform initialization, resize, frame lifecycle, clear color, resource creation, and command execution.
- Examples: `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs`, `src/Videra.Platform.Windows/D3D11Backend.cs`, `src/Videra.Platform.Linux/VulkanBackend.cs`, `src/Videra.Platform.macOS/MetalBackend.cs`, `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`
- Pattern: Strategy interface with runtime-selected implementations

**`IResourceFactory` and `ICommandExecutor`:**
- Purpose: Split GPU resource creation from draw command submission.
- Examples: `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`, `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`, `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`, `src/Videra.Platform.Windows/D3D11CommandExecutor.cs`, `src/Videra.Platform.macOS/MetalCommandExecutor.cs`
- Pattern: Backend-specific adapter pair. The current engine binds camera, world, and style data by fixed slots rather than resource sets.

**`Object3D`:**
- Purpose: Represent one renderable scene object with transform state, solid buffers, and optional wireframe buffers.
- Examples: `src/Videra.Core/Graphics/Object3D.cs`, `samples/Videra.Demo/Services/DemoMeshFactory.cs`, `src/Videra.Core/IO/ModelImporter.cs`
- Pattern: Scene entity with self-owned GPU resources and lazy wireframe buffer creation

**`IVideraNativeHost`:**
- Purpose: Abstract an Avalonia `NativeControlHost` that can provide a platform handle and optional native pointer events.
- Examples: `src/Videra.Avalonia/Controls/IVideraNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`, `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`
- Pattern: UI-platform bridge. Handle lifecycle is unified, but input forwarding is only fully implemented on Windows today.

**`ISurfaceCreator` and `ObjCRuntime`:**
- Purpose: Keep the most platform-specific surface and runtime interop code out of the engine and out of the Avalonia control.
- Examples: `src/Videra.Platform.Linux/ISurfaceCreator.cs`, `src/Videra.Platform.Linux/X11SurfaceCreator.cs`, `src/Videra.Platform.macOS/ObjCRuntime.cs`
- Pattern: Low-level interop seams that isolate native surface creation and Objective-C messaging

## Entry Points

**Process Entry Point:**
- Location: `samples/Videra.Demo/Program.cs`
- Triggers: OS process start
- Responsibilities: Build the Avalonia app, set Windows backend defaults, configure host platform options

**Application Bootstrap:**
- Location: `samples/Videra.Demo/App.axaml.cs`
- Triggers: Avalonia framework initialization
- Responsibilities: Register the macOS Assimp resolver, create the DI host, wire desktop lifecycle, resolve `MainWindow`

**Window Composition:**
- Location: `samples/Videra.Demo/Views/MainWindow.axaml` and `samples/Videra.Demo/Views/MainWindow.axaml.cs`
- Triggers: Window load and `VideraView.BackendReady`
- Responsibilities: Bind UI state to `VideraView`, instantiate importer services only after a backend exists, seed the demo scene

**Reusable Rendering Control:**
- Location: `src/Videra.Avalonia/Controls/VideraView.cs`
- Triggers: Avalonia attach and detach, size changes, native handle creation, render timer ticks
- Responsibilities: Create native hosts or software bitmaps, initialize backends, bridge property changes to the engine, schedule frames

**Runtime Backend Resolver:**
- Location: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
- Triggers: `VideraView.InitializeGraphicsDevice()`
- Responsibilities: Apply preference and environment override rules, load backend assemblies, return a platform default or software fallback

## Error Handling

**Strategy:** Fail fast during backend creation, log aggressively, retry control initialization a few times, and preserve a software fallback whenever native backend loading fails.

**Patterns:**
- Platform backends validate handles and dimensions before initialization and throw `PlatformDependencyException`, `GraphicsInitializationException`, or `ResourceCreationException` from `src/Videra.Core/Exceptions/`.
- `src/Videra.Avalonia/Controls/VideraView.cs` catches initialization errors, logs them, and retries up to five times on the UI thread.
- `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` catches reflection or backend load failures and returns `SoftwareBackend` instead of propagating.
- `src/Videra.Core/Graphics/VideraEngine.cs` guards resize and draw paths, uses initialization checks, and stops the render loop when `RenderFrame()` throws.
- `samples/Videra.Demo/Services/AvaloniaModelImporter.cs` logs and skips individual failed imports instead of crashing the whole demo session.

## Cross-Cutting Concerns

**Logging:** `Microsoft.Extensions.Logging` abstractions are used across `src/Videra.Core/`, `src/Videra.Avalonia/`, and the platform projects. Library code defaults to `NullLoggerFactory`, while the demo app inherits host logging. `VIDERA_FRAMELOG` and `VIDERA_INPUTLOG` toggle high-volume diagnostics in `src/Videra.Avalonia/Controls/VideraView.cs`.

**Validation:** Guard clauses and typed exceptions are the normal pattern. Path and extension checks live in `src/Videra.Core/IO/ModelImporter.cs`; dimension and handle checks live in each backend implementation; analyzer policy is shared through `Directory.Build.props`.

**Authentication:** Not applicable. This repository is a local desktop rendering component and sample app with no auth or identity layer.

**Native dependency resolution:** Cross-platform native library fallback is centralized in `src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs`, while backend-specific surface and runtime interop stays in `src/Videra.Platform.Linux/X11SurfaceCreator.cs` and `src/Videra.Platform.macOS/ObjCRuntime.cs`.

## Architecture Maturity

**Mature / stable:**
- The project boundary between `src/Videra.Core/`, `src/Videra.Avalonia/`, and `src/Videra.Platform.*` is clear and enforced by project references.
- The contract set in `src/Videra.Core/Graphics/Abstractions/` is stable enough that Windows, Linux, macOS, and software backends all implement the same lifecycle shape.
- The render loop from `VideraView` into `VideraEngine` and the scene-object model in `Object3D` are coherent and already mirrored by project-specific tests under `tests/`.

**Still in transition:**
- Platform coverage is uneven. `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` explicitly says Vulkan is only wired for Linux and X11 right now, and `src/Videra.Platform.Linux/ISurfaceCreator.cs` already anticipates a future Wayland implementation.
- Native input forwarding is uneven. `src/Videra.Avalonia/Controls/VideraNativeHost.cs` raises `NativePointer`, while `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs` and `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs` currently only manage handles and sizing.
- macOS interop is still being consolidated. `src/Videra.Platform.macOS/ObjCRuntime.cs` documents that existing files retain inline `DllImport` usage for backward compatibility during migration.
- Some comments and sample defaults lag the runtime architecture. `src/Videra.Avalonia/Controls/VideraView.cs` still describes itself as software-backend rendering only, `samples/Videra.Demo/Views/MainWindow.axaml` hardcodes `PreferredBackend="D3D11"`, and `src/Videra.Core/Graphics/GraphicsBackendPreference.cs` says Vulkan is available on Windows and Linux even though the factory currently restricts it.
- The abstraction surface is slightly ahead of the active engine path. `IResourceFactory.CreateResourceSet` and `ICommandExecutor.SetResourceSet` exist across projects, but `src/Videra.Core/Graphics/VideraEngine.cs` currently binds camera, world, and style data by fixed vertex-buffer slots instead of resource sets.

---

*Architecture analysis: 2026-04-02*
