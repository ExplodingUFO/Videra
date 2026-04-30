# External Integrations

**Analysis Date:** 2026-04-02

## APIs & External Services

**Native graphics and windowing APIs (internal/native, not SaaS):**
- Windows Direct3D 11 and DXGI - Native rendering backend for the Windows host.
  - SDK/Client: `Silk.NET.Direct3D11`, `Silk.NET.DXGI`, and `Silk.NET.Direct3D.Compilers` from `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`
  - Auth: Not applicable
  - Implementation: `src/Videra.Platform.Windows/D3D11Backend.cs`, `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- Windows `user32.dll` - Native child-window creation, subclassing, and pointer message handling for the Avalonia bridge.
  - SDK/Client: direct P/Invoke in `src/Videra.Avalonia/Controls/VideraNativeHost.cs` and `src/Videra.Avalonia/Interop/Win32.cs`
  - Auth: Not applicable
- Linux Vulkan plus `VK_KHR_xlib_surface` - Native rendering backend for Linux.
  - SDK/Client: `Silk.NET.Vulkan`, `Silk.NET.Vulkan.Extensions.KHR`, `Silk.NET.Shaderc` from `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`
  - Auth: Not applicable
  - Implementation: `src/Videra.Platform.Linux/VulkanBackend.cs`, `src/Videra.Platform.Linux/VulkanResourceFactory.cs`, `src/Videra.Platform.Linux/X11SurfaceCreator.cs`
- Linux `libX11.so.6` - X11 display and window interop for both the Avalonia host and Vulkan surface creation.
  - SDK/Client: direct P/Invoke plus `Videra.Core.NativeLibrary.NativeLibraryHelper`
  - Auth: Not applicable
  - Implementation: `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`, `src/Videra.Platform.Linux/X11SurfaceCreator.cs`, `src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs`
- macOS Objective-C runtime and Metal framework - Native rendering backend for macOS.
  - SDK/Client: direct P/Invoke to `/usr/lib/libobjc.dylib` and `/System/Library/Frameworks/Metal.framework/Metal` in `src/Videra.Platform.macOS/ObjCRuntime.cs`
  - Auth: Not applicable
  - Implementation: `src/Videra.Platform.macOS/MetalBackend.cs`, `src/Videra.Platform.macOS/MetalResourceFactory.cs`, `src/Videra.Platform.macOS/MetalCommandExecutor.cs`, `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`

**Package publishing and CI services:**
- GitHub Actions - CI/CD workflow runner.
  - SDK/Client: workflow YAML in `.github/workflows/publish-nuget.yml`
  - Auth: `secrets.GITHUB_TOKEN`
- GitHub Packages - NuGet package publication target.
  - SDK/Client: `dotnet nuget push` in `.github/workflows/publish-nuget.yml`
  - Auth: `secrets.GITHUB_TOKEN`

**Internal runtime module loading (internal repository integration, not external service):**
- Platform backends are loaded dynamically by assembly name from the local solution.
  - SDK/Client: `System.Reflection.Assembly.Load(...)` and `Activator.CreateInstance(...)`
  - Auth: Not applicable
  - Implementation: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`

## Data Storage

**Databases:**
- None detected.

**File Storage:**
- Local filesystem only.
  - Model import reads `.gltf`, `.glb`, and `.obj` files from disk in `src/Videra.Core/IO/ModelImporter.cs`.
  - The sample app opens files through Avalonia's storage provider in `samples/Videra.Demo/Services/AvaloniaModelImporter.cs`.
  - The macOS sample project optionally copies `/opt/homebrew/lib/libassimp.dylib` into the output in `samples/Videra.Demo/Videra.Demo.csproj`.

**Caching:**
- None detected.

## Authentication & Identity

**Auth Provider:**
- None for application runtime.
  - Implementation: Desktop library/sample code in `src/` and `samples/` does not configure user authentication or identity middleware.

## Monitoring & Observability

**Error Tracking:**
- None detected.

**Logs:**
- `Microsoft.Extensions.Logging` abstractions are used throughout `src/Videra.Core`, `src/Videra.Avalonia`, and the platform projects.
- Optional runtime logging switches come from `VIDERA_FRAMELOG` and `VIDERA_INPUTLOG` in `src/Videra.Avalonia/Controls/VideraView.cs` and `src/Videra.Avalonia/Controls/VideraView.Input.cs`.
- Serilog packages are referenced in `src/Videra.Core/Videra.Core.csproj`, but no repository-wide `LoggerConfiguration` or `UseSerilog` setup is detected in current source files.
- The demo app also uses `Debug.WriteLine` around macOS native-library resolution in `samples/Videra.Demo/App.axaml.cs`.

## CI/CD & Deployment

**Hosting:**
- Not a hosted web service. The repository produces class libraries plus a desktop demo app from `samples/Videra.Demo/Videra.Demo.csproj`.
- Package distribution target: GitHub Packages via `.github/workflows/publish-nuget.yml`.

**CI Pipeline:**
- GitHub Actions workflow `.github/workflows/publish-nuget.yml`.
- Trigger modes:
  - tag push matching `v*`
  - manual `workflow_dispatch` with a `version` input
- Publish scope:
  - `src/Videra.Core/Videra.Core.csproj`
  - `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`
  - `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`
  - `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`
  - `src/Videra.Avalonia/Videra.Avalonia.csproj`

## Environment Configuration

**Required env vars:**
- None detected for local runtime or local build.

**Optional env vars:**
- `VIDERA_BACKEND` - backend selection/override in `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`, `src/Videra.Avalonia/Controls/VideraView.cs`, and `samples/Videra.Demo/Program.cs`
- `VIDERA_FRAMELOG` - frame logging switch in `src/Videra.Avalonia/Controls/VideraView.cs`
- `VIDERA_INPUTLOG` - input logging switch in `src/Videra.Avalonia/Controls/VideraView.Input.cs`

**Secrets location:**
- GitHub Actions secret `secrets.GITHUB_TOKEN` in `.github/workflows/publish-nuget.yml`.
- No `.env*` files were detected at the repository root.

## Native Libraries & Toolchains

**Windows:**
- `user32.dll` via direct P/Invoke in `src/Videra.Avalonia/Controls/VideraNativeHost.cs` and `src/Videra.Avalonia/Interop/Win32.cs`.
- D3D11 and DXGI are consumed through Silk.NET bindings declared in `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`.

**Linux:**
- `libX11.so.6` with fallback resolution to `libX11.so` and `libX11` through `src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs`.
- Vulkan shader compilation uses Shaderc at runtime in `src/Videra.Platform.Linux/VulkanResourceFactory.cs`.

**macOS:**
- `/usr/lib/libobjc.dylib` and `/System/Library/Frameworks/Metal.framework/Metal` via `src/Videra.Platform.macOS/ObjCRuntime.cs`.
- `xcrun metal` and `xcrun metallib` compile `src/Videra.Platform.macOS/Shaders.metal` in `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`.
- The sample app contains an optional macOS native-library resolver for `libassimp.dylib` in `samples/Videra.Demo/App.axaml.cs`; no matching `PackageReference` to `Assimp.Net` is present in the repository's current `*.csproj` files.

## Webhooks & Callbacks

**Incoming:**
- None detected.

**Outgoing:**
- None detected.

---

*Integration audit: 2026-04-02*
