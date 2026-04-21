---
phase: 03
type: research
status: Complete
---

# Phase 3 Research: Cross-platform Refinement

## Superseded Scope Note

This research document was written before the later hosted-native-validation work and before the milestone scope was revised around Avalonia `11.3.9` Linux hosting constraints.

Current execution result:

- native Linux rendering closes on `X11`
- Wayland sessions close on `XWayland` compatibility
- macOS closes on centralized/hardened `ObjCRuntime`
- compositor-native Wayland embedding and higher-level macOS safer bindings are deferred beyond this milestone

## Requirement Mapping

| REQ-ID | Requirement | Windows-achievable |
|--------|------------|-------------------|
| MAC-01 | Metal interop refactor | Partial (code refactor, build validation) |
| MAC-02 | Depth buffer consistency | Yes (code changes across all backends) |
| PLAT-01 | Wayland support | Partial (abstraction layer, build only) |
| PLAT-02 | Dynamic library loading | Partial (abstraction, Windows test) |
| PLAT-03 | Build verification across 3 platforms | Already passing |

## MAC-01: Metal Interop Safety

### Current State
- **72 total** `objc_msgSend`/`objc_getClass`/`sel_registerName` calls across 6 files (excluding README)
- **~50 DllImport** declarations for `/usr/lib/libobjc.dylib` across:
  - `MetalBackend.cs`: 11 DllImports
  - `MetalCommandExecutor.cs`: 14 DllImports
  - `MetalResourceFactory.cs`: 13 DllImports
  - `MetalBuffer.cs`: 3 DllImports
  - `MetalPipeline.cs`: 2 DllImports
  - `VideraMacOSNativeHost.cs`: 6 DllImports

### Risk Surface
1. **Memory management**: Objective-C objects created via `AllocInit` are never tracked — manual `release` calls scattered throughout
2. **Type safety**: All `objc_msgSend` calls use raw `IntPtr` — no compile-time checking of selector/method signatures
3. **Error handling**: No checking of Objective-C exception mechanism; nil returns silently propagate
4. **Duplication**: Same ObjC interop helpers duplicated across every file (SEL, SendMessage, AllocInit, etc.)

### Refactor Strategy (achievable on Windows)
1. Extract shared ObjC interop into `ObjCRuntime.cs` helper class
2. Create typed wrapper classes for Metal objects (MTLDevice, MTLCommandQueue, MTLTexture, etc.)
3. Add proper retain/release lifecycle management
4. The actual type-safe binding library (SharpMetal) integration would require NuGet package evaluation — can be done on Windows but runtime validation needs macOS

### Environment-blocked
- Runtime validation of Metal object creation
- Testing that typed wrappers actually work with Metal API

## MAC-02: Depth Buffer Consistency

### Current State
| Backend | Depth Format | Comparison | Clear Value | Resize handling |
|---------|-------------|------------|-------------|-----------------|
| Windows D3D11 | D24UnormS8Uint (24+8) | LessEqual | 1.0f | Dispose + recreate |
| Linux Vulkan | D32Sfloat (32-bit float) | LessOrEqual | 1.0f | Destroy + recreate |
| macOS Metal | (default) | LessEqual (4) | 1.0f | (drawable-managed) |

### Gaps
1. **Format inconsistency**: D3D11 uses 24-bit depth, Vulkan uses 32-bit float, Metal uses platform default
2. **No shared depth configuration**: Each backend independently decides depth format
3. **No depth state management abstraction**: D3D11 has explicit depth states, Vulkan bakes into pipeline, Metal creates descriptor

### Resolution Strategy (achievable on Windows)
1. Standardize on D32Sfloat where available (or D24S8 as minimum guarantee)
2. Add `DepthBufferDescription` to core abstractions for format/clear configuration
3. Unify clear value to 1.0f across all backends (already consistent)
4. Add depth buffer resize tests for D3D11

## PLAT-01: Wayland Support

### Current State
- `VulkanBackend.cs` hardcodes X11 via `KhrXlibSurface` extension
- `VulkanBackend.cs` calls `XOpenDisplay`/`XCloseDisplay` from `libX11.so.6`
- `VideraLinuxNativeHost.cs` has 11 DllImport calls to `libX11.so.6` for window management

### What's needed for Wayland
1. Replace hardcoded `KhrXlibSurface` with runtime detection (X11 vs Wayland)
2. Add `KhrWaylandSurface` extension support
3. Add Wayland display connection (`wl_display_connect`)
4. Create `VideraWaylandNativeHost` or abstract the native host

### Environment-blocked
- All Wayland runtime testing
- Wayland compositor interaction validation

### Achievable on Windows
- Abstract the surface creation logic into a strategy pattern
- Create the Wayland surface code path (compiles but can't test)
- Refactor `VulkanBackend` to accept display server type parameter

## PLAT-02: Dynamic Library Loading

### Current State
Hardcoded library paths:
- `libX11.so.6` (Linux native host: 11 calls, VulkanBackend: 2 calls)
- `/usr/lib/libobjc.dylib` (macOS Metal: ~50 calls)
- `libvulkan.so.1` (implicit via Silk.NET)
- `/System/Library/Frameworks/Metal.framework/Metal` (macOS: MTLCreateSystemDefaultDevice)

### Strategy
1. Create `NativeLibraryLoader` abstraction with:
   - `LoadLibrary(name, fallbackPaths)` — tries multiple paths
   - `GetProcAddress(handle, name)` — symbol resolution
2. On Linux: try `libX11.so.6`, `libX11.so`, `/usr/lib/x86_64-linux-gnu/libX11.so.6`
3. On macOS: try framework path first, then `/usr/local/lib/libobjc.dylib`
4. Add graceful degradation: log warning if library not found, throw `PlatformDependencyException`

### Achievable on Windows
- Design the abstraction layer
- Implement fallback path resolution
- Build validation
- D3D11 already uses Silk.NET which handles this internally

## Phase 3 Execution Strategy

Given the Windows-only environment, Phase 3 should be split into:

### Executable on Windows (Phase 3A)
1. **MAC-02**: Unify depth buffer formats and configuration across backends
2. **MAC-01 partial**: Extract ObjC interop into shared helper, reduce duplication
3. **PLAT-02**: Create `NativeLibraryLoader` abstraction with fallback paths
4. **PLAT-01 partial**: Abstract surface creation strategy for Vulkan backend

### Environment-blocked (Phase 3B — future execution)
1. **MAC-01 full**: Typed Metal wrapper classes — needs macOS runtime validation
2. **PLAT-01 full**: Wayland surface creation — needs Linux Wayland compositor
3. **PLAT-03 full**: Cross-platform build & test verification on all 3 OSes
