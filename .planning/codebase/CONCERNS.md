# Codebase Concerns

**Analysis Date:** 2026-03-28

## Tech Debt

**Incomplete D3D11 backend implementation:**
- Issue: Several interface methods throw NotImplementedException, indicating incomplete abstraction
- Files: `src/Videra.Platform.Windows/D3D11CommandExecutor.cs`, `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- Impact: Runtime exceptions if the resource binding path is exercised
- Fix approach: Implement SetResourceSet, CreateShader (factored internally), and CreateResourceSet to complete the abstraction

**Demo import stubbed:**
- Issue: Import UI exists but service behavior is not fully wired; fallback to null logs warning
- Files: `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs` (line 204 TODO comment)
- Impact: Import flow degrades gracefully but doesn't provide user-facing messaging
- Fix approach: Wire Avalonia messaging/notifications per TODO; complete importer integration

## Missing Critical Features

**No test coverage:**
- Problem: No test projects or test files exist; backend changes are unprotected
- Files: No `*Test*.csproj` or `*Tests*` files detected
- Blocks: Confidence in refactors, CI validation, cross-platform backend parity

**Platform backends unverified:**
- Problem: Linux Vulkan and macOS Metal implementations lack cross-platform testing infrastructure
- Files: `src/Videra.Platform.Linux/*.cs`, `src/Videra.Platform.macOS/*.cs`
- Risk: Platform-specific regressions go undetected

## Fragile Areas

**Inline shader sources in resource factory:**
- Files: `src/Videra.Platform.Windows/D3D11ResourceFactory.cs` (GetShaderSource method; HLSL embedded as string)
- Why fragile: No external shader files; compilation errors detected only at runtime; harder to author and validate shaders
- Safe modification: Keep embedded for now; consider extracting to .hlsl files if complexity grows

**Debug console output scattered in demo:**
- Files: `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs`, `samples/Videra.Demo/Views/MainWindow.axaml.cs`, `samples/Videra.Demo/App.axaml.cs`
- Why fragile: Relies on Console/Debug.WriteLine for diagnostics; no structured logging
- Test coverage: No logging abstractions; diagnostics unavailable in release builds without Debug output

## Known Bugs

**Platform-specific native library assumptions:**
- Symptoms: Assimp pre-load logic in `samples/Videra.Demo/App.axaml.cs` uses platform-specific paths and hardcoded locations
- Files: `samples/Videra.Demo/App.axaml.cs` (lines 51-84)
- Trigger: Running on environments without Homebrew or standard assimp locations
- Workaround: Resolver fallback implemented; degrades gracefully

## Security Considerations

**No identified critical issues.** Project does not expose external services or handle untrusted input at this layer. Demo imports local models only.

## Performance Bottlenecks

**No critical bottlenecks detected.** Rendering uses platform-native APIs (D3D11/Vulkan/Metal) via Silk.NET bindings; software fallback exists.

## Dependencies at Risk

**Silk.NET 2.21.0:**
- Risk: Breaking changes in future Silk.NET versions may require API adjustments
- Impact: Graphics backend abstractions may need updates
- Migration plan: Monitor Silk.NET changelog; pin versions in `src/Videra.Platform.*/*.csproj`

**Avalonia 11.3.9:**
- Risk: UI framework updates may require XAML and binding adjustments
- Impact: Demo app UI logic
- Migration plan: Follow Avalonia release notes; test UI interactions on upgrade

## Missing Documentation

** CONTRIBUTING.md absent:**
- Problem: No contribution guidelines; build/run steps only in README
- Blocks: Onboarding for new contributors, consistent PR practices

**API documentation for public types minimal:**
- Problem: Public abstractions in `src/Videra.Core/Graphics/Abstractions/*.cs` lack XML docs
- Impact: Consumers must read implementation to understand usage

## Observability Gaps

**Logging framework not adopted:**
- Problem: Debug/Console.WriteLine used throughout demo; no structured logging
- Files: `samples/Videra.Demo/**/*.cs`
- Impact: Diagnostics limited to console output; no log levels or filtering
- Recommendation: Consider ILogger/ILoggerFactory integration for production scenarios

## Environment Configuration

**No .env or secrets detected.** Configuration uses environment variables (VIDERA_BACKEND, VIDERA_FRAMELOG) as documented in README; no hardcoded secrets found.

---

*Concerns audit: 2026-03-28*
