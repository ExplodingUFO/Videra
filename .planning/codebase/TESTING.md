# Testing Patterns

**Analysis Date:** 2026-04-02

## Current Test Stack

**Frameworks and packages:**
- xUnit is the primary test runner in `tests/Videra.Core.Tests/Videra.Core.Tests.csproj`, `tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj`, and all three platform test projects.
- FluentAssertions is the common assertion library across the suite.
- Moq is used for abstraction-level unit tests in `tests/Videra.Core.Tests/` and shared helpers in `tests/Tests.Common/`.
- `coverlet.collector` is referenced in the test projects, so coverage collection is wired at the package level.
- `Microsoft.NET.Test.Sdk` is pinned to `18.3.0` in the test projects.

**Repo scale today:**
- `Videra.slnx` includes six test projects: `tests/Tests.Common/`, `tests/Videra.Core.Tests/`, `tests/Videra.Core.IntegrationTests/`, `tests/Videra.Platform.Windows.Tests/`, `tests/Videra.Platform.Linux.Tests/`, and `tests/Videra.Platform.macOS.Tests/`.
- The `tests/` tree contains 44 non-generated C# test/source files.
- There are 367 `[Fact]` or `[Theory]` attributes under `tests/` today.

## Test Project Layout

**`tests/Tests.Common/`:**
- Purpose: Shared fixtures and helpers.
- Key files: `tests/Tests.Common/Platform/NativeHostTestHelpers.cs`
- Notable role: Provides real Win32, X11, and NSView test window helpers for native-host validation.

**`tests/Videra.Core.Tests/`:**
- Purpose: Fast unit coverage for backend-neutral logic.
- Focus areas: cameras, geometry, styles, logging, native library helpers, exception hierarchy, software renderer behavior, and graphics abstractions.
- Example files: `tests/Videra.Core.Tests/Cameras/OrbitCameraTests.cs`, `tests/Videra.Core.Tests/Graphics/Object3DTests.cs`, `tests/Videra.Core.Tests/Graphics/Wireframe/EdgeExtractorTests.cs`

**`tests/Videra.Core.IntegrationTests/`:**
- Purpose: Backend-neutral integration coverage using the software backend and real engine flow.
- Focus areas: `VideraEngine`, `GridRenderer`, `AxisRenderer`, `WireframeRenderer`, and `ModelImporter`.
- Example files: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`, `tests/Videra.Core.IntegrationTests/IO/ModelImporterIntegrationTests.cs`

**Platform-native suites:**
- `tests/Videra.Platform.Windows.Tests/` covers D3D11 smoke, lifecycle, boundary, and unsupported-operation cases.
- `tests/Videra.Platform.Linux.Tests/` covers Vulkan smoke and code-ready lifecycle coverage built around X11 fixtures.
- `tests/Videra.Platform.macOS.Tests/` covers Metal smoke and code-ready lifecycle coverage built around NSView fixtures.

## Common Test Patterns

**Unit style:**
- Constructor and property behavior tests use pure in-memory objects plus FluentAssertions.
- Graphics abstraction tests use Moq to verify `IGraphicsBackend`, `ICommandExecutor`, `IResourceFactory`, and buffer contracts.

**Integration style:**
- Core integration tests run through the software backend so they can execute without a real native window.
- Platform lifecycle tests allocate real native handles through `tests/Tests.Common/Platform/NativeHostTestHelpers.cs` when the matching host OS is available.

**Host guarding:**
- Non-matching hosts return early using `RuntimeInformation.IsOSPlatform(...)`.
- This keeps `dotnet test Videra.slnx` runnable on a single developer machine, but it also means some platform coverage is only code-ready until executed on the matching OS.

## Verification Entry Points

**Primary commands:**
```bash
dotnet build Videra.slnx
dotnet test Videra.slnx
pwsh -File ./verify.ps1 -Configuration Release
./verify.sh --configuration Release
```

**Native validation packages:**
```bash
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-macos
```

**What the scripts do:**
- `verify.ps1` and `verify.sh` run solution build, full-solution tests, and demo build.
- Native Linux and macOS validation are optional flags, not part of the default local verification path.

## Coverage Posture

**What is in place:**
- Coverage collection packages are installed.
- A `coverage/` directory exists in the repo.
- The test suite now covers both backend-neutral behavior and platform-specific lifecycle flows at the code level.

**What is not enforced yet:**
- No coverage threshold is enforced in repo tooling.
- The GitHub Actions workflow at `.github/workflows/publish-nuget.yml` does not run tests or publish coverage.
- The planning state still treats native Linux/macOS execution as an open blocker rather than a fully closed quality gate.

## Confirmed Gaps

**Platform execution gap:**
- Windows real-host validation is in place and reflected in `.planning/STATE.md`.
- Linux and macOS lifecycle suites exist, but their required native-host execution is still environment-blocked according to `.planning/STATE.md`.

**Smoke suite mismatch:**
- `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendSmokeTests.cs` still contains a placeholder-style assertion for reusable X11 fixture expectations.
- `tests/Videra.Platform.macOS.Tests/Backend/MetalBackendSmokeTests.cs` still contains a placeholder-style assertion for reusable NSView fixture expectations.
- The stronger lifecycle suites exist beside them, so the real issue is execution/cleanup of legacy smoke placeholders rather than missing all platform tests.

## Manual Testing Role

**Demo app:**
- `samples/Videra.Demo/` remains the manual verification surface for rendering, model import, camera interaction, style changes, and UI integration.
- This is still useful, but it is no longer the only meaningful verification path.

## Practical Guidance For New Tests

- Put backend-neutral behavioral tests in `tests/Videra.Core.Tests/`.
- Put software-backend engine flow tests in `tests/Videra.Core.IntegrationTests/`.
- Put native D3D11, Vulkan, and Metal lifecycle tests in their matching platform test projects.
- Reuse `tests/Tests.Common/Platform/NativeHostTestHelpers.cs` instead of building one-off platform fixtures.
- If a new test depends on a real host OS, guard it explicitly and keep the non-matching-host behavior honest.

## Drift Since The Previous Map

- The old map incorrectly stated that no test framework or test projects existed.
- The repo now has substantial automated coverage across unit, integration, and platform-specific domains.
- The remaining testing problem is not "no tests"; it is "some platform-native validation is code-ready but not yet executed on matching hosts."

---

*Testing analysis refreshed against the live repo on 2026-04-02*
