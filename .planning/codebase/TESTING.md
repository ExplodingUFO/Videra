# Testing Patterns

**Analysis Date:** 2026-03-28

## Test Framework

**Runner:**
- Not detected - No test framework configured

**Assertion Library:**
- Not detected

**Test Projects:**
- Not detected - No test projects in solution

**Run Commands:**
```bash
# Not applicable - no test projects found
```

## Test File Organization

**Location:**
- Not detected - No `*.Test.cs`, `*.Tests.cs`, or `*.Spec.cs` files found
- No `test/` or `tests/` directories

**Naming:**
- Not applicable

**Structure:**
```
Not detected
```

## Test Structure

**Suite Organization:**
```text
Not applicable - no test files found
```

**Patterns:**
- Not detected

## Mocking

**Framework:** Not detected

**Patterns:**
```text
Not applicable - no mocking in evidence
```

**What to Mock:**
- Not applicable

**What NOT to Mock:**
- Not applicable

## Fixtures and Factories

**Test Data:**
```text
Not applicable
```

**Location:**
- Not detected

## Coverage

**Requirements:** Not enforced/detected

**View Coverage:**
```bash
# No coverage commands detected
```

## Test Types

**Unit Tests:**
- Not detected

**Integration Tests:**
- Not detected

**E2E Tests:**
- Not detected

## Build Verification

**CI/CD:**
- GitHub Actions workflow: `.github/workflows/publish-nuget.yml`
- Triggered on version tags (`v*`) or manual dispatch
- Steps:
  1. Checkout code
  2. Setup .NET 8.0 SDK
  3. Determine version from tag or input
  4. Pack all projects (`dotnet pack`) for Core, Platform.*, Avalonia
  5. Push NuGet packages to GitHub Packages
- **No test execution step in workflow**
- No build verification step (only pack)

**Build verification:**
- `dotnet build` - compiles all projects but no explicit test stage
- Platform-specific builds use runtime identifiers (win-x64, linux-x64)

## Manual Testing

**Demo application:**
- `samples/Videra.Demo` serves as manual test harness
- Interactive 3D viewer with model loading
- Camera controls: orbit, pan, zoom
- Render style switching (Realistic, Tech, Cartoon, X-Ray, Clay, Wireframe)
- Wireframe mode testing (None, AllEdges, SharpEdges)
- Grid and axis rendering controls

**Debug logging:**
- Console.WriteLine extensively used for runtime observation
- Debug.WriteLine for platform-specific diagnostics
- Conditional frame logging: `EnableFrameLogging` property on `VideraEngine`
- Input event logging inferred from code comments

**Manual test scenarios:**
- Model import (GLTF, GLB, OBJ formats) via `AvaloniaModelImporter`
- Transform editing (position, rotation, scale) through ViewModel bindings
- Style preset application via `RenderStyleService`
- Native window host creation (Windows only)

## Common Patterns

**Async Testing:**
```text
Not applicable
```

**Error Testing:**
```text
Not applicable
```

## Gaps

**Missing testing infrastructure:**
1. No unit test projects or frameworks configured
2. No test execution in CI pipeline
3. No mocking framework for abstracting platform backends
4. No integration tests for cross-platform rendering
5. No performance/benchmark tests

**Observable testing gaps:**
- Backend implementations (`D3D11Backend`, `VulkanBackend`, `MetalBackend`) have no automated tests
- Model import logic (`ModelImporter.Load`, `LoadWithSharpGLTF`, `LoadSimpleObj`) has no tests
- Shader compilation (Metal shaders embedded in code) has no verification tests
- Camera matrix calculations (`OrbitCamera.UpdateProjection`) have no validation tests
- Wireframe edge extraction algorithm (`EdgeExtractor.ExtractUniqueEdges`) has no tests
- Style parameter serialization (`StyleJsonConverter`) has no tests
- Resource factory methods have no tests
- Buffer data upload/validation has no tests

**Recommended additions:**
- Unit test project for `Videra.Core` logic (camera, geometry, edge extraction, serialization)
- Integration tests for model loading (sample GLTF/OBJ files in test assets)
- Platform-specific tests using conditional compilation or test inheritance
- CI workflow step for test execution before packaging
- Mock implementations of `IGraphicsBackend` for headless testing
- Visual regression tests for rendering (if applicable)

---

*Testing analysis: 2026-03-28*
