# Phase 3 Verification Matrix

## Build
| Check | Result |
|-------|--------|
| `dotnet build Videra.slnx -c Release` | PASS — 0 errors, 0 warnings |
| `pwsh -File ./verify.ps1 -Configuration Release` | PASS — repository verification green on 2026-04-08 |

## Local Tests
| Project | Passed | Failed | Skipped |
|---------|--------|--------|---------|
| `Tests.Common` | 1 | 0 | 0 |
| `Videra.Core.Tests` | 365 | 0 | 0 |
| `Videra.Core.IntegrationTests` | 69 | 0 | 2 |
| `Videra.Platform.Windows.Tests` | 29 | 0 | 0 |
| `Videra.Platform.Linux.Tests` | 6 | 0 | 12 |
| `Videra.Platform.macOS.Tests` | 1 | 0 | 12 |
| **Total** | **471** | **0** | **26** |

## Hosted Native Evidence

| Workflow | Run | Result | Notes |
|---------|-----|--------|-------|
| `Native Validation` | `24124366491` | PASS | `linux-x11-native`, `linux-wayland-xwayland-native`, `macos-native`, `windows-native` all green |
| `CI` | `24124366425` | PASS | `verify` green; package evidence jobs green |

## Requirement Traceability

### MAC-01: Metal interop refactor
- **Status**: Complete for current milestone scope
- **Evidence**: `ObjCRuntime.cs` is the single Objective-C/Metal P/Invoke entry point. `macos-native` hosted validation is green in run `24124366491`.
- **Deferred future work**: Replacing Objective-C runtime usage with a higher-level safer binding library.

### MAC-02: Depth buffer consistency
- **Status**: Complete
- **Evidence**: Shared `DepthBufferConfiguration` lives in `Videra.Core`; D3D11, Vulkan, and Metal all use LessEqual depth comparison and 1.0f clear depth; Metal now sets `MTLPixelFormatDepth32Float` explicitly; `DepthBufferConfigurationTests` pass.

### PLAT-01: Wayland support
- **Status**: Complete for current milestone scope
- **Evidence**: `LinuxDisplayServerDetector`, `LinuxNativeHostFactory`, `VideraLinuxNativeHost`, diagnostics fields, docs, and CI now close Linux on `X11` native plus Wayland-session `XWayland` compatibility. Hosted run `24124366491` includes green `linux-wayland-xwayland-native`.
- **Deferred future work**: Compositor-native Wayland embedding.

### PLAT-02: Dynamic library loading
- **Status**: Complete
- **Evidence**: `NativeLibraryHelper.cs` provides fallback loading via .NET `NativeLibrary` APIs; X11 fallback names are registered in `X11SurfaceCreator` and `VideraLinuxNativeHost`; `NativeLibraryHelperTests` pass.

### PLAT-03: Cross-platform build verification
- **Status**: Complete
- **Evidence**: `dotnet build Videra.slnx -c Release` passes; local `verify.ps1` passes; hosted `Native Validation` run `24124366491` is green on Windows, Linux X11, Linux Wayland-session `XWayland`, and macOS; hosted `CI` run `24124366425` is green.

## Grep Verification

| Check | Result |
|-------|--------|
| `VulkanBackend.cs` uses `ISurfaceCreator` | PASS |
| `VulkanBackend.cs` defaults to `X11SurfaceCreator` | PASS |
| `NativeLibraryHelper` uses .NET `NativeLibrary` APIs | PASS |
| X11 fallback resolver registered in Linux host + surface creator | PASS |
| macOS backend files no longer duplicate ObjC `DllImport` declarations | PASS |
| `ObjCRuntime.cs` is the sole macOS P/Invoke entry point | PASS |

## Runtime Verification Status

| Platform | Build | Tests | Native runtime |
|----------|-------|-------|----------------|
| Windows | PASS | PASS | PASS (`windows-native`) |
| Linux X11 | PASS | PASS | PASS (`linux-x11-native`) |
| Linux Wayland session | PASS | PASS | PASS via `XWayland` compatibility (`linux-wayland-xwayland-native`) |
| macOS | PASS | PASS | PASS (`macos-native`) |
