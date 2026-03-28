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
- Not detected - No `*.test.cs` or `*.spec.cs` files found

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
- Triggered on version tags or manual dispatch
- Steps: checkout, setup .NET 8.0, determine version, pack projects, push to GitHub Packages
- No test execution step in workflow
- Pack targets: Core, Platform.*, Avalonia projects

**Build verification:**
- `dotnet build` - compiles all projects but no explicit test stage
- Platform-specific builds use runtime identifiers (win-x64, linux-x64, osx-x64)

## Manual Testing

**Demo application:**
- `samples/Videra.Demo` serves as manual test harness
- Interactive 3D viewer with model loading
- Camera controls: orbit, pan, zoom
- Render style switching
- Wireframe mode testing

**Debug logging:**
- Console.WriteLine extensively used for runtime observation
- Debug.WriteLine for platform-specific diagnostics
- Environment variables: `VIDERA_FRAMELOG`, `VIDERA_INPUTLOG` for frame/input logging

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
- Backend implementations (D3D11, Vulkan, Metal) have no automated tests
- Model import logic (`ModelImporter`) has no tests
- Shader compilation (Metal in build) has no verification tests
- Camera matrix calculations have no validation tests
- Wireframe edge extraction algorithm has no tests

**Recommended additions:**
- Unit test project for `Videra.Core` logic (camera, geometry, edge extraction)
- Integration tests for model loading (sample GLTF/OBJ files)
- Platform-specific tests using conditional compilation
- CI workflow step for test execution

---

*Testing analysis: 2026-03-28*
