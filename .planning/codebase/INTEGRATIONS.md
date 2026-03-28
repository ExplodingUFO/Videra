# External Integrations

**Analysis Date:** 2026-03-28

## Native Platform APIs

**Windows:**
- Direct3D 11 - Hardware-accelerated rendering
  - SDK/Client: `Silk.NET.Direct3D11`, `Silk.NET.DXGI`
  - Auth: None (system API)
- DXGI - Swap chain and display management
  - Purpose: Present rendered frames to screen
- user32.dll - Window subclassing for native host integration
  - Entry point: `src/Videra.Avalonia/Interop/Win32.cs`

**Linux:**
- Vulkan - Hardware-accelerated rendering
  - SDK/Client: `Silk.NET.Vulkan`, `Silk.NET.Vulkan.Extensions.KHR`, `Silk.NET.Shaderc`
  - Auth: None (system API)
- libX11.so.6 - X11 window system integration
  - Purpose: Create Vulkan surface from X11 window
  - Entry point: `src/Videra.Platform.Linux/VulkanBackend.cs`

**macOS:**
- Metal - Hardware-accelerated rendering
  - SDK/Client: Objective-C Runtime via P/Invoke (no NuGet package)
  - Auth: None (system framework)
- libobjc.dylib - Objective-C runtime messaging
  - Purpose: Dynamically invoke Metal framework APIs
  - Entry points: `src/Videra.Platform.macOS/MetalBackend.cs`, `MetalResourceFactory.cs`, `MetalCommandExecutor.cs`, `MetalBuffer.cs`
- Metal Framework - GPU rendering
- QuartzCore Framework - Core Animation for CAMetalLayer

**Software Fallback:**
- CPU-based rasterizer (no external dependencies)
  - Entry point: `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`

## File Formats

**3D Model Import:**
- glTF 2.0 (.gltf, .glb) - Primary format via `SharpGLTF.Schema2.ModelRoot.Load()`
  - Implementation: `src/Videra.Core/IO/ModelImporter.cs:48`
- OBJ (.obj) - Basic mesh format via simple parser
  - Implementation: `src/Videra.Core/IO/ModelImporter.cs:174`

**Shader Files:**
- Metal Shading Language (.metal) - macOS GPU shaders
  - Path: `src/Videra.Platform.macOS/Shaders.metal`
  - Compiled at build time via xcrun metal
  - Target: `default.metallib` embedded in output

**Configuration:**
- JSON - Not detected for runtime config
- Environment variables used for backend selection and logging

## Package Feeds

**NuGet Sources:**
- nuget.org - Public NuGet gallery (implicit)
- GitHub Packages - Private feed for publishing
  - URL: `https://nuget.pkg.github.com/ExplodingUFO/index.json`
  - Auth: `GITHUB_TOKEN` in CI workflow
  - Workflow: `.github/workflows/publish-nuget.yml`

## Cross-Project Dependencies

**Internal Project References:**
- `Videra.Platform.Windows` → `Videra.Core`
- `Videra.Platform.Linux` → `Videra.Core`
- `Videra.Platform.macOS` → `Videra.Core`
- `Videra.Avalonia` → `Videra.Core` + platform-specific projects (conditional)
- `Videra.Demo` → `Videra.Avalonia`

**Dynamic Assembly Loading:**
- Platform backends loaded via reflection at runtime
  - Factory: `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
  - Pattern: `System.Reflection.Assembly.Load()` → `Activator.CreateInstance()`

## Environment Configuration

**Required env vars:**
- None strictly required; all have defaults

**Optional env vars:**
- `VIDERA_BACKEND` - Override graphics backend (software/d3d11/vulkan/metal/auto)
- `VIDERA_FRAMELOG` - Enable per-frame logging (1/true)
- `VIDERA_INPUTLOG` - Enable input event logging (1/true)

**Secrets location:**
- No secrets in codebase
- CI uses GitHub Actions `secrets.GITHUB_TOKEN`

## CI/CD & Deployment

**Hosting:**
- GitHub Actions - CI/CD pipeline
  - Workflow: `.github/workflows/publish-nuget.yml`
  - Triggers: Tag push (`v*`), manual dispatch

**Package Publishing:**
- GitHub Packages - NuGet feed
  - Packages: Core, Windows, Linux, macOS, Avalonia projects
  - Versioning: Semver via git tags or manual input

## Webhooks & Callbacks

**Incoming:**
- None

**Outgoing:**
- None

## Native Library Loading

**Windows:**
- `user32.dll` via P/Invoke - Window procedure hooks

**Linux:**
- `libX11.so.6` via P/Invoke - X11 display/connection

**macOS:**
- `/usr/lib/libobjc.dylib` via P/Invoke - Objective-C runtime
- System frameworks loaded dynamically via Objective-C messaging (no explicit DllImport for Metal/QuartzCore)

---

*Integration audit: 2026-03-28*
