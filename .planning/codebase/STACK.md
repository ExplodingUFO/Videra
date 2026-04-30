# Technology Stack

**Analysis Date:** 2026-04-02

## Languages

**Primary:**
- C# on `.NET 8` - All implementation code in `src/`, tests in `tests/`, and the sample app in `samples/Videra.Demo/`.

**Secondary:**
- AXAML/XAML - Desktop UI markup in `samples/Videra.Demo/App.axaml` and `samples/Videra.Demo/Views/MainWindow.axaml`.
- MSBuild XML - Solution and build configuration in `Videra.slnx`, `Directory.Build.props`, and every `*.csproj` under `src/`, `tests/`, and `samples/`.
- Metal Shading Language - Native macOS shader source in `src/Videra.Platform.macOS/Shaders.metal`.
- PowerShell - Windows-oriented repo automation in `verify.ps1` and `clean.ps1`.
- Bash - Unix-oriented repo automation in `verify.sh` and `clean.sh`.
- YAML - CI/CD workflow definition in `.github/workflows/publish-nuget.yml`.

## Runtime

**Environment:**
- .NET 8 runtime and SDK - Every project targets `net8.0`, including `src/Videra.Core/Videra.Core.csproj`, `src/Videra.Avalonia/Videra.Avalonia.csproj`, `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`, `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`, `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`, and `samples/Videra.Demo/Videra.Demo.csproj`.
- SDK pinning: Not detected. `global.json` is absent.
- Other runtimes: Not detected. No `package.json`, `pyproject.toml`, `go.mod`, or `Cargo.toml` files are present at the repository root.

**Package Manager:**
- NuGet via per-project `PackageReference` items in each `*.csproj`.
- Central package management: Not detected. `Directory.Packages.props` is absent.
- Lockfile: missing.

## Frameworks

**Core:**
- .NET SDK-style class library/app projects - All projects in `Videra.slnx`.
- Avalonia `11.3.9` - Cross-platform desktop UI in `src/Videra.Avalonia/Videra.Avalonia.csproj` and `samples/Videra.Demo/Videra.Demo.csproj`.
- Microsoft Extensions hosting/DI/logging `9.0.11` - Host creation and service wiring in `samples/Videra.Demo/App.axaml.cs`; shared logging abstractions in `src/Videra.Core/Videra.Core.csproj` and `src/Videra.Avalonia/Videra.Avalonia.csproj`.
- Silk.NET `2.21.0` - Graphics API bindings in `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj` and `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`.

**Testing:**
- xUnit `2.9.3` - Test runner across `tests/Tests.Common/Tests.Common.csproj`, `tests/Videra.Core.Tests/Videra.Core.Tests.csproj`, `tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj`, `tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj`, `tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj`, and `tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj`.
- Microsoft.NET.Test.Sdk `18.3.0` - Test host in the same test projects.
- FluentAssertions `7.0.0` - Assertion library in `tests/Tests.Common/Tests.Common.csproj` and consuming test projects.
- Moq `4.20.72` - Mocking library in `tests/Tests.Common/Tests.Common.csproj` and `tests/Videra.Core.Tests/Videra.Core.Tests.csproj`.
- coverlet.collector `6.0.2` - Coverage collection in all test projects.

**Build/Dev:**
- MSBuild and `dotnet` CLI - Repo entry points are `Videra.slnx`, `verify.ps1`, and `verify.sh`.
- GitHub Actions - Packaging workflow in `.github/workflows/publish-nuget.yml`.
- Roslyn analyzers and SonarAnalyzer.CSharp - Shared in `Directory.Build.props`.
- EditorConfig-based analyzer and naming rules - Repository root `.editorconfig`.

## Key Dependencies

**Critical:**
- `SharpGLTF.Toolkit` `1.0.6` - glTF/GLB import support declared in `src/Videra.Core/Videra.Core.csproj` and used by `src/Videra.Core/IO/ModelImporter.cs`.
- `Avalonia` and `Avalonia.Desktop` `11.3.9` - UI and desktop hosting declared in `src/Videra.Avalonia/Videra.Avalonia.csproj` and `samples/Videra.Demo/Videra.Demo.csproj`.
- `Silk.NET.Direct3D11`, `Silk.NET.DXGI`, and `Silk.NET.Direct3D.Compilers` `2.21.0` - Windows graphics backend declared in `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj` and implemented in `src/Videra.Platform.Windows/D3D11Backend.cs`.
- `Silk.NET.Vulkan`, `Silk.NET.Vulkan.Extensions.KHR`, and `Silk.NET.Shaderc` `2.21.0` - Linux graphics backend and runtime shader compilation declared in `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj` and implemented in `src/Videra.Platform.Linux/VulkanBackend.cs` and `src/Videra.Platform.Linux/VulkanResourceFactory.cs`.
- `Silk.NET.Core` `2.21.0` - Shared native interop utilities in all three platform backends, including `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`.

**Infrastructure:**
- `Microsoft.Extensions.Hosting` and `Microsoft.Extensions.DependencyInjection` `9.0.11` - Sample app host and service registration in `samples/Videra.Demo/Videra.Demo.csproj` and `samples/Videra.Demo/App.axaml.cs`.
- `Microsoft.Extensions.Logging` and `Microsoft.Extensions.Logging.Abstractions` `9.0.11` - Logging abstraction used across `src/Videra.Core`, `src/Videra.Avalonia`, and platform projects.
- `CommunityToolkit.Mvvm` `8.2.1` - ViewModel support in `samples/Videra.Demo/Videra.Demo.csproj`.
- `Serilog`, `Serilog.Extensions.Logging`, `Serilog.Sinks.Console`, and `Serilog.Sinks.File` - Referenced in `src/Videra.Core/Videra.Core.csproj`; explicit Serilog bootstrap code is not detected in current source files.

## Configuration

**Environment:**
- `VIDERA_BACKEND` - Backend override read in `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`, `src/Videra.Avalonia/Controls/VideraView.cs`, and set on Windows in `samples/Videra.Demo/Program.cs`.
- `VIDERA_FRAMELOG` - Frame logging toggle read in `src/Videra.Avalonia/Controls/VideraView.cs`.
- `VIDERA_INPUTLOG` - Input logging toggle read in `src/Videra.Avalonia/Controls/VideraView.Input.cs`.
- `.env` files: Not detected at repository root.

**Build:**
- Shared analyzer configuration in `Directory.Build.props` and `.editorconfig`.
- Windows-specific RID in `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj` with `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`.
- Linux-specific RID in `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj` with `<RuntimeIdentifier>linux-x64</RuntimeIdentifier>`.
- Conditional platform project wiring in `src/Videra.Avalonia/Videra.Avalonia.csproj`, which references `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`, `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`, or `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj` based on host OS.
- macOS Metal shader compilation target in `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`, invoking `xcrun -sdk macosx metal` and `xcrun -sdk macosx metallib`.
- Repo-level configuration files not detected: `NuGet.config`, `Directory.Packages.props`, `appsettings.json`, and Docker build files.

## Scripts

**Verification:**
- `verify.ps1` - Runs `dotnet build Videra.slnx`, `dotnet test Videra.slnx`, demo build, and optional native Linux/macOS validation packages.
- `verify.sh` - Unix equivalent of `verify.ps1` with the same build/test/native-validation flow.

**Cleanup:**
- `clean.ps1` - Deletes `tmpclaude-*` artifacts recursively.
- `clean.sh` - Unix cleanup for the same `tmpclaude-*` artifacts.

**Packaging:**
- `.github/workflows/publish-nuget.yml` - Packs `src/Videra.Core/Videra.Core.csproj`, `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`, `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`, `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`, and `src/Videra.Avalonia/Videra.Avalonia.csproj`, then pushes the resulting packages to GitHub Packages.

## Project Layout

**Solution:**
- `Videra.slnx` is the top-level solution file and contains 12 projects.

**Libraries under `src/`:**
- `src/Videra.Core/Videra.Core.csproj` - Platform-agnostic rendering core and model import.
- `src/Videra.Avalonia/Videra.Avalonia.csproj` - Avalonia control layer and native host bridge.
- `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj` - Windows Direct3D 11 backend.
- `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj` - Linux Vulkan backend.
- `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj` - macOS Metal backend.

**Sample app under `samples/`:**
- `samples/Videra.Demo/Videra.Demo.csproj` - Desktop demo application using the Avalonia control package.

**Test projects under `tests/`:**
- `tests/Tests.Common/Tests.Common.csproj` - Shared test dependencies/helpers.
- `tests/Videra.Core.Tests/Videra.Core.Tests.csproj` - Unit tests for core code.
- `tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj` - Core integration tests.
- `tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj` - Windows backend tests.
- `tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj` - Linux backend tests.
- `tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj` - macOS backend tests.

## Platform Requirements

**Development:**
- .NET 8 SDK and NuGet restore support.
- Windows development requires a D3D11-capable host for native validation paths in `tests/Videra.Platform.Windows.Tests/`.
- Linux development requires X11 plus Vulkan drivers for native validation paths in `tests/Videra.Platform.Linux.Tests/`.
- macOS development requires Metal-capable hardware and `xcrun` from Xcode Command Line Tools because `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj` compiles `src/Videra.Platform.macOS/Shaders.metal`.

**Production:**
- Windows target: Windows 10+ with a Direct3D 11 compatible GPU, reflected in `README.md` and `src/Videra.Platform.Windows/README.md`.
- Linux target: Linux x64 with X11 and Vulkan 1.2+ support, reflected in `README.md` and `src/Videra.Platform.Linux/README.md`.
- macOS target: macOS 10.15+ with Metal support, reflected in `README.md` and `src/Videra.Platform.macOS/README.md`.
- Sample-app-only native add-on: `samples/Videra.Demo/Videra.Demo.csproj` copies `/opt/homebrew/lib/libassimp.dylib` on macOS when that host path exists.

---

*Stack analysis: 2026-04-02*
